using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMetalsWS.Services
{
    public record NotificationItem
    {
        public string ThreadId { get; init; } = "";
        public string Title { get; init; } = "";         // group name or username fallback
        public string? FirstName { get; init; }          // DM full name support
        public string? LastName { get; init; }
        public string? AvatarUrl { get; init; }
        public string Preview { get; init; } = "";
        public DateTime Timestamp { get; init; }
    }

    public class NotificationService : IDisposable
    {
        private readonly ChatHubClient _hub;
        private readonly IChatRepository _repo;
        private readonly AuthenticationStateProvider _auth;

        private string? _userId;
        private bool _wired;
        private readonly SemaphoreSlim _initGate = new(1, 1);

        public event Action? Changed;

        // Unread counters per-thread and a small recent activity feed
        private readonly Dictionary<string, int> _threadUnread = new();
        private readonly List<NotificationItem> _recent = new();

        // Simple user cache (for DM name/avatar lookups)
        private readonly Dictionary<string, (string? First, string? Last, string? Avatar, string? UserName)> _userCache = new();

        // Cancellation for in-flight bootstraps
        private CancellationTokenSource? _refreshCts;

        public int UnreadTotal => _threadUnread.Values.Sum();
        public IReadOnlyList<NotificationItem> Recent => _recent;

        public NotificationService(ChatHubClient hub, IChatRepository repo, AuthenticationStateProvider auth)
        {
            _hub = hub;
            _repo = repo;
            _auth = auth;
        }

        public async Task InitializeAsync()
        {
            if (_wired) return;

            await _initGate.WaitAsync();
            try
            {
                if (_wired) return;

                var authState = await _auth.GetAuthenticationStateAsync();
                _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(_userId))
                    return;

                // Hook hub events
                _hub.InboxNewMessage += OnInboxNewMessage;
                _hub.ThreadsUpdated += OnThreadsUpdated;
                _hub.ThreadRead += OnThreadRead;

                await _hub.ConnectAsync();

                // Initial unread bootstrap (paged)
                _ = RefreshCountsFromRepo();

                _wired = true;
            }
            finally
            {
                _initGate.Release();
            }
        }

        public int GetThreadUnread(string? threadId)
            => string.IsNullOrEmpty(threadId) ? 0 : (_threadUnread.TryGetValue(threadId, out var c) ? c : 0);

        public void ClearThread(string threadId)
        {
            _threadUnread[threadId] = 0;
            // optional: clear recent items for that thread so the bell list looks "clean"
            _recent.RemoveAll(n => n.ThreadId == threadId);
            Changed?.Invoke();
        }

        public void ClearAll()
        {
            _threadUnread.Clear();
            _recent.Clear();
            Changed?.Invoke();
        }

        // ---------- Hub event handlers ----------

        private Task OnThreadRead(object _)
            => RefreshCountsFromRepo();

        private Task OnThreadsUpdated()
            => RefreshCountsFromRepo();

        private async Task OnInboxNewMessage(MessageDto dto)
        {
            // Expect dto.ThreadId to be "other user id" for DMs
            if (dto.ThreadId is null) return;

            // Increment unread badge locally
            _threadUnread.TryGetValue(dto.ThreadId, out var c);
            _threadUnread[dto.ThreadId] = c + 1;

            string title = dto.SenderName ?? "New message";
            string? first = null, last = null, avatar = null;

            if (_userId != null)
            {
                if (!dto.ThreadId.StartsWith("g:", StringComparison.Ordinal))
                {
                    // DM — threadId is the "other" user id
                    if (!_userCache.TryGetValue(dto.ThreadId, out var cached))
                    {
                        try
                        {
                            // Two participants: me + other. We want the other.
                            var people = (await _repo.GetThreadParticipantsAsync(dto.ThreadId, _userId)).ToList();
                            var other = people.FirstOrDefault(p => p.Id != _userId) ?? people.FirstOrDefault();

                            if (other != null)
                            {
                                cached = (other.FirstName, other.LastName, other.Avatar, other.UserName);
                                _userCache[dto.ThreadId] = cached;
                            }
                        }
                        catch
                        {
                            // ignore lookup failures; we'll just fall back to sender name
                        }
                    }

                    if (cached != default)
                    {
                        first = cached.First;
                        last = cached.Last;
                        avatar = cached.Avatar;
                        var full = $"{first} {last}".Trim();
                        title = !string.IsNullOrWhiteSpace(full) ? full : (cached.UserName ?? title);
                    }
                }
                else
                {
                    // Group — we don’t have group name handy here; a later ThreadsUpdated will fix counts.
                    // Keep sender name as title or set a generic one:
                    title = string.IsNullOrWhiteSpace(title) ? "Group chat" : title;
                }
            }

            _recent.Insert(0, new NotificationItem
            {
                ThreadId = dto.ThreadId,
                Title = title,
                FirstName = first,
                LastName = last,
                AvatarUrl = avatar,
                Preview = dto.Content ?? "",
                Timestamp = dto.CreatedAt
            });
            if (_recent.Count > 30) _recent.RemoveAt(_recent.Count - 1);

            Changed?.Invoke();
        }

        // ---------- Bootstrap / refresh unread counts (paged & cancellable) ----------

        private async Task RefreshCountsFromRepo()
        {
            if (_userId is null) return;

            // Cancel any in-flight refresh and start a new one
            _refreshCts?.Cancel();
            _refreshCts = new CancellationTokenSource();
            var ct = _refreshCts.Token;

            try
            {
                // Page through summaries;  we only need UnreadCount and Id
                const int pageSize = 50;
                int skip = 0;
                int total = 0;

                // Build a working map
                var map = new Dictionary<string, int>(StringComparer.Ordinal);

                do
                {
                    ct.ThrowIfCancellationRequested();

                    var (items, tot) = await _repo.GetThreadSummariesAsync(
                        _userId, searchQuery: null, skip: skip, take: pageSize, ct: ct);

                    total = tot;

                    foreach (var s in items)
                    {
                        if (s.Id == null) continue;
                        map[s.Id] = s.UnreadCount;
                    }

                    skip += items.Count;
                    // Guard in case repo returns a smaller page than requested
                    if (items.Count == 0) break;

                } while (skip < total);

                // Swap in the new map atomically
                _threadUnread.Clear();
                foreach (var kv in map)
                    _threadUnread[kv.Key] = kv.Value;

                Changed?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // expected when a newer refresh supersedes an older one
            }
            catch
            {
                // swallow: don’t break UI if bootstrap fails transiently
            }
        }

        public void Dispose()
        {
            // no hub unhook here because service is scoped for the session
            _refreshCts?.Cancel();
            _refreshCts?.Dispose();
        }
    }
}

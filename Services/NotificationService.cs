using System.Collections.Concurrent;
using System.Text.Json;
using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CMetalsWS.Services
{
    public class NotificationItem
    {
        public string ThreadId { get; set; } = default!;
        public string? Title { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Preview { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SenderId { get; set; }
    }

    /// <summary>
    /// Tracks unread counts & a small “inbox” feed for the current user.
    /// Keeps itself in sync via SignalR events + occasional repo refresh.
    /// </summary>
    public class NotificationService
    {
        private readonly IChatRepository _repo;
        private readonly ChatHubClient _hub;
        private readonly AuthenticationStateProvider _auth;
        private readonly ChatStateService _chatState;

        private string? _userId;

        // fast lookups
        private readonly ConcurrentDictionary<string, int> _threadUnread = new();
        private readonly LinkedList<NotificationItem> _recent = new(); // newest first
        private const int MaxRecent = 25;

        public event Action? Changed;

        public int UnreadTotal => _threadUnread.Values.Sum();
        public IReadOnlyCollection<NotificationItem> Recent => _recent;

        public NotificationService(
            IChatRepository repo,
            ChatHubClient hub,
            AuthenticationStateProvider auth,
            ChatStateService chatState)
        {
            _repo = repo;
            _hub = hub;
            _auth = auth;
            _chatState = chatState;

            // hook hub events
            _hub.ThreadsUpdated += OnThreadsUpdated;
            _hub.MessageReceived += OnMessageReceived;
            _hub.ThreadRead += OnThreadRead;
        }

        public async Task InitializeAsync()
        {
            if (_userId != null) return;

            var authState = await _auth.GetAuthenticationStateAsync();
            var u = authState.User;
            _userId =
                u.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? u.FindFirst("sub")?.Value
                ?? u.FindFirst("oid")?.Value
                ?? u.FindFirst("uid")?.Value;

            await RefreshFromRepo();
        }

        public int GetThreadUnread(string threadId)
            => _threadUnread.TryGetValue(threadId, out var n) ? n : 0;

        private async Task RefreshFromRepo()
        {
            if (_userId == null) return;
            var list = await _repo.GetThreadSummariesAsync(_userId);
            _threadUnread.Clear();
            foreach (var t in list)
            {
                if (!string.IsNullOrEmpty(t.Id))
                    _threadUnread[t.Id] = Math.Max(0, t.UnreadCount);
            }
            Changed?.Invoke();
        }

        private Task OnThreadsUpdated()
            => RefreshFromRepo();

        private Task OnMessageReceived(MessageDto m)
        {
            // Don’t count my own messages
            if (m.SenderId == _userId) return Task.CompletedTask;

            // If I’m currently viewing this thread, ChatConversation should call MarkRead(),
            // which will clear via ThreadRead event. To avoid flicker we can skip increment
            // when the thread is the active one.
            if (_chatState.ActiveThreadId == m.ThreadId)
                return Task.CompletedTask;

            // increment unread for this thread
            if (!string.IsNullOrEmpty(m.ThreadId))
                _threadUnread.AddOrUpdate(m.ThreadId, 1, (_, old) => old + 1);

            // add to recent feed (cap to MaxRecent)
            _recent.AddFirst(new NotificationItem
            {
                ThreadId = m.ThreadId!,
                Title = m.SenderName,
                AvatarUrl = null,   // you can enrich with a user lookup if you store avatars
                Preview = m.Content,
                Timestamp = m.CreatedAt,
                SenderId = m.SenderId
            });
            while (_recent.Count > MaxRecent) _recent.RemoveLast();

            Changed?.Invoke();
            return Task.CompletedTask;
        }

        private Task OnThreadRead(object payload)
        {
            // payload arrives as JsonElement when subscribed as object
            if (payload is JsonElement je)
            {
                var threadId = je.TryGetProperty("ThreadId", out var t) ? t.GetString() : null;
                var readerId = je.TryGetProperty("ReaderId", out var r) ? r.GetString() : null;

                if (!string.IsNullOrEmpty(threadId) && readerId == _userId)
                {
                    _threadUnread[threadId] = 0;
                    Changed?.Invoke();
                }
            }
            return Task.CompletedTask;
        }
        public void ClearAll()
        {
            // If you want to preserve keys and just zero them:
            foreach (var key in _threadUnread.Keys.ToList())
                _threadUnread[key] = 0;

            // Or if your code tolerates missing keys, you could:
            // _threadUnread.Clear();

            Changed?.Invoke();
        }
        /// <summary>
        /// Optional: call this to locally clear a thread’s unread without waiting for hub echo.
        /// (ChatConversation already causes hub to emit ThreadRead).
        /// </summary>
        public void ClearThread(string threadId)
        {
            _threadUnread[threadId] = 0;
            Changed?.Invoke();
        }
    }
}

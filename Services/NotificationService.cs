using CMetalsWS.Data.Chat;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public record NotificationItem
{
    public string ThreadId { get; init; } = "";
    public string Title { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public string Preview { get; init; } = "";
    public DateTime Timestamp { get; init; }
}

public class NotificationService
{
    private readonly ChatHubClient _hub;
    private readonly IChatRepository _repo;
    private readonly AuthenticationStateProvider _auth;

    private string? _userId;
    private bool _wired;

    public event Action? Changed;

    private readonly Dictionary<string, int> _threadUnread = new();
    private readonly List<NotificationItem> _recent = new();

    public int UnreadTotal => _threadUnread.Values.Sum();
    public IReadOnlyList<NotificationItem> Recent => _recent;

    public NotificationService(ChatHubClient hub, IChatRepository repo, AuthenticationStateProvider auth)
    {
        _hub = hub; _repo = repo; _auth = auth;
    }

    public async Task InitializeAsync()
    {
        if (_wired) return;

        var authState = await _auth.GetAuthenticationStateAsync();
        _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        _hub.InboxNewMessage += OnInboxNewMessage;
        _hub.ThreadsUpdated += OnThreadsUpdated;
        _hub.ThreadRead += OnThreadRead;
        await _hub.ConnectAsync();

        await RefreshCountsFromRepo(); // bootstrap unread counts
        _wired = true;
    }

    private async Task RefreshCountsFromRepo()
    {
        if (_userId is null) return;
        var summaries = await _repo.GetThreadSummariesAsync(_userId);
        _threadUnread.Clear();
        foreach (var s in summaries)
            if (s.Id != null) _threadUnread[s.Id] = s.UnreadCount;
        Changed?.Invoke();
    }

    private Task OnThreadRead(object _) => RefreshCountsFromRepo();
    private Task OnThreadsUpdated() => RefreshCountsFromRepo();

    private async Task OnInboxNewMessage(MessageDto dto)
    {
        if (dto.ThreadId is null) return;

        _threadUnread.TryGetValue(dto.ThreadId, out var c);
        _threadUnread[dto.ThreadId] = c + 1;

        // Build a nice title/avatar (best-effort)
        string title = dto.SenderName ?? "New message";
        string? avatar = null;

        if (_userId != null)
        {
            var people = (await _repo.GetThreadParticipantsAsync(dto.ThreadId, _userId)).ToList();
            var other = people.FirstOrDefault(p => p.Id != _userId);
            if (other != null)
            {
                var full = $"{other.FirstName} {other.LastName}".Trim();
                title = string.IsNullOrWhiteSpace(full) ? (other.UserName ?? title) : full;
                avatar = other.Avatar;
            }
        }

        _recent.Insert(0, new NotificationItem
        {
            ThreadId = dto.ThreadId,
            Title = title,
            AvatarUrl = avatar,
            Preview = dto.Content ?? "",
            Timestamp = dto.CreatedAt
        });
        if (_recent.Count > 20) _recent.RemoveAt(_recent.Count - 1);

        Changed?.Invoke();
    }

    public int GetThreadUnread(string? threadId)
        => threadId is null ? 0 : (_threadUnread.TryGetValue(threadId, out var c) ? c : 0);

    public void ClearThread(string threadId)
    {
        _threadUnread[threadId] = 0;
        _recent.RemoveAll(n => n.ThreadId == threadId);
        Changed?.Invoke();
    }

    public void ClearAll()
    {
        _threadUnread.Clear();
        _recent.Clear();
        Changed?.Invoke();
    }
}

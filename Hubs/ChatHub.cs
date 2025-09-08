using CMetalsWS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMetalsWS.Hubs
{
    [Authorize] // require an authenticated user for the hub
    public class ChatHub : Hub
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        private string GetUserIdOrThrow()
        {
            var id = Context.UserIdentifier
                     ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(id))
            {
                var claims = string.Join(", ", Context.User?.Claims?.Select(c => $"{c.Type}: {c.Value}") ?? []);
                throw new HubException($"Sender not found: missing NameIdentifier. Available claims: {claims}");
            }
            return id;
        }

        public async Task SendMessageToUser(string recipientId, string message)
        {
            var senderId = GetUserIdOrThrow();

            var sender = await _userManager.FindByIdAsync(senderId)
                         ?? throw new HubException("Sender not found.");

            using var context = _contextFactory.CreateDbContext();
            context.ChatMessages.Add(new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                RecipientId = recipientId
            });
            await context.SaveChangesAsync();

            // SignalR routes by UserIdentifier. We use Identity user.Id in NameIdentifier, so this matches.
            await Clients.User(recipientId).SendAsync("ReceiveMessage", sender.Id, message);
            await Clients.Caller.SendAsync("ReceiveMessage", sender.Id, message);
        }

        public async Task SendMessageToGroup(int groupId, string message)
        {
            var senderId = GetUserIdOrThrow();

            var sender = await _userManager.FindByIdAsync(senderId)
                         ?? throw new HubException("Sender not found.");

            using var context = _contextFactory.CreateDbContext();
            context.ChatMessages.Add(new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                ChatGroupId = groupId
            });
            await context.SaveChangesAsync();

            await Clients.Group(groupId.ToString()).SendAsync("ReceiveGroupMessage", sender.Id, groupId, message);
        }

        public Task AddToGroup(int groupId)
            => Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());

        public Task RemoveFromGroup(int groupId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());

        public Task SetTypingStateForUser(string recipientId, bool isTyping)
        {
            var senderId = GetUserIdOrThrow();
            return Clients.User(recipientId).SendAsync("ReceiveTypingState", senderId, isTyping);
        }

        public Task SetTypingStateForGroup(int groupId, bool isTyping)
        {
            var senderId = GetUserIdOrThrow();
            return Clients.Group(groupId.ToString()).SendAsync("ReceiveTypingState", senderId, isTyping);
        }

        public Task AckReadUser(string partnerId, int lastMessageId)
        {
            var readerId = GetUserIdOrThrow();
            // notify the partner that readerId has seen up to lastMessageId
            return Clients.User(partnerId).SendAsync("ReceiveReadReceipt", lastMessageId, readerId);
        }

        public Task AckReadGroup(int groupId, int lastMessageId)
        {
            var readerId = GetUserIdOrThrow();
            // broadcast to the group; clients can filter by sender if needed
            return Clients.Group(groupId.ToString()).SendAsync("ReceiveReadReceipt", lastMessageId, readerId);
        }

        public override async Task OnConnectedAsync()
        {
            // fail fast if unauthenticated / no ID
            if (!(Context.User?.Identity?.IsAuthenticated ?? false))
                throw new HubException("Unauthenticated connection.");
            _ = GetUserIdOrThrow();

            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                using var context = _contextFactory.CreateDbContext();
                var groups = await context.ChatGroupUsers
                    .Where(gu => gu.UserId == user.Id)
                    .Select(gu => gu.ChatGroupId.ToString())
                    .ToListAsync();

                foreach (var g in groups)
                    await Groups.AddToGroupAsync(Context.ConnectionId, g);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                using var context = _contextFactory.CreateDbContext();
                var groups = await context.ChatGroupUsers
                    .Where(gu => gu.UserId == user.Id)
                    .Select(gu => gu.ChatGroupId.ToString())
                    .ToListAsync();

                foreach (var g in groups)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, g);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}

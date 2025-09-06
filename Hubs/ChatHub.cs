using CMetalsWS.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CMetalsWS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        public async Task SendMessageToUser(string recipientId, string message)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender == null)
            {
                throw new HubException("User not found.");
            }

            using var context = _contextFactory.CreateDbContext();
            var chatMessage = new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                RecipientId = recipientId
            };

            context.ChatMessages.Add(chatMessage);
            await context.SaveChangesAsync();

            await Clients.User(recipientId).SendAsync("ReceiveMessage", sender.Id, message);
            await Clients.Caller.SendAsync("ReceiveMessage", sender.Id, message);
        }

        public async Task SendMessageToGroup(int groupId, string message)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender == null)
            {
                throw new HubException("User not found.");
            }

            using var context = _contextFactory.CreateDbContext();
            var chatMessage = new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                ChatGroupId = groupId
            };

            context.ChatMessages.Add(chatMessage);
            await context.SaveChangesAsync();

            await Clients.Group(groupId.ToString()).SendAsync("ReceiveGroupMessage", sender.Id, groupId, message);
        }

        public async Task AddToGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task RemoveFromGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                using var context = _contextFactory.CreateDbContext();
                var userGroups = await context.ChatGroupUsers
                    .Where(gu => gu.UserId == user.Id)
                    .Select(gu => gu.ChatGroupId.ToString())
                    .ToListAsync();

                foreach (var group in userGroups)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                using var context = _contextFactory.CreateDbContext();
                var userGroups = await context.ChatGroupUsers
                    .Where(gu => gu.UserId == user.Id)
                    .Select(gu => gu.ChatGroupId.ToString())
                    .ToListAsync();

                foreach (var group in userGroups)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}

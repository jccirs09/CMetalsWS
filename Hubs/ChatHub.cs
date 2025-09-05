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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendMessageToUser(string recipientId, string message)
        {
            var sender = await _userManager.GetUserAsync(Context.User);
            if (sender == null)
            {
                throw new HubException("User not found.");
            }

            var chatMessage = new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                RecipientId = recipientId
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

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

            var chatMessage = new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = sender.Id,
                ChatGroupId = groupId
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

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
                var userGroups = await _context.ChatGroupUsers
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
                var userGroups = await _context.ChatGroupUsers
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

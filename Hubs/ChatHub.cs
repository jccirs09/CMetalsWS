using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CMetalsWS.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IChatRepository chatRepository, UserManager<ApplicationUser> userManager)
        {
            _chatRepository = chatRepository;
            _userManager = userManager;
        }

        private string GetUserIdOrThrow()
        {
            var id = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new HubException("User identifier is missing.");
            }
            return id;
        }

        public async Task SendMessage(string threadId, string content)
        {
            try
            {
                var senderId = GetUserIdOrThrow();
                var messageDto = await _chatRepository.CreateMessageAsync(threadId, senderId, content);

                if (int.TryParse(threadId, out _))
                {
                    // Group message
                    await Clients.Group(threadId).SendAsync("ReceiveMessage", messageDto);
                }
                else
                {
                    // Direct message
                    await Clients.User(threadId).SendAsync("ReceiveMessage", messageDto);
                    await Clients.Caller.SendAsync("ReceiveMessage", messageDto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Clients.Caller.SendAsync("Error", "An error occurred while sending the message.");
            }
        }

        public Task JoinThread(string threadId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, threadId);
        }

        public Task LeaveThread(string threadId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, threadId);
        }

        public async Task Typing(string threadId, bool isTyping)
        {
            var userId = GetUserIdOrThrow();
            var typingDto = new TypingDto { ThreadId = threadId, UserId = userId, IsTyping = isTyping };

            await Clients.GroupExcept(threadId, Context.ConnectionId).SendAsync("UserTyping", typingDto);
        }

        public async Task MarkRead(string threadId)
        {
            var readerId = GetUserIdOrThrow();
            await _chatRepository.MarkThreadAsReadAsync(threadId, readerId);

            var readDto = new { ThreadId = threadId, ReaderId = readerId, Timestamp = DateTime.UtcNow };

            await Clients.GroupExcept(threadId, Context.ConnectionId).SendAsync("ThreadRead", readDto);
        }

        public async Task AddReaction(int messageId, string emoji)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var messageDto = await _chatRepository.AddReactionAsync(messageId, emoji, userId);
                if (messageDto != null)
                {
                    if (int.TryParse(messageDto.ThreadId, out _))
                    {
                        await Clients.Group(messageDto.ThreadId!).SendAsync("ReactionAdded", messageDto);
                    }
                    else
                    {
                        await Clients.User(messageDto.ThreadId!).SendAsync("ReactionAdded", messageDto);
                        await Clients.Caller.SendAsync("ReactionAdded", messageDto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Clients.Caller.SendAsync("Error", "An error occurred while adding the reaction.");
            }
        }

        public async Task RemoveReaction(int messageId, string emoji)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var messageDto = await _chatRepository.RemoveReactionAsync(messageId, emoji, userId);
                if (messageDto != null)
                {
                    if (int.TryParse(messageDto.ThreadId, out _))
                    {
                        await Clients.Group(messageDto.ThreadId!).SendAsync("ReactionRemoved", messageDto);
                    }
                    else
                    {
                        await Clients.User(messageDto.ThreadId!).SendAsync("ReactionRemoved", messageDto);
                        await Clients.Caller.SendAsync("ReactionRemoved", messageDto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Clients.Caller.SendAsync("Error", "An error occurred while removing the reaction.");
            }
        }

        public async Task UpdatePresence(string status)
        {
            var userId = GetUserIdOrThrow();
            await Clients.All.SendAsync("PresenceChanged", new PresenceDto { UserId = userId, Status = status });
        }

        public async Task PinThread(string threadId, bool isPinned)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                await _chatRepository.PinThreadAsync(threadId, userId, isPinned);
                // Notify the caller that the thread list should be updated
                await Clients.Caller.SendAsync("ThreadsUpdated");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Clients.Caller.SendAsync("Error", "An error occurred while pinning the thread.");
            }
        }

        public async Task PinMessage(int messageId, bool isPinned)
        {
            try
            {
                await _chatRepository.PinMessageAsync(messageId, isPinned);
                var message = await _chatRepository.GetMessageAsync(messageId, GetUserIdOrThrow());
                if (message != null && message.ThreadId != null)
                {
                    if (int.TryParse(message.ThreadId, out _))
                    {
                        await Clients.Group(message.ThreadId).SendAsync("MessagePinned", message);
                    }
                    else
                    {
                        await Clients.User(message.ThreadId).SendAsync("MessagePinned", message);
                        await Clients.Caller.SendAsync("MessagePinned", message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Clients.Caller.SendAsync("Error", "An error occurred while pinning the message.");
            }
        }

        public async Task DeleteMessage(int messageId)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var message = await _chatRepository.GetMessageAsync(messageId, userId);
                if (message == null || message.SenderId != userId)
                {
                    // Maybe send a specific error to the caller
                    return;
                }

                var success = await _chatRepository.DeleteMessageForEveryoneAsync(messageId, userId);
                if (success)
                {
                    if (message != null)
                    {
                        await Clients.Group(message.ThreadId!).SendAsync("MessageDeleted", messageId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex);
                // Optionally, notify the caller of the error
                await Clients.Caller.SendAsync("Error", "An error occurred while deleting the message.");
            }
        }

        public async Task UpdateMessage(int messageId, string newContent)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var message = await _chatRepository.GetMessageAsync(messageId, userId);
                if (message == null || message.SenderId != userId)
                {
                    // Maybe send a specific error to the caller
                    return;
                }

                var messageDto = await _chatRepository.UpdateMessageAsync(messageId, newContent, userId);
                if (messageDto != null)
                {
                    if (int.TryParse(messageDto.ThreadId, out _))
                    {
                        await Clients.Group(messageDto.ThreadId!).SendAsync("MessageUpdated", messageDto);
                    }
                    else
                    {
                        await Clients.User(messageDto.ThreadId!).SendAsync("MessageUpdated", messageDto);
                        await Clients.Caller.SendAsync("MessageUpdated", messageDto);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex);
                // Optionally, notify the caller of the error
                await Clients.Caller.SendAsync("Error", "An error occurred while updating the message.");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdOrThrow();
            var summaries = await _chatRepository.GetThreadSummariesAsync(userId);
            foreach (var summary in summaries)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, summary.Id!);
            }
            // Announce presence
            await Clients.All.SendAsync("PresenceChanged", new PresenceDto { UserId = userId, Status = "Online" });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserIdOrThrow();
            var summaries = await _chatRepository.GetThreadSummariesAsync(userId);
            foreach (var summary in summaries)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, summary.Id!);
            }
            // Announce presence
            await Clients.All.SendAsync("PresenceChanged", new PresenceDto { UserId = userId, Status = "Offline" });

            await base.OnDisconnectedAsync(exception);
        }
    }
}

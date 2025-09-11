using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
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

        private static string GetUserGroupName(string userId) => $"user:{userId}";
        private static string GetDmGroupName(string user1, string user2) =>
            string.CompareOrdinal(user1, user2) < 0
                ? $"dm:{user1}:{user2}"
                : $"dm:{user2}:{user1}";

        private static string GetThreadGroupKey(string threadId, string currentUserId) =>
            threadId.StartsWith("g:")
                ? threadId
                : GetDmGroupName(currentUserId, threadId);

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdOrThrow();
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserIdOrThrow();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinThread(string threadId)
        {
            await VerifyUserIsParticipantAsync(threadId);
            var userId = GetUserIdOrThrow();
            var groupKey = GetThreadGroupKey(threadId, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupKey);
        }

        public async Task LeaveThread(string threadId)
        {
            var userId = GetUserIdOrThrow();
            var groupKey = GetThreadGroupKey(threadId, userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupKey);
        }

        public async Task SendMessage(string threadId, string content)
        {
            var currentUserId = GetUserIdOrThrow();
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return;

                await VerifyUserIsParticipantAsync(threadId);

                // Persist the message and get the DTO we broadcast everywhere
                var dto = await _chatRepository.CreateMessageAsync(threadId, currentUserId, content);

                // Broadcast to the active thread group (DM normalized or group)
                var key = GetThreadGroupKey(threadId, currentUserId);
                await Clients.Group(key).SendAsync("ReceiveMessage", dto);

                // Update thread lists and nudge inbox for each participant
                var participants = await _chatRepository.GetThreadParticipantsAsync(threadId, currentUserId);
                foreach (var uid in participants.Where(u => !string.IsNullOrEmpty(u.Id)).Select(u => u.Id!))
                {
                    await Clients.Group(GetUserGroupName(uid)).SendAsync("ThreadsUpdated");

                    // inbox ping for everyone except the sender
                    if (uid != currentUserId)
                    {
                        await Clients.Group(GetUserGroupName(uid)).SendAsync("InboxNewMessage", dto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub.SendMessage] user={currentUserId} threadId={threadId} failed: {ex}");
                throw new HubException($"SendMessage failed: {ex.GetBaseException().Message}");
            }
        }


        public async Task UpdateMessage(int messageId, string newContent)
        {
            var userId = GetUserIdOrThrow();
            var updatedMessage = await _chatRepository.UpdateMessageAsync(messageId, newContent, userId);
            if (updatedMessage?.ThreadId != null)
            {
                var groupKey = GetThreadGroupKey(updatedMessage.ThreadId, userId);
                await Clients.Group(groupKey).SendAsync("MessageUpdated", updatedMessage);
            }
        }

        public async Task DeleteMessage(int messageId)
        {
            var userId = GetUserIdOrThrow();
            var message = await _chatRepository.GetMessageAsync(messageId, userId);
            if (message?.ThreadId == null) return;

            var success = await _chatRepository.DeleteMessageForEveryoneAsync(messageId, userId);
            if (success)
            {
                var groupKey = GetThreadGroupKey(message.ThreadId, userId);
                await Clients.Group(groupKey).SendAsync("MessageDeleted", messageId);
            }
        }

        public async Task AddReaction(int messageId, string emoji)
        {
            var userId = GetUserIdOrThrow();
            var messageDto = await _chatRepository.AddReactionAsync(messageId, emoji, userId);
            if (messageDto?.ThreadId != null)
            {
                var groupKey = GetThreadGroupKey(messageDto.ThreadId, userId);
                await Clients.Group(groupKey).SendAsync("MessageUpdated", messageDto);
            }
        }

        public async Task RemoveReaction(int messageId, string emoji)
        {
            var userId = GetUserIdOrThrow();
            var messageDto = await _chatRepository.RemoveReactionAsync(messageId, emoji, userId);
            if (messageDto?.ThreadId != null)
            {
                var groupKey = GetThreadGroupKey(messageDto.ThreadId, userId);
                await Clients.Group(groupKey).SendAsync("MessageUpdated", messageDto);
            }
        }

        public async Task PinMessage(int messageId, bool isPinned)
        {
            var userId = GetUserIdOrThrow();
            var success = await _chatRepository.PinMessageAsync(messageId, isPinned);
            if (success)
            {
                var message = await _chatRepository.GetMessageAsync(messageId, userId);
                if (message?.ThreadId != null)
                {
                    var groupKey = GetThreadGroupKey(message.ThreadId, userId);
                    await Clients.Group(groupKey).SendAsync("MessagePinned", message);
                }
            }
        }

        public async Task PinThread(string threadId, bool isPinned)
        {
            var userId = GetUserIdOrThrow();
            var success = await _chatRepository.PinThreadAsync(threadId, userId, isPinned);
            if (success)
            {
                await Clients.Group(GetUserGroupName(userId)).SendAsync("ThreadsUpdated");
            }
        }

        public async Task Typing(string threadId, bool isTyping)
        {
            var userId = GetUserIdOrThrow();
            var groupKey = GetThreadGroupKey(threadId, userId);
            await Clients.GroupExcept(groupKey, Context.ConnectionId).SendAsync("UserTyping", new TypingDto { ThreadId = threadId, UserId = userId, IsTyping = isTyping });
        }

        public async Task MarkRead(string threadId)
        {
            var userId = GetUserIdOrThrow();
            await _chatRepository.MarkThreadAsReadAsync(threadId, userId);
            // Notify other sessions of the current user that the thread is read
            await Clients.Group(GetUserGroupName(userId)).SendAsync("ThreadRead", new { ThreadId = threadId, ReaderId = userId });
            await Clients.Group(GetUserGroupName(userId)).SendAsync("ThreadsUpdated");
        }

        private string GetUserIdOrThrow()
        {
            var id = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new HubException("User identifier is missing.");
            }
            return id;
        }

        private async Task VerifyUserIsParticipantAsync(string threadId)
        {
            var userId = GetUserIdOrThrow();
            var isParticipant = await _chatRepository.IsParticipantAsync(threadId, userId);
            if (!isParticipant)
            {
                throw new HubException("You are not a participant of this thread.");
            }
        }
    }
}

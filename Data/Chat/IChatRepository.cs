using CMetalsWS.Data.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Data.Chat
{
    public interface IChatRepository
    {
        Task<IEnumerable<ThreadSummary>> GetThreadSummariesAsync(string userId, string? searchQuery = null);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(string threadId, string currentUserId, DateTime? before = null, int take = 50);
        Task<MessageDto?> GetMessageAsync(int messageId, string currentUserId);
        Task<MessageDto> CreateMessageAsync(string threadId, string senderId, string content);
        Task<MessageDto?> UpdateMessageAsync(int messageId, string newContent, string currentUserId);
        Task<bool> DeleteMessageForEveryoneAsync(int messageId, string currentUserId);
        Task<MessageDto?> AddReactionAsync(int messageId, string emoji, string userId);
        Task<MessageDto?> RemoveReactionAsync(int messageId, string emoji, string userId);
        Task MarkThreadAsReadAsync(string threadId, string userId);
        Task<IEnumerable<ApplicationUser>> GetThreadParticipantsAsync(string threadId, string currentUserId);
        Task<bool> PinThreadAsync(string threadId, string userId, bool isPinned);
        Task<bool> PinMessageAsync(int messageId, bool isPinned);

        Task<bool> IsParticipantAsync(string threadId, string userId);

        // Group Management
        Task<IEnumerable<ChatGroup>> GetAllGroupsAsync();
        Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds);
        Task UpdateGroupAsync(ChatGroup group, List<string> userIds);
        Task DeleteGroupAsync(int groupId);
    }
}

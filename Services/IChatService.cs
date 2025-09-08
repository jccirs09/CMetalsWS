using CMetalsWS.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IChatService
    {
        Task<List<ApplicationUser>> GetUsersAsync();
        Task<ApplicationUser?> GetUserDetailsAsync(string userId);
        Task<List<ChatMessage>> GetConversationAsync(string currentUserId, string contactId);
        Task SaveMessageAsync(ChatMessage message);
        Task<List<ChatGroup>> GetUserGroupsAsync(string userId);
        Task<List<ChatGroup>> GetAllGroupsAsync();
        Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds);
        Task UpdateGroupAsync(ChatGroup group, List<string> userIds);
        Task DeleteGroupAsync(int groupId);
        Task<List<ChatMessage>> GetGroupConversationAsync(int groupId);
        Task<List<ChatMessage>> GetRecentConversationsAsync(string userId);
        Task<ChatGroup?> GetGroupAsync(int groupId);
        Task<List<ChatMessage>> GetConversationBeforeAsync(string currentUserId, string contactId, DateTime before);
        Task<List<ChatMessage>> GetGroupConversationBeforeAsync(int groupId, DateTime before);
    }
}

using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IChatService
    {
        Task<Dictionary<string, List<ApplicationUser>>> SearchUsersAsync(string query, int? branchId = null, int skip = 0, int take = 30);
        Task<List<ApplicationUser>> GetBranchUsersAsync(int branchId, int skip = 0, int take = 100);
        Task<List<ChatGroup>> SearchGroupsAsync(string query, int skip = 0, int take = 30);
        Task<ThreadSummary> GetOrCreateThreadAsync(string currentUserId, string? otherUserId = null, int? groupId = null);
    }
}

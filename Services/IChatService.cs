using CMetalsWS.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IChatService
    {
        Task<List<ApplicationUser>> SearchUsersAsync(string query, int skip = 0, int take = 30);
        Task<List<ChatGroup>> SearchGroupsAsync(string query, int skip = 0, int take = 30);
    }
}

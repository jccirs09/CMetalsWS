using System.Collections.Generic;
using System.Threading.Tasks;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IAssignmentService
    {
        Task<List<CMetalsWS.Models.AssignableOptionDto>> ListAssignableOptionsAsync(int itemId);
        Task AssignToMachineAsync(int itemId, int machineId, string userId);
        Task SendToPullingAsync(int itemId, BuildingCategory category, string userId);
        Task ClearAssignmentAsync(int itemId, string userId);
    }
}

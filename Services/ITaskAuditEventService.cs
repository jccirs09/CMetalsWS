using CMetalsWS.Data;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface ITaskAuditEventService
    {
        Task CreateAuditEventAsync(int taskId, TaskType taskType, AuditEventType eventType, string userId, string? notes = null);
        Task<AuditEventType?> GetLastEventTypeForTaskAsync(int taskId, TaskType taskType);
        Task<Dictionary<int, AuditEventType>> GetLastEventTypesForTasksAsync(List<int> taskIds, TaskType taskType);
    }
}

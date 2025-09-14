using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IWorkOrderService
    {
        Task<List<WorkOrder>> GetAsync(int? branchId = null);
        Task<List<WorkOrder>> GetByCategoryAsync(MachineCategory category, int? branchId = null);
        Task<WorkOrder?> GetByIdAsync(int id);
        Task CreateAsync(WorkOrder workOrder, string userId);
        Task CreateAsync(WorkOrder workOrder, string createdBy, string userId);
        Task UpdateAsync(WorkOrder workOrder, string updatedBy);
        Task ScheduleAsync(int id, DateTime start, DateTime? end);
        Task MarkWorkOrderCompleteAsync(WorkOrder workOrder, string updatedBy);
        Task SetStatusAsync(int id, WorkOrderStatus status, string updatedBy);

        // New operator commands
        Task StartWorkOrderAsync(int workOrderId, string userId);
        Task PauseWorkOrderAsync(int workOrderId, string userId);
        Task ResumeWorkOrderAsync(int workOrderId, string userId);
        Task CompleteWorkOrderAsync(int workOrderId, string userId);
        Task SwapCoilAsync(int workOrderId, int newCoilInventoryId, CoilSwapReason reason, string? notes, string userId);
    }
}

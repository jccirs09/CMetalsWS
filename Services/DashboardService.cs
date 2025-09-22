using CMetalsWS.Data;
using CMetalsWS.Models;
using CMetalsWS.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly PickingListService _pickingListService;
    private readonly ITaskAuditEventService _taskAuditEventService;
    private readonly UserService _userService;

    public DashboardService(
        ApplicationDbContext db,
        PickingListService pickingListService,
        ITaskAuditEventService taskAuditEventService,
        UserService userService)
    {
        _db = db;
        _pickingListService = pickingListService;
        _taskAuditEventService = taskAuditEventService;
        _userService = userService;
    }

    public async Task<List<NowPlayingDto>> GetNowPlayingAsync(int? branchId)
    {
        var sheetPullingQueue = await _pickingListService.GetSheetPullingQueueAsync();

        if (branchId.HasValue)
        {
            sheetPullingQueue = sheetPullingQueue.Where(t => t.PickingList.BranchId == branchId.Value).ToList();
        }

        if (!sheetPullingQueue.Any())
            return new List<NowPlayingDto>();

        var taskIds = sheetPullingQueue.Select(t => t.Id).ToList();
        var lastEvents = await _taskAuditEventService.GetLastEventsForTasksAsync(taskIds, TaskType.Pulling);

        var inProgressTasks = sheetPullingQueue
            .Where(t => lastEvents.TryGetValue(t.Id, out var lastEvent) &&
                        (lastEvent.EventType == AuditEventType.Start || lastEvent.EventType == AuditEventType.Resume))
            .ToList();

        if (!inProgressTasks.Any())
            return new List<NowPlayingDto>();

        var operatorIds = lastEvents.Values.Select(e => e.UserId).Distinct().ToList();
        var operators = await _userService.GetUsersByIdsAsync(operatorIds);
        var operatorDict = operators.ToDictionary(u => u.Id, u => u.FullName);

        var result = new List<NowPlayingDto>();
        foreach (var task in inProgressTasks)
        {
            var progress = (task.Weight ?? 0) == 0 ? 0 : (int)((task.PulledWeight / task.Weight) * 100);
            lastEvents.TryGetValue(task.Id, out var lastEvent);
            var operatorName = (lastEvent != null && operatorDict.TryGetValue(lastEvent.UserId, out var name)) ? name : "N/A";

            result.Add(new NowPlayingDto
            {
                SalesOrderNumber = task.PickingList.SalesOrderNumber,
                Status = "In Progress",
                MachineName = task.Machine?.Name ?? "N/A",
                CustomerName = task.PickingList.SoldTo,
                LineItems = task.PickingList.Items.Count,
                TotalWeight = task.PickingList.TotalWeight,
                OperatorName = operatorName,
                Progress = progress > 100 ? 100 : (int)progress
            });
        }

        return result;
    }

    public async Task<ProductionDashboardDto> GetProductionSummaryAsync(int? branchId)
    {
        var workOrders = await _db.WorkOrders
            .Where(w => !branchId.HasValue || w.BranchId == branchId.Value)
            .ToListAsync();

        var current = workOrders.Count(w => w.Status == WorkOrderStatus.InProgress);
        var pending = workOrders.Count(w => w.Status == WorkOrderStatus.Pending);
        var completed = workOrders.Count(w => w.Status == WorkOrderStatus.Completed);
        var awaiting = workOrders.Count(w => w.Status == WorkOrderStatus.Awaiting);

        // Add more aggregated logic here.

        return new ProductionDashboardDto
        {
            Current = current,
            Pending = pending,
            Completed = completed,
            Awaiting = awaiting
        };
    }
}

public class ProductionDashboardDto
{
    public int Current { get; set; }
    public int Pending { get; set; }
    public int Completed { get; set; }
    public int Awaiting { get; set; }
}

using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly PickingListService _pickingListService;
        private readonly ITaskAuditEventService _taskAuditEventService;
        private readonly UserService _userService;
        private readonly MachineService _machineService;

        public DashboardService(
            ApplicationDbContext db,
            PickingListService pickingListService,
            ITaskAuditEventService taskAuditEventService,
            UserService userService,
            MachineService machineService)
        {
            _db = db;
            _pickingListService = pickingListService;
            _taskAuditEventService = taskAuditEventService;
            _userService = userService;
            _machineService = machineService;
        }

        public async Task<List<MachinePullingStatusDto>> GetMachinePullingStatusAsync(int? branchId)
        {
            var machines = (await _machineService.GetMachinesAsync())
                .Where(m => m.Category == MachineCategory.Sheet || m.Category == MachineCategory.Coil);

            if (branchId.HasValue)
            {
                machines = machines.Where(m => m.BranchId == branchId.Value);
            }

            var sheetTasks = await _pickingListService.GetSheetPullingQueueAsync();
            var coilTasks = await _pickingListService.GetCoilPullingQueueAsync();
            var allAssignedTasks = sheetTasks.Concat(coilTasks).ToList();

            var result = new List<MachinePullingStatusDto>();

            var allTaskIds = allAssignedTasks.Select(t => t.Id).ToList();
            var lastEvents = await _taskAuditEventService.GetLastEventsForTasksAsync(allTaskIds, TaskType.Picking);

            var allOperatorIds = lastEvents.Values.Select(e => e.UserId).Distinct().ToList();
            var operators = await _userService.GetUsersByIdsAsync(allOperatorIds);
            var operatorDict = operators.ToDictionary(u => u.Id, u => u.FullName);

            var allActiveTasks = allAssignedTasks
                .Where(t => lastEvents.TryGetValue(t.Id, out var lastEvent) &&
                            (lastEvent.EventType == AuditEventType.Start ||
                             lastEvent.EventType == AuditEventType.Resume ||
                             lastEvent.EventType == AuditEventType.Pause))
                .ToList();

            var activePickingListIds = allActiveTasks.Select(t => t.PickingListId).Distinct().ToList();
            var activePickingLists = await _db.PickingLists
                .Include(p => p.Items)
                .AsNoTracking()
                .Where(p => activePickingListIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);


            foreach (var machine in machines)
            {
                var machineStatus = new MachinePullingStatusDto
                {
                    MachineName = machine.Name,
                    InProgressOrders = new List<NowPlayingDto>()
                };

                var tasksForMachine = allAssignedTasks.Where(t => t.MachineId == machine.Id).ToList();
                machineStatus.TotalAssignedItems = tasksForMachine.Count;
                machineStatus.TotalAssignedWeight = tasksForMachine.Sum(t => t.Weight ?? 0);

                var activeTasksForMachine = allActiveTasks
                    .Where(t => t.MachineId == machine.Id)
                    .ToList();

                if (activeTasksForMachine.Any())
                {
                    var activeGroups = activeTasksForMachine.GroupBy(t => t.PickingListId);

                    foreach (var group in activeGroups)
                    {
                        if (!activePickingLists.TryGetValue(group.Key, out var pickingList))
                        {
                            continue;
                        }

                        var totalItems = pickingList.Items.Count;
                        var completedItems = pickingList.Items.Count(i => i.Status == PickingLineStatus.Completed);
                        var progress = (totalItems == 0) ? 0 : (completedItems * 100) / totalItems;

                        var firstTaskInGroup = group.First();
                        lastEvents.TryGetValue(firstTaskInGroup.Id, out var lastEvent);
                        var operatorName = (lastEvent != null && operatorDict.TryGetValue(lastEvent.UserId, out var name)) ? name : "N/A";
                        var status = (lastEvent?.EventType == AuditEventType.Pause) ? "Paused" : "In Progress";

                        machineStatus.InProgressOrders.Add(new NowPlayingDto
                        {
                            SalesOrderNumber = pickingList.SalesOrderNumber,
                            Status = status,
                            MachineName = machine.Name,
                            CustomerName = pickingList.SoldTo,
                            LineItems = totalItems,
                            TotalWeight = pickingList.TotalWeight,
                            OperatorName = operatorName,
                            Progress = progress
                        });
                    }
                }
                else
                {
                    // Find the last completed task for this machine
                    var lastCompletedEvent = await _db.TaskAuditEvents
                        .Where(e => e.TaskType == TaskType.Packing && e.EventType == AuditEventType.Complete && e.PickingListItemId.HasValue)
                        .Join(_db.PickingListItems, e => e.PickingListItemId.Value, i => i.Id, (e, i) => new { Event = e, Item = i })
                        .Where(x => x.Item.MachineId == machine.Id)
                        .OrderByDescending(x => x.Event.Timestamp)
                        .FirstOrDefaultAsync();


                    if (lastCompletedEvent != null)
                    {
                        var task = await _db.PickingListItems
                            .Include(i => i.PickingList)
                            .ThenInclude(p => p.Items)
                            .Include(i => i.Machine)
                            .FirstOrDefaultAsync(i => i.Id == lastCompletedEvent.Event.PickingListItemId);

                        if (task != null)
                        {
                            var operatorName = operatorDict.TryGetValue(lastCompletedEvent.Event.UserId, out var name) ? name : "N/A";
                            machineStatus.LastCompletedOrder = new NowPlayingDto
                            {
                                SalesOrderNumber = task.PickingList.SalesOrderNumber,
                                Status = "Completed",
                                MachineName = task.Machine?.Name ?? "N/A",
                                CustomerName = task.PickingList.SoldTo,
                                LineItems = task.PickingList.Items.Count,
                                TotalWeight = task.PickingList.TotalWeight,
                                OperatorName = operatorName,
                                Progress = 100
                            };
                        }
                    }
                }
                result.Add(machineStatus);
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

        public async Task<List<RegionStatsDto>> GetRegionStatsAsync(int? branchId)
        {
            var regions = await _db.DestinationRegions.AsNoTracking().ToListAsync();
            var stats = regions.ToDictionary(r => r.Id, r => new RegionStatsDto { RegionId = r.Id, RegionName = r.Name });

            // Get load statistics
            var loadsByRegion = await _db.Loads
                .Where(l => l.DestinationRegionId != null && (!branchId.HasValue || l.OriginBranchId == branchId.Value))
                .GroupBy(l => l.DestinationRegionId!.Value)
                .Select(g => new
                {
                    RegionId = g.Key,
                    TotalLoads = g.Count(),
                    ActiveLoads = g.Count(l => l.Status != LoadStatus.Delivered)
                })
                .ToListAsync();

            foreach (var loadStat in loadsByRegion)
            {
                if (stats.TryGetValue(loadStat.RegionId, out var regionStat))
                {
                    regionStat.ActiveOrders = loadStat.ActiveLoads;
                    regionStat.CompletionPercentage = loadStat.TotalLoads > 0 ? (int)((double)(loadStat.TotalLoads - loadStat.ActiveLoads) / loadStat.TotalLoads * 100) : 100;
                }
            }

            // Get pending pickup statistics
            var allAvailableLists = await _pickingListService.GetAvailableForLoadingAsync(branchId);
            var ordersByRegion = allAvailableLists
                .Where(p => p.Customer?.DestinationRegionId != null)
                .GroupBy(p => p.Customer!.DestinationRegionId!.Value);

            foreach (var group in ordersByRegion)
            {
                if (stats.TryGetValue(group.Key, out var regionStat))
                {
                    regionStat.TotalOrders = group.Count();
                    regionStat.PendingPickups = group.Count();
                    regionStat.TotalWeight = group.Sum(pl => pl.Items.Sum(i => i.RemainingWeight));
                }
            }

            foreach (var regionStat in stats.Values)
            {
                // Set other placeholders
                regionStat.AverageCost = 0; // Placeholder
                regionStat.AverageDeliveryTime = "N/A"; // Placeholder
                regionStat.PrimaryContactName = "N/A"; // Placeholder
                regionStat.PrimaryContactPhone = "N/A"; // Placeholder
            }

            return stats.Values.OrderBy(s => s.RegionName).ToList();
        }
    }
}
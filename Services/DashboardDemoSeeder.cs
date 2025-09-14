using CMetalsWS.Configuration;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class DashboardDemoSeeder
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IOptions<DashboardSettings> _settings;

        public DashboardDemoSeeder(IDbContextFactory<ApplicationDbContext> contextFactory, IOptions<DashboardSettings> settings)
        {
            _contextFactory = contextFactory;
            _settings = settings;
        }

        public async Task SeedAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Seeding should be idempotent. If we already have one of our demo machines, we assume the DB is seeded.
            if (await context.Machines.AnyAsync(m => m.Name == "CTL Line 1"))
            {
                return;
            }

            var branch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Surrey");
            if (branch == null)
            {
                // If the main seeder hasn't run, we can't continue.
                // This seeder depends on the IdentityDataSeeder running first.
                return;
            }

            var machines = new List<Machine>
            {
                new Machine { Name = "CTL Line 1", Code = "CTL1", BranchId = branch.Id, Category = MachineCategory.CTL, RateValue = 2600, RateUnits = RateUnit.WeightPerHour, DefaultSetupMins = 30 },
                new Machine { Name = "CTL Line 2", Code = "CTL2", BranchId = branch.Id, Category = MachineCategory.CTL, RateValue = 2800, RateUnits = RateUnit.WeightPerHour, DefaultSetupMins = 30 },
                new Machine { Name = "Slitter A", Code = "SLIT1", BranchId = branch.Id, Category = MachineCategory.Slitter, RateValue = 1800, RateUnits = RateUnit.WeightPerHour, DefaultSetupMins = 60 },
            };
            await context.Machines.AddRangeAsync(machines);
            await context.SaveChangesAsync();

            var workOrders = new List<WorkOrder>
            {
                new WorkOrder
                {
                    WorkOrderNumber = "WO-1001", TagNumber = "T1001", BranchId = branch.Id, MachineId = machines[0].Id,
                    MachineCategory = machines[0].Category, DueDate = DateTime.Today, Status = WorkOrderStatus.InProgress,
                    ScheduledStartDate = DateTime.UtcNow.AddHours(-2), ScheduledEndDate = DateTime.UtcNow.AddHours(2),
                    ActualStartDate = DateTime.UtcNow.AddHours(-2), EstimatedMinutes = 240,
                    Items = new List<WorkOrderItem> { new WorkOrderItem { ItemCode = "18GA-STEEL", Description = "18GA Galvanized Steel", CustomerName = "Midwest Steel" } }
                },
                new WorkOrder
                {
                    WorkOrderNumber = "WO-1002", TagNumber = "T1002", BranchId = branch.Id, MachineId = machines[1].Id,
                    MachineCategory = machines[1].Category, DueDate = DateTime.Today, Status = WorkOrderStatus.InProgress,
                    ScheduledStartDate = DateTime.UtcNow.AddHours(-1), ScheduledEndDate = DateTime.UtcNow.AddHours(3),
                    ActualStartDate = DateTime.UtcNow.AddHours(-1), EstimatedMinutes = 240,
                    Items = new List<WorkOrderItem> { new WorkOrderItem { ItemCode = "14GA-ALUM", Description = "14GA Aluminum Sheet", CustomerName = "Alumax" } },
                    CoilUsages = new List<WorkOrderCoilUsage>
                    {
                        new WorkOrderCoilUsage { Sequence = 1, CoilInventoryId = "INV-COIL-001", CoilTagNumber = "C-A-1", StartedAt = DateTime.UtcNow.AddHours(-1) },
                        new WorkOrderCoilUsage { Sequence = 2, CoilInventoryId = "INV-COIL-002", CoilTagNumber = "C-A-2", StartedAt = DateTime.UtcNow.AddMinutes(-30), Reason = "End of coil" }
                    }
                },
                new WorkOrder
                {
                    WorkOrderNumber = "WO-1003", TagNumber = "T1003", BranchId = branch.Id, MachineId = machines[2].Id,
                    MachineCategory = machines[2].Category, DueDate = DateTime.Today, Status = WorkOrderStatus.Pending, // This one is in setup
                    ScheduledStartDate = DateTime.UtcNow.AddHours(1), ScheduledEndDate = DateTime.UtcNow.AddHours(4),
                    EstimatedMinutes = 180,
                    Items = new List<WorkOrderItem> { new WorkOrderItem { ItemCode = "20GA-STAINLESS", Description = "20GA Stainless Steel", CustomerName = "Detroit Fabrication" } }
                }
            };
            await context.WorkOrders.AddRangeAsync(workOrders);
            await context.SaveChangesAsync();

            if (_settings.Value.LogisticsEnabled)
            {
                var loads = new List<Load>();
                for (int i = 0; i < 18; i++)
                {
                    loads.Add(new Load
                    {
                        LoadNumber = $"LD-{DateTime.Today:yyMMdd}-{i+1}",
                        OriginBranchId = branch.Id,
                        ShippingDate = DateTime.Today,
                        Status = (i < 6) ? LoadStatus.InTransit : LoadStatus.Scheduled,
                        TotalWeight = 12000 + i * 500
                    });
                }
                await context.Loads.AddRangeAsync(loads);
                await context.SaveChangesAsync();
            }

            if (_settings.Value.PullingEnabled)
            {
                var pickingList = new PickingList { SalesOrderNumber = "PULL-DEMO-20250913", BranchId = branch.Id, Status = PickingListStatus.InProgress };
                context.PickingLists.Add(pickingList);
                await context.SaveChangesAsync();

                var user1 = await context.Users.FirstOrDefaultAsync(u => u.Email.Contains("admin"));
                if (user1 == null) { return; }

                var pullingTasks = new List<PickingListItem>
                {
                    new PickingListItem { PickingListId = pickingList.Id, ItemId = "PULL-001", ItemDescription = "Demo Pull Item 1", Status = PickingLineStatus.InProgress, Weight = 1240, Quantity = 1 },
                    new PickingListItem { PickingListId = pickingList.Id, ItemId = "PULL-002", ItemDescription = "Demo Pull Item 2", Status = PickingLineStatus.InProgress, Weight = 890, Quantity = 1 }
                };
                await context.PickingListItems.AddRangeAsync(pullingTasks);
                await context.SaveChangesAsync();

                var auditEvents = new List<TaskAuditEvent>
                {
                    new TaskAuditEvent { TaskId = pullingTasks[0].Id, TaskType = TaskType.Pulling, EventType = AuditEventType.Start, Timestamp = DateTime.UtcNow.AddMinutes(-92), UserId = user1.Id },
                    new TaskAuditEvent { TaskId = pullingTasks[1].Id, TaskType = TaskType.Pulling, EventType = AuditEventType.Start, Timestamp = DateTime.UtcNow.AddMinutes(-45), UserId = user1.Id }
                };
                await context.TaskAuditEvents.AddRangeAsync(auditEvents);
                await context.SaveChangesAsync();
            }
        }
    }
}

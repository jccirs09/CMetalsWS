using CMetalsWS.Configuration;
using CMetalsWS.Data;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DashboardDemoSeeder(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IOptions<DashboardSettings> settings,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _contextFactory = contextFactory;
            _settings = settings;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // For this demo, we'll start with a clean slate on each run to ensure consistency.
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();

            // Seed the essential Admin role and user, as they are needed by the seeder logic.
            const string adminRoleName = "Admin";
            if (!await _roleManager.RoleExistsAsync(adminRoleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName, Description = "Administrator" });
            }

            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
                var result = await _userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                }
            }

            var branch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Surrey");
            if (branch == null)
            {
                branch = new Branch { Name = "Surrey", Code = "SUR" };
                context.Branches.Add(branch);
                await context.SaveChangesAsync();
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
                // This is a simplified representation. A real seeder would be more complex.
                var pickingList = new PickingList { SalesOrderNumber = "PULL-DEMO-20250913", BranchId = branch.Id, Status = PickingListStatus.InProgress };
                context.PickingLists.Add(pickingList);
                await context.SaveChangesAsync();

                var user1 = await context.Users.FirstOrDefaultAsync(u => u.Email.Contains("admin"));
                if (user1 == null) { /* In a real app, create a demo user */ return; }

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

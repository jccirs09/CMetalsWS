using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMetalsWS.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<AssignmentService> _logger;
        private readonly ITaskAuditEventService _taskAuditEventService;

        public AssignmentService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ILogger<AssignmentService> logger,
            ITaskAuditEventService taskAuditEventService)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _taskAuditEventService = taskAuditEventService;
        }

        public async Task<List<AssignableOptionDto>> ListAssignableOptionsAsync(int itemId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.Include(i => i.PickingList).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return new List<AssignableOptionDto>();

            var machines = await db.Machines
                .Where(m => m.BranchId == item.PickingList.BranchId)
                .ToListAsync();

            var options = new List<AssignableOptionDto>();
            if (item.Length.HasValue)
            {
                options.AddRange(machines.Where(m => m.Category == MachineCategory.CTL).Select(m => new AssignableOptionDto { Name = m.Name, Type = "Machine", MachineId = m.Id }));
                options.Add(new AssignableOptionDto { Name = "Pulling: Sheet", Type = "Pulling", BuildingCategory = (byte)BuildingCategory.Sheet });
            }
            else
            {
                options.AddRange(machines.Where(m => m.Category == MachineCategory.Slitter).Select(m => new AssignableOptionDto { Name = m.Name, Type = "Machine", MachineId = m.Id }));
                options.Add(new AssignableOptionDto { Name = "Pulling: Coil", Type = "Pulling", BuildingCategory = (byte)BuildingCategory.Coil });
            }
            return options;
        }

        public async Task AssignToMachineAsync(int itemId, int machineId, string userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;

            var machine = await db.Machines.FindAsync(machineId);
            if (machine == null) return;

            if (item.Length.HasValue && machine.Category != MachineCategory.CTL)
            {
                throw new InvalidOperationException("Sheet length present → assign to a CTL machine or Pulling: Sheet.");
            }
            if (!item.Length.HasValue && machine.Category != MachineCategory.Slitter)
            {
                throw new InvalidOperationException("No sheet length → assign to a Slitter machine or Pulling: Coil.");
            }

            item.MachineId = machineId;
            item.BuildingCategory = BuildingCategory.None;
            item.AssignedBy = userId;
            item.AssignedAt = DateTime.UtcNow;

            await _taskAuditEventService.CreateAuditEventAsync(
                item.Id, TaskType.Pulling, AuditEventType.Start, userId, $"Assigned to machine {machine.Name}");

            await db.SaveChangesAsync();
        }

        public async Task SendToPullingAsync(int itemId, BuildingCategory category, string userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;

            if (item.Length.HasValue && category != BuildingCategory.Sheet)
            {
                throw new InvalidOperationException("Sheet length present → assign to a CTL machine or Pulling: Sheet.");
            }
            if (!item.Length.HasValue && category != BuildingCategory.Coil)
            {
                throw new InvalidOperationException("No sheet length → assign to a Slitter machine or Pulling: Coil.");
            }

            item.MachineId = null;
            item.BuildingCategory = category;
            item.AssignedBy = userId;
            item.AssignedAt = DateTime.UtcNow;

            await _taskAuditEventService.CreateAuditEventAsync(
                item.Id, TaskType.Pulling, AuditEventType.Start, userId, $"Sent to pulling: {category}");

            await db.SaveChangesAsync();
        }

        public async Task ClearAssignmentAsync(int itemId, string userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var item = await db.PickingListItems.FindAsync(itemId);
            if (item == null) return;

            item.MachineId = null;
            item.BuildingCategory = BuildingCategory.None;
            item.AssignedBy = null;
            item.AssignedAt = null;

            await _taskAuditEventService.CreateAuditEventAsync(
                item.Id, TaskType.Pulling, AuditEventType.Start, userId, "Assignment cleared");

            await db.SaveChangesAsync();
        }
    }
}

using CMetalsWS.Data;
using CMetalsWS.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class TaskAuditEventService : ITaskAuditEventService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IHubContext<ScheduleHub> _hubContext;

        public TaskAuditEventService(IDbContextFactory<ApplicationDbContext> contextFactory, IHubContext<ScheduleHub> hubContext)
        {
            _contextFactory = contextFactory;
            _hubContext = hubContext;
        }

        public async Task CreateAuditEventAsync(int pickingListItemId, TaskType taskType, AuditEventType eventType, string userId, string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var auditEvent = new TaskAuditEvent
            {
                PickingListItemId = pickingListItemId,
                TaskType = taskType,
                EventType = eventType,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Notes = notes
            };

            context.TaskAuditEvents.Add(auditEvent);
            await context.SaveChangesAsync();
            if (taskType == TaskType.Picking || taskType == TaskType.Packing)
            {
                await _hubContext.Clients.All.SendAsync("PullingStatusUpdated");
            }
        }

        public async Task<AuditEventType?> GetLastEventTypeForTaskAsync(int pickingListItemId, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var lastEvent = await context.TaskAuditEvents
                .Where(e => e.PickingListItemId == pickingListItemId && e.TaskType == taskType)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            return lastEvent?.EventType;
        }

        public async Task<Dictionary<int, AuditEventType>> GetLastEventTypesForTasksAsync(List<int> pickingListItemIds, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!pickingListItemIds.Any())
            {
                return new Dictionary<int, AuditEventType>();
            }

            var lastEvents = await context.TaskAuditEvents
                .Where(e => e.TaskType == taskType && e.PickingListItemId.HasValue && pickingListItemIds.Contains(e.PickingListItemId.Value))
                .GroupBy(e => e.PickingListItemId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToDictionaryAsync(k => k.PickingListItemId.Value, v => v.EventType);

            return lastEvents;
        }

        public async Task<Dictionary<int, TaskAuditEvent>> GetLastEventsForTasksAsync(List<int> pickingListItemIds, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!pickingListItemIds.Any())
            {
                return new Dictionary<int, TaskAuditEvent>();
            }

            var lastEvents = await context.TaskAuditEvents
                .Where(e => e.TaskType == taskType && e.PickingListItemId.HasValue && pickingListItemIds.Contains(e.PickingListItemId.Value))
                .GroupBy(e => e.PickingListItemId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToDictionaryAsync(k => k.PickingListItemId.Value, v => v);

            return lastEvents;
        }

        public async Task<List<TaskAuditEvent>> GetEventsForTaskAsync(int taskId, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TaskAuditEvents
                .Include(e => e.User)
                .Where(e => e.PickingListItemId == taskId && e.TaskType == taskType)
                .OrderBy(e => e.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
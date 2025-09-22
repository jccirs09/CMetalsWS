using CMetalsWS.Data;
using CMetalsWS.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class TaskAuditEventService : ITaskAuditEventService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IHubContext<DashboardHub, IDashboardClient> _dashboardHubContext;

        public TaskAuditEventService(IDbContextFactory<ApplicationDbContext> contextFactory, IHubContext<DashboardHub, IDashboardClient> dashboardHubContext)
        {
            _contextFactory = contextFactory;
            _dashboardHubContext = dashboardHubContext;
        }

        public async Task CreateAuditEventAsync(int taskId, TaskType taskType, AuditEventType eventType, string userId, string? notes = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var auditEvent = new TaskAuditEvent
            {
                TaskId = taskId,
                TaskType = taskType,
                EventType = eventType,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Notes = notes
            };

            context.TaskAuditEvents.Add(auditEvent);
            await context.SaveChangesAsync();
            if (taskType == TaskType.Pulling)
            {
                await _dashboardHubContext.Clients.All.DashboardUpdated();
            }
        }

        public async Task<AuditEventType?> GetLastEventTypeForTaskAsync(int taskId, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var lastEvent = await context.TaskAuditEvents
                .Where(e => e.TaskId == taskId && e.TaskType == taskType)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefaultAsync();

            return lastEvent?.EventType;
        }

        public async Task<Dictionary<int, AuditEventType>> GetLastEventTypesForTasksAsync(List<int> taskIds, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!taskIds.Any())
            {
                return new Dictionary<int, AuditEventType>();
            }

            var lastEvents = await context.TaskAuditEvents
                .Where(e => e.TaskType == taskType && taskIds.Contains(e.TaskId))
                .GroupBy(e => e.TaskId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToDictionaryAsync(k => k.TaskId, v => v.EventType);

            return lastEvents;
        }

        public async Task<Dictionary<int, TaskAuditEvent>> GetLastEventsForTasksAsync(List<int> taskIds, TaskType taskType)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (!taskIds.Any())
            {
                return new Dictionary<int, TaskAuditEvent>();
            }

            var lastEvents = await context.TaskAuditEvents
                .Where(e => e.TaskType == taskType && taskIds.Contains(e.TaskId))
                .GroupBy(e => e.TaskId)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToDictionaryAsync(k => k.TaskId, v => v);

            return lastEvents;
        }
    }
}

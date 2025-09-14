using CMetalsWS.Configuration;
using CMetalsWS.Data;
using CMetalsWS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class DashboardFeed : IDashboardFeed
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IOptions<DashboardSettings> _settings;
        private readonly IMemoryCache _cache;
        private readonly ThroughputCalculator _throughputCalculator;
        private readonly NowPlayingProjector _nowPlayingProjector;
        private readonly ITaskAuditEventService _taskAuditEventService;
        private readonly UserManager<ApplicationUser> _userManager;

        private const string SummaryCacheKey = "DashboardSummary";

        public DashboardFeed(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IOptions<DashboardSettings> settings,
            IMemoryCache cache,
            ThroughputCalculator throughputCalculator,
            NowPlayingProjector nowPlayingProjector,
            ITaskAuditEventService taskAuditEventService,
            UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _settings = settings;
            _cache = cache;
            _throughputCalculator = throughputCalculator;
            _nowPlayingProjector = nowPlayingProjector;
            _taskAuditEventService = taskAuditEventService;
            _userManager = userManager;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            if (_cache.TryGetValue(SummaryCacheKey, out DashboardSummaryDto summary))
            {
                return summary;
            }

            using var context = await _contextFactory.CreateDbContextAsync();

            var inProgressWos = await context.WorkOrders
                .AsNoTracking()
                .Include(wo => wo.Machine)
                .Include(wo => wo.Items)
                .Where(wo => wo.Status == WorkOrderStatus.InProgress)
                .ToListAsync();

            decimal ctlLbsPerHour = inProgressWos
                .Where(wo => wo.Machine?.Category == MachineCategory.CTL)
                .Sum(wo => _throughputCalculator.CalculateLbsPerHour(wo.Machine!, wo));

            decimal slitterLbsPerHour = inProgressWos
                .Where(wo => wo.Machine?.Category == MachineCategory.Slitter)
                .Sum(wo => _throughputCalculator.CalculateLbsPerHour(wo.Machine!, wo));

            var setupSlitterCount = await context.WorkOrders
                .AsNoTracking()
                .CountAsync(wo => wo.Machine!.Category == MachineCategory.Slitter && wo.Status == WorkOrderStatus.Pending); // Assuming Pending == Setup

            summary = new DashboardSummaryDto
            {
                CtlLbsPerHour = ctlLbsPerHour,
                SlitterLbsPerHour = slitterLbsPerHour,
                IsCtlRunning = ctlLbsPerHour > 0,
                IsSlitterInSetup = setupSlitterCount > 0
            };

            if (_settings.Value.PullingEnabled)
            {
                // Placeholder for Pulling KPI calculation
                summary.PullingLbsPerHour = 0;
                summary.ActivePullingSessions = 0;
            }

            if (_settings.Value.LogisticsEnabled)
            {
                var today = DateTime.UtcNow.Date; // This should be adjusted for user's timezone
                summary.LoadsToday = await context.Loads.AsNoTracking().CountAsync(l => l.ShippingDate.HasValue && l.ShippingDate.Value.Date == today);
                summary.LoadsInTransit = await context.Loads.AsNoTracking().CountAsync(l => l.Status == LoadStatus.InTransit);
            }

            _cache.Set(SummaryCacheKey, summary, TimeSpan.FromSeconds(2));
            return summary;
        }

        public async Task<List<NowPlayingDto>> GetNowPlayingAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var activeWorkOrders = await context.WorkOrders
                .AsNoTracking()
                .Include(wo => wo.Machine)
                .Include(wo => wo.Items)
                .Include(wo => wo.CoilUsages)
                .Where(wo => wo.Status == WorkOrderStatus.InProgress || wo.Status == WorkOrderStatus.Paused || wo.Status == WorkOrderStatus.Pending)
                .OrderBy(wo => wo.Machine!.Name)
                .ToListAsync();

            return activeWorkOrders.Select(wo => _nowPlayingProjector.Project(wo)).ToList();
        }

        public async Task<List<ActivePullingSessionDto>> GetActivePullingAsync()
        {
            if (!_settings.Value.PullingEnabled)
            {
                return new List<ActivePullingSessionDto>();
            }

            // This logic is complex and based on assumptions about TaskAuditEventService
            // It will need to be implemented carefully.
            // For now, return an empty list.
            await Task.CompletedTask;
            return new List<ActivePullingSessionDto>();
        }
    }
}

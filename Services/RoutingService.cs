using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class RoutingService
    {
        private readonly ApplicationDbContext _db;
        private readonly LoadService _loads;

        public RoutingService(ApplicationDbContext db, LoadService loads)
        {
            _db = db;
            _loads = loads;
        }

        // Build routes by grouping loads with schedule on date by Customer.LocationCode
        public async Task<List<TruckRoute>> BuildRoutesForDateAsync(int branchId, DateTime routeDate)
        {
            var date = routeDate.Date;

            var loads = await _loads.GetLoadsAsync(branchId, date);

            var eligible = loads
                .Where(l => l.ScheduledStart.HasValue && l.ScheduledStart.Value.Date == date)
                .Where(l => l.Status == LoadStatus.Scheduled || l.Status == LoadStatus.Pending)
                .ToList();

            var groups = eligible
                .GroupBy(l => LoadService.GetLoadRegionCode(l))
                .ToList();

            // Delete existing planned routes for that day to rebuild cleanly
            var existing = await _db.TruckRoutes
                .Include(r => r.Stops)
                .Where(r => r.BranchId == branchId && r.RouteDate == date && r.Status == RouteStatus.Planned)
                .ToListAsync();

            if (existing.Count > 0)
            {
                _db.TruckRoutes.RemoveRange(existing);
                await _db.SaveChangesAsync();
            }

            var created = new List<TruckRoute>();

            foreach (var g in groups)
            {
                var route = new TruckRoute
                {
                    BranchId = branchId,
                    RouteDate = date,
                    RegionCode = string.IsNullOrWhiteSpace(g.Key) ? "UNSET" : g.Key,
                    Status = RouteStatus.Planned
                };

                var orderedLoads = g
                    .OrderBy(l => l.ReadyDate ?? l.ScheduledEnd ?? l.ScheduledStart)
                    .ThenBy(l => l.LoadNumber)
                    .ToList();

                int order = 1;
                foreach (var l in orderedLoads)
                {
                    route.Stops.Add(new TruckRouteStop
                    {
                        LoadId = l.Id,
                        StopOrder = order++,
                        PlannedStart = l.ScheduledStart,
                        PlannedEnd = l.ScheduledEnd
                    });
                }

                _db.TruckRoutes.Add(route);
                created.Add(route);
            }

            await _db.SaveChangesAsync();
            return created;
        }

        public async Task<List<TruckRoute>> GetRoutesAsync(int? branchId, DateTime routeDate)
        {
            var date = routeDate.Date;

            var q = _db.TruckRoutes
                .Include(r => r.Truck)
                .Include(r => r.Stops)
                    .ThenInclude(s => s.Load)
                .Where(r => r.RouteDate == date);

            if (branchId.HasValue)
                q = q.Where(r => r.BranchId == branchId.Value);

            return await q
                .OrderBy(r => r.RegionCode)
                .ThenBy(r => r.TruckId)
                .ToListAsync();
        }

        public async Task UpdateRouteAsync(TruckRoute route)
        {
            var existing = await _db.TruckRoutes
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.Id == route.Id);
            if (existing == null) return;

            existing.TruckId = route.TruckId;
            existing.Status = route.Status;
            existing.ModifiedUtc = DateTime.UtcNow;

            // Overwrite stops if provided
            if (route.Stops?.Count > 0)
            {
                existing.Stops.Clear();
                foreach (var s in route.Stops.OrderBy(s => s.StopOrder))
                    existing.Stops.Add(new TruckRouteStop
                    {
                        LoadId = s.LoadId,
                        StopOrder = s.StopOrder,
                        PlannedStart = s.PlannedStart,
                        PlannedEnd = s.PlannedEnd,
                        Notes = s.Notes
                    });
            }

            await _db.SaveChangesAsync();
        }
    }
}

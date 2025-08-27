using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;


public class DashboardService
{
    private readonly ApplicationDbContext _db;
    public DashboardService(ApplicationDbContext db) => _db = db;

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

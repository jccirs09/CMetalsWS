using CMetalsWS.Data;

public class LoadService
{
    private readonly ApplicationDbContext _db;
    public LoadService(ApplicationDbContext db) => _db = db;

    public async Task<List<Load>> GetLoadsAsync(int? branchId)
    {
        var query = _db.Loads.Include(l => l.Truck).Include(l => l.Items);
        if (branchId.HasValue) query = query.Where(l => l.BranchId == branchId.Value);
        return await query.ToListAsync();
    }

    public async Task CreateAsync(Load load)
    {
        load.LoadNumber = await GenerateLoadNumber(load.BranchId);
        _db.Loads.Add(load);
        await _db.SaveChangesAsync();
    }

    private async Task<string> GenerateLoadNumber(int branchId)
    {
        var branchCode = (await _db.Branches.FindAsync(branchId))?.Code ?? "00";
        var next = await _db.Loads.CountAsync(l => l.BranchId == branchId) + 1;
        return $"L{branchCode}{next:000000}";
    }
}

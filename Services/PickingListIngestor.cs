using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMetalsWS.Services
{
    public class PickingListIngestor : IPickingListIngestor
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IPdfParsingService _pdfParsingService;
        private readonly ILogger<PickingListIngestor> _logger;

        public PickingListIngestor(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IPdfParsingService pdfParsingService,
            ILogger<PickingListIngestor> logger)
        {
            _dbContextFactory = dbContextFactory;
            _pdfParsingService = pdfParsingService;
            _logger = logger;
        }

        public async Task<int> UploadAsync(Stream pdf, string fileName, int branchId, string uploadedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var parsedList = await _pdfParsingService.ParseAsync(pdf, fileName);

            var existingList = await db.PickingLists
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BranchId == branchId && p.RawTextHash == parsedList.RawTextHash);

            if (existingList != null)
            {
                _logger.LogInformation("Duplicate picking list detected. BranchId: {BranchId}, Hash: {Hash}", branchId, parsedList.RawTextHash);
                return -existingList.Id;
            }

            parsedList.BranchId = branchId;

            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                db.PickingLists.Add(parsedList);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully ingested new picking list {SalesOrderNumber}", parsedList.SalesOrderNumber);
                return parsedList.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to ingest picking list {SalesOrderNumber}", parsedList.SalesOrderNumber);
                throw;
            }
        }
    }
}

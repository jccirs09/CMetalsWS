using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Models;
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

        public async Task<IngestResult> UploadAsync(Stream pdf, string fileName, int branchId, string uploadedBy)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var parsedList = await _pdfParsingService.ParseAsync(pdf, fileName);

            var existingList = await db.PickingLists
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BranchId == branchId && p.RawTextHash == parsedList.RawTextHash);

            if (existingList != null)
            {
                _logger.LogInformation("Duplicate picking list detected. BranchId: {BranchId}, Hash: {Hash}", branchId, parsedList.RawTextHash);
                return new IngestResult { PickingListId = existingList.Id, IsDuplicate = true };
            }

            parsedList.BranchId = branchId;
            parsedList.UploadedBy = uploadedBy;
            parsedList.UploadedAt = DateTime.UtcNow;

            try
            {
                db.PickingLists.Add(parsedList);
                await db.SaveChangesAsync();
                _logger.LogInformation("Successfully ingested new picking list {SalesOrderNumber}", parsedList.SalesOrderNumber);
                return new IngestResult { PickingListId = parsedList.Id, IsDuplicate = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest picking list {SalesOrderNumber}", parsedList.SalesOrderNumber);
                throw;
            }
        }
    }
}

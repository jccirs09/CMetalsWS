using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class PickingListImportService : IPickingListImportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<PickingListImportService> _logger;

        public PickingListImportService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<PickingListImportService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<PickingListImport> CreateImportAsync(int branchId, string sourcePdfPath, string imagesPath, string modelUsed)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var newImport = new PickingListImport
            {
                BranchId = branchId,
                SourcePdfPath = sourcePdfPath,
                ImagesPath = imagesPath,
                ModelUsed = modelUsed,
                StartedUtc = DateTime.UtcNow,
                Status = ImportStatus.Processing
            };

            dbContext.PickingListImports.Add(newImport);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Created new PickingListImport with Id {ImportId}", newImport.Id);
            return newImport;
        }

        public async Task UpdateImportSuccessAsync(int importId, int pickingListId, string rawJson)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var import = await dbContext.PickingListImports.FindAsync(importId);
            if (import != null)
            {
                import.PickingListId = pickingListId;
                import.CompletedUtc = DateTime.UtcNow;
                import.Status = ImportStatus.Success;
                import.RawJson = rawJson;
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully updated PickingListImport {ImportId} to Success.", importId);
            }
            else
            {
                _logger.LogWarning("Could not find PickingListImport with Id {ImportId} to update.", importId);
            }
        }

        public async Task UpdateImportFailedAsync(int importId, string error, string? rawJson = null)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var import = await dbContext.PickingListImports.FindAsync(importId);
            if (import != null)
            {
                import.CompletedUtc = DateTime.UtcNow;
                import.Status = ImportStatus.Failed;
                import.Error = error;
                import.RawJson = rawJson;
                await dbContext.SaveChangesAsync();
                _logger.LogError("Updated PickingListImport {ImportId} to Failed. Error: {Error}", importId, error);
            }
            else
            {
                _logger.LogWarning("Could not find PickingListImport with Id {ImportId} to update.", importId);
            }
        }

        public async Task<PickingListImport?> GetLatestImportByPickingListIdAsync(int pickingListId)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.PickingListImports
                .Where(i => i.PickingListId == pickingListId)
                .OrderByDescending(i => i.StartedUtc)
                .FirstOrDefaultAsync();
        }
    }
}

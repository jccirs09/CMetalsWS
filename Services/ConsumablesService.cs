using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Services
{
    public class ConsumablesService : IConsumablesService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ConsumablesService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // --- Consumable Item Management ---

        public async Task<List<ConsumableItem>> GetItemsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.ConsumableItems.OrderBy(i => i.Name).ToListAsync();
        }

        public async Task<ConsumableItem?> GetItemAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.ConsumableItems.FindAsync(id);
        }

        public async Task<ConsumableItem> CreateItemAsync(ConsumableItem item)
        {
            using var context = _dbContextFactory.CreateDbContext();
            // Ensure unique ItemCode
            if (await context.ConsumableItems.AnyAsync(i => i.ItemCode == item.ItemCode))
            {
                throw new InvalidOperationException($"Item with code '{item.ItemCode}' already exists.");
            }

            context.ConsumableItems.Add(item);
            await context.SaveChangesAsync();
            return item;
        }

        public async Task<ConsumableItem> UpdateItemAsync(ConsumableItem item)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var existing = await context.ConsumableItems.FindAsync(item.Id);
            if (existing == null) throw new InvalidOperationException("Item not found");

            // Check if ItemCode is changing and if it conflicts
            if (existing.ItemCode != item.ItemCode && await context.ConsumableItems.AnyAsync(i => i.ItemCode == item.ItemCode))
            {
                throw new InvalidOperationException($"Item with code '{item.ItemCode}' already exists.");
            }

            existing.ItemCode = item.ItemCode;
            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.UnitOfMeasure = item.UnitOfMeasure;
            existing.MinQuantity = item.MinQuantity;
            existing.MaxQuantity = item.MaxQuantity;
            existing.IsActive = item.IsActive;

            // Note: CurrentQuantity is NOT updated here, only via transactions

            await context.SaveChangesAsync();
            return existing;
        }

        public async Task ToggleItemActiveAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var item = await context.ConsumableItems.FindAsync(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                await context.SaveChangesAsync();
            }
        }

        // --- Inventory Transactions ---

        public async Task RecordTransactionAsync(ConsumableTransaction transaction)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var item = await context.ConsumableItems.FindAsync(transaction.ConsumableItemId);
            if (item == null) throw new InvalidOperationException("Item not found");

            // Calculate new quantity based on transaction type
            int change = 0;
            switch (transaction.TransactionType)
            {
                case ConsumableTransactionType.Usage:
                    change = -Math.Abs(transaction.Quantity);
                    break;
                case ConsumableTransactionType.Receipt:
                    change = Math.Abs(transaction.Quantity);
                    break;
                case ConsumableTransactionType.Adjustment:
                    // For adjustment, we treat the quantity as the change amount (can be pos or neg)
                    // Wait, usually adjustment is "set to X" or "add/sub X".
                    // The requirement says: "Usage (deduct)", "Receipt (add)", "Adjustment (manual corrections)".
                    // It also says: "Quantity" in transaction.
                    // If I say Adjustment +5, it adds 5. If I say Adjustment -5, it subtracts 5.
                    // So I will just use the quantity as is for Adjustment, assuming the UI handles the sign.
                    // BUT, to be safe and clear, for Usage/Receipt I force the sign. For Adjustment, I trust the input.
                    // Actually, let's assume Adjustment Quantity in the transaction object represents the delta.
                    change = transaction.Quantity;
                    break;
            }

            // Check for negative stock
            if (item.CurrentQuantity + change < 0)
            {
                throw new InvalidOperationException("Transaction would result in negative stock.");
            }

            item.CurrentQuantity += change;
            transaction.Timestamp = DateTime.UtcNow; // Ensure server time

            context.ConsumableTransactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<List<ConsumableTransaction>> GetItemHistoryAsync(int itemId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.ConsumableTransactions
                .Where(t => t.ConsumableItemId == itemId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        // --- Procurement Requests ---

        public async Task<List<ConsumableItem>> GetReorderListAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.ConsumableItems
                .Where(i => i.IsActive && i.CurrentQuantity <= i.MinQuantity)
                .ToListAsync();
        }

        public async Task<List<ProcurementRequest>> GetProcurementRequestsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.ProcurementRequests
                .Include(r => r.ConsumableItem)
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();
        }

        public async Task<ProcurementRequest> CreateProcurementRequestAsync(ProcurementRequest request)
        {
            using var context = _dbContextFactory.CreateDbContext();

            // Auto-generate RequestNumber if empty
            if (string.IsNullOrEmpty(request.RequestNumber))
            {
                request.RequestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            }

            request.Status = ProcurementRequestStatus.Draft;
            request.RequestedDate = DateTime.UtcNow;

            context.ProcurementRequests.Add(request);
            await context.SaveChangesAsync();
            return request;
        }

        public async Task UpdateProcurementRequestStatusAsync(int requestId, ProcurementRequestStatus status, string performedBy)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var request = await context.ProcurementRequests.FindAsync(requestId);
            if (request == null) throw new InvalidOperationException("Request not found");

            // If changing to Received, increase inventory
            if (status == ProcurementRequestStatus.Received && request.Status != ProcurementRequestStatus.Received)
            {
                var item = await context.ConsumableItems.FindAsync(request.ConsumableItemId);
                if (item != null)
                {
                    // Create a receipt transaction
                    var transaction = new ConsumableTransaction
                    {
                        ConsumableItemId = item.Id,
                        TransactionType = ConsumableTransactionType.Receipt,
                        Quantity = request.QuantityRequested,
                        Timestamp = DateTime.UtcNow,
                        PerformedBy = performedBy,
                        Reference = request.RequestNumber,
                        Notes = "Auto-generated from Procurement Request"
                    };

                    item.CurrentQuantity += request.QuantityRequested;
                    context.ConsumableTransactions.Add(transaction);

                    request.ActualDeliveryDate = DateTime.UtcNow;
                }
            }

            request.Status = status;
            await context.SaveChangesAsync();
        }
    }
}

using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class FulfillmentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public FulfillmentService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task RecordFulfillmentAsync(int pickingListItemId, decimal fulfilledQuantity, FulfillmentType fulfillmentType, string recordedById, int? loadId = null, string? notes = null)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var item = await db.PickingListItems.FindAsync(pickingListItemId);
            if (item == null)
            {
                throw new ArgumentException("Picking list item not found.", nameof(pickingListItemId));
            }

            var alreadyFulfilled = await db.OrderItemFulfillments
                .Where(f => f.PickingListItemId == pickingListItemId)
                .SumAsync(f => f.FulfilledQuantity);

            if (fulfilledQuantity <= 0)
            {
                throw new InvalidOperationException("Fulfilled quantity must be positive.");
            }

            if (fulfilledQuantity > (item.Quantity - alreadyFulfilled))
            {
                throw new InvalidOperationException("Fulfilled quantity cannot exceed the remaining quantity.");
            }

            var fulfillmentRecord = new OrderItemFulfillment
            {
                PickingListItemId = pickingListItemId,
                FulfilledQuantity = fulfilledQuantity,
                FulfillmentType = fulfillmentType,
                RecordedById = recordedById,
                LoadId = loadId,
                FulfillmentDate = DateTime.UtcNow,
                Notes = notes
            };

            db.OrderItemFulfillments.Add(fulfillmentRecord);
            await db.SaveChangesAsync();
        }

        public async Task RecordFullOrderPickupAsync(int pickingListId, string recordedById)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var pickingListItems = await db.PickingListItems
                .Where(i => i.PickingListId == pickingListId)
                .ToListAsync();

            var fulfilledQuantities = await db.OrderItemFulfillments
                .Where(f => pickingListItems.Select(i => i.Id).Contains(f.PickingListItemId))
                .GroupBy(f => f.PickingListItemId)
                .Select(g => new { PickingListItemId = g.Key, FulfilledQuantity = g.Sum(f => f.FulfilledQuantity) })
                .ToDictionaryAsync(x => x.PickingListItemId, x => x.FulfilledQuantity);

            var newFulfillmentRecords = new List<OrderItemFulfillment>();

            foreach (var item in pickingListItems)
            {
                var alreadyFulfilled = fulfilledQuantities.GetValueOrDefault(item.Id, 0);
                var remainingQuantity = item.Quantity - alreadyFulfilled;

                if (remainingQuantity > 0)
                {
                    newFulfillmentRecords.Add(new OrderItemFulfillment
                    {
                        PickingListItemId = item.Id,
                        FulfilledQuantity = remainingQuantity,
                        FulfillmentType = FulfillmentType.CustomerPickup,
                        RecordedById = recordedById,
                        FulfillmentDate = DateTime.UtcNow,
                        Notes = "Full order pickup recorded."
                    });
                }
            }

            if (newFulfillmentRecords.Any())
            {
                db.OrderItemFulfillments.AddRange(newFulfillmentRecords);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<OrderItemFulfillment>> GetFulfillmentHistoryForItemAsync(int pickingListItemId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.OrderItemFulfillments
                .AsNoTracking()
                .Where(f => f.PickingListItemId == pickingListItemId)
                .Include(f => f.RecordedBy)
                .Include(f => f.Load)
                .OrderByDescending(f => f.FulfillmentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetFulfilledQuantityAsync(int pickingListItemId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.OrderItemFulfillments
                .Where(f => f.PickingListItemId == pickingListItemId)
                .SumAsync(f => f.FulfilledQuantity);
        }

        public async Task<ValidationResult> ValidateOrderForPickup(int pickingListId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var items = await db.PickingListItems
                .AsNoTracking()
                .Where(i => i.PickingListId == pickingListId)
                .ToListAsync();

            if (!items.Any())
            {
                return new ValidationResult(false, "Order not found or contains no items.");
            }

            foreach (var item in items)
            {
                if (item.Status != PickingLineStatus.Packed && item.Status != PickingLineStatus.Completed)
                {
                    return new ValidationResult(false, $"Order cannot be picked up. Item '{item.ItemDescription}' has status '{item.Status}', but must be 'Packed' or 'Completed'.");
                }
            }

            return new ValidationResult(true);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
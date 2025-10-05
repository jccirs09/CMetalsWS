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

        /// <summary>
        /// Records a fulfillment event for a picking list item.
        /// </summary>
        /// <param name="pickingListItemId">The ID of the item being fulfilled.</param>
        /// <param name="fulfilledQuantity">The quantity being fulfilled.</param>
        /// <param name="fulfillmentType">The type of fulfillment (Delivery or CustomerPickup).</param>
        /// <param name="recordedById">The ID of the user recording the event.</param>
        /// <param name="loadId">The optional ID of the load if fulfillment is via delivery.</param>
        /// <param name="notes">Optional notes for the fulfillment record.</param>
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

        /// <summary>
        /// Gets the fulfillment history for a specific picking list item.
        /// </summary>
        /// <param name="pickingListItemId">The ID of the picking list item.</param>
        /// <returns>A list of fulfillment records for the item.</returns>
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

        /// <summary>
        /// Gets the total fulfilled quantity for a given picking list item.
        /// </summary>
        /// <param name="pickingListItemId">The ID of the picking list item.</param>
        /// <returns>The sum of all fulfilled quantities for the item.</returns>
        public async Task<decimal> GetFulfilledQuantityAsync(int pickingListItemId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.OrderItemFulfillments
                .Where(f => f.PickingListItemId == pickingListItemId)
                .SumAsync(f => f.FulfilledQuantity);
        }
    }
}
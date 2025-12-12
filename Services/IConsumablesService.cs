using CMetalsWS.Data;

namespace CMetalsWS.Services
{
    public interface IConsumablesService
    {
        // Consumable Item Management
        Task<List<ConsumableItem>> GetItemsAsync();
        Task<ConsumableItem?> GetItemAsync(int id);
        Task<ConsumableItem> CreateItemAsync(ConsumableItem item);
        Task<ConsumableItem> UpdateItemAsync(ConsumableItem item);
        Task ToggleItemActiveAsync(int id);

        // Inventory Transactions
        Task RecordTransactionAsync(ConsumableTransaction transaction);
        Task<List<ConsumableTransaction>> GetItemHistoryAsync(int itemId);

        // Procurement Requests
        Task<List<ConsumableItem>> GetReorderListAsync();
        Task<List<ProcurementRequest>> GetProcurementRequestsAsync();
        Task<ProcurementRequest> CreateProcurementRequestAsync(ProcurementRequest request);
        Task UpdateProcurementRequestStatusAsync(int requestId, ProcurementRequestStatus status, string performedBy);
    }
}

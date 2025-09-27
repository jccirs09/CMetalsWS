using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Services
{
    /// <summary>
    /// Manages the state for the multi-step work order creation process.
    /// Registered as a scoped service to maintain state throughout the user's session on the page.
    /// </summary>
    public class WorkOrderCreationStateService
    {
        public WorkOrder Model { get; private set; } = new();
        public List<Machine> Machines { get; set; } = new();
        public Machine? SelectedMachine { get; set; }
        public InventoryItem? ParentItem { get; set; }
        public List<PickingListItem> AvailablePickingListItems { get; set; } = new();
        public WorkOrderItem NewStockItem { get; set; } = new() { IsStockItem = true };
        public WorkOrderItem? EditingStockItem { get; set; }
        public HashSet<PickingListItem> SelectedSourceItems { get; set; } = new();
        public int? SelectedSourceIdForNewLine { get; set; }
        public WorkOrderItem NewProductionLine { get; set; } = new();

        /// <summary>
        /// Resets the state to its initial values for creating a new work order.
        /// </summary>
        public void Reset()
        {
            Model = new WorkOrder();
            SelectedMachine = null;
            ParentItem = null;
            AvailablePickingListItems = new List<PickingListItem>();
            NewStockItem = new WorkOrderItem { IsStockItem = true };
            EditingStockItem = null;
            SelectedSourceItems = new HashSet<PickingListItem>();
            SelectedSourceIdForNewLine = null;
            NewProductionLine = new WorkOrderItem();
        }
    }
}
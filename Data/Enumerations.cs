namespace CMetalsWS.Data
{
    public enum WorkOrderStatus
    {
        Draft,
        Scheduled,
        InProduction,
        Completed,
        Cancelled
    }

    public enum PickingListStatus
    {
        Pending,
        Scheduled,
        Picked,
        Shipped,
        Cancelled
    }
}

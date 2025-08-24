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
    public enum PickingLineStatus
    {
        Pending = 0,
        AssignedProduction = 1,
        AssignedPulling = 2,
        InProgress = 3,
        Completed = 4,
        Canceled = 5,
        WorkOrder = 6

    }
}

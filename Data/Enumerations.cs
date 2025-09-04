namespace CMetalsWS.Data
{

    public enum PickingListStatus
    {
        Pending,
        Awaiting,
        OnHold,
        InProgress,
        Scheduled,
        Picked,
        Completed,
        ReadyToShip,
        Shipped,
        Cancelled
    }
    public enum PickingLineStatus
    {
        Pending,
        AssignedProduction,
        AssignedPulling,
        InProgress,
        Completed,
        Canceled,
        WorkOrder,
        Awaiting
    }

    public enum TaskType
    {
        WorkOrder,
        Pulling
    }

    public enum AuditEventType
    {
        Start,
        Pause,
        Resume,
        Complete
    }

    // --- Enums for Load Planning ---
    public enum LoadType { StandardDelivery, PoolTruck, BranchTransfer }
    public enum ShippingGroup { Standard, Pool, BranchTransfer }
    public enum LoadStatus { Pending, Loaded, Scheduled, InTransit, Delivered, Canceled }
}




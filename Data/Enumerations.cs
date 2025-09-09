namespace CMetalsWS.Data
{
    public enum DestinationRegionCategory
    {
        LOCAL = 0,
        ISLAND = 1,
        OKANAGAN = 2,
        OUT_OF_TOWN = 3
    }

    public enum DockType { NONE = 0, DOCK = 1, GRADE = 2, RAMP = 3 }
    public enum PreferredTruckType { PICKUP = 0, VAN_5TON = 1, FLATBED = 2, TEN_TON = 3, TRACTOR_53 = 4 }
    public enum PriorityLevel { LOW = 0, NORMAL = 1, HIGH = 2 }

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
        Cancelled,
        Ready
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

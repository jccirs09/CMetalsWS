namespace CMetalsWS.Data
{
    // --- Enums for Customer & Routing ---
    public enum DestinationRegionCategory { LOCAL, ISLAND, OKANAGAN, OUT_OF_TOWN }
    public enum DockType { NONE, DOCK, GRADE, RAMP }
    public enum PreferredTruckType { PICKUP, VAN_5TON, FLATBED, TEN_TON, TRACTOR_53 }
    public enum Priority { LOW, NORMAL, HIGH }


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
        Awaiting,
        Picked,
        Packed,
        Paused
    }

    public enum TaskType
    {
        WorkOrder,
        Picking,
        Packing
    }

    public enum AuditEventType
    {
        Create,
        Update,
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




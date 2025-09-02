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
}

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
        Pending = 0,
        AssignedProduction = 1,
        AssignedPulling = 2,
        InProgress = 3,
        Completed = 4,
        Canceled = 5,
        WorkOrder = 6,
        Awaiting = 7

    }
}

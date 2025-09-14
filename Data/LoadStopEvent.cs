using System;

namespace CMetalsWS.Data
{
    public class LoadStopEvent
    {
        public int Id { get; set; }

        public int LoadId { get; set; }
        public virtual Load? Load { get; set; }

        public int StopSequence { get; set; }

        public DateTime? ArriveUtc { get; set; }
        public DateTime? DepartUtc { get; set; }
    }
}

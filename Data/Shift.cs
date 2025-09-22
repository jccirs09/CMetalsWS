using System;
using System.Collections.Generic;

namespace CMetalsWS.Data
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = default!;
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}

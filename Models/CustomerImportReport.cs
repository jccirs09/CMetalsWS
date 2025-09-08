using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class CustomerImportReport
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

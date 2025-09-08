using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class CustomerEnrichmentResult
    {
        public Customer? EnrichedCustomer { get; set; }
        public List<Customer> Candidates { get; set; } = new List<Customer>();
        public bool RequiresManualSelection { get; set; }
    }
}

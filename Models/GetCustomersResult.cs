using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Models
{
    public class GetCustomersResult
    {
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public int TotalCount { get; set; }
    }
}

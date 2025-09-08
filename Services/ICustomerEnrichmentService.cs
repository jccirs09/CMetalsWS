using CMetalsWS.Data;
using CMetalsWS.Models;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface ICustomerEnrichmentService
    {
        Task<CustomerEnrichmentResult> EnrichAndCategorizeCustomerAsync(Customer customer, string? address);
    }
}

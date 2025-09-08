using CMetalsWS.Data;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface ICustomerEnrichmentService
    {
        Task<Customer> EnrichAndCategorizeCustomerAsync(Customer customer, string address);
    }
}

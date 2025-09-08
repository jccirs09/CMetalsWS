using CMetalsWS.Data;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IGooglePlacesService
    {
        Task<Customer> EnrichCustomerAddressAsync(Customer customer, string address);
    }
}

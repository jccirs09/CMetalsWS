using CMetalsWS.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IGooglePlacesService
    {
        Task<List<Customer>> SearchPlacesAsync(string query);
        Task<Customer?> GetPlaceDetailsAsync(string placeId);
    }
}

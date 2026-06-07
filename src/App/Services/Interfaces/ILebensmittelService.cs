using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    public interface ILebensmittelService
    {
        Task<LebensmittelKatalog> CreateLebensmittelAsync(LebensmittelKatalog lebensmittel);
        Task<LebensmittelKatalog> GetLebensmittelByIdAsync(int id);
        Task<IEnumerable<LebensmittelKatalog>> GetAllLebensmittelAsync();
        Task<IEnumerable<LebensmittelKatalog>> SearchLebensmittelAsync(string suchbegriff);
        Task<LebensmittelKatalog> UpdateLebensmittelAsync(LebensmittelKatalog lebensmittel);
        Task<bool> DeleteLebensmittelAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    public class LebensmittelService : ILebensmittelService
    {
        private readonly IRepository<LebensmittelKatalog> _repository;

        public LebensmittelService(IRepository<LebensmittelKatalog> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<LebensmittelKatalog> CreateLebensmittelAsync(LebensmittelKatalog lebensmittel)
        {
            ValidateLebensmittel(lebensmittel);

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentException("Lebensmittel-Name darf nicht leer sein.");

            if (!IsValidEinheit(lebensmittel.Einheit))
                throw new ArgumentException($"Ungültige Einheit: {lebensmittel.Einheit}");

            lebensmittel.ErstelltAm = DateTime.UtcNow;
            return await _repository.AddAsync(lebensmittel);
        }

        public async Task<LebensmittelKatalog> GetLebensmittelByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<LebensmittelKatalog>> GetAllLebensmittelAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<LebensmittelKatalog>> SearchLebensmittelAsync(string suchbegriff)
        {
            if (string.IsNullOrWhiteSpace(suchbegriff))
                return await _repository.GetAllAsync();

            var alle = await _repository.GetAllAsync();
            return alle.Where(l => l.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<LebensmittelKatalog> UpdateLebensmittelAsync(LebensmittelKatalog lebensmittel)
        {
            ValidateLebensmittel(lebensmittel);

            if (lebensmittel.Id <= 0)
                throw new ArgumentException("Ungültige ID.");

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentException("Lebensmittel-Name darf nicht leer sein.");

            if (!IsValidEinheit(lebensmittel.Einheit))
                throw new ArgumentException($"Ungültige Einheit: {lebensmittel.Einheit}");

            return await _repository.UpdateAsync(lebensmittel);
        }

        public async Task<bool> DeleteLebensmittelAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.DeleteAsync(id);
        }

        private void ValidateLebensmittel(LebensmittelKatalog lebensmittel)
        {
            if (lebensmittel is null)
                throw new ArgumentNullException(nameof(lebensmittel));

            if (string.IsNullOrWhiteSpace(lebensmittel.Name))
                throw new ArgumentNullException(nameof(lebensmittel.Name));
        }

        private bool IsValidEinheit(string einheit)
        {
            var validEinheiten = new[] { "g", "ml", "Stück" };
            return validEinheiten.Contains(einheit, StringComparer.OrdinalIgnoreCase);
        }
    }
}

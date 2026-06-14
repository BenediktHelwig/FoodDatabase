using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    public class ProduktInstanzService : IProduktInstanzService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        public ProduktInstanzService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ProduktInstanz> CreateAsync(int lebensmittelKatalogId, decimal menge, DateTime verfallsdatum, string lagerort)
        {
            // Validierungen
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein.");

            if (menge < 0)
                throw new ArgumentException("Menge darf nicht negativ sein.");

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.");

            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var produktInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = lebensmittelKatalogId,
                Menge = menge,
                Verfallsdatum = verfallsdatum,
                Einkaufsdatum = DateTime.UtcNow,
                Lagerort = lagerort,
                ErstelltAm = DateTime.UtcNow
            };

            return await _repository.AddAsync(produktInstanz);
        }

        public async Task<ProduktInstanz> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<ProduktInstanz>> GetByLebensmittelAsync(int lebensmittelKatalogId)
        {
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein.");

            var alle = await _repository.GetAllAsync();
            return alle
                .Where(p => p.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();
        }

        public async Task<List<ProduktInstanz>> GetByLagerortAsync(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var alle = await _repository.GetAllAsync();
            return alle
                .Where(p => p.Lagerort == lagerort)
                .ToList();
        }

        public async Task<List<ProduktInstanz>> GetNachVerfallsdatumSortiertAsync()
        {
            var alle = await _repository.GetAllAsync();
            return alle
                .OrderBy(p => p.Verfallsdatum)
                .ToList();
        }

        public async Task<List<ProduktInstanz>> GetVerfallenenAsync(DateTime? standdatum = null)
        {
            var referenzdatum = standdatum ?? DateTime.Today;
            var alle = await _repository.GetAllAsync();
            return alle
                .Where(p => p.Verfallsdatum <= referenzdatum)
                .ToList();
        }

        public async Task UpdateAsync(int id, decimal menge, DateTime verfallsdatum, string lagerort)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            if (menge < 0)
                throw new ArgumentException("Menge darf nicht negativ sein.");

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.");

            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentNullException(nameof(lagerort), "Lagerort darf nicht null oder leer sein.");

            if (!LagerortKonstanten.IsValidLagerort(lagerort))
                throw new ArgumentException($"Ungültiger Lagerort: {lagerort}");

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz == null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            instanz.Menge = menge;
            instanz.Verfallsdatum = verfallsdatum;
            instanz.Lagerort = lagerort;

            await _repository.UpdateAsync(instanz);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            var success = await _repository.DeleteAsync(id);
            if (!success)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");
        }

        public async Task<int> GetTagesBisVerfallAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz == null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            var tage = (instanz.Verfallsdatum - DateTime.Today).Days;
            return tage;
        }
    }
}

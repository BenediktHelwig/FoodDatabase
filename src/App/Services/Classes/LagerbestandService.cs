using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    public class LagerbestandService : ILagerbestandService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        public LagerbestandService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository;
        }

        public async Task<decimal> GetGesamtmengeAsync(int lebensmittelKatalogId)
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                throw new NullReferenceException(nameof(allInstanzen));

            return allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .Sum(x => x.Menge);
        }

        public async Task<bool> CheckMindestbestandUnterschrittenAsync(int lebensmittelKatalogId)
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null || !allInstanzen.Any())
                return true;

            var instanzenFuerLebensmittel = allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();

            if (!instanzenFuerLebensmittel.Any())
                return true;

            var gesamtmenge = instanzenFuerLebensmittel.Sum(x => x.Menge);
            var mindestbestand = instanzenFuerLebensmittel.FirstOrDefault()?.MindestbestandMenge ?? 0;

            return gesamtmenge < mindestbestand;
        }

        public async Task<List<ProduktInstanz>> GetEinkaufslistenEintraegeAsync()
        {
            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                return new List<ProduktInstanz>();

            var grouped = allInstanzen
                .GroupBy(x => x.LebensmittelKatalogId)
                .Select(g => new
                {
                    LebensmittelId = g.Key,
                    Gesamtmenge = g.Sum(x => x.Menge),
                    Mindestbestand = g.FirstOrDefault()?.MindestbestandMenge ?? 0,
                    Instanzen = g.ToList()
                })
                .Where(x => x.Gesamtmenge < x.Mindestbestand)
                .SelectMany(x => x.Instanzen)
                .ToList();

            return grouped;
        }

        public async Task<List<ProduktInstanz>> GetBestaendePorLagerortAsync(string lagerort)
        {
            if (string.IsNullOrWhiteSpace(lagerort))
                throw new ArgumentException("Lagerort darf nicht null oder leer sein.", nameof(lagerort));

            var allInstanzen = await _repository.GetAllAsync();
            if (allInstanzen is null)
                return new List<ProduktInstanz>();

            return allInstanzen
                .Where(x => x.Lagerort == lagerort)
                .ToList();
        }

        public async Task<ProduktInstanz> AddToBestandAsync(int lebensmittelKatalogId, decimal menge, decimal mindestbestandMenge, DateTime verfallsdatum, string lagerort)
        {
            if (menge <= 0)
                throw new ArgumentException("Menge muss größer als 0 sein.", nameof(menge));

            if (verfallsdatum < DateTime.Today)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.", nameof(verfallsdatum));

            var newInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = lebensmittelKatalogId,
                Menge = menge,
                MindestbestandMenge = mindestbestandMenge,
                Verfallsdatum = verfallsdatum,
                Einkaufsdatum = DateTime.Today,
                Lagerort = lagerort,
                ErstelltAm = DateTime.UtcNow
            };

            return await _repository.AddAsync(newInstanz);
        }

        public async Task<ProduktInstanz> UpdateMengeAsync(int id, decimal newMenge)
        {
            if (newMenge <= 0)
                throw new ArgumentException("Menge muss größer als 0 sein.", nameof(newMenge));

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            instanz.Menge = newMenge;
            return await _repository.UpdateAsync(instanz);
        }

        public async Task RemoveFromBestandAsync(int id)
        {
            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            await _repository.DeleteAsync(id);
        }

        public async Task<bool> ValidateBestandAsync(int id, decimal requiredMenge)
        {
            var instanz = await _repository.GetByIdAsync(id);
            if (instanz is null)
                throw new KeyNotFoundException($"ProduktInstanz mit ID {id} nicht gefunden.");

            return instanz.Menge >= requiredMenge;
        }
    }
}

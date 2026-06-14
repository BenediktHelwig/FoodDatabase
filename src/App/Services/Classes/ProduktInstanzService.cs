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
        private readonly IRepository<LebensmittelKatalog> _lebensmittelRepository;
        private readonly IRepository<Lagerort> _lagerortRepository;

        public ProduktInstanzService(
            IRepository<ProduktInstanz> repository,
            IRepository<LebensmittelKatalog> lebensmittelRepository,
            IRepository<Lagerort> lagerortRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _lebensmittelRepository = lebensmittelRepository ?? throw new ArgumentNullException(nameof(lebensmittelRepository));
            _lagerortRepository = lagerortRepository ?? throw new ArgumentNullException(nameof(lagerortRepository));
        }

        public async Task<ProduktInstanz> CreateProduktInstanzAsync(ProduktInstanz instanz)
        {
            ValidateProduktInstanz(instanz);
            ValidateLebensmittelId(instanz.LebensmittelKatalogId);
            ValidateLagerortId(instanz.LagerortId);
            ValidateMengen(instanz.AktuelleM enge, instanz.MindestbestandMenge);
            ValidateDatum(instanz.Verfallsdatum, instanz.Einkaufsdatum);

            instanz.ErstelltAm = DateTime.UtcNow;
            return await _repository.AddAsync(instanz);
        }

        public async Task<ProduktInstanz> GetProduktInstanzByIdAsync(int id)
        {
            ValidateId(id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ProduktInstanz>> GetAllProduktInstanzenAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenByLebensmittelIdAsync(int lebensmittelId)
        {
            if (lebensmittelId <= 0)
                throw new ArgumentException("Lebensmittel-ID muss größer als 0 sein.");

            var alle = await _repository.GetAllAsync();
            return alle.Where(p => p.LebensmittelKatalogId == lebensmittelId).ToList();
        }

        public async Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenByLagerortIdAsync(int lagerortId)
        {
            if (lagerortId <= 0)
                throw new ArgumentException("Lagerort-ID muss größer als 0 sein.");

            var alle = await _repository.GetAllAsync();
            return alle.Where(p => p.LagerortId == lagerortId).ToList();
        }

        public async Task<IEnumerable<ProduktInstanz>> GetAbgelaufeneProduktInstanzenAsync()
        {
            var alle = await _repository.GetAllAsync();
            return alle.Where(p => IsAbgelaufen(p)).ToList();
        }

        public async Task<IEnumerable<ProduktInstanz>> GetProduktInstanzenBaldAbgelaufen_Async(int tagesBis)
        {
            if (tagesBis <= 0)
                throw new ArgumentException("Tage muss größer als 0 sein.");

            var alle = await _repository.GetAllAsync();
            var grenze = DateTime.UtcNow.AddDays(tagesBis);

            return alle.Where(p =>
                p.Verfallsdatum > DateTime.UtcNow &&
                p.Verfallsdatum <= grenze)
                .ToList();
        }

        public async Task<ProduktInstanz> UpdateProduktInstanzAsync(ProduktInstanz instanz)
        {
            ValidateProduktInstanz(instanz);
            ValidateId(instanz.Id);

            return await _repository.UpdateAsync(instanz);
        }

        public async Task<ProduktInstanz> ReduceProduktInstanzMengeAsync(int id, double mengeZuReduzieren)
        {
            ValidateId(id);
            if (mengeZuReduzieren <= 0)
                throw new ArgumentException("Menge zu reduzieren muss größer als 0 sein.");

            var instanz = await _repository.GetByIdAsync(id);
            if (instanz == null)
                throw new InvalidOperationException($"ProduktInstanz mit ID {id} nicht gefunden.");

            if (instanz.AktuelleM enge < mengeZuReduzieren)
                throw new ArgumentException($"Verfügbare Menge ({instanz.AktuelleM enge}) ist kleiner als zu reduzierende Menge ({mengeZuReduzieren}).");

            instanz.AktuelleM enge -= mengeZuReduzieren;
            return await _repository.UpdateAsync(instanz);
        }

        public async Task<bool> DeleteProduktInstanzAsync(int id)
        {
            ValidateId(id);
            return await _repository.DeleteAsync(id);
        }

        public bool IsUnterMindestbestand(ProduktInstanz instanz)
        {
            if (instanz == null)
                throw new ArgumentNullException(nameof(instanz));

            return instanz.AktuelleM enge < instanz.MindestbestandMenge;
        }

        public bool IsAbgelaufen(ProduktInstanz instanz)
        {
            if (instanz == null)
                throw new ArgumentNullException(nameof(instanz));

            return instanz.Verfallsdatum < DateTime.UtcNow;
        }

        public int CalculateTagesBisAblauf(ProduktInstanz instanz)
        {
            if (instanz == null)
                throw new ArgumentNullException(nameof(instanz));

            var tage = (instanz.Verfallsdatum - DateTime.UtcNow).Days;
            return tage;
        }

        // === Private Validation Methods ===

        private void ValidateProduktInstanz(ProduktInstanz instanz)
        {
            if (instanz == null)
                throw new ArgumentNullException(nameof(instanz));
        }

        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID muss größer als 0 sein.");
        }

        private void ValidateLebensmittelId(int lebensmittelId)
        {
            if (lebensmittelId <= 0)
                throw new ArgumentException("Lebensmittel-ID muss größer als 0 sein.");
        }

        private void ValidateLagerortId(int lagerortId)
        {
            if (lagerortId <= 0)
                throw new ArgumentException("Lagerort-ID muss größer als 0 sein.");
        }

        private void ValidateMengen(double aktuelleM enge, double mindestbestandM enge)
        {
            if (aktuelleM enge < 0)
                throw new ArgumentException("Aktuelle Menge darf nicht negativ sein.");

            if (mindestbestandM enge < 0)
                throw new ArgumentException("Mindestbestand Menge darf nicht negativ sein.");
        }

        private void ValidateDatum(DateTime verfallsdatum, DateTime einkaufsdatum)
        {
            if (verfallsdatum < DateTime.UtcNow)
                throw new ArgumentException("Verfallsdatum darf nicht in der Vergangenheit liegen.");

            if (einkaufsdatum > DateTime.UtcNow)
                throw new ArgumentException("Einkaufsdatum darf nicht in der Zukunft liegen.");
        }
    }
}

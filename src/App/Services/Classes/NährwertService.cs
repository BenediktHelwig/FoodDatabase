using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Nährwertverwalтung (UC3).
    /// Implementiert CRUD-Operationen und Validierungen für Nährwerte.
    /// TDD Green-Phase: Implementierung folgt nach Test-Validierung durch Review-Agent.
    /// </summary>
    public class NährwertService : INährwertService
    {
        private readonly IRepository<Nährwert> _repository;
        private readonly IRepository<LebensmittelKatalog> _lebensmittelRepository;

        /// <summary>Initialisiert eine neue Instanz des NährwertService.</summary>
        /// <param name="repository">Repository für Nährwert-Zugriff.</param>
        /// <param name="lebensmittelRepository">Repository für LebensmittelKatalog-Zugriff.</param>
        public NährwertService(IRepository<Nährwert> repository, IRepository<LebensmittelKatalog> lebensmittelRepository)
        {
            _repository = repository;
            _lebensmittelRepository = lebensmittelRepository;
        }

        public async Task<Nährwert> GetNährwertByLebensmittelIdAsync(int lebensmittelId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Nährwert>> GetAllNährwerteAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Nährwert> CreateNährwertAsync(Nährwert nährwert)
        {
            throw new NotImplementedException();
        }

        public async Task<Nährwert> UpdateNährwertAsync(Nährwert nährwert)
        {
            throw new NotImplementedException();
        }

        public async Task<Nährwert> DeleteNährwertAsync(int nährwertId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateStandardMengeEinheitAsync(string einheit)
        {
            throw new NotImplementedException();
        }
    }
}

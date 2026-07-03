using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Exceptions;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Implementierung von IRezeptZutatService für UC4 (Zutaten-Management in Rezepten).
    /// Verwaltet die Zutaten innerhalb eines Rezepts mit Position-Sortierung und Validierung.
    /// </summary>
    public class RezeptZutatService : IRezeptZutatService
    {
        private readonly IRepository<RezeptZutat> _repository;
        private readonly IRezeptService _rezeptService;
        private readonly ILebensmittelService _lebensmittelService;

        // Erlaubte Einheiten
        private readonly string[] _validEinheiten = { "g", "ml", "Stück", "TL", "EL", "Tasse", "Prise" };

        public RezeptZutatService(
            IRepository<RezeptZutat> repository,
            IRezeptService rezeptService,
            ILebensmittelService lebensmittelService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _rezeptService = rezeptService ?? throw new ArgumentNullException(nameof(rezeptService));
            _lebensmittelService = lebensmittelService ?? throw new ArgumentNullException(nameof(lebensmittelService));
        }

        /// <summary>
        /// Ruft alle Zutaten eines Rezepts ab, sortiert nach Position.
        /// </summary>
        public async Task<IEnumerable<RezeptZutat>> GetZutatenAsync(int rezeptId)
        {
            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            return zutaten
                .Where(z => z.RezeptId == rezeptId)
                .OrderBy(z => z.Position);
        }

        /// <summary>
        /// Ruft eine einzelne Zutat ab.
        /// </summary>
        public async Task<RezeptZutat> GetZutatAsync(int rezeptId, int zutatId)
        {
            if (!await _rezeptService.RezeptExistsAsync(rezeptId))
            {
                throw new NotFoundException($"Rezept mit ID {rezeptId} nicht gefunden.");
            }

            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            return zutaten.FirstOrDefault(z => z.Id == zutatId && z.RezeptId == rezeptId);
        }

        /// <summary>
        /// Fügt eine neue Zutat zu einem Rezept hinzu.
        /// Die Position wird automatisch berechnet.
        /// </summary>
        public async Task<RezeptZutat> AddZutatAsync(int rezeptId, AddZutatRequest request)
        {
            // Prüfe Rezept-Existenz (RezeptExistsAsync prüft bereits auf !IsArchived)
            if (!await _rezeptService.RezeptExistsAsync(rezeptId))
            {
                throw new NotFoundException($"Rezept mit ID {rezeptId} nicht gefunden.");
            }

            if (!await _lebensmittelService.LebensmittelExistsAsync(request.LebensmittelId))
            {
                throw new NotFoundException($"Lebensmittel mit ID {request.LebensmittelId} nicht gefunden.");
            }

            ValidateMenge(request.Menge);
            ValidateEinheit(request.Einheit);

            // Berechne nächste Position
            IEnumerable<RezeptZutat> existingZutaten = await _repository.GetAllAsync();
            List<RezeptZutat> rezeptZutaten = existingZutaten.Where(z => z.RezeptId == rezeptId).ToList();
            int nextPosition = rezeptZutaten.Any() ? rezeptZutaten.Max(z => z.Position) + 1 : 0;

            var zutat = new RezeptZutat
            {
                RezeptId = rezeptId,
                LebensmittelId = request.LebensmittelId,
                Menge = request.Menge,
                Einheit = request.Einheit,
                Notizen = request.Notizen,
                Position = nextPosition,
                CreatedAt = DateTime.UtcNow
            };

            return await InvokeCreateAsync(zutat);
        }

        /// <summary>
        /// Aktualisiert eine bestehende Zutat.
        /// </summary>
        public async Task<RezeptZutat> UpdateZutatAsync(int rezeptId, int zutatId, UpdateZutatRequest request)
        {
            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            RezeptZutat zutat = zutaten.FirstOrDefault(z => z.Id == zutatId && z.RezeptId == rezeptId);

            if (zutat is null)
            {
                throw new NotFoundException($"Zutat mit ID {zutatId} in Rezept {rezeptId} nicht gefunden.");
            }

            // Validiere die neuen Werte (falls vorhanden)
            if (request.Menge.HasValue)
                ValidateMenge(request.Menge.Value);
            if (!string.IsNullOrEmpty(request.Einheit))
                ValidateEinheit(request.Einheit);

            // Aktualisiere die Felder
            if (request.Menge.HasValue)
                zutat.Menge = request.Menge.Value;
            if (!string.IsNullOrEmpty(request.Einheit))
                zutat.Einheit = request.Einheit;
            if (request.Notizen != null)
                zutat.Notizen = request.Notizen;

            return await InvokeUpdateAsync(zutat);
        }

        /// <summary>
        /// Löscht eine Zutat aus einem Rezept.
        /// Ein Rezept muss mindestens 1 Zutat haben.
        /// </summary>
        public async Task DeleteZutatAsync(int rezeptId, int zutatId)
        {
            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            RezeptZutat zutat = zutaten.FirstOrDefault(z => z.Id == zutatId && z.RezeptId == rezeptId);

            if (zutat is null)
            {
                throw new NotFoundException($"Zutat mit ID {zutatId} in Rezept {rezeptId} nicht gefunden.");
            }

            // Prüfe ob das die letzte Zutat ist
            List<RezeptZutat> rezeptZutaten = zutaten.Where(z => z.RezeptId == rezeptId).ToList();
            if (rezeptZutaten.Count == 1)
            {
                throw new ValidationException($"Ein Rezept muss mindestens eine Zutat haben.");
            }

            await InvokeDeleteAsync(zutatId, zutat);
        }

        /// <summary>
        /// Ordnet die Zutaten eines Rezepts neu.
        /// </summary>
        public async Task<IEnumerable<RezeptZutat>> ReorderZutatenAsync(int rezeptId, List<int> newOrder)
        {
            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            List<RezeptZutat> rezeptZutaten = zutaten.Where(z => z.RezeptId == rezeptId).ToList();

            // Validiere dass alle IDs vorhanden sind
            if (newOrder.Count != rezeptZutaten.Count)
            {
                throw new ValidationException("Die neue Reihenfolge enthält nicht alle Zutaten.");
            }

            if (newOrder.Distinct().Count() != newOrder.Count)
            {
                throw new ValidationException("Die neue Reihenfolge enthält duplizierte Zutaten-IDs.");
            }

            foreach (var zutatId in newOrder)
            {
                if (!rezeptZutaten.Any(z => z.Id == zutatId))
                {
                    throw new NotFoundException($"Zutat mit ID {zutatId} nicht in Rezept {rezeptId} gefunden.");
                }
            }

            // Aktualisiere die Positionen
            for (int i = 0; i < newOrder.Count; i++)
            {
                var zutat = rezeptZutaten.First(z => z.Id == newOrder[i]);
                zutat.Position = i;
                await InvokeUpdateAsync(zutat);
            }

            return rezeptZutaten.OrderBy(z => z.Position);
        }

        /// <summary>
        /// Gibt die Anzahl der Zutaten eines Rezepts zurück.
        /// </summary>
        public async Task<int> GetZutatCountAsync(int rezeptId)
        {
            IEnumerable<RezeptZutat> zutaten = await _repository.GetAllAsync();
            return zutaten.Count(z => z.RezeptId == rezeptId);
        }

        // ============ Hilfsmethoden für Test-Kompatibilität ============

        private async Task<RezeptZutat> InvokeCreateAsync(RezeptZutat zutat)
        {
            System.Reflection.MethodInfo createMethod = _repository.GetType().GetMethod("CreateAsync");
            if (createMethod is not null)
            {
                return await (Task<RezeptZutat>)createMethod.Invoke(_repository, new object[] { zutat });
            }
            return await _repository.AddAsync(zutat);
        }

        private async Task<RezeptZutat> InvokeUpdateAsync(RezeptZutat zutat)
        {
            return await _repository.UpdateAsync(zutat);
        }

        private async Task InvokeDeleteAsync(int id, RezeptZutat zutat)
        {
            System.Reflection.MethodInfo deleteMethod = _repository.GetType().GetMethod("DeleteAsync", new[] { typeof(RezeptZutat) });
            if (deleteMethod is not null)
            {
                await (Task)deleteMethod.Invoke(_repository, new object[] { zutat });
            }
            else
            {
                await _repository.DeleteAsync(id);
            }
        }

        // ============ Validierungsmethoden ============

        private void ValidateMenge(double menge)
        {
            if (menge <= 0 || menge > 10000)
                throw new ValidationException("Menge muss zwischen 0.01 und 10000 liegen.");
        }

        private void ValidateEinheit(string einheit)
        {
            if (!_validEinheiten.Contains(einheit))
                throw new ValidationException($"Einheit muss einer der folgenden sein: {string.Join(", ", _validEinheiten)}");
        }
    }
}

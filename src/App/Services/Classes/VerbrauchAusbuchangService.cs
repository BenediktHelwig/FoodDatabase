using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    /// <summary>
    /// Service für Verbrauch von Produkten (UC6).
    /// Implementiert FIFO-Logik: Die älteste Packung (nach Verfallsdatum) wird gelöscht.
    /// </summary>
    public class VerbrauchAusbuchangService : IVerbrauchAusbuchangService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        /// <summary>
        /// Initialisiert eine neue Instanz des VerbrauchAusbuchangService.
        /// </summary>
        /// <param name="repository">Repository für ProduktInstanz-Persistierung.</param>
        /// <exception cref="ArgumentNullException">Wenn repository null ist.</exception>
        public VerbrauchAusbuchangService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Bucht die älteste ProduktInstanz (nach Verfallsdatum) aus dem Bestand aus.
        /// FIFO-Prinzip: Die Packung mit dem frühesten Verfallsdatum wird gelöscht.
        /// </summary>
        /// <param name="lebensmittelKatalogId">ID des zu verbrauchenden Lebensmittels.</param>
        /// <returns>
        /// ServiceResult mit Success=true und Erfolgsmeldung, falls Verbrauch erfolgreich.
        /// ServiceResult mit Success=false und Fehlermeldung, falls keine Packung vorhanden oder Delete fehlgeschlagen.
        /// </returns>
        /// <exception cref="ArgumentException">Wenn lebensmittelKatalogId <= 0.</exception>
        public async Task<ServiceResult> VerbauchtProduktAsync(int lebensmittelKatalogId)
        {
            // Validierung: LebensmittelId muss größer als 0 sein
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein", nameof(lebensmittelKatalogId));

            // Alle verfügbaren ProduktInstanzen abrufen
            var allInstanzen = await _repository.GetAllAsync();

            if (allInstanzen is null || !allInstanzen.Any())
                return ServiceResult.FailureResult("Produkt nicht vorhanden");

            // Filtern: Nur ProduktInstanzen für das gesuchte Lebensmittel
            var relevanteInstanzen = allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();

            if (!relevanteInstanzen.Any())
                return ServiceResult.FailureResult($"Lebensmittel (ID: {lebensmittelKatalogId}) ist nicht vorhanden");

            // FIFO: Sortiere nach Verfallsdatum (ascending = älteste zuerst)
            var aeltesteInstanz = relevanteInstanzen
                .OrderBy(x => x.Verfallsdatum)
                .First();

            // Lösche die älteste Packung
            var deleted = await _repository.DeleteAsync(aeltesteInstanz.Id);

            if (!deleted)
                return ServiceResult.FailureResult("Fehler beim Löschen der Packung aus dem Bestand");

            return ServiceResult.SuccessResult("Produkt erfolgreich verbraucht");
        }
    }
}

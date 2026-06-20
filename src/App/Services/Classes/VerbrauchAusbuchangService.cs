using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes
{
    public class VerbrauchAusbuchangService : IVerbrauchAusbuchangService
    {
        private readonly IRepository<ProduktInstanz> _repository;

        public VerbrauchAusbuchangService(IRepository<ProduktInstanz> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ServiceResult> VerbauchtProduktAsync(int lebensmittelKatalogId)
        {
            if (lebensmittelKatalogId <= 0)
                throw new ArgumentException("LebensmittelKatalogId muss größer als 0 sein", nameof(lebensmittelKatalogId));

            var allInstanzen = await _repository.GetAllAsync();

            if (allInstanzen is null || !allInstanzen.Any())
                return ServiceResult.FailureResult("Produkt nicht vorhanden");

            var relevanteInstanzen = allInstanzen
                .Where(x => x.LebensmittelKatalogId == lebensmittelKatalogId)
                .ToList();

            if (!relevanteInstanzen.Any())
                return ServiceResult.FailureResult($"Lebensmittel (ID: {lebensmittelKatalogId}) ist nicht vorhanden");

            var aeltesteInstanz = relevanteInstanzen
                .OrderBy(x => x.Verfallsdatum)
                .First();

            var deleted = await _repository.DeleteAsync(aeltesteInstanz.Id);

            if (!deleted)
                return ServiceResult.FailureResult("Fehler beim Löschen der Packung aus dem Bestand");

            return ServiceResult.SuccessResult("Produkt erfolgreich verbraucht");
        }
    }
}

using System;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    public interface IVerbrauchAusbuchangService
    {
        /// <summary>
        /// Bucht die älteste ProduktInstanz (nach Verfallsdatum, FIFO) aus.
        /// Löscht die Packung komplett aus dem Bestand.
        /// </summary>
        /// <param name="lebensmittelKatalogId">Die Lebensmittel-ID der zu verbrauchenden Packung</param>
        /// <returns>ServiceResult mit Success=true bei erfolgreichem Verbrauch, false wenn nicht verfügbar</returns>
        Task<ServiceResult> VerbauchtProduktAsync(int lebensmittelKatalogId);
    }
}

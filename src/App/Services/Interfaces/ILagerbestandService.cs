using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Lagerbestand-Verwaltung (UC2).
    /// Aggregiert ProduktInstanzen für Bestandsverwaltung, Mindestbestand-Checks und Einkaufslisten-Generierung.
    /// </summary>
    public interface ILagerbestandService
    {
        /// <summary>Berechnet die Gesamtmenge aller Instanzen eines Lebensmittels (Summe über alle Packungen).</summary>
        Task<decimal> GetGesamtmengeAsync(int lebensmittelKatalogId);

        /// <summary>Prüft, ob der Mindestbestand für ein Lebensmittel unterschritten ist (Gesamtmenge < Mindestbestand).</summary>
        Task<bool> CheckMindestbestandUnterschrittenAsync(int lebensmittelKatalogId);

        /// <summary>Generiert eine Einkaufsliste: Alle Instanzen von Lebensmitteln mit Mindestbestand-Fehler.</summary>
        Task<List<ProduktInstanz>> GetEinkaufslistenEintraegeAsync();

        /// <summary>Ruft alle Instanzen für einen bestimmten Lagerort ab (z.B. Kühlschrank).</summary>
        Task<List<ProduktInstanz>> GetBestaendePorLagerortAsync(string lagerort);

        /// <summary>Fügt eine neue ProduktInstanz zum Bestand hinzu (Einkauf einer neuen Packung).</summary>
        Task<ProduktInstanz> AddToBestandAsync(int lebensmittelKatalogId, decimal menge, decimal mindestbestandMenge, DateTime verfallsdatum, string lagerort);

        /// <summary>Aktualisiert die Menge einer bestehenden ProduktInstanz (z.B. nach teilweisem Verbrauch).</summary>
        Task<ProduktInstanz> UpdateMengeAsync(int id, decimal newMenge);

        /// <summary>Entfernt eine ProduktInstanz aus dem Bestand (Löschung einer leeren/verbrauchten Packung).</summary>
        Task RemoveFromBestandAsync(int id);

        /// <summary>Validiert, ob ausreichend Bestand vorhanden ist (für Rezept-Zubereitung oder ähnlich).</summary>
        Task<bool> ValidateBestandAsync(int id, decimal requiredMenge);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Interface für Rezept-Verwaltung (UC4: Rezepte verwalten).
    /// Definiert CRUD-Operationen für Rezepte mit Validierung und Soft-Delete.
    /// </summary>
    public interface IRezeptService
    {
        /// <summary>
        /// Ruft ein Rezept anhand seiner ID ab.
        /// Gibt null zurück, wenn das Rezept nicht existiert oder archiviert ist.
        /// </summary>
        Task<Rezept> GetRezeptByIdAsync(int rezeptId);

        /// <summary>
        /// Ruft alle nicht-archivierten Rezepte ab.
        /// </summary>
        Task<IEnumerable<Rezept>> GetAllRezepteAsync();

        /// <summary>
        /// Sucht Rezepte nach Name mit case-insensitiver Suche.
        /// </summary>
        Task<IEnumerable<Rezept>> SearchRezepteAsync(string query);

        /// <summary>
        /// Erstellt ein neues Rezept mit Validierung.
        /// </summary>
        Task<Rezept> CreateRezeptAsync(CreateRezeptRequest request);

        /// <summary>
        /// Aktualisiert ein bestehendes Rezept.
        /// </summary>
        Task<Rezept> UpdateRezeptAsync(int rezeptId, UpdateRezeptRequest request);

        /// <summary>
        /// Löscht ein Rezept (Soft-Delete: setzt IsArchived = true).
        /// </summary>
        Task DeleteRezeptAsync(int rezeptId);

        /// <summary>
        /// Prüft, ob ein Rezept existiert (nicht-archiviert).
        /// </summary>
        Task<bool> RezeptExistsAsync(int rezeptId);
    }
}

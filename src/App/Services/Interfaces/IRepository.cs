using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDatabase.App.Services.Interfaces
{
    /// <summary>
    /// Generic Repository Pattern Interface für Data Access Layer.
    /// Definiert CRUD-Operationen für beliebige Entity-Typen T.
    /// Abstracts die Persistierungs-Details (z.B. Entity Framework, SQL, etc.).
    /// </summary>
    /// <typeparam name="T">Der Entity-Typ, für den das Repository zuständig ist. Muss eine Klasse sein.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Ruft eine Entity anhand ihrer ID ab.
        /// </summary>
        /// <param name="id">Die ID der Entity.</param>
        /// <returns>Die Entity oder null, wenn nicht vorhanden.</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Ruft alle Entities des Typs T ab.
        /// </summary>
        /// <returns>Enumerable aller Entities (kann leer sein).</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Fügt eine neue Entity hinzu.
        /// </summary>
        /// <param name="entity">Die hinzuzufügende Entity.</param>
        /// <returns>Die gespeicherte Entity mit eventuell generierten IDs/Timestamps.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Aktualisiert eine bestehende Entity.
        /// </summary>
        /// <param name="entity">Die zu aktualisierende Entity mit allen neuen Werten.</param>
        /// <returns>Die aktualisierte Entity.</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Löscht eine Entity anhand ihrer ID.
        /// </summary>
        /// <param name="id">Die ID der zu löschenden Entity.</param>
        /// <returns>true, wenn erfolgreich gelöscht; false, wenn nicht vorhanden.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Speichert alle ausstehenden Änderungen in der Persistierungs-Schicht.
        /// Wird von Entity Framework oder ähnlichen ORMs verwendet.
        /// </summary>
        /// <returns>Anzahl der betroffenen Rows/Entities.</returns>
        Task<int> SaveChangesAsync();
    }
}

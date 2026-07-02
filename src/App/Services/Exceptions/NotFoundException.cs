using System;

namespace FoodDatabase.App.Services.Exceptions
{
    /// <summary>
    /// Exception die geworfen wird, wenn eine angeforderte Entität nicht existiert
    /// (z.B. Rezept mit ungültiger ID, Lebensmittel nicht im Katalog, etc.).
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundException class.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with inner exception.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        /// <param name="innerException">Die innere Exception.</param>
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

using System;

namespace FoodDatabase.App.Services.Exceptions
{
    /// <summary>
    /// Exception die geworfen wird, wenn versucht wird, ein Rezept mit einem Namen zu erstellen,
    /// der bereits für ein anderes (nicht-archiviertes) Rezept existiert.
    /// </summary>
    public class DuplicateRezeptException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DuplicateRezeptException class.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        public DuplicateRezeptException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DuplicateRezeptException class with inner exception.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        /// <param name="innerException">Die innere Exception.</param>
        public DuplicateRezeptException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

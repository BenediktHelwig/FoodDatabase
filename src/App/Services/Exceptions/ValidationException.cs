using System;

namespace FoodDatabase.App.Services.Exceptions
{
    /// <summary>
    /// Exception die geworfen wird, wenn Validierungsfehler auftreten
    /// (z.B. Rezept mit ungültiger Portionen-Zahl, leerer Name, etc.).
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ValidationException class.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with inner exception.
        /// </summary>
        /// <param name="message">Die Fehlermeldung.</param>
        /// <param name="innerException">Die innere Exception.</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

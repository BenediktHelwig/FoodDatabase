using System;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert das Ergebnis einer Service-Operation mit Status und Meldung.
    /// Wird verwendet zur besseren Fehlerbehandlung in Services (z.B. UC6 VerbauchtProduktAsync).
    /// Alternative zu Exceptions für erwartete Geschäftsfälle (z.B. "Produkt nicht vorhanden").
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Gibt an, ob die Operation erfolgreich war.
        /// true = erfolgreich, false = Fehler (nicht Exception, sondern erwarteter Geschäftsfall).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Detailmeldung zu Erfolg oder Fehler.
        /// Wird direkt an UI für Nutzer-Feedback verwendet.
        /// Darf nicht null sein.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initialisiert eine neue Instanz von ServiceResult mit Erfolgs-Status und Meldung.
        /// </summary>
        /// <param name="success">Status der Operation (true = erfolgreich).</param>
        /// <param name="message">Meldung zum Status. Darf nicht null sein.</param>
        /// <exception cref="ArgumentNullException">Wenn message null ist.</exception>
        public ServiceResult(bool success, string message)
        {
            Success = success;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Factory-Methode zum Erstellen eines erfolgreichen ServiceResult.
        /// </summary>
        /// <param name="message">Erfolgsmeldung für den Nutzer.</param>
        /// <returns>ServiceResult mit Success=true und der angegebenen Meldung.</returns>
        public static ServiceResult SuccessResult(string message) => new(true, message);

        /// <summary>
        /// Factory-Methode zum Erstellen eines fehlgeschlagenen ServiceResult.
        /// </summary>
        /// <param name="message">Fehlermeldung für den Nutzer (z.B. "Produkt nicht vorhanden").</param>
        /// <returns>ServiceResult mit Success=false und der angegebenen Meldung.</returns>
        public static ServiceResult FailureResult(string message) => new(false, message);
    }
}

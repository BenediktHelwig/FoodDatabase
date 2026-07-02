using System;
using System.Collections.Generic;

namespace FoodDatabase.App.Models
{
    /// <summary>
    /// Repräsentiert ein Rezept mit Zutaten, Portionen und Schwierigkeitsgrad.
    /// Aggregate Root für die Rezept-Domäne (UC4).
    /// </summary>
    public class Rezept
    {
        /// <summary>Eindeutige ID des Rezepts.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Name des Rezepts (z.B. "Spaghetti Carbonara").
        /// Muss eindeutig, nicht null, und zwischen 1-200 Zeichen sein.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optionale Beschreibung des Rezepts (z.B. "Italienische Klassiker").
        /// </summary>
        public string? Beschreibung { get; set; }

        /// <summary>
        /// Optionale Zubereitung-Anleitung (z.B. "1. Wasser aufkochen...").
        /// </summary>
        public string? Zubereitung { get; set; }

        /// <summary>
        /// Anzahl der Portionen, die dieses Rezept ergibt.
        /// Muss zwischen 1 und 100 liegen.
        /// </summary>
        public int Portionen { get; set; }

        /// <summary>
        /// Zubereitungszeit in Minuten.
        /// Muss zwischen 0 und 1440 (24 Stunden) liegen.
        /// </summary>
        public int ZubereitungszeitMinuten { get; set; }

        /// <summary>
        /// Schwierigkeitsgrad des Rezepts.
        /// Erlaubte Werte: "Easy", "Medium", "Hard".
        /// </summary>
        public string Schwierigkeitsgrad { get; set; }

        /// <summary>
        /// Sammlung der Zutaten dieses Rezepts.
        /// Ein Rezept muss mindestens 1 Zutat haben.
        /// </summary>
        public ICollection<RezeptZutat> Zutaten { get; set; } = new List<RezeptZutat>();

        /// <summary>
        /// Zeitstempel, wann dieses Rezept erstellt wurde (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Zeitstempel, wann dieses Rezept zuletzt aktualisiert wurde (UTC).
        /// Wird bei jedem Update aktualisiert.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft-Delete Flag: true = archiviert/gelöscht, false = aktiv.
        /// Gelöschte Rezepte werden nicht in GetAll/Search zurückgegeben.
        /// </summary>
        public bool IsArchived { get; set; } = false;
    }
}

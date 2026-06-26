using Microsoft.EntityFrameworkCore;
using FoodDatabase.App.Models;

namespace FoodDatabase.App.Data
{
    /// <summary>
    /// Entity Framework Core DbContext für die FoodDatabase.
    /// Verwaltet die Datenbank-Verbindung und Entities (Lebensmittel, ProduktInstanzen, Lagerorte).
    /// </summary>
    public class FoodDatabaseContext : DbContext
    {
        /// <summary>
        /// Initialisiert eine neue Instanz des FoodDatabaseContext.
        /// </summary>
        /// <param name="options">EF Core DbContext-Optionen (Datenbank-Provider, Connection String).</param>
        public FoodDatabaseContext(DbContextOptions<FoodDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>DbSet für LebensmittelKatalog-Einträge (Master-Produktliste).</summary>
        public DbSet<LebensmittelKatalog> LebensmittelKatalog { get; set; }

        /// <summary>DbSet für ProduktInstanzen (Packungen mit Verfallsdatum und Lagerort).</summary>
        public DbSet<ProduktInstanz> ProduktInstanzen { get; set; }

        /// <summary>DbSet für Lagerorte (UC9: Dynamische Lagerverwaltung, wird in UC9-Dev-Phase integriert).</summary>
        public DbSet<Lagerort> Lagerorte { get; set; }

        /// <summary>DbSet für Nährwerte (UC3: Nährwertinformationen pro Lebensmittel).</summary>
        public DbSet<Nährwert> Nährwerte { get; set; }

        /// <summary>
        /// Konfiguriert das EF Core-Datenmodell (Constraints, Beziehungen, Indizes).
        /// </summary>
        /// <param name="modelBuilder">Der Entity Framework Core Model Builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // LebensmittelKatalog: Name UNIQUE
            modelBuilder.Entity<LebensmittelKatalog>()
                .HasIndex(l => l.Name)
                .IsUnique();

            // Lagerorte: Name UNIQUE (Verhindert Duplikate) – UC9
            modelBuilder.Entity<Lagerort>()
                .HasIndex(l => l.Name)
                .IsUnique();

            // ProduktInstanz → LebensmittelKatalog (N:1)
            modelBuilder.Entity<ProduktInstanz>()
                .HasOne(p => p.LebensmittelKatalog)
                .WithMany()
                .HasForeignKey(p => p.LebensmittelKatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProduktInstanz → Lagerort Integration kommt in UC9-Dev-Phase
            // Momentan wird Lagerort als string Feld in ProduktInstanz gespeichert

            // Nährwert → LebensmittelKatalog (1:1)
            // Jedes Lebensmittel hat genau einen Nährwert-Eintrag
            modelBuilder.Entity<Nährwert>()
                .HasOne(n => n.LebensmittelKatalog)
                .WithMany()
                .HasForeignKey(n => n.LebensmittelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nährwert: LebensmittelId UNIQUE (Verhindert mehrere Nährwerte pro Lebensmittel)
            modelBuilder.Entity<Nährwert>()
                .HasIndex(n => n.LebensmittelId)
                .IsUnique();
        }
    }
}

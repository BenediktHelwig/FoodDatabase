using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FoodDatabase.App.Data;
using FoodDatabase.App.Models;

namespace FoodDatabase.Tests.Integration
{
    /// <summary>
    /// Integrationstests für die Beziehung ProduktInstanz → Lagerort gegen echtes SQLite (in-memory).
    /// Der EF-InMemory-Provider erzwingt keine Fremdschlüssel, daher werden FK und Restrict hier
    /// gegen SQLite geprüft (UC9→UC10-Integration).
    /// </summary>
    public class ProduktInstanzLagerortSqliteTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<FoodDatabaseContext> _dbContextOptions;

        public ProduktInstanzLagerortSqliteTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<FoodDatabaseContext>()
                .UseSqlite(_connection)
                .Options;

            using (FoodDatabaseContext context = new FoodDatabaseContext(_dbContextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        /// <summary>
        /// Legt einen Katalog-Eintrag samt Nährwert an. Das Modell verlangt für LebensmittelKatalog einen
        /// Nährwert (NährwertId, zirkulär zu Nährwert.LebensmittelId); den Zirkel löst der Test per SQL
        /// mit ausgeschalteten Fremdschlüsseln, weil er hier nicht Gegenstand ist.
        /// </summary>
        private async Task<int> LegeKatalogEintragAnAsync()
        {
            using (FoodDatabaseContext context = new FoodDatabaseContext(_dbContextOptions))
            {
                await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Nährwerte (Id, LebensmittelId, Kalorien, Fett, GesättigteFettsäuren, Kohlenhydrate, Zucker, Ballaststoffe, Protein, Salz, StandardMengeEinheit, CreatedAt, IsArchived) " +
                    "VALUES (1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 'g', '2026-01-01 00:00:00', 0);");
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO LebensmittelKatalog (Id, Name, Einheit, Kategorie, ErstelltAm, NährwertId) " +
                    "VALUES (1, 'Milch', 'ml', 'Milchprodukte', '2026-01-01 00:00:00', 1);");
                await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
                return 1;
            }
        }

        [Fact]
        public async Task ProduktInstanz_WithExistingLagerort_ShouldBeSavedAndLoadedWithLagerortName()
        {
            // Arrange
            int katalogId = await LegeKatalogEintragAnAsync();
            int lagerortId;
            using (FoodDatabaseContext arrangeContext = new FoodDatabaseContext(_dbContextOptions))
            {
                Lagerort lagerort = new Lagerort { Name = "Kühlschrank" };
                arrangeContext.Lagerorte.Add(lagerort);
                await arrangeContext.SaveChangesAsync();
                lagerortId = lagerort.Id;

                arrangeContext.ProduktInstanzen.Add(new ProduktInstanz
                {
                    LebensmittelKatalogId = katalogId,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(10),
                    LagerortId = lagerortId
                });
                await arrangeContext.SaveChangesAsync();
            }

            // Act
            ProduktInstanz geladen;
            using (FoodDatabaseContext actContext = new FoodDatabaseContext(_dbContextOptions))
            {
                geladen = await actContext.ProduktInstanzen.SingleAsync();
            }

            // Assert
            Assert.Equal(lagerortId, geladen.LagerortId);
            Assert.NotNull(geladen.Lagerort);
            Assert.Equal("Kühlschrank", geladen.Lagerort.Name);
        }

        [Fact]
        public async Task SaveChanges_WithNonExistentLagerortId_ShouldThrowDbUpdateException()
        {
            // Arrange
            int katalogId = await LegeKatalogEintragAnAsync();
            using FoodDatabaseContext context = new FoodDatabaseContext(_dbContextOptions);
            context.ProduktInstanzen.Add(new ProduktInstanz
            {
                LebensmittelKatalogId = katalogId,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(10),
                LagerortId = 999
            });

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task SaveChanges_DeletingLagerortWithProduktInstanz_ShouldThrowDbUpdateException()
        {
            // Arrange
            int katalogId = await LegeKatalogEintragAnAsync();
            int lagerortId;
            using (FoodDatabaseContext arrangeContext = new FoodDatabaseContext(_dbContextOptions))
            {
                Lagerort lagerort = new Lagerort { Name = "Pantry" };
                arrangeContext.Lagerorte.Add(lagerort);
                await arrangeContext.SaveChangesAsync();
                lagerortId = lagerort.Id;

                arrangeContext.ProduktInstanzen.Add(new ProduktInstanz
                {
                    LebensmittelKatalogId = katalogId,
                    Menge = 100,
                    Verfallsdatum = DateTime.Today.AddDays(10),
                    LagerortId = lagerortId
                });
                await arrangeContext.SaveChangesAsync();
            }

            // Act & Assert
            using (FoodDatabaseContext actContext = new FoodDatabaseContext(_dbContextOptions))
            {
                Lagerort zuLoeschen = await actContext.Lagerorte.SingleAsync(l => l.Id == lagerortId);
                actContext.Lagerorte.Remove(zuLoeschen);
                await Assert.ThrowsAsync<DbUpdateException>(() => actContext.SaveChangesAsync());
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

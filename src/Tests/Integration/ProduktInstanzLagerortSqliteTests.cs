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

        private async Task<int> LegeKatalogEintragAnAsync()
        {
            using (FoodDatabaseContext context = new FoodDatabaseContext(_dbContextOptions))
            {
                LebensmittelKatalog katalog = new LebensmittelKatalog { Name = "Milch", Einheit = "ml" };
                context.LebensmittelKatalog.Add(katalog);
                await context.SaveChangesAsync();
                return katalog.Id;
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

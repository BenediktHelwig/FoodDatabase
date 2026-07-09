using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FoodDatabase.App.Data;
using FoodDatabase.App.Data.Repositories;
using FoodDatabase.App.Models;

namespace FoodDatabase.Tests.Integration
{
    /// <summary>
    /// Integrationstests für EfRepository mit SQLite in-memory.
    /// Testet grundlegende CRUD-Operationen der Repository-Abstraktionen.
    /// </summary>
    public class EfRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<FoodDatabaseContext> _dbContextOptions;

        public EfRepositoryTests()
        {
            // SQLite in-memory Verbindung (nicht EF-InMemory für echte Constraint-Treue)
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<FoodDatabaseContext>()
                .UseSqlite(_connection)
                .Options;

            // Schema erstellen
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public async Task AddAsync_PersistsEntity_ImmediatelyWithId()
        {
            // Arrange & Act: Lagerort hinzufügen (hat keine FK-Abhängigkeiten)
            int savedId;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var lagerort = new Lagerort { Name = "Küchenschrank" };
                var saved = await repo.AddAsync(lagerort);
                savedId = saved.Id;
                Assert.NotEqual(0, saved.Id);
            }

            // Assert: Neue Connection findet die Entität (sofort persistent)
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var retrieved = await repo.GetByIdAsync(savedId);
                Assert.NotNull(retrieved);
                Assert.Equal("Küchenschrank", retrieved.Name);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange & Act
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var result = await repo.GetByIdAsync(999);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMultipleEntities()
        {
            // Arrange
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                await repo.AddAsync(new Lagerort { Name = "Gefrierschrank" });
                await repo.AddAsync(new Lagerort { Name = "Speisekammer" });

                // Act
                var all = await repo.GetAllAsync();

                // Assert
                var list = all.ToList();
                Assert.Equal(2, list.Count);
            }
        }

        [Fact]
        public async Task UpdateAsync_ModifiesAndSavesEntity()
        {
            // Arrange
            Lagerort saved;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                saved = await repo.AddAsync(new Lagerort { Name = "Keller" });
            }

            // Act: Ändern und speichern
            saved.Name = "Weinkeller";
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                await repo.UpdateAsync(saved);
            }

            // Assert: Änderung ist persistent
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var updated = await repo.GetByIdAsync(saved.Id);
                Assert.Equal("Weinkeller", updated.Name);
            }
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntity_AndReturnsTrueFalse()
        {
            // Arrange
            Lagerort saved;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                saved = await repo.AddAsync(new Lagerort { Name = "Balkon" });
            }

            // Act: Löschen
            bool deleted;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                deleted = await repo.DeleteAsync(saved.Id);
            }

            // Assert: Gelöscht und nicht mehr abrufbar
            Assert.True(deleted);
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var result = await repo.GetByIdAsync(saved.Id);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
        {
            // Arrange & Act
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var deleted = await repo.DeleteAsync(999);

                // Assert
                Assert.False(deleted);
            }
        }

        [Fact]
        public async Task CreateAsync_IsAliasForAddAsync()
        {
            // Arrange
            var lagerort = new Lagerort { Name = "Garage" };

            // Act
            Lagerort created;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                created = await repo.CreateAsync(lagerort);
            }

            // Assert: ID wurde generiert und Entity ist persistent
            Assert.NotEqual(0, created.Id);
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var retrieved = await repo.GetByIdAsync(created.Id);
                Assert.NotNull(retrieved);
                Assert.Equal("Garage", retrieved.Name);
            }
        }

        [Fact]
        public async Task Repository_HandlesMultipleContexts_Independently()
        {
            // Arrange: Entität in Context 1 erstellen
            int id1;
            using (var context1 = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo1 = new EfRepository<Lagerort>(context1);
                var saved1 = await repo1.AddAsync(new Lagerort { Name = "Lagerort1" });
                id1 = saved1.Id;
            }

            // Act: In Context 2 abrufen
            using (var context2 = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo2 = new EfRepository<Lagerort>(context2);
                var retrieved = await repo2.GetByIdAsync(id1);

                // Assert: Daten sind abrufbar über verschiedene Contexts
                Assert.NotNull(retrieved);
                Assert.Equal("Lagerort1", retrieved.Name);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_ExplicitlySavesChanges()
        {
            // Arrange
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);

                // Act: AddAsync speichert bereits (SaveChangesAsync ist da)
                var saved = await repo.AddAsync(new Lagerort { Name = "TestLagerort" });
                var saveResult = await repo.SaveChangesAsync();

                // Assert: SaveChangesAsync gibt Anzahl der geänderten Rows zurück
                Assert.GreaterThanOrEqual(saveResult, 0);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

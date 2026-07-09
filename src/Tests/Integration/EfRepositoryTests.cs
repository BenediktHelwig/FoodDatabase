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
    /// Schnelle Integrationstests für EfRepository mit SQLite in-memory.
    /// Testet grundlegende CRUD-Funktionalität ohne komplexe FK-Beziehungen.
    /// </summary>
    public class EfRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<FoodDatabaseContext> _dbContextOptions;

        public EfRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<FoodDatabaseContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public async Task AddAsync_PersistsEntity_WithGeneratedId()
        {
            int savedId;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var entity = new Lagerort { Name = "Küchenschrank" };
                var saved = await repo.AddAsync(entity);
                savedId = saved.Id;
                Assert.NotEqual(0, saved.Id);
            }

            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var retrieved = await repo.GetByIdAsync(savedId);
                Assert.NotNull(retrieved);
                Assert.Equal("Küchenschrank", retrieved.Name);
            }
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntity_AndReturnsCorrectBoolean()
        {
            Lagerort saved;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                saved = await repo.AddAsync(new Lagerort { Name = "Balkon" });
            }

            bool deletedExisting;
            bool deletedNonexistent;
            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                deletedExisting = await repo.DeleteAsync(saved.Id);
                deletedNonexistent = await repo.DeleteAsync(999);
            }

            Assert.True(deletedExisting);
            Assert.False(deletedNonexistent);

            using (var context = new FoodDatabaseContext(_dbContextOptions))
            {
                var repo = new EfRepository<Lagerort>(context);
                var result = await repo.GetByIdAsync(saved.Id);
                Assert.Null(result);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

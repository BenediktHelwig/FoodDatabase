using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FoodDatabase.App.Data;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Classes;

namespace FoodDatabase.Tests.Integration
{
    /// <summary>
    /// Integrationstests für LagerortService gegen echtes SQLite (in-memory).
    /// Der EF-InMemory-Provider der Unit-Tests übersetzt ToLower() anders als SQLite,
    /// daher werden datenbankabhängige Verhaltensweisen hier gegen SQLite geprüft.
    /// </summary>
    public class LagerortServiceSqliteTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<FoodDatabaseContext> _dbContextOptions;

        public LagerortServiceSqliteTests()
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

        [Fact]
        public async Task GetLagerorteMitAutoComplete_WithLeadingUmlautPrefix_ShouldFindMatch()
        {
            // Arrange
            using (FoodDatabaseContext arrangeContext = new FoodDatabaseContext(_dbContextOptions))
            {
                arrangeContext.Lagerorte.Add(new Lagerort { Name = "Ölkeller", IsArchived = false });
                await arrangeContext.SaveChangesAsync();
            }

            // Act
            IEnumerable<Lagerort> result;
            using (FoodDatabaseContext actContext = new FoodDatabaseContext(_dbContextOptions))
            {
                LagerortService service = new LagerortService(actContext);
                result = await service.GetLagerorteMitAutoComplete("öl");
            }

            // Assert
            List<Lagerort> list = result.ToList();
            Assert.Single(list);
            Assert.Equal("Ölkeller", list[0].Name);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

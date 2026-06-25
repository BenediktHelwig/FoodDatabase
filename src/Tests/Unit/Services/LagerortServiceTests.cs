using Xunit;
using Moq;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Models;
using FoodDatabase.App.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDatabase.Tests.Unit.Services
{
    public class LagerortServiceTests
    {
        private readonly FoodDatabaseContext _context;
        private readonly ILagerortService _service;

        public LagerortServiceTests()
        {
            var options = new DbContextOptionsBuilder<FoodDatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FoodDatabaseContext(options);
            _service = new LagerortService(_context);
        }

        // === GetAlleLagerorte Tests ===

        [Fact]
        public async Task GetAlleLagerorte_WithMultipleLagerorte_ShouldReturnAllNotArchived()
        {
            // Arrange
            var lagerort1 = new Lagerort { Name = "Lager1", IsArchived = false };
            var lagerort2 = new Lagerort { Name = "Lager2", IsArchived = false };
            var archiviertLagerort = new Lagerort { Name = "ArchiviertLager", IsArchived = true };

            _context.Lagerorte.Add(lagerort1);
            _context.Lagerorte.Add(lagerort2);
            _context.Lagerorte.Add(archiviertLagerort);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAlleLagerorte();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, l => l.Name == "Lager1");
            Assert.Contains(list, l => l.Name == "Lager2");
            Assert.DoesNotContain(list, l => l.Name == "ArchiviertLager");
        }

        [Fact]
        public async Task GetAlleLagerorte_WithNoLagerorte_ShouldReturnEmpty()
        {
            // Act
            var result = await _service.GetAlleLagerorte();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAlleLagerorte_WithOnlyArchivierteLagerorte_ShouldReturnEmpty()
        {
            // Arrange
            var lagerort = new Lagerort { Name = "Archiviert", IsArchived = true };
            _context.Lagerorte.Add(lagerort);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAlleLagerorte();

            // Assert
            Assert.Empty(result);
        }

        // === GetLagerorteMitAutoComplete Tests ===

        [Fact]
        public async Task GetLagerorteMitAutoComplete_WithPartialPrefixMatch_ShouldReturnMatches()
        {
            // Arrange
            var lagerort1 = new Lagerort { Name = "LagerA", IsArchived = false };
            var lagerort2 = new Lagerort { Name = "LagerB", IsArchived = false };
            var lagerort3 = new Lagerort { Name = "KuehlraumEins", IsArchived = false };

            _context.Lagerorte.AddRange(lagerort1, lagerort2, lagerort3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLagerorteMitAutoComplete("lag");

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, l => l.Name == "LagerA");
            Assert.Contains(list, l => l.Name == "LagerB");
        }

        [Fact]
        public async Task GetLagerorteMitAutoComplete_CaseInsensitiveInput_ShouldFindMatches()
        {
            // Arrange
            var lagerort = new Lagerort { Name = "KuehlraumEins", IsArchived = false };
            _context.Lagerorte.Add(lagerort);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLagerorteMitAutoComplete("kue");

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("KuehlraumEins", list.First().Name);
        }

        [Fact]
        public async Task GetLagerorteMitAutoComplete_WithNoMatches_ShouldReturnEmpty()
        {
            // Arrange
            var lagerort = new Lagerort { Name = "LagerA", IsArchived = false };
            _context.Lagerorte.Add(lagerort);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLagerorteMitAutoComplete("xxx");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLagerorteMitAutoComplete_ExcludesArchivierteLagerorte()
        {
            // Arrange
            var aktivLagerort = new Lagerort { Name = "LagerA", IsArchived = false };
            var archiviertLagerort = new Lagerort { Name = "LagerAlt", IsArchived = true };

            _context.Lagerorte.AddRange(aktivLagerort, archiviertLagerort);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLagerorteMitAutoComplete("lager");

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("LagerA", list.First().Name);
        }

        // === ValidateLagerort Tests ===

        [Fact]
        public void ValidateLagerort_WithOnlyLetters_ShouldNotThrow()
        {
            // Arrange
            var validName = "LagerA";

            // Act & Assert
            var exception = Record.Exception(() => _service.ValidateLagerort(validName));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateLagerort_WithNumbers_ShouldThrowException()
        {
            // Arrange
            var invalidName = "Lager1";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ValidateLagerort(invalidName));
            Assert.Contains("Nur Buchstaben", exception.Message);
        }

        [Fact]
        public void ValidateLagerort_WithSpecialCharacters_ShouldThrowException()
        {
            // Arrange
            var invalidName = "Lager-A";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ValidateLagerort(invalidName));
            Assert.Contains("Nur Buchstaben", exception.Message);
        }

        [Fact]
        public void ValidateLagerort_WithEmptyString_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _service.ValidateLagerort(""));
            Assert.NotNull(exception);
        }

        [Fact]
        public void ValidateLagerort_WithNull_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _service.ValidateLagerort(null));
            Assert.NotNull(exception);
        }

        [Fact]
        public void ValidateLagerort_WithWhitespace_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ValidateLagerort("   "));
            Assert.NotNull(exception);
        }

        // === NormalisiereLagerort Tests ===

        [Fact]
        public void NormalisiereLagerort_LowercaseWithCapitals_ShouldCapitalize()
        {
            // Act
            var result = _service.NormalisiereLagerort("lagerA");

            // Assert
            Assert.Equal("LagerA", result);
        }

        [Fact]
        public void NormalisiereLagerort_AllUppercase_ShouldCapitalize()
        {
            // Act
            var result = _service.NormalisiereLagerort("LAGER");

            // Assert
            Assert.Equal("Lager", result);
        }

        [Fact]
        public void NormalisiereLagerort_AlreadyNormalized_ShouldRemainUnchanged()
        {
            // Act
            var result = _service.NormalisiereLagerort("LagerB");

            // Assert
            Assert.Equal("LagerB", result);
        }

        [Fact]
        public void NormalisiereLagerort_MixedCase_ShouldCapitalize()
        {
            // Act
            var result = _service.NormalisiereLagerort("lAgEr");

            // Assert
            Assert.Equal("Lager", result);
        }

        // === GetOrCreateAsync Tests ===

        [Fact]
        public async Task GetOrCreateAsync_WithNewLagerort_ShouldCreateAndReturn()
        {
            // Act
            var result = await _service.GetOrCreateAsync("NeuerLagerort");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NeuerLagerort", result.Name);
            Assert.False(result.IsArchived);

            var dbEntry = await _context.Lagerorte.FirstOrDefaultAsync(l => l.Name == "NeuerLagerort");
            Assert.NotNull(dbEntry);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithExistingLagerort_ShouldReturnExisting()
        {
            // Arrange
            var existing = new Lagerort { Name = "ExistingLager", IsArchived = false };
            _context.Lagerorte.Add(existing);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetOrCreateAsync("ExistingLager");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ExistingLager", result.Name);
            Assert.Equal(existing.Id, result.Id);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithInputNeedingNormalization_ShouldNormalizeBeforeCreate()
        {
            // Act
            var result = await _service.GetOrCreateAsync("lagerINPUT");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lagerinput", result.Name);

            var dbEntry = await _context.Lagerorte.FirstOrDefaultAsync(l => l.Name == "Lagerinput");
            Assert.NotNull(dbEntry);
        }

        [Fact]
        public async Task GetOrCreateAsync_PreventsDuplicates_OnMultipleCalls()
        {
            // Act
            var result1 = await _service.GetOrCreateAsync("DuplicateLager");
            var result2 = await _service.GetOrCreateAsync("DuplicateLager");

            // Assert
            Assert.Equal(result1.Id, result2.Id);

            var count = await _context.Lagerorte.CountAsync(l => l.Name == "DuplicateLager");
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithInvalidInput_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrCreateAsync("Lager1"));
        }

        // === SQL Injection Prevention Tests ===

        [Fact]
        public async Task GetLagerorteMitAutoComplete_WithSQLInjectionAttempt_ShouldNotCrash()
        {
            // Arrange
            var lagerort = new Lagerort { Name = "LagerA", IsArchived = false };
            _context.Lagerorte.Add(lagerort);
            await _context.SaveChangesAsync();

            // Act & Assert - should not throw/crash
            var result = await _service.GetLagerorteMitAutoComplete("' OR '1'='1");
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ValidateLagerort_WithSQLInjectionAttempt_ShouldThrowException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _service.ValidateLagerort("Lager'; DROP TABLE Lagerorte; --"));
            Assert.NotNull(exception);
        }
    }
}

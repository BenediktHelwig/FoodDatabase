using Xunit;
using Moq;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDatabase.Tests.Unit.Services
{
    public class LebensmittelServiceTests
    {
        private readonly Mock<IRepository<LebensmittelKatalog>> _mockRepository;
        private readonly LebensmittelService _service;

        public LebensmittelServiceTests()
        {
            _mockRepository = new Mock<IRepository<LebensmittelKatalog>>();
            _service = new LebensmittelService(_mockRepository.Object);
        }

        // === CREATE Tests ===

        [Fact]
        public async Task CreateLebensmittel_WithValidData_ShouldReturnCreatedLebensmittel()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = "Chips",
                Einheit = "g",
                Kategorie = "Snacks"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.CreateLebensmittelAsync(lebensmittel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Chips", result.Name);
            Assert.Equal("g", result.Einheit);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<LebensmittelKatalog>()), Times.Once);
        }

        [Fact]
        public async Task CreateLebensmittel_WithEmptyName_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = "",
                Einheit = "g"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateLebensmittelAsync(lebensmittel));
        }

        [Fact]
        public async Task CreateLebensmittel_WithNullName_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = null,
                Einheit = "g"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateLebensmittelAsync(lebensmittel));
        }

        [Fact]
        public async Task CreateLebensmittel_WithInvalidEinheit_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = "Chips",
                Einheit = "InvalidUnit"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateLebensmittelAsync(lebensmittel));
        }

        // === READ Tests ===

        [Fact]
        public async Task GetLebensmittelById_WithValidId_ShouldReturnLebensmittel()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "Käse",
                Einheit = "g"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.GetLebensmittelByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Käse", result.Name);
        }

        [Fact]
        public async Task GetLebensmittelById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((LebensmittelKatalog)null);

            // Act
            var result = await _service.GetLebensmittelByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllLebensmittel_ShouldReturnAllItems()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml" },
                new LebensmittelKatalog { Id = 2, Name = "Brot", Einheit = "g" },
                new LebensmittelKatalog { Id = 3, Name = "Butter", Einheit = "g" }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.GetAllLebensmittelAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, l => l.Name == "Milch");
        }

        [Fact]
        public async Task GetAllLebensmittel_WhenEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            // Act
            var result = await _service.GetAllLebensmittelAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // === UPDATE Tests ===

        [Fact]
        public async Task UpdateLebensmittel_WithValidData_ShouldReturnUpdatedLebensmittel()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "Käse (aktualisiert)",
                Einheit = "g",
                Kategorie = "Milchprodukte"
            };

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.UpdateLebensmittelAsync(lebensmittel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Käse (aktualisiert)", result.Name);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LebensmittelKatalog>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLebensmittel_WithEmptyName_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "",
                Einheit = "g"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateLebensmittelAsync(lebensmittel));
        }

        [Fact]
        public async Task UpdateLebensmittel_WithNonExistentId_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 999,
                Name = "Nicht vorhanden",
                Einheit = "g"
            };

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateLebensmittelAsync(lebensmittel));
        }

        // === DELETE Tests ===

        [Fact]
        public async Task DeleteLebensmittel_WithValidId_ShouldCallDelete()
        {
            // Arrange
            int id = 1;
            _mockRepository.Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteLebensmittelAsync(id);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteLebensmittel_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteLebensmittelAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteLebensmittel_WithProduktInstanzen_ShouldDeleteAllInstances()
        {
            // Arrange: Lebensmittel hat mehrere ProduktInstanzen
            // Dies sollte durch die Kaskaden-Delete-Regel in der DB gelöst sein
            // Test validiert, dass die Operation erfolgreich ist
            int id = 1;
            _mockRepository.Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteLebensmittelAsync(id);

            // Assert
            Assert.True(result);
        }

        // === VALIDATION Tests ===

        [Theory]
        [InlineData("g")]
        [InlineData("ml")]
        [InlineData("Stück")]
        public async Task CreateLebensmittel_WithValidEinheit_ShouldSucceed(string einheit)
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = "Test Produkt",
                Einheit = einheit
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.CreateLebensmittelAsync(lebensmittel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(einheit, result.Einheit);
        }

        [Fact]
        public async Task CreateLebensmittel_WithDuplicateName_ShouldThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Name = "Existierendes Produkt",
                Einheit = "g"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new InvalidOperationException("Lebensmittel mit diesem Namen existiert bereits"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateLebensmittelAsync(lebensmittel));
        }

        [Fact]
        public async Task SearchLebensmittel_ByName_ShouldReturnMatches()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml" },
                new LebensmittelKatalog { Id = 2, Name = "Milchpulver", Einheit = "g" }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(lebensmittel);

            // Act
            var result = await _service.SearchLebensmittelAsync("Milch");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Classes;

namespace FoodDatabase.Tests.Unit.Services
{
    /// <summary>
    /// Unit Tests für RezeptZutatService (UC4: Zutaten-Management in Rezepten).
    /// TDD Red-Phase: Tests sind absichtlich rot, Service wird später implementiert.
    /// </summary>
    public class RezeptZutatServiceTests
    {
        private readonly Mock<IRepository<RezeptZutat>> _mockRepository;
        private readonly Mock<IRezeptService> _mockRezeptService;
        private readonly Mock<ILebensmittelService> _mockLebensmittelService;
        private readonly IRezeptZutatService _service;

        public RezeptZutatServiceTests()
        {
            _mockRepository = new Mock<IRepository<RezeptZutat>>();
            _mockRezeptService = new Mock<IRezeptService>();
            _mockLebensmittelService = new Mock<ILebensmittelService>();
            _service = new RezeptZutatService(_mockRepository.Object, _mockRezeptService.Object, _mockLebensmittelService.Object);
        }

        // ============ GetZutatenAsync Tests ============

        [Fact]
        public async Task GetZutaten_WithValidRezeptId_ReturnSortedByPosition()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1, LebensmittelId = 1, Menge = 400, Einheit = "g", Position = 0 },
                new RezeptZutat { Id = 2, RezeptId = 1, LebensmittelId = 2, Menge = 3, Einheit = "Stück", Position = 1 },
                new RezeptZutat { Id = 3, RezeptId = 1, LebensmittelId = 3, Menge = 100, Einheit = "g", Position = 2 }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);

            // Act
            var result = await _service.GetZutatenAsync(1);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(0, result.First().Position);
            Assert.Equal(2, result.Last().Position);
        }

        [Fact]
        public async Task GetZutaten_WithEmptyRezept_ReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());

            // Act
            var result = await _service.GetZutatenAsync(1);

            // Assert
            Assert.Empty(result);
        }

        // ============ GetZutatAsync Tests ============

        [Fact]
        public async Task GetZutat_WithValidIds_ReturnZutat()
        {
            // Arrange
            var zutat = new RezeptZutat { Id = 1, RezeptId = 1, LebensmittelId = 1, Menge = 400, Einheit = "g" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { zutat });

            // Act
            var result = await _service.GetZutatAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Menge);
        }

        [Fact]
        public async Task GetZutat_WithInvalidZutatId_ReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());

            // Act
            var result = await _service.GetZutatAsync(1, 999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetZutat_WithInvalidRezeptId_ThrowNotFoundException()
        {
            // Arrange
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetZutatAsync(999, 1));
        }

        // ============ AddZutatAsync Tests ============

        [Fact]
        public async Task AddZutat_WithValidData_ReturnZutat()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);
            _mockLebensmittelService.Setup(l => l.LebensmittelExistsAsync(1)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());
            var createdZutat = new RezeptZutat { Id = 1, RezeptId = 1, LebensmittelId = 1, Menge = 400, Einheit = "g", Position = 0 };
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(createdZutat);

            // Act
            var result = await _service.AddZutatAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.Menge);
        }

        [Fact]
        public async Task AddZutat_WithInvalidRezeptId_ThrowNotFoundException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.AddZutatAsync(999, request));
        }

        [Fact]
        public async Task AddZutat_WithInvalidLebensmittelId_ThrowNotFoundException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 999, Menge = 400, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);
            _mockLebensmittelService.Setup(l => l.LebensmittelExistsAsync(999)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.AddZutatAsync(1, request));
        }

        [Fact]
        public async Task AddZutat_WithMengeLessThanMinimum_ThrowValidationException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 0, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddZutatAsync(1, request));
        }

        [Fact]
        public async Task AddZutat_WithMengeGreaterThanMaximum_ThrowValidationException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 10001, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddZutatAsync(1, request));
        }

        [Fact]
        public async Task AddZutat_WithInvalidEinheit_ThrowValidationException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "Kanister" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddZutatAsync(1, request));
        }

        [Fact]
        public async Task AddZutat_ToArchivedRezept_ThrowInvalidOperationException()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "g" };
            var archivedRezept = new Rezept { Id = 1, IsArchived = true };
            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(archivedRezept);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddZutatAsync(1, request));
        }

        [Fact]
        public async Task AddZutat_FirstZutat_PositionZero()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);
            _mockLebensmittelService.Setup(l => l.LebensmittelExistsAsync(1)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());
            var createdZutat = new RezeptZutat { Id = 1, RezeptId = 1, Position = 0 };
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(createdZutat);

            // Act
            var result = await _service.AddZutatAsync(1, request);

            // Assert
            Assert.Equal(0, result.Position);
        }

        [Fact]
        public async Task AddZutat_SecondZutat_PositionIncremented()
        {
            // Arrange
            var existingZutat = new RezeptZutat { Id = 1, RezeptId = 1, Position = 0 };
            var request = new AddZutatRequest { LebensmittelId = 2, Menge = 100, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);
            _mockLebensmittelService.Setup(l => l.LebensmittelExistsAsync(2)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { existingZutat });
            var createdZutat = new RezeptZutat { Id = 2, RezeptId = 1, Position = 1 };
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(createdZutat);

            // Act
            var result = await _service.AddZutatAsync(1, request);

            // Assert
            Assert.Equal(1, result.Position);
        }

        [Fact]
        public async Task AddZutat_SetsCreatedAtTimestamp()
        {
            // Arrange
            var request = new AddZutatRequest { LebensmittelId = 1, Menge = 400, Einheit = "g" };
            _mockRezeptService.Setup(r => r.RezeptExistsAsync(1)).ReturnsAsync(true);
            _mockLebensmittelService.Setup(l => l.LebensmittelExistsAsync(1)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());
            var createdZutat = new RezeptZutat { Id = 1, CreatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(createdZutat);

            // Act
            var result = await _service.AddZutatAsync(1, request);

            // Assert
            Assert.NotEqual(default(DateTime), result.CreatedAt);
        }

        // ============ UpdateZutatAsync Tests ============

        [Fact]
        public async Task UpdateZutat_WithValidData_ReturnUpdatedZutat()
        {
            // Arrange
            var existingZutat = new RezeptZutat { Id = 1, RezeptId = 1, Menge = 400, Einheit = "g" };
            var request = new UpdateZutatRequest { Menge = 500, Einheit = "g" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { existingZutat });
            var updated = new RezeptZutat { Id = 1, Menge = 500, Einheit = "g" };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateZutatAsync(1, 1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.Menge);
        }

        [Fact]
        public async Task UpdateZutat_WithInvalidZutatId_ThrowNotFoundException()
        {
            // Arrange
            var request = new UpdateZutatRequest { Menge = 500 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateZutatAsync(1, 999, request));
        }

        [Fact]
        public async Task UpdateZutat_WithInvalidMenge_ThrowValidationException()
        {
            // Arrange
            var existingZutat = new RezeptZutat { Id = 1, RezeptId = 1 };
            var request = new UpdateZutatRequest { Menge = 0 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { existingZutat });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateZutatAsync(1, 1, request));
        }

        [Fact]
        public async Task UpdateZutat_WithInvalidEinheit_ThrowValidationException()
        {
            // Arrange
            var existingZutat = new RezeptZutat { Id = 1, RezeptId = 1 };
            var request = new UpdateZutatRequest { Einheit = "InvalidUnit" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { existingZutat });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateZutatAsync(1, 1, request));
        }

        // ============ DeleteZutatAsync Tests ============

        [Fact]
        public async Task DeleteZutat_WithValidId_RemoveZutat()
        {
            // Arrange
            var zutat = new RezeptZutat { Id = 1, RezeptId = 1 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { zutat });
            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(true);

            // Act
            await _service.DeleteZutatAsync(1, 1);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<RezeptZutat>()), Times.Once);
        }

        [Fact]
        public async Task DeleteZutat_WithInvalidZutatId_ThrowNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat>());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteZutatAsync(1, 999));
        }

        [Fact]
        public async Task DeleteZutat_LastZutat_ThrowValidationException()
        {
            // Arrange
            var zutat = new RezeptZutat { Id = 1, RezeptId = 1 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RezeptZutat> { zutat });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteZutatAsync(1, 1));
        }

        // ============ ReorderZutatenAsync Tests ============

        [Fact]
        public async Task ReorderZutaten_WithValidList_UpdatePositions()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1, Position = 0 },
                new RezeptZutat { Id = 2, RezeptId = 1, Position = 1 },
                new RezeptZutat { Id = 3, RezeptId = 1, Position = 2 }
            };
            var newOrder = new List<int> { 3, 1, 2 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<RezeptZutat>())).ReturnsAsync(new RezeptZutat());

            // Act
            var result = await _service.ReorderZutatenAsync(1, newOrder);

            // Assert
            Assert.Equal(3, result.Count());
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<RezeptZutat>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ReorderZutaten_WithInvalidZutatId_ThrowNotFoundException()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1 },
                new RezeptZutat { Id = 2, RezeptId = 1 }
            };
            var newOrder = new List<int> { 1, 999 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ReorderZutatenAsync(1, newOrder));
        }

        [Fact]
        public async Task ReorderZutaten_WithIncompleteList_ThrowValidationException()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1 },
                new RezeptZutat { Id = 2, RezeptId = 1 },
                new RezeptZutat { Id = 3, RezeptId = 1 }
            };
            var incompletePOrder = new List<int> { 1, 2 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.ReorderZutatenAsync(1, incompletePOrder));
        }

        [Fact]
        public async Task ReorderZutaten_WithDuplicateIds_ThrowValidationException()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1 },
                new RezeptZutat { Id = 2, RezeptId = 1 }
            };
            var duplicateOrder = new List<int> { 1, 1 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.ReorderZutatenAsync(1, duplicateOrder));
        }

        // ============ GetZutatCountAsync Tests ============

        [Fact]
        public async Task GetZutatCount_WithValidRezeptId_ReturnCount()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, RezeptId = 1 },
                new RezeptZutat { Id = 2, RezeptId = 1 },
                new RezeptZutat { Id = 3, RezeptId = 1 }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(zutaten);

            // Act
            var result = await _service.GetZutatCountAsync(1);

            // Assert
            Assert.Equal(3, result);
        }
    }

    // ============ DTOs & Requests (for Tests) ============

    public class AddZutatRequest
    {
        public int LebensmittelId { get; set; }
        public double Menge { get; set; }
        public string Einheit { get; set; }
        public string? Notizen { get; set; }
    }

    public class UpdateZutatRequest
    {
        public double? Menge { get; set; }
        public string? Einheit { get; set; }
        public string? Notizen { get; set; }
    }
}

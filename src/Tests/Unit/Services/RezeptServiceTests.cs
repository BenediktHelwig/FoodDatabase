using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Exceptions;

namespace FoodDatabase.Tests.Unit.Services
{
    /// <summary>
    /// Unit Tests für RezeptService (UC4: Rezepte verwalten).
    /// TDD Red-Phase: Tests sind absichtlich rot, Service wird später implementiert.
    /// </summary>
    public class RezeptServiceTests
    {
        private readonly Mock<IRepository<Rezept>> _mockRepository;
        private readonly Mock<ILebensmittelService> _mockLebensmittelService;
        private readonly IRezeptService _service;

        public RezeptServiceTests()
        {
            _mockRepository = new Mock<IRepository<Rezept>>();
            _mockLebensmittelService = new Mock<ILebensmittelService>();
            _service = new RezeptService(_mockRepository.Object, _mockLebensmittelService.Object);
        }

        // ============ GetRezeptByIdAsync Tests ============

        [Fact]
        public async Task GetRezeptById_WithValidId_ReturnRezept()
        {
            // Arrange
            var rezept = new Rezept
            {
                Id = 1,
                Name = "Spaghetti Carbonara",
                Portionen = 4,
                ZubereitungszeitMinuten = 20,
                Schwierigkeitsgrad = "Medium",
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });

            // Act
            var result = await _service.GetRezeptByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spaghetti Carbonara", result.Name);
            Assert.Equal(4, result.Portionen);
        }

        [Fact]
        public async Task GetRezeptById_WithInvalidId_ReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act
            var result = await _service.GetRezeptByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRezeptById_WithArchivedRezept_ReturnNull()
        {
            // Arrange
            var rezept = new Rezept
            {
                Id = 1,
                Name = "Spaghetti Carbonara",
                IsArchived = true
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });

            // Act
            var result = await _service.GetRezeptByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        // ============ GetAllRezepteAsync Tests ============

        [Fact]
        public async Task GetAllRezepte_ReturnOnlyNonArchivedRezepte()
        {
            // Arrange
            var rezepte = new List<Rezept>
            {
                new Rezept { Id = 1, Name = "Spaghetti", IsArchived = false },
                new Rezept { Id = 2, Name = "Pizza", IsArchived = true },
                new Rezept { Id = 3, Name = "Salat", IsArchived = false }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rezepte);

            // Act
            var result = await _service.GetAllRezepteAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.DoesNotContain(result, r => r.Id == 2);
        }

        [Fact]
        public async Task GetAllRezepte_WithNoRezepte_ReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act
            var result = await _service.GetAllRezepteAsync();

            // Assert
            Assert.Empty(result);
        }

        // ============ SearchRezepteAsync Tests ============

        [Fact]
        public async Task SearchRezepte_WithValidName_ReturnMatchingRezepte()
        {
            // Arrange
            var rezepte = new List<Rezept>
            {
                new Rezept { Id = 1, Name = "Spaghetti Carbonara", IsArchived = false },
                new Rezept { Id = 2, Name = "Spaghetti al Pomodoro", IsArchived = false },
                new Rezept { Id = 3, Name = "Pizza Margherita", IsArchived = false }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rezepte);

            // Act
            var result = await _service.SearchRezepteAsync("Spaghetti");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Contains("Spaghetti", r.Name));
        }

        [Fact]
        public async Task SearchRezepte_WithNoResults_ReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act
            var result = await _service.SearchRezepteAsync("NonExistent");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchRezepte_CaseInsensitive()
        {
            // Arrange
            var rezepte = new List<Rezept>
            {
                new Rezept { Id = 1, Name = "Spaghetti Carbonara", IsArchived = false }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rezepte);

            // Act
            var result = await _service.SearchRezepteAsync("spaghetti");

            // Assert
            Assert.Single(result);
        }

        // ============ CreateRezeptAsync Tests ============

        [Fact]
        public async Task CreateRezept_WithValidData_ReturnRezept()
        {
            // Arrange
            var request = new CreateRezeptRequest
            {
                Name = "Spaghetti Carbonara",
                Portionen = 4,
                ZubereitungszeitMinuten = 20,
                Schwierigkeitsgrad = "Medium",
                Beschreibung = "Italienische Klassiker",
                Zubereitung = "1. Wasser aufkochen..."
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Rezept>())).ReturnsAsync(new Rezept { Id = 1, Name = request.Name });

            // Act
            var result = await _service.CreateRezeptAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spaghetti Carbonara", result.Name);
            Assert.Equal(4, result.Portionen);
        }

        [Fact]
        public async Task CreateRezept_WithDuplicateName_ThrowDuplicateException()
        {
            // Arrange
            var existingRezept = new Rezept { Name = "Spaghetti Carbonara", IsArchived = false };
            var request = new CreateRezeptRequest { Name = "Spaghetti Carbonara", Portionen = 4 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { existingRezept });

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateRezeptException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithNullName_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = null, Portionen = 4 };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithEmptyName_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "", Portionen = 4 };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithNameTooLong_ThrowValidationException()
        {
            // Arrange
            var longName = new string('a', 201);
            var request = new CreateRezeptRequest { Name = longName, Portionen = 4 };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithPortionenLessThanOne_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 0 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithPortionenGreaterThanHundred_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 101 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithNegativeZubereitungszeit_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 4, ZubereitungszeitMinuten = -5 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithZubereitzeitGreaterThanMax_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 4, ZubereitungszeitMinuten = 1441 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithInvalidSchwierigkeitsgrad_ThrowValidationException()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 4, Schwierigkeitsgrad = "Ultrahard" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRezeptAsync(request));
        }

        [Fact]
        public async Task CreateRezept_WithNullOptionalFields_Succeed()
        {
            // Arrange
            var request = new CreateRezeptRequest
            {
                Name = "Test",
                Portionen = 4,
                ZubereitungszeitMinuten = 20,
                Schwierigkeitsgrad = "Easy",
                Beschreibung = null,
                Zubereitung = null
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Rezept>())).ReturnsAsync(new Rezept { Id = 1 });

            // Act
            var result = await _service.CreateRezeptAsync(request);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateRezept_SetsCreatedAtTimestamp()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 4 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());
            var createdRezept = new Rezept { Id = 1, CreatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Rezept>())).ReturnsAsync(createdRezept);

            // Act
            var result = await _service.CreateRezeptAsync(request);

            // Assert
            Assert.NotEqual(default(DateTime), result.CreatedAt);
        }

        [Fact]
        public async Task CreateRezept_SetsIsArchivedToFalse()
        {
            // Arrange
            var request = new CreateRezeptRequest { Name = "Test", Portionen = 4 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Rezept>())).ReturnsAsync(new Rezept { Id = 1, IsArchived = false });

            // Act
            var result = await _service.CreateRezeptAsync(request);

            // Assert
            Assert.False(result.IsArchived);
        }

        // ============ UpdateRezeptAsync Tests ============

        [Fact]
        public async Task UpdateRezept_WithValidData_ReturnUpdatedRezept()
        {
            // Arrange
            var existingRezept = new Rezept { Id = 1, Name = "Spaghetti", Portionen = 4, IsArchived = false };
            var request = new UpdateRezeptRequest { Name = "Spaghetti Carbonara", Portionen = 6 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { existingRezept });
            var updated = new Rezept { Id = 1, Name = request.Name ?? "Spaghetti", Portionen = request.Portionen ?? 4, UpdatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rezept>())).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateRezeptAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spaghetti Carbonara", result.Name);
            Assert.Equal(6, result.Portionen);
        }

        [Fact]
        public async Task UpdateRezept_WithNonExistingRezept_ThrowNotFoundException()
        {
            // Arrange
            var request = new UpdateRezeptRequest { Name = "Test" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateRezeptAsync(999, request));
        }

        [Fact]
        public async Task UpdateRezept_WithArchivedRezept_ThrowInvalidOperationException()
        {
            // Arrange
            var archivedRezept = new Rezept { Id = 1, IsArchived = true };
            var request = new UpdateRezeptRequest { Name = "Test" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { archivedRezept });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateRezeptAsync(1, request));
        }

        [Fact]
        public async Task UpdateRezept_WithDuplicateName_ThrowDuplicateException()
        {
            // Arrange
            var rezept1 = new Rezept { Id = 1, Name = "Spaghetti", IsArchived = false };
            var rezept2 = new Rezept { Id = 2, Name = "Pizza", IsArchived = false };
            var request = new UpdateRezeptRequest { Name = "Pizza" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept1, rezept2 });

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateRezeptException>(() => _service.UpdateRezeptAsync(1, request));
        }

        [Fact]
        public async Task UpdateRezept_UpdatesUpdatedAtTimestamp()
        {
            // Arrange
            var existingRezept = new Rezept { Id = 1, Name = "Test", IsArchived = false };
            var request = new UpdateRezeptRequest { Name = "Updated" };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { existingRezept });
            var updated = new Rezept { Id = 1, UpdatedAt = DateTime.UtcNow };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rezept>())).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateRezeptAsync(1, request);

            // Assert
            Assert.NotEqual(default(DateTime), result.UpdatedAt);
        }

        [Fact]
        public async Task UpdateRezept_WithInvalidPortionen_ThrowValidationException()
        {
            // Arrange
            var existingRezept = new Rezept { Id = 1, IsArchived = false };
            var request = new UpdateRezeptRequest { Portionen = 101 };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { existingRezept });

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateRezeptAsync(1, request));
        }

        // ============ DeleteRezeptAsync Tests ============

        [Fact]
        public async Task DeleteRezept_WithValidId_SetsIsArchivedToTrue()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Name = "Test", IsArchived = false };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Rezept>())).ReturnsAsync(rezept);

            // Act
            await _service.DeleteRezeptAsync(1);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Rezept>(x => x.IsArchived == true)), Times.Once);
        }

        [Fact]
        public async Task DeleteRezept_WithNonExistingRezept_ThrowNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteRezeptAsync(999));
        }

        [Fact]
        public async Task DeleteRezept_RemovedFromGetAllRezepte()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Name = "Test", IsArchived = false };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });
            var archivedRezept = new Rezept { Id = 1, Name = "Test", IsArchived = true };

            // Act
            await _service.DeleteRezeptAsync(1);

            // Setup new mock state for GetAll
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());
            var result = await _service.GetAllRezepteAsync();

            // Assert
            Assert.Empty(result);
        }

        // ============ RezeptExistsAsync Tests ============

        [Fact]
        public async Task RezeptExists_WithExistingNonArchivedRezept_ReturnTrue()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, IsArchived = false };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });

            // Act
            var result = await _service.RezeptExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RezeptExists_WithNonExistingRezept_ReturnFalse()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept>());

            // Act
            var result = await _service.RezeptExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RezeptExists_WithArchivedRezept_ReturnFalse()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, IsArchived = true };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Rezept> { rezept });

            // Act
            var result = await _service.RezeptExistsAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}

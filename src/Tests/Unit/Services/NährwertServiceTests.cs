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
    /// Unit Tests für NährwertService (UC3: Nährwerte verwalten).
    /// TDD Red-Phase: Tests sind absichtlich rot, da der Service noch nicht implementiert ist.
    /// </summary>
    public class NährwertServiceTests
    {
        private readonly Mock<IRepository<Nährwert>> _mockRepository;
        private readonly Mock<IRepository<LebensmittelKatalog>> _mockLebensmittelRepository;
        private readonly INährwertService _service;

        public NährwertServiceTests()
        {
            _mockRepository = new Mock<IRepository<Nährwert>>();
            _mockLebensmittelRepository = new Mock<IRepository<LebensmittelKatalog>>();
            _service = new NährwertService(_mockRepository.Object, _mockLebensmittelRepository.Object);
        }

        // ============ GetNährwertByLebensmittelId Tests ============

        [Fact]
        public async Task GetNährwertByLebensmittelId_WithValidId_ReturnNährwert()
        {
            // Arrange
            var lebensmittelId = 1;
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = lebensmittelId,
                Kalorien = 150,
                Fett = 10.5,
                GesättigteFettsäuren = 3.2,
                Kohlenhydrate = 20.0,
                Zucker = 5.0,
                Protein = 8.0,
                Ballaststoffe = 2.5,
                Salz = 0.5,
                StandardMengeEinheit = "g"
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Nährwert> { nährwert });

            // Act
            var result = await _service.GetNährwertByLebensmittelIdAsync(lebensmittelId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(lebensmittelId, result.LebensmittelId);
            Assert.Equal(150, result.Kalorien);
        }

        [Fact]
        public async Task GetNährwertByLebensmittelId_WithInvalidId_ReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Nährwert>());

            // Act
            var result = await _service.GetNährwertByLebensmittelIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetNährwertByLebensmittelId_WithArchivedNährwert_ReturnNull()
        {
            // Arrange
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 150,
                StandardMengeEinheit = "g",
                IsArchived = true
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Nährwert> { nährwert });

            // Act
            var result = await _service.GetNährwertByLebensmittelIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        // ============ CreateNährwert Tests ============

        [Fact]
        public async Task CreateNährwert_WithValidData_ReturnNährwert()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Apfel", Einheit = "g" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 52,
                Fett = 0.2,
                GesättigteFettsäuren = 0.03,
                Kohlenhydrate = 14.0,
                Zucker = 10.0,
                Protein = 0.26,
                Ballaststoffe = 2.4,
                Salz = 0.0,
                StandardMengeEinheit = "g"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);

            // Act
            var result = await _service.CreateNährwertAsync(nährwert);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(52, result.Kalorien);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Nährwert>()), Times.Once);
        }

        [Fact]
        public async Task CreateNährwert_WithNullLebensmittelId_ThrowException()
        {
            // Arrange
            var nährwert = new Nährwert
            {
                LebensmittelId = 0,
                Kalorien = 150,
                StandardMengeEinheit = "g"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNährwertAsync(nährwert));
        }

        [Fact]
        public async Task CreateNährwert_WithInvalidEinheit_ThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 61,
                Fett = 3.3,
                StandardMengeEinheit = "l" // Ungültig! Nur "g" oder "ml"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNährwertAsync(nährwert));
        }

        [Fact]
        public async Task CreateNährwert_WithNegativeKalorien_ThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Test", Einheit = "g" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = -100, // Ungültig!
                StandardMengeEinheit = "g"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNährwertAsync(nährwert));
        }

        [Fact]
        public async Task CreateNährwert_WithNullEinheit_ThrowException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Test", Einheit = "g" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 150,
                StandardMengeEinheit = null // Ungültig!
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNährwertAsync(nährwert));
        }

        // ============ UpdateNährwert Tests ============

        [Fact]
        public async Task UpdateNährwert_WithValidData_UpdateSuccessful()
        {
            // Arrange
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100,
                Fett = 5.0,
                StandardMengeEinheit = "g"
            };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Nährwert> { nährwert });

            // Act
            nährwert.Kalorien = 150;
            var result = await _service.UpdateNährwertAsync(nährwert);

            // Assert
            Assert.Equal(150, result.Kalorien);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Nährwert>()), Times.Once);
        }

        [Fact]
        public async Task UpdateNährwert_WithInvalidEinheit_ThrowException()
        {
            // Arrange
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "kg" // Ungültig!
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateNährwertAsync(nährwert));
        }

        // ============ DeleteNährwert Tests ============

        [Fact]
        public async Task DeleteNährwert_WithValidId_MarkAsArchived()
        {
            // Arrange
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 150,
                StandardMengeEinheit = "g",
                IsArchived = false
            };
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);

            // Act
            nährwert.IsArchived = true;
            var result = await _service.DeleteNährwertAsync(1);

            // Assert
            Assert.True(result.IsArchived);
        }

        // ============ ValidateStandardMengeEinheit Tests ============

        [Theory]
        [InlineData("g")]
        [InlineData("ml")]
        public async Task ValidateStandardMengeEinheit_WithValidEinheit_ReturnTrue(string einheit)
        {
            // Act
            var result = await _service.ValidateStandardMengeEinheitAsync(einheit);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("kg")]
        [InlineData("l")]
        [InlineData("Portion")]
        [InlineData("")]
        public async Task ValidateStandardMengeEinheit_WithInvalidEinheit_ReturnFalse(string einheit)
        {
            // Act
            var result = await _service.ValidateStandardMengeEinheitAsync(einheit);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateStandardMengeEinheit_WithNull_ReturnFalse()
        {
            // Act
            var result = await _service.ValidateStandardMengeEinheitAsync(null);

            // Assert
            Assert.False(result);
        }

        // ============ GetAllNährwerte Tests ============

        [Fact]
        public async Task GetAllNährwerte_ReturnAllNonArchived()
        {
            // Arrange
            var nährwerte = new List<Nährwert>
            {
                new Nährwert { Id = 1, LebensmittelId = 1, Kalorien = 50, StandardMengeEinheit = "g", IsArchived = false },
                new Nährwert { Id = 2, LebensmittelId = 2, Kalorien = 100, StandardMengeEinheit = "ml", IsArchived = false },
                new Nährwert { Id = 3, LebensmittelId = 3, Kalorien = 200, StandardMengeEinheit = "g", IsArchived = true }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(nährwerte);

            // Act
            var result = await _service.GetAllNährwerteAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.DoesNotContain(result, n => n.IsArchived);
        }

        // ============ Edge Cases & Security Tests ============

        [Fact]
        public async Task CreateNährwert_WithZeroProtein_Success()
        {
            // Arrange - Protein kann 0 sein
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Wasser", Einheit = "ml" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 0,
                Fett = 0.0,
                Protein = 0.0, // OK
                Ballaststoffe = 0.0, // OK
                Salz = 0.0, // OK
                StandardMengeEinheit = "ml"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);

            // Act
            var result = await _service.CreateNährwertAsync(nährwert);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.Protein);
        }

        [Fact]
        public async Task CreateNährwert_WithDecimalValues_Success()
        {
            // Arrange - Dezimalwerte sind erlaubt
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Müsli", Einheit = "g" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 375,
                Fett = 7.5,
                GesättigteFettsäuren = 1.2,
                Kohlenhydrate = 65.3,
                Zucker = 15.8,
                Protein = 9.4,
                Ballaststoffe = 4.2,
                Salz = 0.05,
                StandardMengeEinheit = "g"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);

            // Act
            var result = await _service.CreateNährwertAsync(nährwert);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7.5, result.Fett);
            Assert.Equal(0.05, result.Salz);
        }

        [Fact]
        public async Task CreateNährwert_WithDuplicateLebensmittelId_ThrowException()
        {
            // Arrange - Jedes Lebensmittel kann nur einen Nährwert haben (UNIQUE constraint)
            var existingNährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "g"
            };
            var newNährwert = new Nährwert
            {
                LebensmittelId = 1, // Duplicate!
                Kalorien = 150,
                StandardMengeEinheit = "g"
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Nährwert> { existingNährwert });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateNährwertAsync(newNährwert));
        }

        [Fact]
        public async Task CreateNährwert_WithVeryHighCalories_Success()
        {
            // Arrange - Kalorien können hoch sein (int max)
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Öl", Einheit = "ml" };
            var nährwert = new Nährwert
            {
                LebensmittelId = 1,
                Kalorien = 900, // Sehr hoch, aber gültig
                Fett = 100.0,
                StandardMengeEinheit = "ml"
            };
            _mockLebensmittelRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LebensmittelKatalog> { lebensmittel });
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Nährwert>())).ReturnsAsync(nährwert);

            // Act
            var result = await _service.CreateNährwertAsync(nährwert);

            // Assert
            Assert.Equal(900, result.Kalorien);
        }
    }
}


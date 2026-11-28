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
    public class ProduktInstanzServiceTests
    {
        private const int ArchiviertLagerortId = 4;

        private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
        private readonly Mock<IRepository<Lagerort>> _mockLagerortRepository;
        private readonly ProduktInstanzService _service;

        public ProduktInstanzServiceTests()
        {
            _mockRepository = new Mock<IRepository<ProduktInstanz>>();
            _mockLagerortRepository = new Mock<IRepository<Lagerort>>();
            _mockLagerortRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Lagerort { Id = 1, Name = "Kühlschrank" });
            _mockLagerortRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Lagerort { Id = 2, Name = "Tiefkühler" });
            _mockLagerortRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Lagerort { Id = 3, Name = "Pantry" });
            _mockLagerortRepository.Setup(r => r.GetByIdAsync(ArchiviertLagerortId)).ReturnsAsync(new Lagerort { Id = ArchiviertLagerortId, Name = "Alt", IsArchived = true });
            _service = new ProduktInstanzService(_mockRepository.Object, _mockLagerortRepository.Object);
        }

        // === CREATE Tests (8) ===

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnCreatedProduktInstanz()
        {
            // Arrange
            var produktInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(10),
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(produktInstanz);

            // Act
            var result = await _service.CreateAsync(1, 500, DateTime.Today.AddDays(10), 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.LebensmittelKatalogId);
            Assert.Equal(500, result.Menge);
            Assert.Equal(1, result.LagerortId);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithLebensmittelKatalogIdZero_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(0, 500, DateTime.Today.AddDays(10), 1));
        }

        [Fact]
        public async Task CreateAsync_WithNegativeMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(1, -100, DateTime.Today.AddDays(10), 1));
        }

        [Fact]
        public async Task CreateAsync_WithVerfallsdatumInPast_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(1, 500, DateTime.Today.AddDays(-1), 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999)]
        public async Task CreateAsync_WithInvalidLagerortId_ShouldThrowArgumentException(int lagerortId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(1, 500, DateTime.Today.AddDays(10), lagerortId));
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithArchivedLagerort_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(1, 500, DateTime.Today.AddDays(10), ArchiviertLagerortId));
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Never);
        }

        // === READ Tests (6) ===

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnProduktInstanz()
        {
            // Arrange
            var produktInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(10),
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(produktInstanz);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(500, result.Menge);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ProduktInstanz)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByLebensmittelAsync_WithValidId_ShouldReturnFilteredList()
        {
            // Arrange
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 1, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(5), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 2, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(15), LagerortId = 2 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetByLebensmittelAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.LebensmittelKatalogId));
        }

        [Fact]
        public async Task GetByLagerortAsync_WithValidLagerort_ShouldReturnFilteredList()
        {
            // Arrange
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(5), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(15), LagerortId = 2 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetByLagerortAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.LagerortId));
        }

        [Fact]
        public async Task GetVerfallenenAsync_WithExpiredProducts_ShouldReturnExpiredItems()
        {
            // Arrange
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(-5), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(5), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 200, Verfallsdatum = DateTime.Today, LagerortId = 2 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetVerfallenenAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Id=1 (past) und Id=3 (today)
            Assert.All(result, p => Assert.True(p.Verfallsdatum <= DateTime.Today));
        }

        [Fact]
        public async Task GetNachVerfallsdatumSortiertAsync_ShouldReturnSortedByExpiryAscending()
        {
            // Arrange
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(20), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(5), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 2 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetNachVerfallsdatumSortiertAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(5, (result[0].Verfallsdatum - DateTime.Today).Days);
            Assert.Equal(10, (result[1].Verfallsdatum - DateTime.Today).Days);
            Assert.Equal(20, (result[2].Verfallsdatum - DateTime.Today).Days);
        }

        // === UPDATE Tests (4) ===

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var existingInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(10),
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingInstanz);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(existingInstanz);

            // Act
            await _service.UpdateAsync(1, 600, DateTime.Today.AddDays(15), 2);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNegativeMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(1, -100, DateTime.Today.AddDays(10), 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999)]
        public async Task UpdateAsync_WithInvalidLagerortId_ShouldThrowArgumentException(int lagerortId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(1, 500, DateTime.Today.AddDays(10), lagerortId));
        }

        [Fact]
        public async Task UpdateAsync_WithArchivedLagerort_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(1, 500, DateTime.Today.AddDays(10), ArchiviertLagerortId));
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldSetNewLagerortId()
        {
            // Arrange
            ProduktInstanz existingInstanz = new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 1 };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInstanz);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProduktInstanz>())).ReturnsAsync(existingInstanz);

            // Act
            await _service.UpdateAsync(1, 600, DateTime.Today.AddDays(15), 2);

            // Assert
            Assert.Equal(2, existingInstanz.LagerortId);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ProduktInstanz)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(999, 500, DateTime.Today.AddDays(10), 1));
        }

        // === DELETE Tests (3) ===

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteAsync(1);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(999))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_WithNegativeId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(-1));
        }

        // === BUSINESS LOGIC Tests (6) ===

        [Fact]
        public async Task GetTagesBisVerfallAsync_WithFutureExpiry_ShouldReturnCorrectDays()
        {
            // Arrange
            var daysUntilExpiry = 10;
            var produktInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(daysUntilExpiry),
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(produktInstanz);

            // Act
            var result = await _service.GetTagesBisVerfallAsync(1);

            // Assert
            Assert.Equal(daysUntilExpiry, result);
        }

        [Fact]
        public async Task GetVerfallenenAsync_WithCustomStanddate_ShouldFilterCorrectly()
        {
            // Arrange
            var customDate = DateTime.Today.AddDays(5);
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(3), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(7), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(5), LagerortId = 2 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetVerfallenenAsync(customDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Id=1 (3 days) und Id=3 (5 days)
            Assert.All(result, p => Assert.True(p.Verfallsdatum <= customDate));
        }

        [Fact]
        public async Task GetNachVerfallsdatumSortiertAsync_WithMultipleItems_ShouldMaintainFIFOOrder()
        {
            // Arrange - FIFO bedeutet, älteste zuerst (ascending order)
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(50), LagerortId = 1 },
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 1, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(30), LagerortId = 1 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetNachVerfallsdatumSortiertAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id); // 10 days (älteste zuerst)
            Assert.Equal(2, result[1].Id); // 30 days
            Assert.Equal(3, result[2].Id); // 50 days (neueste zuletzt)
        }

        [Fact]
        public async Task CreateAsync_WithZeroMenge_ShouldSucceed()
        {
            // Arrange - Menge=0 ist erlaubt (nicht negativ)
            var produktInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                Menge = 0,
                Verfallsdatum = DateTime.Today.AddDays(10),
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(produktInstanz);

            // Act
            var result = await _service.CreateAsync(1, 0, DateTime.Today.AddDays(10), 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Menge);
        }

        [Fact]
        public async Task GetByLebensmittelAsync_WithMultipleInstances_ShouldReturnAll()
        {
            // Arrange
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 5, Menge = 100, Verfallsdatum = DateTime.Today.AddDays(10), LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 5, Menge = 200, Verfallsdatum = DateTime.Today.AddDays(20), LagerortId = 1 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 5, Menge = 150, Verfallsdatum = DateTime.Today.AddDays(15), LagerortId = 2 },
                new ProduktInstanz { Id = 4, LebensmittelKatalogId = 6, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(25), LagerortId = 1 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetByLebensmittelAsync(5);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, p => Assert.Equal(5, p.LebensmittelKatalogId));
        }

        [Fact]
        public async Task GetVerfallenenAsync_WithVerfallsdatumToday_ShouldIncludeItem()
        {
            // Arrange - Verfallsdatum = heute sollte als abgelaufen zählen
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, Verfallsdatum = DateTime.Today, LagerortId = 1 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, Verfallsdatum = DateTime.Today.AddDays(1), LagerortId = 1 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.GetVerfallenenAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        // === VALIDATION Tests (Edge Cases) ===

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task CreateAsync_WithAllExistingLagerorte_ShouldSucceed(int lagerortId)
        {
            // Arrange
            ProduktInstanz gespeichert = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .Callback<ProduktInstanz>(p => gespeichert = p)
                .ReturnsAsync((ProduktInstanz p) => p);

            // Act
            var result = await _service.CreateAsync(1, 500, DateTime.Today.AddDays(10), lagerortId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(lagerortId, result.LagerortId);
            Assert.Equal(lagerortId, gespeichert.LagerortId);
        }

        [Fact]
        public async Task CreateAsync_WithVerfallsdatumToday_ShouldSucceed()
        {
            // Arrange - Verfallsdatum = heute ist erlaubt (nicht in der Vergangenheit)
            var produktInstanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today,
                LagerortId = 1
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(produktInstanz);

            // Act
            var result = await _service.CreateAsync(1, 500, DateTime.Today, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DateTime.Today, result.Verfallsdatum);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999)]
        public async Task GetByLagerortAsync_WithInvalidLagerortId_ShouldThrowArgumentException(int lagerortId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByLagerortAsync(lagerortId));
        }

        [Fact]
        public async Task GetByLagerortAsync_WithArchivedLagerort_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByLagerortAsync(ArchiviertLagerortId));
        }

        [Fact]
        public async Task GetTagesBisVerfallAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ProduktInstanz)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetTagesBisVerfallAsync(999));
        }
    }
}

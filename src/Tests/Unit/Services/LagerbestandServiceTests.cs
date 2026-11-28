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
    public class LagerbestandServiceTests
    {
        private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
        private readonly LagerbestandService _service;

        public LagerbestandServiceTests()
        {
            _mockRepository = new Mock<IRepository<ProduktInstanz>>();
            _service = new LagerbestandService(_mockRepository.Object);
        }

        // === GetGesamtmengeAsync Tests (4) ===

        [Fact]
        public async Task GetGesamtmengeAsync_WithValidLebensmittelId_ShouldSumAllMengen()
        {
            // Arrange
            var lebensmittelId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = lebensmittelId, Menge = 500, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = lebensmittelId, Menge = 300, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = lebensmittelId, Menge = 200, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetGesamtmengeAsync(lebensmittelId);

            // Assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public async Task GetGesamtmengeAsync_WithNoInstanzen_ShouldReturnZero()
        {
            // Arrange
            var lebensmittelId = 999;
            var instanzen = new List<ProduktInstanz>();

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetGesamtmengeAsync(lebensmittelId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetGesamtmengeAsync_WithDifferentLebensmittelIds_ShouldFilterCorrectly()
        {
            // Arrange
            var lebensmittelId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, Menge = 200, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetGesamtmengeAsync(lebensmittelId);

            // Assert
            Assert.Equal(700, result);
        }

        [Fact]
        public async Task GetGesamtmengeAsync_WithNullResult_ShouldReturnZero()
        {
            // Arrange
            var lebensmittelId = 1;
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync((List<ProduktInstanz>)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _service.GetGesamtmengeAsync(lebensmittelId));
        }

        // === CheckMindestbestandUnterschrittenAsync Tests (4) ===

        [Fact]
        public async Task CheckMindestbestandUnterschrittenAsync_WhenUnderMindestand_ShouldReturnTrue()
        {
            // Arrange
            var lebensmittelId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = lebensmittelId, Menge = 50, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = lebensmittelId, Menge = 30, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.CheckMindestbestandUnterschrittenAsync(lebensmittelId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckMindestbestandUnterschrittenAsync_WhenAboveMindestand_ShouldReturnFalse()
        {
            // Arrange
            var lebensmittelId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = lebensmittelId, Menge = 150, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = lebensmittelId, Menge = 100, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.CheckMindestbestandUnterschrittenAsync(lebensmittelId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckMindestbestandUnterschrittenAsync_WhenEqualMindestand_ShouldReturnFalse()
        {
            // Arrange
            var lebensmittelId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = lebensmittelId, Menge = 100, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.CheckMindestbestandUnterschrittenAsync(lebensmittelId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckMindestbestandUnterschrittenAsync_WithNoInstanzen_ShouldReturnTrue()
        {
            // Arrange
            var lebensmittelId = 999;
            var instanzen = new List<ProduktInstanz>();

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.CheckMindestbestandUnterschrittenAsync(lebensmittelId);

            // Assert
            Assert.True(result);
        }

        // === GetEinkaufslistenEintraegeAsync Tests (3) ===

        [Fact]
        public async Task GetEinkaufslistenEintraegeAsync_WithUnterschrittenBestaende_ShouldReturnList()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 50, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 75, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslistenEintraegeAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.LebensmittelKatalogId == 1);
            Assert.Contains(result, x => x.LebensmittelKatalogId == 3);
        }

        [Fact]
        public async Task GetEinkaufslistenEintraegeAsync_WithNoUnterschrittenBestaende_ShouldReturnEmpty()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 150, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslistenEintraegeAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslistenEintraegeAsync_WithEmptyRepository_ShouldReturnEmpty()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProduktInstanz>());

            // Act
            var result = await _service.GetEinkaufslistenEintraegeAsync();

            // Assert
            Assert.Empty(result);
        }

        // === GetBestaendePorLagerortAsync Tests (3) ===

        [Fact]
        public async Task GetBestaendePorLagerortAsync_WithValidLagerort_ShouldReturnList()
        {
            // Arrange
            int lagerortId = 1;
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, LagerortId = 1, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, Menge = 300, LagerortId = 2, MindestbestandMenge = 100 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 3, Menge = 200, LagerortId = 1, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetBestaendePorLagerortAsync(lagerortId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(lagerortId, x.LagerortId));
        }

        [Fact]
        public async Task GetBestaendePorLagerortAsync_WithEmptyLagerort_ShouldReturnEmpty()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, Menge = 500, LagerortId = 2, MindestbestandMenge = 100 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetBestaendePorLagerortAsync(1);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetBestaendePorLagerortAsync_WithInvalidLagerortId_ShouldThrowArgumentException(int lagerortId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ProduktInstanz>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetBestaendePorLagerortAsync(lagerortId));
        }

        // === AddToBestandAsync Tests (6) ===

        [Fact]
        public async Task AddToBestandAsync_WithValidData_ShouldReturnProduktInstanz()
        {
            // Arrange
            var lebensmittelId = 1;
            var menge = 500;
            var mindestbestandMenge = 100;
            var verfallsdatum = DateTime.Today.AddDays(10);
            int lagerortId = 1;

            var newInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = lebensmittelId,
                Menge = menge,
                MindestbestandMenge = mindestbestandMenge,
                Verfallsdatum = verfallsdatum,
                LagerortId = lagerortId
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(newInstanz);

            // Act
            var result = await _service.AddToBestandAsync(lebensmittelId, menge, mindestbestandMenge, verfallsdatum, lagerortId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(lebensmittelId, result.LebensmittelKatalogId);
            Assert.Equal(menge, result.Menge);
            Assert.Equal(mindestbestandMenge, result.MindestbestandMenge);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task AddToBestandAsync_WithNegativeMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddToBestandAsync(1, -100, 100, DateTime.Today.AddDays(10), 1));
        }

        [Fact]
        public async Task AddToBestandAsync_WithZeroMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddToBestandAsync(1, 0, 100, DateTime.Today.AddDays(10), 1));
        }

        [Fact]
        public async Task AddToBestandAsync_WithPastVerfallsdatum_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddToBestandAsync(1, 100, 100, DateTime.Today.AddDays(-1), 1));
        }

        [Fact]
        public async Task AddToBestandAsync_WithValidData_ShouldSetLagerortId()
        {
            // Arrange
            ProduktInstanz gespeichert = null;
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .Callback<ProduktInstanz>(p => gespeichert = p)
                .ReturnsAsync((ProduktInstanz p) => p);

            // Act
            await _service.AddToBestandAsync(1, 500, 100, DateTime.Today.AddDays(10), 7);

            // Assert
            Assert.NotNull(gespeichert);
            Assert.Equal(7, gespeichert.LagerortId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddToBestandAsync_WithInvalidLagerortId_ShouldThrowArgumentException(int lagerortId)
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddToBestandAsync(1, 500, 100, DateTime.Today.AddDays(10), lagerortId));
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Never);
        }

        // === UpdateMengeAsync Tests (4) ===

        [Fact]
        public async Task UpdateMengeAsync_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var id = 1;
            var newMenge = 300;
            var existingInstanz = new ProduktInstanz { Id = id, Menge = 500, MindestbestandMenge = 100 };
            var updatedInstanz = new ProduktInstanz { Id = id, Menge = newMenge, MindestbestandMenge = 100 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existingInstanz);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(updatedInstanz);

            // Act
            var result = await _service.UpdateMengeAsync(id, newMenge);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newMenge, result.Menge);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMengeAsync_WithNegativeMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateMengeAsync(1, -100));
        }

        [Fact]
        public async Task UpdateMengeAsync_WithZeroMenge_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateMengeAsync(1, 0));
        }

        [Fact]
        public async Task UpdateMengeAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ProduktInstanz)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.UpdateMengeAsync(999, 100));
        }

        // === RemoveFromBestandAsync Tests (3) ===

        [Fact]
        public async Task RemoveFromBestandAsync_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var id = 1;
            var instanz = new ProduktInstanz { Id = id, Menge = 500, MindestbestandMenge = 100 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(instanz);
            _mockRepository.Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);

            // Act
            await _service.RemoveFromBestandAsync(id);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task RemoveFromBestandAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ProduktInstanz)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.RemoveFromBestandAsync(999));
        }

        [Fact]
        public async Task RemoveFromBestandAsync_WithValidId_ShouldCallRepositoryDelete()
        {
            // Arrange
            var id = 1;
            var instanz = new ProduktInstanz { Id = id, Menge = 500 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(instanz);
            _mockRepository.Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);

            // Act
            await _service.RemoveFromBestandAsync(id);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        // === ValidateBestandAsync Tests (4) ===

        [Fact]
        public async Task ValidateBestandAsync_WithValidMenge_ShouldReturnTrue()
        {
            // Arrange
            var id = 1;
            var menge = 100;
            var instanz = new ProduktInstanz { Id = id, Menge = 500 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(instanz);

            // Act
            var result = await _service.ValidateBestandAsync(id, menge);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateBestandAsync_WithInsufficientMenge_ShouldReturnFalse()
        {
            // Arrange
            var id = 1;
            var requestedMenge = 600;
            var instanz = new ProduktInstanz { Id = id, Menge = 500 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(instanz);

            // Act
            var result = await _service.ValidateBestandAsync(id, requestedMenge);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateBestandAsync_WithEqualMenge_ShouldReturnTrue()
        {
            // Arrange
            var id = 1;
            var menge = 500;
            var instanz = new ProduktInstanz { Id = id, Menge = 500 };

            _mockRepository.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(instanz);

            // Act
            var result = await _service.ValidateBestandAsync(id, menge);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateBestandAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ProduktInstanz)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.ValidateBestandAsync(999, 100));
        }
    }
}

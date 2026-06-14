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
        private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
        private readonly Mock<IRepository<LebensmittelKatalog>> _mockLebensmittelRepository;
        private readonly Mock<IRepository<Lagerort>> _mockLagerortRepository;
        private readonly IProduktInstanzService _service;

        public ProduktInstanzServiceTests()
        {
            _mockRepository = new Mock<IRepository<ProduktInstanz>>();
            _mockLebensmittelRepository = new Mock<IRepository<LebensmittelKatalog>>();
            _mockLagerortRepository = new Mock<IRepository<Lagerort>>();
            _service = new ProduktInstanzService(
                _mockRepository.Object,
                _mockLebensmittelRepository.Object,
                _mockLagerortRepository.Object
            );
        }

        // === CREATE Tests ===

        [Fact]
        public async Task CreateProduktInstanz_WithValidData_ShouldReturnCreatedInstance()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(instanz with { Id = 1 });

            // Act
            var result = await _service.CreateProduktInstanzAsync(instanz);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(500, result.AktuelleM enge);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task CreateProduktInstanz_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateProduktInstanzAsync(null));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithInvalidLebensmittelId_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 0,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithInvalidLagerortId_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 0,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithNegativeAktuelleM enge_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = -100,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithNegativeMindestbestandMenge_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = -50,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithVerfallsdatumInPast_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(-1),
                Einkaufsdatum = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task CreateProduktInstanz_WithEinkaufsdatumInFuture_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow.AddDays(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduktInstanzAsync(instanz));
        }

        // === READ Tests ===

        [Fact]
        public async Task GetProduktInstanzById_WithValidId_ShouldReturnProduktInstanz()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(instanz);

            // Act
            var result = await _service.GetProduktInstanzByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetProduktInstanzById_WithInvalidId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetProduktInstanzByIdAsync(0));
        }

        [Fact]
        public async Task GetProduktInstanzById_WithNegativeId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetProduktInstanzByIdAsync(-1));
        }

        [Fact]
        public async Task GetAllProduktInstanzen_ShouldReturnAllInstances()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, LagerortId = 1, AktuelleM enge = 500 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, LagerortId = 1, AktuelleM enge = 300 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, LagerortId = 2, AktuelleM enge = 200 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetAllProduktInstanzenAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProduktInstanzenByLebensmittelId_WithValidId_ShouldReturnMatchingInstances()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, LagerortId = 1, AktuelleM enge = 500 },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, LagerortId = 2, AktuelleM enge = 200 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen.Concat(new List<ProduktInstanz>
                {
                    new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, LagerortId = 1, AktuelleM enge = 300 }
                }));

            // Act
            var result = await _service.GetProduktInstanzenByLebensmittelIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.Equal(1, i.LebensmittelKatalogId));
        }

        [Fact]
        public async Task GetProduktInstanzenByLagerortId_WithValidId_ShouldReturnMatchingInstances()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, LagerortId = 1, AktuelleM enge = 500 },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, LagerortId = 1, AktuelleM enge = 300 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen.Concat(new List<ProduktInstanz>
                {
                    new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, LagerortId = 2, AktuelleM enge = 200 }
                }));

            // Act
            var result = await _service.GetProduktInstanzenByLagerortIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.Equal(1, i.LagerortId));
        }

        [Fact]
        public async Task GetAbgelaufeneProduktInstanzen_ShouldReturnExpiredInstances()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, LagerortId = 1, Verfallsdatum = DateTime.UtcNow.AddDays(-1) },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, LagerortId = 1, Verfallsdatum = DateTime.UtcNow.AddDays(5) },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, LagerortId = 2, Verfallsdatum = DateTime.UtcNow.AddDays(-5) }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetAbgelaufeneProduktInstanzenAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.True(i.Verfallsdatum < DateTime.UtcNow));
        }

        [Fact]
        public async Task GetProduktInstanzenBaldAbgelaufen_WithinThreeDays_ShouldReturnWarningInstances()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = 1, LagerortId = 1, Verfallsdatum = DateTime.UtcNow.AddDays(2) },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = 2, LagerortId = 1, Verfallsdatum = DateTime.UtcNow.AddDays(10) },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = 1, LagerortId = 2, Verfallsdatum = DateTime.UtcNow.AddDays(-1) }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetProduktInstanzenBaldAbgelaufen_Async(3);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        // === UPDATE Tests ===

        [Fact]
        public async Task UpdateProduktInstanz_WithValidData_ShouldReturnUpdatedInstance()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 300,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(instanz);

            // Act
            var result = await _service.UpdateProduktInstanzAsync(instanz);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.AktuelleM enge);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProduktInstanz_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateProduktInstanzAsync(null));
        }

        [Fact]
        public async Task UpdateProduktInstanz_WithInvalidId_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 0,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateProduktInstanzAsync(instanz));
        }

        [Fact]
        public async Task ReduceProduktInstanzMenge_WithValidData_ShouldReduceMenge()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 500,
                MindestbestandMenge = 200,
                Verfallsdatum = DateTime.UtcNow.AddDays(30),
                Einkaufsdatum = DateTime.UtcNow
            };

            var updatedInstanz = instanz with { AktuelleM enge = 250 };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(instanz);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProduktInstanz>()))
                .ReturnsAsync(updatedInstanz);

            // Act
            var result = await _service.ReduceProduktInstanzMengeAsync(1, 250);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(250, result.AktuelleM enge);
        }

        [Fact]
        public async Task ReduceProduktInstanzMenge_WithMoreThanAvailable_ShouldThrowArgumentException()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 100,
                MindestbestandMenge = 50
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(instanz);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ReduceProduktInstanzMengeAsync(1, 200));
        }

        // === DELETE Tests ===

        [Fact]
        public async Task DeleteProduktInstanz_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteProduktInstanzAsync(1);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteProduktInstanz_WithInvalidId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteProduktInstanzAsync(0));
        }

        [Fact]
        public async Task DeleteProduktInstanz_WithNegativeId_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteProduktInstanzAsync(-5));
        }

        // === BUSINESS LOGIC Tests ===

        [Fact]
        public async Task IsUnterMindestbestand_WithMengeBelowThreshold_ShouldReturnTrue()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 100,
                MindestbestandMenge = 200
            };

            // Act
            var result = _service.IsUnterMindestbestand(instanz);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUnterMindestbestand_WithMengeAboveThreshold_ShouldReturnFalse()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                AktuelleM enge = 300,
                MindestbestandMenge = 200
            };

            // Act
            var result = _service.IsUnterMindestbestand(instanz);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAbgelaufen_WithDateInPast_ShouldReturnTrue()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                Verfallsdatum = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = _service.IsAbgelaufen(instanz);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAbgelaufen_WithDateInFuture_ShouldReturnFalse()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                Verfallsdatum = DateTime.UtcNow.AddDays(10)
            };

            // Act
            var result = _service.IsAbgelaufen(instanz);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CalculateTagesBisAblauf_ShouldReturnDaysRemaining()
        {
            // Arrange
            var instanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                LagerortId = 1,
                Verfallsdatum = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var result = _service.CalculateTagesBisAblauf(instanz);

            // Assert
            Assert.InRange(result, 6, 7); // Allow for rounding
        }
    }
}

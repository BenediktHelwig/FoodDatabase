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
    public class VerbrauchAusbuchangServiceTests
    {
        private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
        private readonly VerbrauchAusbuchangService _service;

        public VerbrauchAusbuchangServiceTests()
        {
            _mockRepository = new Mock<IRepository<ProduktInstanz>>();
            _service = new VerbrauchAusbuchangService(_mockRepository.Object);
        }

        // === HAPPY PATH: Verbrauch erfolgreich ===

        [Fact]
        public async Task VerbauchtProduktAsync_WithValidLebensmittelId_ShouldDeleteOldestProdukt()
        {
            // Arrange
            var lebensmittelId = 1;
            var today = DateTime.Today;

            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 1, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(10), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 2, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(5), Lagerort = LagerortKonstanten.Pantry },
                new ProduktInstanz { Id = 3, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(20), Lagerort = LagerortKonstanten.Kühlschrank }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Produkt erfolgreich verbraucht", result.Message);
            _mockRepository.Verify(r => r.DeleteAsync(2), Times.Once); // ID 2 hat ältestes MHD
        }

        [Fact]
        public async Task VerbauchtProduktAsync_WithMultipleProdukte_ShouldDeleteOldestByVerfallsdatum()
        {
            // Arrange
            var lebensmittelId = 5;
            var today = DateTime.Today;

            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 10, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(30), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 11, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(1), Lagerort = LagerortKonstanten.Pantry }, // Ältestes
                new ProduktInstanz { Id = 12, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(15), Lagerort = LagerortKonstanten.Tiefkühler }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.DeleteAsync(11), Times.Once); // ID 11 ältestes (1 Tag)
        }

        [Fact]
        public async Task VerbauchtProduktAsync_WithSingleProdukt_ShouldDeleteIt()
        {
            // Arrange
            var lebensmittelId = 7;
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 99, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = DateTime.Today.AddDays(10), Lagerort = LagerortKonstanten.Kühlschrank }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(99))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.DeleteAsync(99), Times.Once);
        }

        // === ERROR CASES ===

        [Fact]
        public async Task VerbauchtProduktAsync_WithNoProduktAvailable_ShouldReturnFailure()
        {
            // Arrange
            var lebensmittelId = 999;
            var produktInstanzen = new List<ProduktInstanz>(); // Empty

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("nicht vorhanden", result.Message);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task VerbauchtProduktAsync_WithLebensmittelIdZero_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.VerbauchtProduktAsync(0));
        }

        [Fact]
        public async Task VerbauchtProduktAsync_WithNegativeLebensmittelId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.VerbauchtProduktAsync(-5));
        }

        // === FIFO-Ordering Tests ===

        [Fact]
        public async Task VerbauchtProduktAsync_ShouldRespectFIFOOrderingByVerfallsdatum()
        {
            // Arrange: Same Lebensmittel, verschiedene Verfallsdaten
            var lebensmittelId = 10;
            var today = DateTime.Today;

            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 101, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(100), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 102, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(3), Lagerort = LagerortKonstanten.Kühlschrank },   // Ältestes
                new ProduktInstanz { Id = 103, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(50), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 104, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(25), Lagerort = LagerortKonstanten.Kühlschrank }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.DeleteAsync(102), Times.Once); // 102 ältestes (3 Tage)
        }

        [Fact]
        public async Task VerbauchtProduktAsync_ShouldIgnoreDifferentLebensmittel()
        {
            // Arrange: Mix von Lebensmittel-IDs
            var targetLebensmittelId = 20;
            var today = DateTime.Today;

            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 200, LebensmittelKatalogId = 99, Menge = 1000, Verfallsdatum = today.AddDays(1), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 201, LebensmittelKatalogId = targetLebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(10), Lagerort = LagerortKonstanten.Kühlschrank },
                new ProduktInstanz { Id = 202, LebensmittelKatalogId = 88, Menge = 1000, Verfallsdatum = today.AddDays(5), Lagerort = LagerortKonstanten.Kühlschrank }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(targetLebensmittelId);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.DeleteAsync(201), Times.Once); // Nur ID 201 für Lebensmittel 20
        }

        // === Edge Cases ===

        [Fact]
        public async Task VerbauchtProduktAsync_WithDeleteRepositoryFailure_ShouldReturnFailure()
        {
            // Arrange
            var lebensmittelId = 30;
            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 300, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = DateTime.Today.AddDays(10), Lagerort = LagerortKonstanten.Kühlschrank }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(false); // Delete failed

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Fehler", result.Message);
        }

        [Fact]
        public async Task VerbauchtProduktAsync_WithExpiredProdukt_ShouldStillDelete()
        {
            // Arrange: Produkt ist bereits abgelaufen, wird trotzdem gelöscht (Cleanup)
            var lebensmittelId = 40;
            var today = DateTime.Today;

            var produktInstanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz { Id = 400, LebensmittelKatalogId = lebensmittelId, Menge = 1000, Verfallsdatum = today.AddDays(-5), Lagerort = LagerortKonstanten.Kühlschrank } // Verfallen
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(produktInstanzen);

            _mockRepository.Setup(r => r.DeleteAsync(400))
                .ReturnsAsync(true);

            // Act
            var result = await _service.VerbauchtProduktAsync(lebensmittelId);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.DeleteAsync(400), Times.Once);
        }
    }
}

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
    /// Unit Tests für NährwertCalculator (UC4 + UC5: Nährwertberechnung für Rezepte).
    /// TDD Red-Phase: Tests sind absichtlich rot, Calculator wird später implementiert.
    /// </summary>
    public class NährwertCalculatorTests
    {
        private readonly Mock<IRezeptService> _mockRezeptService;
        private readonly Mock<IRezeptZutatService> _mockRezeptZutatService;
        private readonly Mock<INährwertService> _mockNährwertService;
        private readonly Mock<IEinheitConverter> _mockEinheitConverter;
        private readonly INährwertCalculator _calculator;

        public NährwertCalculatorTests()
        {
            _mockRezeptService = new Mock<IRezeptService>();
            _mockRezeptZutatService = new Mock<IRezeptZutatService>();
            _mockNährwertService = new Mock<INährwertService>();
            _mockEinheitConverter = new Mock<IEinheitConverter>();
            _calculator = new NährwertCalculator(
                _mockRezeptService.Object,
                _mockRezeptZutatService.Object,
                _mockNährwertService.Object,
                _mockEinheitConverter.Object
            );
        }

        // ============ CalculateRezeptNährwerteAsync Tests ============

        [Fact]
        public async Task CalculateRezeptNährwerte_WithValidRezept_ReturnCompleteNährwerte()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Name = "Spaghetti Carbonara", Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 400, Einheit = "g" }
            };
            var nährwert = new Nährwert
            {
                Kalorien = 150,
                Fett = 10.5,
                Kohlenhydrate = 20.0,
                Protein = 8.0
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(400, "g")).Returns(400);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.RezeptId);
            Assert.NotNull(result.GesamtnährwerteDto);
            Assert.NotNull(result.ProPortionNährwerteDto);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithNonExistingRezept_ThrowNotFoundException()
        {
            // Arrange
            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(999)).ReturnsAsync((Rezept?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _calculator.CalculateRezeptNährwerteAsync(999));
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithSingleZutat_CalculateCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 150, Fett = 10.0 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.GesamtnährwerteDto.Kalorien); // 150 * (100/100) = 150
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithMultipleZutaten_SumCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 200, Einheit = "g" },
                new RezeptZutat { Id = 2, LebensmittelId = 2, Menge = 100, Einheit = "g" }
            };
            var nährwert1 = new Nährwert { Kalorien = 150 };
            var nährwert2 = new Nährwert { Kalorien = 200 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert1);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(2)).ReturnsAsync(nährwert2);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(200, "g")).Returns(200);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // (150 * 2) + (200 * 1) = 300 + 200 = 500
            Assert.Equal(500, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithZutatWithoutNährwert_IgnoreThatZutat()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" },
                new RezeptZutat { Id = 2, LebensmittelId = 2, Menge = 100, Einheit = "g" }
            };
            var nährwert1 = new Nährwert { Kalorien = 150 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert1);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(2)).ReturnsAsync((Nährwert?)null);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.GesamtnährwerteDto.Kalorien); // Only Zutat 1
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithAllZutatenWithoutNährwert_ReturnZeroNährwerte()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync((Nährwert?)null);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_ProPortionCalculation()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 400, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 300 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(400, "g")).Returns(400);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // Nährwert 300 kcal/100g, 400g Menge = (300 * 4) / 4 Portionen = 300 pro Portion
            Assert.Equal(300, result.ProPortionNährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithDecimalValues_RoundCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 3 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" }
            };
            var nährwert = new Nährwert { Fett = 10.5 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 10.5 / 3 = 3.5
            Assert.Equal(3.5, result.ProPortionNährwerteDto.Fett, 1);
        }

        // ============ CalculateProPortionAsync Tests ============

        [Fact]
        public async Task CalculateProPortion_WithValidRezept_ReturnDividedNährwerte()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 4 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 400, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 300 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(400, "g")).Returns(400);

            // Act
            var result = await _calculator.CalculateProPortionAsync(1);

            // Assert
            Assert.NotNull(result);
            // Nährwert 300 kcal/100g, 400g Menge = (300 * 4) / 4 Portionen = 300 pro Portion
            Assert.Equal(300, result.Kalorien);
        }

        [Fact]
        public async Task CalculateProPortion_WithLargeRecipe_HandleCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 10 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 1000, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 300 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(1000, "g")).Returns(1000);

            // Act
            var result = await _calculator.CalculateProPortionAsync(1);

            // Assert
            Assert.NotNull(result);
            // Nährwert 300 kcal/100g, 1000g Menge = (300 * 10) / 10 Portionen = 300 pro Portion
            Assert.Equal(300, result.Kalorien);
        }

        // ============ Edge Case Tests ============

        [Fact]
        public async Task CalculateRezeptNährwerte_WithMinimumMenge_CalculateCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 1 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 0.01, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 100 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(0.01, "g")).Returns(0.01);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 100 * (0.01 / 100) = 0.01
            Assert.True(result.GesamtnährwerteDto.Kalorien >= 0);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithMaximumMenge_CalculateCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 1 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 10000, Einheit = "g" }
            };
            var nährwert = new Nährwert { Kalorien = 100 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(10000, "g")).Returns(10000);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 100 * (10000 / 100) = 10000
            Assert.Equal(10000, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithTeaspoonEinheit_ConvertCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 1 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 1, Einheit = "TL" }
            };
            var nährwert = new Nährwert { Kalorien = 100 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(1, "TL")).Returns(5);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 100 * (5 / 100) = 5
            Assert.Equal(5, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithTablespoonEinheit_ConvertCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 1 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 1, Einheit = "EL" }
            };
            var nährwert = new Nährwert { Kalorien = 100 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(1, "EL")).Returns(15);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 100 * (15 / 100) = 15
            Assert.Equal(15, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateRezeptNährwerte_WithDecimalNährwerte_HandleCorrectly()
        {
            // Arrange
            var rezept = new Rezept { Id = 1, Portionen = 1 };
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" }
            };
            var nährwert = new Nährwert { Fett = 10.567 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(1)).ReturnsAsync(rezept);
            _mockRezeptZutatService.Setup(r => r.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            _mockNährwertService.Setup(n => n.GetNährwertByLebensmittelIdAsync(1)).ReturnsAsync(nährwert);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _calculator.CalculateRezeptNährwerteAsync(1);

            // Assert
            Assert.NotNull(result);
            // 10.567 * (100 / 100) = 10.567, rounded to 10.6
            Assert.True(result.GesamtnährwerteDto.Fett >= 10.5);
        }
    }
}

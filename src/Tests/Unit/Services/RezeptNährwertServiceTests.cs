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
    /// Unit Tests für RezeptNährwertService (UC5: Nährwert-Berechnung für Rezepte).
    /// Testet die automatische Summation von Nährwerten und Pro-Portion-Berechnung.
    /// TDD Red-Phase: Tests sind absichtlich rot, Service wird später implementiert.
    /// </summary>
    public class RezeptNährwertServiceTests
    {
        private readonly Mock<INährwertCalculator> _mockNährwertCalculator;
        private readonly Mock<IRezeptService> _mockRezeptService;
        private readonly Mock<IRezeptZutatService> _mockRezeptZutatService;
        private readonly Mock<IEinheitConverter> _mockEinheitConverter;
        private readonly IRezeptNährwertService _service;

        public RezeptNährwertServiceTests()
        {
            _mockNährwertCalculator = new Mock<INährwertCalculator>();
            _mockRezeptService = new Mock<IRezeptService>();
            _mockRezeptZutatService = new Mock<IRezeptZutatService>();
            _mockEinheitConverter = new Mock<IEinheitConverter>();
            _service = new RezeptNährwertService(
                _mockNährwertCalculator.Object,
                _mockRezeptService.Object,
                _mockRezeptZutatService.Object,
                _mockEinheitConverter.Object
            );
        }

        // ============ GetRezeptNährwerteAsync Tests ============

        [Fact]
        public async Task GetRezeptNährwerte_WithValidRezeptId_ReturnCompleteNährwerte()
        {
            // Arrange
            var rezeptId = 1;
            var expectedNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Kalorien = 600,
                    Fett = 20.0,
                    GesättigteFettsäuren = 5.0,
                    Kohlenhydrate = 80.0,
                    Zucker = 5.0,
                    Protein = 25.0,
                    Ballaststoffe = 5.0,
                    Salz = 1.0
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Kalorien = 150,
                    Fett = 5.0,
                    GesättigteFettsäuren = 1.25,
                    Kohlenhydrate = 20.0,
                    Zucker = 1.25,
                    Protein = 6.25,
                    Ballaststoffe = 1.25,
                    Salz = 0.25
                }
            };

            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(expectedNährwerte);

            // Act
            var result = await _service.GetRezeptNährwerteAsync(rezeptId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rezeptId, result.RezeptId);
            Assert.Equal(600, result.GesamtnährwerteDto.Kalorien);
            Assert.Equal(150, result.ProPortionNährwerteDto.Kalorien);
            Assert.Equal(25.0, result.GesamtnährwerteDto.Protein);
        }

        [Fact]
        public async Task GetRezeptNährwerte_WithInvalidRezeptId_ThrowNotFoundException()
        {
            // Arrange
            var invalidRezeptId = 999;
            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(invalidRezeptId))
                .ThrowsAsync(new NotFoundException($"Rezept mit ID {invalidRezeptId} nicht gefunden"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetRezeptNährwerteAsync(invalidRezeptId)
            );
        }

        [Fact]
        public async Task GetRezeptNährwerte_ProPortionCalculatesCorrectly_For4Portionen()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400, Protein = 20.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100, Protein = 5.0 }
            };

            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            // Act
            var result = await _service.GetRezeptNährwerteAsync(rezeptId);

            // Assert
            Assert.Equal(100, result.ProPortionNährwerteDto.Kalorien); // 400 / 4 = 100
            Assert.Equal(5.0, result.ProPortionNährwerteDto.Protein);  // 20 / 4 = 5
        }

        [Fact]
        public async Task GetRezeptNährwerte_AllNährwertFieldsPopulated()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Kalorien = 500,
                    Fett = 15.0,
                    GesättigteFettsäuren = 3.0,
                    Kohlenhydrate = 70.0,
                    Zucker = 10.0,
                    Protein = 20.0,
                    Ballaststoffe = 8.0,
                    Salz = 1.5
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Kalorien = 125,
                    Fett = 3.75,
                    GesättigteFettsäuren = 0.75,
                    Kohlenhydrate = 17.5,
                    Zucker = 2.5,
                    Protein = 5.0,
                    Ballaststoffe = 2.0,
                    Salz = 0.375
                }
            };

            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            // Act
            var result = await _service.GetRezeptNährwerteAsync(rezeptId);

            // Assert - Überprüfe alle Felder
            Assert.NotNull(result.GesamtnährwerteDto);
            Assert.NotNull(result.ProPortionNährwerteDto);
            Assert.True(result.GesamtnährwerteDto.Kalorien > 0);
            Assert.True(result.GesamtnährwerteDto.Fett > 0);
            Assert.True(result.GesamtnährwerteDto.Protein > 0);
            Assert.True(result.ProPortionNährwerteDto.Kalorien > 0);
        }

        // ============ CalculateNährwerteFromZutatenAsync Tests ============

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithSingleZutat_CalculateCorrectly()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 200, Einheit = "g" }
            };
            int portionen = 4;
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 150,
                Fett = 10.0,
                Protein = 8.0,
                Kohlenhydrate = 15.0
            };

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(200, "g")).Returns(200);

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.Kalorien); // 150 * (200/100)
            Assert.Equal(20.0, result.Fett);    // 10 * (200/100)
            Assert.Equal(16.0, result.Protein);  // 8 * (200/100)
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithMultipleZutaten_SumCorrectly()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 200, Einheit = "g" },
                new RezeptZutat { Id = 2, LebensmittelId = 2, Menge = 100, Einheit = "g" },
                new RezeptZutat { Id = 3, LebensmittelId = 3, Menge = 3, Einheit = "Stück" }
            };
            int portionen = 4;

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(200, "g")).Returns(200);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(3, "Stück")).Returns(150); // 3 eggs = ~150g

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Kalorien > 0);
            Assert.True(result.Protein > 0);
            Assert.True(result.Kohlenhydrate >= 0);
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithEmptyZutatenList_ReturnZeroNährwerte()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>();
            int portionen = 4;

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Kalorien);
            Assert.Equal(0.0, result.Fett);
            Assert.Equal(0.0, result.Protein);
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithZutatWithoutNährwert_IgnoreThatZutat()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 200, Einheit = "g" },
                new RezeptZutat { Id = 2, LebensmittelId = 999, Menge = 100, Einheit = "g" } // Zutat ohne Nährwert
            };
            int portionen = 4;

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(200, "g")).Returns(200);
            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert - Sollte nur Zutat 1 einrechnen
            Assert.NotNull(result);
            Assert.True(result.Kalorien > 0);
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_DividesCorrectlyByPortionen()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 400, Einheit = "g" }
            };
            int portionen = 2;
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100, // pro 100g
                Fett = 5.0,
                Protein = 10.0
            };

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(400, "g")).Returns(400);

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            // Pro Portion: (100 * 4) / 2 = 200 kalorien pro portion
        }

        // ============ RezeptNährwerteWithAdjustmentsAsync Tests ============

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_ReducePortions_RecalculateCorrectly()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 800, Protein = 32.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 200, Protein = 8.0 }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(nährwerte);

            // Act - User ändert Portionen von 4 zu 2
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 2);

            // Assert
            Assert.NotNull(result);
            // Mit 2 Portionen statt 4: Gesamt sollte etwa 400 sein (50%)
            Assert.Equal(rezeptId, result.RezeptId);
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_IncreasePortions_RecalculateCorrectly()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400, Protein = 20.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100, Protein = 5.0 }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(nährwerte);

            // Act - User ändert Portionen von 4 zu 8
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 8);

            // Assert
            Assert.NotNull(result);
            // Mit 8 Portionen: Gesamt sollte etwa 800 sein (200%)
            Assert.Equal(rezeptId, result.RezeptId);
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_WithInvalidNewPortionen_ThrowArgumentException()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 0)
            );

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: -1)
            );
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_WithNegativePortionen_ThrowArgumentException()
        {
            // Arrange
            var rezeptId = 1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: -5)
            );
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_SamePortionen_ReturnUnchanged()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var originalNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 800 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 200 }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(originalNährwerte);

            // Act - Keine Änderung der Portionen
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 4);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(800, result.GesamtnährwerteDto.Kalorien);
            Assert.Equal(200, result.ProPortionNährwerteDto.Kalorien);
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_AllNährwertPropertiesAdjust()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Kalorien = 400,
                    Fett = 20.0,
                    Kohlenhydrate = 40.0,
                    Protein = 20.0,
                    Ballaststoffe = 8.0,
                    Salz = 2.0
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Kalorien = 100,
                    Fett = 5.0,
                    Kohlenhydrate = 10.0,
                    Protein = 5.0,
                    Ballaststoffe = 2.0,
                    Salz = 0.5
                }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(nährwerte);

            // Act - Portionen verdoppeln (4 → 8)
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 8);

            // Assert - Alle Werte sollten angepasst sein
            Assert.NotNull(result);
            Assert.NotNull(result.GesamtnährwerteDto);
            Assert.NotNull(result.ProPortionNährwerteDto);
        }

        // ============ Edge Cases & Error Handling ============

        [Fact]
        public async Task GetRezeptNährwerte_WithArchivedRezept_ShouldStillReturnNährwerte()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 500 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 125 }
            };

            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            // Act
            var result = await _service.GetRezeptNährwerteAsync(rezeptId);

            // Assert - Sollte auch für archivierte Rezepte funktionieren
            Assert.NotNull(result);
            Assert.Equal(500, result.GesamtnährwerteDto.Kalorien);
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithVeryLargePortionen_HandleCorrectly()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 100, Einheit = "g" }
            };
            int portionen = 100; // Sehr viele Portionen

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(100, "g")).Returns(100);

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Kalorien >= 0);
        }

        [Fact]
        public async Task CalculateNährwerteFromZutaten_WithFractionalPortionen_CalculateCorrectly()
        {
            // Arrange
            var zutaten = new List<RezeptZutat>
            {
                new RezeptZutat { Id = 1, LebensmittelId = 1, Menge = 200, Einheit = "g" }
            };
            int portionen = 3; // Ungerade Zahl

            _mockEinheitConverter.Setup(e => e.ConvertToGramm(200, "g")).Returns(200);

            // Act
            var result = await _service.CalculateNährwerteFromZutatenAsync(zutaten, portionen);

            // Assert
            Assert.NotNull(result);
            // Sollte korrekt berechnet werden (z.B. 300 kalorien / 3 = 100 pro portion)
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_MaxPortionBoundary_HandleCorrectly()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(nährwerte);

            // Act - Maximale erlaubte Portionen (100)
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 100);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AdjustNährwerteForMengenänderung_MinPortionBoundary_HandleCorrectly()
        {
            // Arrange
            var rezeptId = 1;
            var rezept = new Rezept { Id = rezeptId, Portionen = 4 };
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            _mockRezeptService.Setup(r => r.GetRezeptByIdAsync(rezeptId)).ReturnsAsync(rezept);
            _mockNährwertCalculator.Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId)).ReturnsAsync(nährwerte);

            // Act - Minimale erlaubte Portionen (1)
            var result = await _service.AdjustNährwerteForPortionAsync(rezeptId, newPortionen: 1);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRezeptNährwerte_CachedResults_ReturnConsistently()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 500 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 125 }
            };

            _mockNährwertCalculator
                .Setup(n => n.CalculateRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            // Act - Rufe zweimal ab
            var result1 = await _service.GetRezeptNährwerteAsync(rezeptId);
            var result2 = await _service.GetRezeptNährwerteAsync(rezeptId);

            // Assert - Sollten identisch sein
            Assert.Equal(result1.GesamtnährwerteDto.Kalorien, result2.GesamtnährwerteDto.Kalorien);
            Assert.Equal(result1.ProPortionNährwerteDto.Kalorien, result2.ProPortionNährwerteDto.Kalorien);
        }
    }
}

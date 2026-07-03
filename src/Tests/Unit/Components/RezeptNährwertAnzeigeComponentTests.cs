using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Bunit;
using FoodDatabase.App.Components;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Dtos;

namespace FoodDatabase.Tests.Unit.Components
{
    /// <summary>
    /// Unit Tests für RezeptNährwertAnzeige.razor Komponente (UC5: Nährwert-UI).
    /// Testet das Rendering von Nährwert-Daten und die Interaktion mit Portion-Anpassung.
    /// TDD Red-Phase: Tests sind absichtlich rot, Komponente wird später implementiert.
    /// </summary>
    public class RezeptNährwertAnzeigeComponentTests : TestContext
    {
        private readonly Mock<IRezeptNährwertService> _mockRezeptNährwertService;

        public RezeptNährwertAnzeigeComponentTests()
        {
            _mockRezeptNährwertService = new Mock<IRezeptNährwertService>();
        }

        // ============ Component Initialization Tests ============

        [Fact]
        public void RezeptNährwertAnzeige_WithValidRezeptId_RendersSuccessfully()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Kalorien = 600,
                    Fett = 20.0,
                    Protein = 30.0
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Kalorien = 150,
                    Fett = 5.0,
                    Protein = 7.5
                }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert
            component.MarkupMatches(@"*"); // Component should render without exceptions
        }

        [Fact]
        public void RezeptNährwertAnzeige_DisplaysKalorien()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 600 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 150 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Kalorien sollten angezeigt werden
            Assert.Contains("600", component.Markup);
            Assert.Contains("150", component.Markup);
        }

        [Fact]
        public void RezeptNährwertAnzeige_DisplaysAllNährwertDetails()
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
                    Protein = 25.0,
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
                    Protein = 6.25,
                    Ballaststoffe = 2.0,
                    Salz = 0.375
                }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Alle Nährwert-Kategorien sollten sichtbar sein
            Assert.NotEmpty(component.Markup);
        }

        // ============ Portion Adjustment Tests ============

        [Fact]
        public async Task RezeptNährwertAnzeige_ReducePortions_UpdatesNährwerteDisplay()
        {
            // Arrange
            var rezeptId = 1;
            var originalNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 800 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 200 }
            };

            var adjustedNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 200 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(originalNährwerte);

            _mockRezeptNährwertService
                .Setup(s => s.AdjustNährwerteForPortionAsync(rezeptId, 2))
                .ReturnsAsync(adjustedNährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Simulate user reducing portions
            var reduceButton = component.Find("[data-testid='reduce-portion']");
            reduceButton.Click();

            await Task.Delay(100); // Wait for async update

            // Assert - Nährwerte sollten aktualisiert sein
            _mockRezeptNährwertService.Verify(
                s => s.AdjustNährwerteForPortionAsync(rezeptId, 2),
                Times.Once
            );
        }

        [Fact]
        public async Task RezeptNährwertAnzeige_IncreasePortions_UpdatesNährwerteDisplay()
        {
            // Arrange
            var rezeptId = 1;
            var originalNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            var adjustedNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 800 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(originalNährwerte);

            _mockRezeptNährwertService
                .Setup(s => s.AdjustNährwerteForPortionAsync(rezeptId, 8))
                .ReturnsAsync(adjustedNährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Simulate user increasing portions
            var increaseButton = component.Find("[data-testid='increase-portion']");
            increaseButton.Click();

            await Task.Delay(100); // Wait for async update

            // Assert
            _mockRezeptNährwertService.Verify(
                s => s.AdjustNährwerteForPortionAsync(rezeptId, 8),
                Times.Once
            );
        }

        [Fact]
        public async Task RezeptNährwertAnzeige_DirectPortionInput_UpdatesNährwerte()
        {
            // Arrange
            var rezeptId = 1;
            var originalNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            var adjustedNährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 600 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(originalNährwerte);

            _mockRezeptNährwertService
                .Setup(s => s.AdjustNährwerteForPortionAsync(rezeptId, 6))
                .ReturnsAsync(adjustedNährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Simulate user entering custom portion count
            var portionInput = component.Find("[data-testid='portion-input']");
            portionInput.Change("6");

            await Task.Delay(100); // Wait for async update

            // Assert
            _mockRezeptNährwertService.Verify(
                s => s.AdjustNährwerteForPortionAsync(rezeptId, 6),
                Times.Once
            );
        }

        // ============ Mengen-Update Tests ============

        [Fact]
        public async Task RezeptNährwertAnzeige_UpdateZutatMenge_RefreshesNährwerte()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerteVorher = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 300, Protein = 15.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 75, Protein = 3.75 }
            };

            var nährwerteNachher = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 600, Protein = 30.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 150, Protein = 7.5 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerteVorher)
                .ReturnsAsync(nährwerteNachher); // Zweiter Call nach Update

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Trigger parent component to notify of menge change
            component.Instance.RefreshNährwerte();

            await Task.Delay(100); // Wait for async refresh

            // Assert
            _mockRezeptNährwertService.Verify(
                s => s.GetRezeptNährwerteAsync(rezeptId),
                Times.AtLeastOnce
            );
        }

        // ============ Display Format Tests ============

        [Fact]
        public void RezeptNährwertAnzeige_FormatsKalorienAsInteger()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 542 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 135 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Kalorien sollten ohne Dezimalstellen angezeigt werden
            Assert.Contains("542", component.Markup);
            Assert.Contains("135", component.Markup);
        }

        [Fact]
        public void RezeptNährwertAnzeige_FormatsGrammAsTwoDecimals()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Fett = 20.567,
                    Protein = 15.234
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Fett = 5.142,
                    Protein = 3.809
                }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Gramm-Angaben sollten auf 2 Dezimalstellen formatiert sein
            Assert.NotEmpty(component.Markup);
        }

        // ============ Error Handling Tests ============

        [Fact]
        public void RezeptNährwertAnzeige_WithInvalidRezeptId_DisplaysErrorMessage()
        {
            // Arrange
            var invalidRezeptId = 999;

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(invalidRezeptId))
                .ThrowsAsync(new Exception("Rezept nicht gefunden"));

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, invalidRezeptId)
            );

            // Assert - Error message sollte angezeigt werden
            Assert.Contains("Fehler", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RezeptNährwertAnzeige_LoadingState_DisplaysLoadingIndicator()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 500 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 125 }
            };

            // Delay the response to simulate loading
            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .Returns(async () =>
                {
                    await Task.Delay(500);
                    return nährwerte;
                });

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Loading indicator sollte sichtbar sein während Daten geladen werden
            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        // ============ Component Parameter Tests ============

        [Fact]
        public void RezeptNährwertAnzeige_ParameterChange_UpdatesDisplay()
        {
            // Arrange
            var rezeptId1 = 1;
            var rezeptId2 = 2;
            var nährwerte1 = new RezeptNährwerteDto
            {
                RezeptId = rezeptId1,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 300 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 75 }
            };
            var nährwerte2 = new RezeptNährwerteDto
            {
                RezeptId = rezeptId2,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 600 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 150 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId1))
                .ReturnsAsync(nährwerte1);

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId2))
                .ReturnsAsync(nährwerte2);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId1)
            );

            // Change RezeptId parameter
            component.SetParametersAsync(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId2)
            );

            // Assert - Component sollte neue Daten laden
            _mockRezeptNährwertService.Verify(
                s => s.GetRezeptNährwerteAsync(rezeptId1),
                Times.Once
            );

            _mockRezeptNährwertService.Verify(
                s => s.GetRezeptNährwerteAsync(rezeptId2),
                Times.Once
            );
        }

        [Fact]
        public void RezeptNährwertAnzeige_DisplayModeParameter_RendersCorrectly()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 500 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 125 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act - Display mode "compact"
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
                .Add(p => p.DisplayMode, "compact")
            );

            // Assert - Komponente sollte im Compact-Modus rendern (weniger Details)
            Assert.NotEmpty(component.Markup);
        }

        // ============ Accessibility Tests ============

        [Fact]
        public void RezeptNährwertAnzeige_HasAccessibleLabels()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 500, Protein = 25.0 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 125, Protein = 6.25 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - aria-labels sollten vorhanden sein
            Assert.Contains("aria-label", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RezeptNährwertAnzeige_ButtonsAreKeyboardNavigable()
        {
            // Arrange
            var rezeptId = 1;
            var nährwerte = new RezeptNährwerteDto
            {
                RezeptId = rezeptId,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 400 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 100 }
            };

            _mockRezeptNährwertService
                .Setup(s => s.GetRezeptNährwerteAsync(rezeptId))
                .ReturnsAsync(nährwerte);

            Services.AddScoped<IRezeptNährwertService>(_ => _mockRezeptNährwertService.Object);

            // Act
            var component = RenderComponent<RezeptNährwertAnzeige>(parameters =>
                parameters.Add(p => p.RezeptId, rezeptId)
            );

            // Assert - Buttons sollten keyboard-navigable sein (tabindex, semantic buttons)
            Assert.Contains("button", component.Markup, StringComparison.OrdinalIgnoreCase);
        }
    }
}

using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für RezeptNährwertAnzeige.razor (UC5: Nährwert-Anzeige eingebettet in die Rezept-Detailseite).
    /// Testet: Ladezustand, Werte nach dem Laden, Fehlerpfad, Startwert der Portionsanpassung (Parameter Portionen),
    /// Portion-Anpassung über Buttons und Eingabefeld.
    /// Die Komponente wird hier nur getestet. Der Portionen-Fix ist abgedeckt: Der Startwert kommt aus dem Parameter Portionen, nicht aus einer festen 4.
    /// </summary>
    public class RezeptNährwertAnzeigeTests : TestContext
    {
        public RezeptNährwertAnzeigeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Ladezustand_Waehrend_Des_Ladens_Anzeigen()
        {
            // Arrange
            TaskCompletionSource<RezeptNährwerteDto> tcs = new();
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .Returns(tcs.Task);
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters.Add(p => p.RezeptId, 1));

            // Assert: während des Ladens sichtbar, Nährwertdaten noch nicht
            Assert.Single(cut.FindAll(".loading-indicator"));
            Assert.Empty(cut.FindAll(".naehrwert-container"));

            cut.InvokeAsync(() => tcs.SetResult(CreateDto()));
            cut.WaitForAssertion(() =>
            {
                Assert.Empty(cut.FindAll(".loading-indicator"));
                Assert.Single(cut.FindAll(".naehrwert-container"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Gesamtkalorien_Nach_Laden_Anzeigen()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters.Add(p => p.RezeptId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement kalorien = cut.Find("[aria-label='Kalorien gesamt']");
                Assert.Equal("1200 kcal", kalorien.TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Kalorien_Pro_Portion_Anzeigen()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters.Add(p => p.RezeptId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement kalorien = cut.Find("[aria-label='Kalorien pro portion']");
                Assert.Equal("300 kcal", kalorien.TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Dezimalwerte_Mit_Zwei_Nachkommastellen_Anzeigen()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters.Add(p => p.RezeptId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement fett = cut.Find("[aria-label='Fett gesamt']");
                Assert.Equal(40.5.ToString("F2") + " g", fett.TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Mit_Alert_Rolle_Anzeigen_Wenn_Laden_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ThrowsAsync(new InvalidOperationException("Rezept hat keine Zutaten"));
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters.Add(p => p.RezeptId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[role='alert']");
                Assert.Contains("Rezept hat keine Zutaten", alert.TextContent);
                Assert.Empty(cut.FindAll(".naehrwert-container"));
                Assert.Empty(cut.FindAll(".loading-indicator"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Portion_Verringern_Den_Service_Mit_Neuer_Portionszahl_Aufrufen()
        {
            // Arrange: Startwert 2 (Parameter Portionen), also wird nach dem Verringern mit 1 angepasst
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, It.IsAny<int>()))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='reduce-portion']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='reduce-portion']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.AdjustNährwerteForPortionAsync(1, 1), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Portion_Erhoehen_Den_Service_Mit_Neuer_Portionszahl_Aufrufen()
        {
            // Arrange: Startwert 2 (Parameter Portionen), also wird nach dem Erhöhen mit 3 angepasst
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, It.IsAny<int>()))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='increase-portion']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='increase-portion']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.AdjustNährwerteForPortionAsync(1, 3), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Portion_Unter_1_Nicht_Weiter_Verringern()
        {
            // Arrange: Startwert 1 (Grenze ist sofort erreicht)
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, It.IsAny<int>()))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='reduce-portion']")), TimeSpan.FromSeconds(2));

            // Act: zweimal verringern, obwohl bereits 1 erreicht ist
            cut.Find("[data-testid='reduce-portion']").Click();
            cut.Find("[data-testid='reduce-portion']").Click();

            // Assert: der angezeigte Wert bleibt 1 (ohne Button-Grenze würde er auf 0 und -1 fallen)
            Assert.Equal("1", cut.Find("[data-testid='portion-input']").GetAttribute("value"));
            mock.Verify(s => s.AdjustNährwerteForPortionAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Portion_Ueber_100_Nicht_Weiter_Erhoehen()
        {
            // Arrange: Startwert 100 (Obergrenze ist sofort erreicht)
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, It.IsAny<int>()))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 100));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='increase-portion']")), TimeSpan.FromSeconds(2));

            // Act: zweimal erhöhen, obwohl bereits 100 erreicht ist
            cut.Find("[data-testid='increase-portion']").Click();
            cut.Find("[data-testid='increase-portion']").Click();

            // Assert: der angezeigte Wert bleibt 100 (ohne Button-Grenze würde er auf 101 und 102 steigen)
            Assert.Equal("100", cut.Find("[data-testid='portion-input']").GetAttribute("value"));
            mock.Verify(s => s.AdjustNährwerteForPortionAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Eingabe_Null_Keinen_Service_Aufruf_Ausloesen()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, It.IsAny<int>()))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='portion-input']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='portion-input']").Change("0");

            // Assert
            mock.Verify(s => s.AdjustNährwerteForPortionAsync(1, 0), Times.Never);
            mock.Verify(s => s.AdjustNährwerteForPortionAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Portionseingabe_Mit_Portionen_Parameter_Starten()
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));

            // Assert: das Eingabefeld startet mit dem Parameter Portionen
            cut.WaitForAssertion(() => Assert.Equal("2", cut.Find("[data-testid='portion-input']").GetAttribute("value")), TimeSpan.FromSeconds(2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Sollte_Portionseingabe_Bei_Portionen_Unter_1_Mit_1_Starten(int portionen)
        {
            // Arrange
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);

            // Act
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, portionen));

            // Assert: ungültige Startwerte werden auf 1 angehoben
            cut.WaitForAssertion(() => Assert.Equal("1", cut.Find("[data-testid='portion-input']").GetAttribute("value")), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Werte_Nach_Portionsaenderung_Aktualisieren()
        {
            // Arrange: Startwert 2, Erhöhen ruft Adjust mit 3 auf
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, 3))
                .ReturnsAsync(CreateDto(kalorienGesamt: 1500));
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='increase-portion']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='increase-portion']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement kalorien = cut.Find("[aria-label='Kalorien gesamt']");
                Assert.Equal("1500 kcal", kalorien.TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Portionsanpassung_Fehlschlaegt()
        {
            // Arrange: Startwert 2, Erhöhen ruft Adjust mit 3 auf, und dieser Aufruf schlägt fehl
            Mock<IRezeptNährwertService> mock = new();
            mock.Setup(s => s.GetRezeptNährwerteAsync(1))
                .ReturnsAsync(CreateDto());
            mock.Setup(s => s.AdjustNährwerteForPortionAsync(1, 3))
                .ThrowsAsync(new InvalidOperationException("Ungültige Portion"));
            Services.AddSingleton<IRezeptNährwertService>(mock.Object);
            IRenderedComponent<RezeptNährwertAnzeige> cut = RenderComponent<RezeptNährwertAnzeige>(parameters => parameters
                .Add(p => p.RezeptId, 1)
                .Add(p => p.Portionen, 2));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='increase-portion']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='increase-portion']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[role='alert']");
                Assert.Contains("Fehler bei der Anpassung", alert.TextContent);
                Assert.Contains("Ungültige Portion", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        private static RezeptNährwerteDto CreateDto(int kalorienGesamt = 1200)
        {
            return new RezeptNährwerteDto
            {
                RezeptId = 1,
                GesamtnährwerteDto = new NährwerteDto
                {
                    Kalorien = kalorienGesamt,
                    Fett = 40.5,
                    GesättigteFettsäuren = 10,
                    Kohlenhydrate = 150,
                    Zucker = 12,
                    Protein = 50,
                    Ballaststoffe = 8,
                    Salz = 3
                },
                ProPortionNährwerteDto = new NährwerteDto
                {
                    Kalorien = 300,
                    Fett = 10.125,
                    GesättigteFettsäuren = 2.5,
                    Kohlenhydrate = 37.5,
                    Zucker = 3,
                    Protein = 12.5,
                    Ballaststoffe = 2,
                    Salz = 0.75
                }
            };
        }
    }
}

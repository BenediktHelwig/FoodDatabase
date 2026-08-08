using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components.Pages.Lager;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für VerbrauchListe.razor Komponente (UC6).
    /// Testet Funktionalität: Tabelle rendern, Mindestbestand-Badge, Verbrauch-Ausbuchung mit ServiceResult-Handling.
    /// </summary>
    public class VerbrauchListeTests : TestContext
    {
        public VerbrauchListeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Tabelle_Mit_Verbrauchzeilen_Rendern()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "l", Kategorie = "Molkereiprodukte" },
                new LebensmittelKatalog { Id = 2, Name = "Käse", Einheit = "g", Kategorie = "Molkereiprodukte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(1))
                .ReturnsAsync(5.0m);
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(2))
                .ReturnsAsync(2.5m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(2, rows.Count);
                Assert.Contains("Milch", cut.Markup);
                Assert.Contains("Käse", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Badge_Anzeigen_Bei_Mindestbestand_Unterschritten()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "kg", Kategorie = "Trockenprodukte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(1))
                .ReturnsAsync(0.5m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(1))
                .ReturnsAsync(true);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement badge = cut.Find("[data-testid='badge-mindestbestand-1']");
                Assert.NotNull(badge);
                Assert.Contains("Mindestbestand unterschritten", badge.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Badge_Nicht_Anzeigen_Wenn_Mindestbestand_Ok()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Zucker", Einheit = "kg", Kategorie = "Trockenprodukte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(1))
                .ReturnsAsync(10.0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(1))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> badges = cut.FindAll("[data-testid='badge-mindestbestand-1']");
                Assert.Empty(badges);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Meldung_Anzeigen_Wenn_Liste_Leer()
        {
            // Arrange
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)new List<LebensmittelKatalog>());

            Mock<ILagerbestandService> lagerbestandMock = new();
            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Keine Lebensmittel vorhanden.", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_VerbauchtProdukt_Aufrufen_Bei_Button_Klick()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 5, Name = "Butter", Einheit = "g", Kategorie = "Molkereiprodukte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(5))
                .ReturnsAsync(250.0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(5))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();
            verbrauchMock.Setup(s => s.VerbauchtProduktAsync(5))
                .ReturnsAsync(ServiceResult.SuccessResult("Produkt erfolgreich verbraucht"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-verbrauchen-5']");
                button.Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                verbrauchMock.Verify(s => s.VerbauchtProduktAsync(5), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Erfolgs_Alert_Anzeigen_Bei_Verbrauch_Erfolg()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 3, Name = "Joghurt", Einheit = "g", Kategorie = "Molkereiprodukte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(3))
                .ReturnsAsync(500.0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(3))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();
            verbrauchMock.Setup(s => s.VerbauchtProduktAsync(3))
                .ReturnsAsync(ServiceResult.SuccessResult("Produkt erfolgreich verbraucht"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-verbrauchen-3']");
                button.Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-ergebnis']");
                Assert.NotNull(alert);
                Assert.Contains("Produkt erfolgreich verbraucht", alert.TextContent);
                string classAttr = alert.GetAttribute("class") ?? "";
                Assert.Contains("alert-success", classAttr ?? "");
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Alert_Anzeigen_Bei_Verbrauch_Fehler()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 4, Name = "Eier", Einheit = "Stück", Kategorie = "Tierische Produkte" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(4))
                .ReturnsAsync(12.0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(4))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();
            verbrauchMock.Setup(s => s.VerbauchtProduktAsync(4))
                .ReturnsAsync(ServiceResult.FailureResult("Produkt nicht vorhanden"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-verbrauchen-4']");
                button.Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-ergebnis']");
                Assert.NotNull(alert);
                Assert.Contains("Produkt nicht vorhanden", alert.TextContent);
                string classAttr = alert.GetAttribute("class") ?? "";
                Assert.Contains("alert-warning", classAttr);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Liste_Nach_Erfolgreicher_Ausbuchung_Neuladen()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 2, Name = "Brot", Einheit = "g", Kategorie = "Backwaren" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(2))
                .ReturnsAsync(800.0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(2))
                .ReturnsAsync(false);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();
            verbrauchMock.Setup(s => s.VerbauchtProduktAsync(2))
                .ReturnsAsync(ServiceResult.SuccessResult("Erfolgreich verbraucht"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-verbrauchen-2']");
                button.Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                lebensmittelMock.Verify(s => s.GetAllLebensmittelAsync(), Times.Exactly(2));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Alert_Anzeigen_Bei_Exception()
        {
            // Arrange
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ThrowsAsync(new Exception("Datenbankverbindungsfehler"));

            Mock<ILagerbestandService> lagerbestandMock = new();
            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.Contains("Fehler beim", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Button_Disabled_Sein_Bei_Gesamtmenge_Null()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 6, Name = "Öl", Einheit = "ml", Kategorie = "Öle und Fette" }
            };
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync((IEnumerable<LebensmittelKatalog>)lebensmittel);

            Mock<ILagerbestandService> lagerbestandMock = new();
            lagerbestandMock.Setup(s => s.GetGesamtmengeAsync(6))
                .ReturnsAsync(0m);
            lagerbestandMock.Setup(s => s.CheckMindestbestandUnterschrittenAsync(6))
                .ReturnsAsync(true);

            Mock<IVerbrauchAusbuchangService> verbrauchMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerbestandService>(lagerbestandMock.Object);
            Services.AddSingleton<IVerbrauchAusbuchangService>(verbrauchMock.Object);

            // Act
            IRenderedComponent<VerbrauchListe> cut = RenderComponent<VerbrauchListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-verbrauchen-6']");
                Assert.NotNull(button);
                Assert.True(button.HasAttribute("disabled"));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

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
    /// bUnit Tests für LagerortListe.razor Komponente (UC9).
    /// Testet Funktionalität: Tabelle rendern, Fehlererkennung, Neue-Lagerorte-Navigation.
    /// </summary>
    public class LagerortListeTests : TestContext
    {
        public LagerortListeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Tabelle_Mit_Lagerorten_Rendern()
        {
            // Arrange
            List<Lagerort> lagerorte = new()
            {
                new Lagerort { Id = 1, Name = "Kühlschrank", CreatedAt = DateTime.UtcNow, IsArchived = false },
                new Lagerort { Id = 2, Name = "Tiefkühler", CreatedAt = DateTime.UtcNow, IsArchived = false },
                new Lagerort { Id = 3, Name = "Keller", CreatedAt = DateTime.UtcNow, IsArchived = false }
            };
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(lagerorte);
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortListe> cut = RenderComponent<LagerortListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                Assert.True(cut.Markup.Contains("Kühlschrank"));
                Assert.True(cut.Markup.Contains("Tiefkühler"));
                Assert.True(cut.Markup.Contains("Keller"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Spalten_Name_Und_ErstelltAm_Haben()
        {
            // Arrange
            List<Lagerort> lagerorte = new()
            {
                new Lagerort { Id = 1, Name = "Kühlschrank", CreatedAt = DateTime.UtcNow, IsArchived = false }
            };
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(lagerorte);
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortListe> cut = RenderComponent<LagerortListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement table = cut.Find("[data-testid='tabelle-lagerorte']");
                Assert.NotNull(table);
                Assert.True(table.OuterHtml.Contains("<th>Name</th>"));
                Assert.True(table.OuterHtml.Contains("<th>Erstellt am</th>"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Meldung_Anzeigen_Wenn_Liste_Leer()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(new List<Lagerort>());
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortListe> cut = RenderComponent<LagerortListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Keine Lagerorte gefunden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neu_Button_Haben()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(new List<Lagerort>());
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortListe> cut = RenderComponent<LagerortListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-neu']");
                Assert.NotNull(button);
                Assert.Contains("Neu", button.TextContent);
                Assert.Contains("/lagerorte/neu", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Bei_Service_Exception()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetAlleLagerorte())
                .ThrowsAsync(new Exception("Service Error"));
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortListe> cut = RenderComponent<LagerortListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Abrufen"));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

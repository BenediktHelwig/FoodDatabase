using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components.Pages.Lebensmittel;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.Tests.Ui.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für LebensmittelListe.razor Komponente (UC1).
    /// Testet Funktionalität: Tabelle rendern, Suche, Lösch-Modal, Neu/Bearbeiten-Navigation.
    /// </summary>
    public class LebensmittelListeTests : TestContext
    {
        public LebensmittelListeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Tabelle_Mit_Lebensmitteln_Rendern()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(3);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                Assert.True(cut.Markup.Contains("Lebensmittel 1"));
                Assert.True(cut.Markup.Contains("Lebensmittel 2"));
                Assert.True(cut.Markup.Contains("Lebensmittel 3"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Korrekte_Spalten_In_Tabellenkopf_Haben()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement table = cut.Find("[data-testid='tabelle-lebensmittel']");
                Assert.NotNull(table);
                Assert.True(table.OuterHtml.Contains("<th>Name</th>"));
                Assert.True(table.OuterHtml.Contains("<th>Einheit</th>"));
                Assert.True(table.OuterHtml.Contains("<th>Kategorie</th>"));
                Assert.True(table.OuterHtml.Contains("<th>Aktionen</th>"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Keine_Meldung_Anzeigen_Wenn_Liste_Nicht_Leer()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.False(cut.Markup.Contains("Keine Lebensmittel gefunden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Meldung_Anzeigen_Wenn_Liste_Leer()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Keine Lebensmittel gefunden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Suche_Ausführen_Mit_Suchbegriff()
        {
            // Arrange
            List<LebensmittelKatalog> allItems = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            List<LebensmittelKatalog> searchResults = new()
            {
                LebensmittelTestDataBuilder.CreateLebensmittel()
                    .WithId(1)
                    .WithName("Mehl")
                    .WithEinheit("g")
                    .WithKategorie("Getreide")
                    .Build()
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(allItems);
            mock.Setup(s => s.SearchLebensmittelAsync("Mehl"))
                .ReturnsAsync(searchResults);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-suche']").Input("Mehl");
                cut.Find("[data-testid='btn-suche']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.SearchLebensmittelAsync("Mehl"), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Alle_Abrufen_Bei_Leerer_Suche()
        {
            // Arrange
            var allItems = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(allItems);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-suche']").Input("");
                cut.Find("[data-testid='btn-suche']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.GetAllLebensmittelAsync(), Times.AtLeastOnce);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Alle_Anzeigen_Button_Haben()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-alle']");
                Assert.NotNull(button);
                Assert.Contains("Alle anzeigen", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Alle_Anzeigen_Button_Funktionieren()
        {
            // Arrange
            List<LebensmittelKatalog> allItems = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            List<LebensmittelKatalog> searchResults = new()
            {
                allItems[0]
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(allItems);
            mock.Setup(s => s.SearchLebensmittelAsync(It.IsAny<string>()))
                .ReturnsAsync(searchResults);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-suche']").Input("Test");
                cut.Find("[data-testid='btn-alle']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.GetAllLebensmittelAsync(), Times.AtLeastOnce);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neu_Button_Haben()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-neu']");
                Assert.NotNull(button);
                Assert.Contains("Neu", button.TextContent);
                Assert.Contains("/lebensmittel/neu", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Detail_Links_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement detailLink1 = cut.Find("[data-testid='btn-detail-1']");
                Assert.NotNull(detailLink1);
                Assert.Contains("/lebensmittel/1", detailLink1.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Links_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement editLink1 = cut.Find("[data-testid='btn-bearbeiten-1']");
                Assert.NotNull(editLink1);
                Assert.Contains("/lebensmittel/1/bearbeiten", editLink1.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Löschen_Buttons_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement deleteBtn1 = cut.Find("[data-testid='btn-löschen-1']");
                Assert.NotNull(deleteBtn1);
                Assert.Contains("Löschen", deleteBtn1.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestätigungsmodal_Öffnen_Beim_Löschen()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            IReadOnlyList<IElement> modal = cut.FindAll("[data-testid='modal-bestätigung']");
            Assert.NotEmpty(modal);
            Assert.True(cut.Markup.Contains("Bestätigung erforderlich"));
        }

        [Fact]
        public void Sollte_Bestätigungsmodal_Schließen_Beim_Abbrechen()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
                cut.Find("[data-testid='btn-abbrechen']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            IReadOnlyList<IElement> modal = cut.FindAll("[data-testid='modal-bestätigung']");
            Assert.Empty(modal);
        }

        [Fact]
        public void Sollte_Bestätigung_Service_Aufrufen()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            mock.Setup(s => s.DeleteLebensmittelAsync(1))
                .ReturnsAsync(true);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
                cut.Find("[data-testid='btn-löschen-bestätigt']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.DeleteLebensmittelAsync(1), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Löschen_Fehlschlägt()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            mock.Setup(s => s.DeleteLebensmittelAsync(1))
                .ReturnsAsync(false);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
                cut.Find("[data-testid='btn-löschen-bestätigt']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("konnte nicht gelöscht werden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Bei_Service_Exception()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ThrowsAsync(new Exception("Service Error"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Abrufen"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Beim_Löschen_Mit_Exception()
        {
            // Arrange
            List<LebensmittelKatalog> lebensmittel = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            mock.Setup(s => s.DeleteLebensmittelAsync(1))
                .ThrowsAsync(new Exception("Delete failed"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
                cut.Find("[data-testid='btn-löschen-bestätigt']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Löschen"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Suche_Fehler_Anzeigen()
        {
            // Arrange
            var allItems = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(allItems);
            mock.Setup(s => s.SearchLebensmittelAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Search error"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-suche']").Input("Test");
                cut.Find("[data-testid='btn-suche']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler bei der Suche"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Tabelle_Nach_Löschen_Neu_Laden()
        {
            // Arrange
            var initialList = LebensmittelTestDataBuilder.CreateTestLebensmittelList(2);
            var listAfterDelete = LebensmittelTestDataBuilder.CreateTestLebensmittelList(1);

            var mock = new Mock<ILebensmittelService>();
            var callCount = 0;
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .Returns(() =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<LebensmittelKatalog>>(
                        callCount == 1 ? initialList : listAfterDelete);
                });
            mock.Setup(s => s.DeleteLebensmittelAsync(1))
                .ReturnsAsync(true);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
                cut.Find("[data-testid='btn-löschen-bestätigt']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.GetAllLebensmittelAsync(), Times.AtLeast(2));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

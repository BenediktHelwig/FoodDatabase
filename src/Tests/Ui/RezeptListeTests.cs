using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components.Pages.Rezepte;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für RezeptListe.razor (UC4: Rezepte verwalten).
    /// Testet: Tabelle rendern, Suche, Alle anzeigen, Lösch-Modal, Fehlerpfade, Navigationslinks.
    /// </summary>
    public class RezeptListeTests : TestContext
    {
        public RezeptListeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Tabelle_Mit_Rezepten_Rendern()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(3));
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                Assert.Contains("Rezept 1", cut.Markup);
                Assert.Contains("Rezept 2", cut.Markup);
                Assert.Contains("Rezept 3", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Korrekte_Spalten_In_Tabellenkopf_Haben()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Act & Assert
            cut.WaitForAssertion(() =>
            {
                List<string> spalten = cut.FindAll("thead th")
                    .Select(th => th.TextContent.Trim())
                    .ToList();
                Assert.Equal(new[] { "Name", "Portionen", "Zeit (min)", "Schwierigkeit", "Aktionen" }, spalten);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Portionen_Und_Zeit_In_Der_Zeile_Anzeigen()
        {
            // Arrange
            Rezept rezept = CreateRezept(1, "Pasta", SchwierigkeitsgradAnzeige.Medium);
            rezept.Portionen = 4;
            rezept.ZubereitungszeitMinuten = 30;
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(new List<Rezept> { rezept });
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IHtmlCollection<IElement> cells = cut.Find("[data-testid='zeile-1']").QuerySelectorAll("td");
                Assert.Equal("4", cells[1].TextContent.Trim());
                Assert.Equal("30", cells[2].TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Theory]
        [InlineData(SchwierigkeitsgradAnzeige.Easy, "Leicht")]
        [InlineData(SchwierigkeitsgradAnzeige.Medium, "Mittel")]
        [InlineData(SchwierigkeitsgradAnzeige.Hard, "Schwer")]
        public void Sollte_Schwierigkeitsgrad_Auf_Deutsch_Anzeigen(string gespeichert, string erwartetDeutsch)
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(new List<Rezept> { CreateRezept(1, "Pasta", gespeichert) });
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IHtmlCollection<IElement> cells = cut.Find("[data-testid='zeile-1']").QuerySelectorAll("td");
                Assert.Equal(erwartetDeutsch, cells[3].TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Suche_Mit_Begriff_An_Service_Weitergeben()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            mock.Setup(s => s.SearchRezepteAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Rezept>());
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-suche']").Change("Pasta");
            cut.Find("[data-testid='btn-suche']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.SearchRezepteAsync("Pasta"), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Suchergebnis_In_Tabelle_Anzeigen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(3));
            mock.Setup(s => s.SearchRezepteAsync("Pasta"))
                .ReturnsAsync(new List<Rezept> { CreateRezept(2, "Pasta Pesto") });
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Equal(3, cut.FindAll("[data-testid^='zeile-']").Count), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-suche']").Change("Pasta");
            cut.Find("[data-testid='btn-suche']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Pasta Pesto", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Suche_Mit_Leerem_Begriff_Alle_Rezepte_Laden()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(2));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[data-testid^='zeile-']").Count), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-suche']").Change("   ");
            cut.Find("[data-testid='btn-suche']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.GetAllRezepteAsync(), Times.Exactly(2));
                mock.Verify(s => s.SearchRezepteAsync(It.IsAny<string>()), Times.Never);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Alle_Anzeigen_Die_Liste_Neu_Laden()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(2));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[data-testid^='zeile-']").Count), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='btn-alle']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.GetAllRezepteAsync(), Times.Exactly(2)), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neu_Button_Zum_Formular_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            IElement neuButton = cut.Find("[data-testid='btn-neu']");
            Assert.Equal("/rezepte/neu", neuButton.GetAttribute("href"));
        }

        [Fact]
        public void Sollte_Detail_Link_Zur_Detailseite_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(new List<Rezept> { CreateRezept(7, "Pasta") });
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement detailLink = cut.Find("[data-testid='btn-detail-7']");
                Assert.Equal("/rezepte/7", detailLink.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Link_Zur_Bearbeitungsseite_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(new List<Rezept> { CreateRezept(7, "Pasta") });
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement bearbeitenLink = cut.Find("[data-testid='btn-bearbeiten-7']");
                Assert.Equal("/rezepte/7/bearbeiten", bearbeitenLink.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Hinweis_Bei_Leerer_Liste_Zeigen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(new List<Rezept>());
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Single(cut.FindAll("[data-testid='hinweis-leer']"));
                Assert.Empty(cut.FindAll("[data-testid='tabelle-rezepte']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Hinweis_Bei_Leerem_Suchergebnis_Zeigen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(2));
            mock.Setup(s => s.SearchRezepteAsync("Nichts"))
                .ReturnsAsync(new List<Rezept>());
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[data-testid^='zeile-']").Count), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-suche']").Change("Nichts");
            cut.Find("[data-testid='btn-suche']").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='hinweis-leer']")), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Laden_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ThrowsAsync(new InvalidOperationException("Datenbank nicht erreichbar"));
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler beim Abrufen der Rezepte", alert.TextContent);
                Assert.Contains("Datenbank nicht erreichbar", alert.TextContent);
                Assert.Empty(cut.FindAll("[data-testid='hinweis-leer']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Suche_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            mock.Setup(s => s.SearchRezepteAsync("Pasta"))
                .ThrowsAsync(new InvalidOperationException("Timeout"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-suche']").Change("Pasta");
            cut.Find("[data-testid='btn-suche']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler bei der Suche", alert.TextContent);
                Assert.Contains("Timeout", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestaetigungsmodal_Oeffnen_Beim_Loeschen_Klick()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='btn-löschen-1']").Click();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='modal-bestätigung']"));
            mock.Verify(s => s.DeleteRezeptAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Rezept_Nach_Bestaetigung_Loeschen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            mock.Setup(s => s.DeleteRezeptAsync(1))
                .Returns(Task.CompletedTask);
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='btn-löschen-1']").Click();
            cut.Find("[data-testid='btn-löschen-bestätigt']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.DeleteRezeptAsync(1), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Loeschen_Die_ID_Der_Angeklickten_Zeile_Verwenden()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(2));
            mock.Setup(s => s.DeleteRezeptAsync(2))
                .Returns(Task.CompletedTask);
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[data-testid^='zeile-']").Count), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='btn-löschen-2']").Click();
            cut.Find("[data-testid='btn-löschen-bestätigt']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.DeleteRezeptAsync(2), Times.Once);
                mock.Verify(s => s.DeleteRezeptAsync(1), Times.Never);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beim_Abbrechen_Im_Modal_Nicht_Loeschen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='btn-löschen-1']").Click();

            // Act
            cut.Find("[data-testid='btn-abbrechen']").Click();

            // Assert
            Assert.Empty(cut.FindAll("[data-testid='modal-bestätigung']"));
            mock.Verify(s => s.DeleteRezeptAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Liste_Nach_Loeschen_Neu_Laden()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            mock.Setup(s => s.DeleteRezeptAsync(1))
                .Returns(Task.CompletedTask);
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='btn-löschen-1']").Click();

            // Act
            cut.Find("[data-testid='btn-löschen-bestätigt']").Click();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.GetAllRezepteAsync(), Times.Exactly(2)), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Loeschen_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetAllRezepteAsync())
                .ReturnsAsync(CreateRezepte(1));
            mock.Setup(s => s.DeleteRezeptAsync(1))
                .ThrowsAsync(new InvalidOperationException("Gesperrt"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptListe> cut = RenderComponent<RezeptListe>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='zeile-']")), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='btn-löschen-1']").Click();

            // Act
            cut.Find("[data-testid='btn-löschen-bestätigt']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler beim Löschen", alert.TextContent);
                Assert.Contains("Gesperrt", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        private static Rezept CreateRezept(int id, string name, string schwierigkeitsgrad = SchwierigkeitsgradAnzeige.Medium)
        {
            return new Rezept
            {
                Id = id,
                Name = name,
                Portionen = 2,
                ZubereitungszeitMinuten = 20,
                Schwierigkeitsgrad = schwierigkeitsgrad
            };
        }

        private static List<Rezept> CreateRezepte(int anzahl)
        {
            List<Rezept> rezepte = new();
            for (int i = 1; i <= anzahl; i++)
            {
                rezepte.Add(CreateRezept(i, $"Rezept {i}"));
            }
            return rezepte;
        }
    }
}

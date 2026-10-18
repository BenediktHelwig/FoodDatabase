using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FoodDatabase.App.Components.Pages.Rezepte;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Exceptions;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für RezeptForm.razor (UC4: Rezepte verwalten).
    /// Testet: Neu-/Bearbeiten-Modus, Felder und Request-Inhalt, Navigation, Abbrechen-Ziele,
    /// Fehlerpfade (Validierung, Duplikat, nicht gefunden) und die Kopie-Bindung beim Bearbeiten.
    /// </summary>
    public class RezeptFormTests : TestContext
    {
        public RezeptFormTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Neu_Formular_Mit_Portionen_1_Starten()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement portionen = cut.Find("[data-testid='input-portionen']");
                Assert.Equal("1", portionen.GetAttribute("value"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neu_Formular_Mit_Schwierigkeitsgrad_Mittel_Starten()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement select = cut.Find("[data-testid='select-schwierigkeitsgrad']");
                Assert.Equal(SchwierigkeitsgradAnzeige.Medium, select.GetAttribute("value"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Schwierigkeitsgrad_Select_Genau_Drei_Deutsche_Optionen_Haben()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> optionen = cut.FindAll("[data-testid='select-schwierigkeitsgrad'] option");
                Assert.Equal(3, optionen.Count);
                Assert.Equal("Leicht", optionen[0].TextContent.Trim());
                Assert.Equal("Mittel", optionen[1].TextContent.Trim());
                Assert.Equal("Schwer", optionen[2].TextContent.Trim());
                Assert.Equal(SchwierigkeitsgradAnzeige.Easy, optionen[0].GetAttribute("value"));
                Assert.Equal(SchwierigkeitsgradAnzeige.Medium, optionen[1].GetAttribute("value"));
                Assert.Equal(SchwierigkeitsgradAnzeige.Hard, optionen[2].GetAttribute("value"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neu_Ruft_Keinen_Ladevorgang_Auf()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            RenderComponent<RezeptForm>();

            // Assert
            mock.Verify(s => s.GetRezeptByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Neues_Rezept_Mit_Eingegebenen_Werten_An_Service_Uebergeben()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.CreateRezeptAsync(It.IsAny<CreateRezeptRequest>()))
                .ReturnsAsync(CreateRezept(7));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-name']").Change("Pasta");
            cut.Find("[data-testid='input-beschreibung']").Change("Einfach");
            cut.Find("[data-testid='input-zubereitung']").Change("Kochen");
            cut.Find("[data-testid='input-portionen']").Change("2");
            cut.Find("[data-testid='input-zubereitungszeit']").Change("15");
            cut.Find("[data-testid='select-schwierigkeitsgrad']").Change(SchwierigkeitsgradAnzeige.Hard);
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.CreateRezeptAsync(It.Is<CreateRezeptRequest>(r =>
                r.Name == "Pasta"
                && r.Beschreibung == "Einfach"
                && r.Zubereitung == "Kochen"
                && r.Portionen == 2
                && r.ZubereitungszeitMinuten == 15
                && r.Schwierigkeitsgrad == SchwierigkeitsgradAnzeige.Hard)), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nach_Neu_Speichern_Zur_Detailseite_Des_Neuen_Rezepts_Navigieren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.CreateRezeptAsync(It.IsAny<CreateRezeptRequest>()))
                .ReturnsAsync(CreateRezept(7));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='input-name']").Change("Pasta");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() => Assert.EndsWith("/rezepte/7", nav.Uri), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Formular_Mit_Geladenen_Werten_Vorbelegen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("Spaghetti Carbonara", cut.Find("[data-testid='input-name']").GetAttribute("value"));
                Assert.Equal("4", cut.Find("[data-testid='input-portionen']").GetAttribute("value"));
                Assert.Equal("30", cut.Find("[data-testid='input-zubereitungszeit']").GetAttribute("value"));
                Assert.Equal(SchwierigkeitsgradAnzeige.Easy, cut.Find("[data-testid='select-schwierigkeitsgrad']").GetAttribute("value"));
                Assert.Equal("Italienisch", cut.Find("[data-testid='input-beschreibung']").GetAttribute("value"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Request_Die_Geaenderten_Werte_Enthalten()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            mock.Setup(s => s.UpdateRezeptAsync(1, It.IsAny<UpdateRezeptRequest>()))
                .ReturnsAsync(CreateRezept(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-name']").Change("Carbonara Neu");
            cut.Find("[data-testid='input-portionen']").Change("6");
            cut.Find("[data-testid='select-schwierigkeitsgrad']").Change(SchwierigkeitsgradAnzeige.Hard);
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() => mock.Verify(s => s.UpdateRezeptAsync(1, It.Is<UpdateRezeptRequest>(r =>
                r.Name == "Carbonara Neu"
                && r.Portionen == 6
                && r.Schwierigkeitsgrad == SchwierigkeitsgradAnzeige.Hard)), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nach_Bearbeiten_Speichern_Zur_Detailseite_Navigieren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            mock.Setup(s => s.UpdateRezeptAsync(1, It.IsAny<UpdateRezeptRequest>()))
                .ReturnsAsync(CreateRezept(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() => Assert.EndsWith("/rezepte/1", nav.Uri), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Abbrechen_Bei_Neu_Zur_Liste_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();

            // Act & Assert
            cut.WaitForAssertion(() =>
            {
                IElement abbrechen = cut.Find("[data-testid='btn-abbrechen']");
                Assert.Equal("/rezepte", abbrechen.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Abbrechen_Bei_Bearbeiten_Zum_Detail_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));

            // Act & Assert
            cut.WaitForAssertion(() =>
            {
                IElement abbrechen = cut.Find("[data-testid='btn-abbrechen']");
                Assert.Equal("/rezepte/1", abbrechen.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Validierungsfehler_Beim_Anlegen_Anzeigen_Ohne_Navigation()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.CreateRezeptAsync(It.IsAny<CreateRezeptRequest>()))
                .ThrowsAsync(new ValidationException("Name darf nicht leer sein"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            string uriVorher = nav.Uri;
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Name darf nicht leer sein", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
            Assert.Equal(uriVorher, nav.Uri);
        }

        [Fact]
        public void Sollte_Duplikat_Beim_Anlegen_Anzeigen_Ohne_Navigation()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.CreateRezeptAsync(It.IsAny<CreateRezeptRequest>()))
                .ThrowsAsync(new DuplicateRezeptException("Rezept 'Pasta' existiert bereits"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            string uriVorher = nav.Uri;
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("[data-testid='input-name']").Change("Pasta");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("existiert bereits", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
            Assert.Equal(uriVorher, nav.Uri);
        }

        [Fact]
        public void Sollte_Validierungsfehler_Beim_Bearbeiten_Anzeigen_Ohne_Navigation()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            mock.Setup(s => s.UpdateRezeptAsync(1, It.IsAny<UpdateRezeptRequest>()))
                .ThrowsAsync(new ValidationException("Portionen muss zwischen 1 und 100 liegen"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            string uriVorher = nav.Uri;
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("zwischen 1 und 100", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
            Assert.Equal(uriVorher, nav.Uri);
        }

        [Fact]
        public void Sollte_NotFound_Beim_Bearbeiten_Anzeigen_Ohne_Navigation()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            mock.Setup(s => s.UpdateRezeptAsync(1, It.IsAny<UpdateRezeptRequest>()))
                .ThrowsAsync(new NotFoundException("Rezept 1 wurde inzwischen gelöscht"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
            string uriVorher = nav.Uri;
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("wurde inzwischen gelöscht", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
            Assert.Equal(uriVorher, nav.Uri);
        }

        [Fact]
        public void Sollte_Allgemeinen_Fehler_Beim_Speichern_Anzeigen()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.CreateRezeptAsync(It.IsAny<CreateRezeptRequest>()))
                .ThrowsAsync(new Exception("Datenbank voll"));
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler beim Speichern", alert.TextContent);
                Assert.Contains("Datenbank voll", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nicht_Gefunden_Anzeigen_Wenn_Rezept_Fehlt()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(99))
                .ReturnsAsync((Rezept)null!);
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 99));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Rezept nicht gefunden", cut.Markup);
                Assert.Empty(cut.FindAll("[data-testid='input-name']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Laden_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ThrowsAsync(new InvalidOperationException("Verbindung verloren"));
            Services.AddSingleton<IRezeptService>(mock.Object);

            // Act
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler beim Laden des Rezepts", alert.TextContent);
                Assert.Contains("Verbindung verloren", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Eingabe_Das_Geladene_Rezept_Nicht_Veraendern()
        {
            // Arrange
            Rezept geladen = CreateRezept(1);
            Mock<IRezeptService> mock = new();
            mock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(geladen);
            Services.AddSingleton<IRezeptService>(mock.Object);
            IRenderedComponent<RezeptForm> cut = RenderComponent<RezeptForm>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("form")), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='input-name']").Change("Geändert");
            cut.Find("[data-testid='input-portionen']").Change("9");

            // Guard: die Eingabe muss im Formular angekommen sein, sonst wäre der Test wertlos.
            Assert.Equal("Geändert", cut.Find("[data-testid='input-name']").GetAttribute("value"));

            // Act: Abbrechen ist ein reiner Link ohne Handler. Das Verlassen der Seite verwirft die Eingaben,
            // das Rezept-Objekt des Services darf dabei nie verändert worden sein.

            // Assert
            Assert.Equal("Spaghetti Carbonara", geladen.Name);
            Assert.Equal(4, geladen.Portionen);
        }

        private static Rezept CreateRezept(int id)
        {
            return new Rezept
            {
                Id = id,
                Name = "Spaghetti Carbonara",
                Beschreibung = "Italienisch",
                Zubereitung = "1. Wasser aufkochen",
                Portionen = 4,
                ZubereitungszeitMinuten = 30,
                Schwierigkeitsgrad = SchwierigkeitsgradAnzeige.Easy
            };
        }
    }
}

using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components;
using FoodDatabase.App.Components.Pages.Rezepte;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für RezeptDetail.razor (UC4: Rezepte verwalten, Detailansicht mit Zutaten (nur lesend)).
    /// Testet: Stammdaten, Zutaten-Reihenfolge und Lebensmittel-Namensauflösung, Nährwertanzeige
    /// nur bei vorhandenen Zutaten, Fehler- und Nicht-gefunden-Zustand.
    /// </summary>
    public class RezeptDetailTests : TestContext
    {
        public RezeptDetailTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Titel_Mit_Rezeptnamen_Anzeigen()
        {
            // Arrange
            RegistriereServices(CreateRezept(1), new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() => Assert.Equal("Spaghetti Carbonara", cut.Find("[data-testid='titel']").TextContent.Trim()), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Stammdaten_Portionen_Zeit_Und_Schwierigkeit_Anzeigen()
        {
            // Arrange
            RegistriereServices(CreateRezept(1), new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("4", cut.Find("[data-testid='wert-portionen']").TextContent.Trim());
                Assert.Equal("30 min", cut.Find("[data-testid='wert-zeit']").TextContent.Trim());
                Assert.Equal("Mittel", cut.Find("[data-testid='wert-schwierigkeit']").TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beschreibung_Und_Zubereitung_Anzeigen()
        {
            // Arrange
            RegistriereServices(CreateRezept(1), new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Italienisch", cut.Find("[data-testid='wert-beschreibung']").TextContent);
                Assert.Contains("1. Wasser aufkochen", cut.Find("[data-testid='wert-zubereitung']").TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beschreibung_Ausblenden_Wenn_Leer()
        {
            // Arrange
            Rezept rezept = CreateRezept(1);
            rezept.Beschreibung = null;
            RegistriereServices(rezept, new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("Spaghetti Carbonara", cut.Find("[data-testid='titel']").TextContent.Trim());
                Assert.Empty(cut.FindAll("[data-testid='wert-beschreibung']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Zutaten_In_Positionsreihenfolge_Anzeigen()
        {
            // Arrange: Die Liste kommt bereits nach Position sortiert vom Service; die IDs sind absichtlich nicht aufsteigend.
            List<RezeptZutat> zutaten = new()
            {
                CreateZutat(id: 9, position: 0, lebensmittelName: "Mehl"),
                CreateZutat(id: 3, position: 1, lebensmittelName: "Zucker"),
                CreateZutat(id: 5, position: 2, lebensmittelName: "Salz")
            };
            RegistriereServices(CreateRezept(1), zutaten);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                List<string> namen = cut.FindAll("[data-testid^='zeile-zutat-'] td:nth-child(1)")
                    .Select(td => td.TextContent.Trim())
                    .ToList();
                Assert.Equal(new[] { "Mehl", "Zucker", "Salz" }, namen);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Lebensmittelnamen_Ueber_Navigation_Aufloesen()
        {
            // Arrange
            List<RezeptZutat> zutaten = new()
            {
                CreateZutat(id: 1, position: 0, lebensmittelName: "Spaghetti Hartweizen")
            };
            RegistriereServices(CreateRezept(1), zutaten);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IHtmlCollection<IElement> cells = cut.Find("[data-testid='zeile-zutat-1']").QuerySelectorAll("td");
                Assert.Equal("Spaghetti Hartweizen", cells[0].TextContent.Trim());
                Assert.Equal("200", cells[1].TextContent.Trim());
                Assert.Equal("g", cells[2].TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Unbekanntes_Lebensmittel_Anzeigen_Wenn_Navigation_Fehlt()
        {
            // Arrange
            List<RezeptZutat> zutaten = new()
            {
                CreateZutat(id: 1, position: 0, lebensmittelName: null)
            };
            RegistriereServices(CreateRezept(1), zutaten);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement erstesFeld = cut.Find("[data-testid='zeile-zutat-1'] td");
                Assert.Equal("Unbekanntes Lebensmittel", erstesFeld.TextContent.Trim());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Hinweis_Statt_Naehrwertanzeige_Zeigen_Wenn_Keine_Zutaten_Vorhanden()
        {
            // Arrange
            RegistriereServices(CreateRezept(1), new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Single(cut.FindAll("[data-testid='hinweis-keine-zutaten']"));
                Assert.Empty(cut.FindComponents<RezeptNährwertAnzeige>());
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Naehrwertanzeige_Mit_Rezept_Id_Anzeigen_Wenn_Zutaten_Vorhanden()
        {
            // Arrange
            List<RezeptZutat> zutaten = new()
            {
                CreateZutat(id: 1, position: 0, lebensmittelName: "Mehl")
            };
            RegistriereServices(CreateRezept(1), zutaten);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IRenderedComponent<RezeptNährwertAnzeige>> naehrwerte = cut.FindComponents<RezeptNährwertAnzeige>();
                Assert.Single(naehrwerte);
                Assert.Equal(1, naehrwerte[0].Instance.RezeptId);
                Assert.Empty(cut.FindAll("[data-testid='hinweis-keine-zutaten']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Naehrwertanzeige_Portionen_Vom_Rezept_Uebernehmen()
        {
            // Arrange: bewusst ein anderer Wert als der Standard aus CreateRezept (4)
            Rezept rezept = CreateRezept(1);
            rezept.Portionen = 6;
            List<RezeptZutat> zutaten = new()
            {
                CreateZutat(id: 1, position: 0, lebensmittelName: "Mehl")
            };
            RegistriereServices(rezept, zutaten);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IRenderedComponent<RezeptNährwertAnzeige> naehrwerte = cut.FindComponent<RezeptNährwertAnzeige>();
                Assert.Equal(6, naehrwerte.Instance.Portionen);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nicht_Gefunden_Anzeigen_Wenn_Rezept_Fehlt()
        {
            // Arrange
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(99))
                .ReturnsAsync((Rezept)null!);
            RegistriereMocks(rezeptMock, new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 99));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement hinweis = cut.Find("[data-testid='nicht-gefunden']");
                Assert.Contains("Rezept nicht gefunden", hinweis.TextContent);
                Assert.Empty(cut.FindAll("[data-testid='titel']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Zurueck_Button_Bei_Nicht_Gefundenem_Rezept_Zur_Liste_Fuehren()
        {
            // Arrange
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(99))
                .ReturnsAsync((Rezept)null!);
            RegistriereMocks(rezeptMock, new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 99));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement zurueck = cut.Find("[data-testid='btn-zurück']");
                Assert.Equal("/rezepte", zurueck.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Rezept_Laden_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(1))
                .ThrowsAsync(new InvalidOperationException("Verbindung verloren"));
            RegistriereMocks(rezeptMock, new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Fehler beim Laden des Rezepts", alert.TextContent);
                Assert.Contains("Verbindung verloren", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Zutaten_Laden_Fehlschlaegt()
        {
            // Arrange
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(1))
                .ReturnsAsync(CreateRezept(1));
            Mock<IRezeptZutatService> zutatMock = new();
            zutatMock.Setup(s => s.GetZutatenAsync(1))
                .ThrowsAsync(new InvalidOperationException("Zutaten nicht lesbar"));
            RegistriereMocks(rezeptMock, zutatMock);

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement alert = cut.Find("[data-testid='alert-fehler']");
                Assert.Contains("Zutaten nicht lesbar", alert.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Ladeindikator_Waehrend_Des_Ladens_Zeigen()
        {
            // Arrange
            TaskCompletionSource<Rezept> tcs = new();
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(1))
                .Returns(tcs.Task);
            RegistriereMocks(rezeptMock, new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert: solange das Rezept lädt, ist der Ladezustand sichtbar und die Stammdaten fehlen
            Assert.Contains("Lädt...", cut.Markup);
            Assert.Empty(cut.FindAll("[data-testid='titel']"));

            cut.InvokeAsync(() => tcs.SetResult(CreateRezept(1)));
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("Spaghetti Carbonara", cut.Find("[data-testid='titel']").TextContent.Trim());
                Assert.DoesNotContain("Lädt...", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Button_Zur_Bearbeitungsseite_Fuehren()
        {
            // Arrange
            RegistriereServices(CreateRezept(1), new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement bearbeiten = cut.Find("[data-testid='btn-bearbeiten']");
                Assert.Equal("/rezepte/1/bearbeiten", bearbeiten.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        private void RegistriereServices(Rezept rezept, List<RezeptZutat> zutaten)
        {
            Mock<IRezeptService> rezeptMock = new();
            rezeptMock.Setup(s => s.GetRezeptByIdAsync(rezept.Id))
                .ReturnsAsync(rezept);
            RegistriereMocks(rezeptMock, zutaten);
        }

        private void RegistriereMocks(Mock<IRezeptService> rezeptMock, List<RezeptZutat> zutaten)
        {
            Mock<IRezeptZutatService> zutatMock = new();
            zutatMock.Setup(s => s.GetZutatenAsync(It.IsAny<int>()))
                .ReturnsAsync(zutaten);
            RegistriereMocks(rezeptMock, zutatMock);
        }

        private void RegistriereMocks(Mock<IRezeptService> rezeptMock, Mock<IRezeptZutatService> zutatMock)
        {
            // Nährwert-Service immer registrieren: RezeptNährwertAnzeige wird ggf. eingebettet.
            Mock<IRezeptNährwertService> naehrwertMock = new();
            naehrwertMock.Setup(s => s.GetRezeptNährwerteAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateNaehrwerte());

            Services.AddSingleton<IRezeptService>(rezeptMock.Object);
            Services.AddSingleton<IRezeptZutatService>(zutatMock.Object);
            Services.AddSingleton<IRezeptNährwertService>(naehrwertMock.Object);
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
                Schwierigkeitsgrad = SchwierigkeitsgradAnzeige.Medium
            };
        }

        private static RezeptZutat CreateZutat(int id, int position, string? lebensmittelName)
        {
            RezeptZutat zutat = new()
            {
                Id = id,
                RezeptId = 1,
                LebensmittelId = id,
                Menge = 200,
                Einheit = "g",
                Notizen = null,
                Position = position
            };

            // Ohne Name bleibt die Navigation leer (Fallback „Unbekanntes Lebensmittel“ im Detail).
            if (lebensmittelName is not null)
            {
                zutat.Lebensmittel = new LebensmittelKatalog { Id = id, Name = lebensmittelName };
            }

            return zutat;
        }

        private static RezeptNährwerteDto CreateNaehrwerte()
        {
            return new RezeptNährwerteDto
            {
                RezeptId = 1,
                GesamtnährwerteDto = new NährwerteDto { Kalorien = 1000 },
                ProPortionNährwerteDto = new NährwerteDto { Kalorien = 250 }
            };
        }
    }
}

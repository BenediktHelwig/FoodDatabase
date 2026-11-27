using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
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
    /// bUnit Tests für RezeptDetail.razor (UC4: Rezepte verwalten, Detailansicht mit Zutaten-Bearbeitungsmodus).
    /// Testet: Stammdaten, Zutaten-Reihenfolge und Lebensmittel-Namensauflösung, Nährwertanzeige
    /// nur bei vorhandenen Zutaten, Fehler- und Nicht-gefunden-Zustand, Zutaten-CRUD (Modus,
    /// Hinzufügen, Entfernen, Validierung, Speicher-Reihenfolge, Abbrechen, Lösch-Modal, Service-Fehler).
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

        // ============ Zutaten bearbeiten (UC4 WP5 Teil 2) ============

        [Fact]
        public void Sollte_Zutaten_Bearbeiten_Button_In_Der_Ansicht_Zeigen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });

            // Act
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Single(cut.FindAll("[data-testid='btn-zutaten-bearbeiten']"));
                Assert.Single(cut.FindAll("[data-testid='tabelle-zutaten']"));
                Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten-bearbeiten']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Menge_Einheit_Und_Notizen_Editierbar_Machen_Im_Bearbeitungsmodus()
        {
            // Arrange
            RezeptZutat zutat = CreateZutat(1, 0, "Mehl");
            zutat.Notizen = "fein gesiebt";
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { zutat });

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            Assert.Equal("200", cut.Find("[data-testid='input-menge-0']").GetAttribute("value"));
            Assert.Equal("g", cut.Find("[data-testid='select-einheit-0']").GetAttribute("value"));
            Assert.Equal("fein gesiebt", cut.Find("[data-testid='input-notizen-0']").GetAttribute("value"));
            Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten']"));
        }

        [Fact]
        public void Sollte_Lebensmittel_Im_Bearbeitungsmodus_Als_Text_Und_Nicht_Editierbar_Zeigen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            IElement zelle = cut.Find("[data-testid='text-lebensmittel-0']");
            Assert.Equal("Mehl", zelle.TextContent.Trim());
            Assert.Empty(zelle.QuerySelectorAll("input, select"));
        }

        [Fact]
        public void Sollte_Bearbeitungsmodus_Auch_Ohne_Zutaten_Oeffnen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='btn-zutaten-speichern']"));
            Assert.Single(cut.FindAll("[data-testid='eingabezeile']"));
            Assert.Empty(cut.FindAll("[data-testid^='edit-zeile-']"));
        }

        [Fact]
        public void Sollte_Lebensmittel_Dropdown_Nach_Name_Sortiert_Zeigen()
        {
            // Arrange: Der Katalog kommt absichtlich unsortiert vom Service.
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            List<string> namen = cut.FindAll("[data-testid='select-lebensmittel-neu'] option")
                .Select(o => o.TextContent.Trim())
                .Skip(1)
                .ToList();
            Assert.Equal(new[] { "Apfel", "Mehl", "Zucker" }, namen);
        }

        [Fact]
        public void Sollte_Einheiten_Dropdown_Vollstaendig_Mit_Default_Gramm_Zeigen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            List<string> einheiten = cut.FindAll("[data-testid='select-einheit-neu'] option")
                .Select(o => o.TextContent.Trim())
                .ToList();
            Assert.Equal(new[] { "g", "ml", "Stück", "TL", "EL", "Tasse", "Prise" }, einheiten);
            Assert.Equal("g", cut.Find("[data-testid='select-einheit-neu']").GetAttribute("value"));
        }

        [Fact]
        public void Sollte_Zeile_Lokal_Anhaengen_Und_Eingabezeile_Leeren_Beim_Plus()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Act
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "1.5", einheit: "EL", notiz: "gehäuft");

            // Assert
            Assert.Equal(2, cut.FindAll("[data-testid^='edit-zeile-']").Count);
            Assert.Equal("Apfel", cut.Find("[data-testid='text-lebensmittel-1']").TextContent.Trim());
            Assert.Equal("1.5", cut.Find("[data-testid='input-menge-1']").GetAttribute("value"));
            Assert.Equal("EL", cut.Find("[data-testid='select-einheit-1']").GetAttribute("value"));
            Assert.Equal("gehäuft", cut.Find("[data-testid='input-notizen-1']").GetAttribute("value"));
            Assert.Equal("0", cut.Find("[data-testid='select-lebensmittel-neu']").GetAttribute("value"));
            Assert.True(string.IsNullOrEmpty(cut.Find("[data-testid='input-menge-neu']").GetAttribute("value")));
            Assert.Equal("g", cut.Find("[data-testid='select-einheit-neu']").GetAttribute("value"));
            Assert.True(string.IsNullOrEmpty(cut.Find("[data-testid='input-notizen-neu']").GetAttribute("value")));
            umgebung.Zutat.Verify(s => s.AddZutatAsync(It.IsAny<int>(), It.IsAny<AddZutatRequest>()), Times.Never);
        }

        [Fact]
        public void Sollte_Dasselbe_Lebensmittel_Mehrfach_Hinzufuegen_Koennen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(10, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Act
            FügeZeileHinzu(cut, lebensmittelId: "10", menge: "50", einheit: "g", notiz: "");
            FügeZeileHinzu(cut, lebensmittelId: "10", menge: "75", einheit: "g", notiz: "");

            // Assert
            Assert.Equal(3, cut.FindAll("[data-testid^='edit-zeile-']").Count);
            Assert.Empty(cut.FindAll("[data-testid='fehler-eingabezeile']"));
        }

        [Fact]
        public void Sollte_Zeile_Ohne_Meldung_Entfernen_Auch_Die_Letzte()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Act
            cut.Find("[data-testid='btn-entfernen-0']").Click();

            // Assert
            Assert.Empty(cut.FindAll("[data-testid^='edit-zeile-']"));
            Assert.Empty(cut.FindAll("[data-testid='alert-fehler']"));
            Assert.Empty(cut.FindAll("[data-testid^='fehler-']"));
            Assert.Empty(cut.FindAll("[data-testid='modal-rezept-löschen']"));
            umgebung.Zutat.Verify(s => s.DeleteZutatAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Beim_Plus_Ohne_Lebensmittel_Meldung_Zeigen_Und_Nichts_Hinzufuegen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-menge-neu']").Change("100");

            // Act
            cut.Find("[data-testid='btn-hinzufügen']").Click();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='fehler-eingabezeile']"));
            Assert.Empty(cut.FindAll("[data-testid^='edit-zeile-']"));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("10000.01")]
        public void Sollte_Beim_Plus_Mit_Menge_Ausserhalb_Der_Grenzen_Meldung_Zeigen(string menge)
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='select-lebensmittel-neu']").Change("10");
            cut.Find("[data-testid='input-menge-neu']").Change(menge);

            // Act
            cut.Find("[data-testid='btn-hinzufügen']").Click();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='fehler-eingabezeile']"));
            Assert.Empty(cut.FindAll("[data-testid^='edit-zeile-']"));
        }

        [Theory]
        [InlineData("0.01")]
        [InlineData("10000")]
        public void Sollte_Beim_Plus_Mengen_An_Den_Grenzen_Akzeptieren(string menge)
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>());
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Act
            FügeZeileHinzu(cut, lebensmittelId: "10", menge: menge, einheit: "g", notiz: "");

            // Assert
            Assert.Single(cut.FindAll("[data-testid^='edit-zeile-']"));
            Assert.Empty(cut.FindAll("[data-testid='fehler-eingabezeile']"));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("10000.01")]
        public void Sollte_Beim_Speichern_Ungueltige_Bestehende_Zeile_An_Der_Zeile_Melden_Und_Keinen_Service_Rufen(string menge)
        {
            // Arrange: Zeile 0 wird ungültig editiert, Zeile 1 bleibt gültig.
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>
            {
                CreateZutat(1, 0, "Mehl"),
                CreateZutat(2, 1, "Zucker")
            });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-menge-0']").Change(menge);

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='fehler-zeile-0']"));
            Assert.Empty(cut.FindAll("[data-testid='fehler-zeile-1']"));
            VerifiziereKeineSchreibaufrufe(umgebung);
            Assert.Single(cut.FindAll("[data-testid='btn-zutaten-speichern']"));
        }

        [Fact]
        public void Sollte_Beim_Speichern_Ungueltige_Neue_Zeile_Nicht_Zum_Service_Schicken()
        {
            // Arrange: Menge wird nach dem Hinzufügen auf 0 gesetzt.
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");
            cut.Find("[data-testid='input-menge-1']").Change("0");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            Assert.Single(cut.FindAll("[data-testid='fehler-zeile-1']"));
            VerifiziereKeineSchreibaufrufe(umgebung);
        }

        [Fact]
        public void Sollte_Beim_Speichern_Add_Dann_Update_Dann_Delete_Aufrufen()
        {
            // Arrange: Zeile 1 unverändert, Zeile 2 geändert, Zeile 3 entfernt, eine neue Zeile.
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>
            {
                CreateZutat(1, 0, "Mehl"),
                CreateZutat(2, 1, "Zucker"),
                CreateZutat(3, 2, "Salz")
            });
            List<string> aufrufe = new();
            umgebung.Zutat.Setup(s => s.AddZutatAsync(1, It.IsAny<AddZutatRequest>()))
                .Callback(() => aufrufe.Add("Add"))
                .ReturnsAsync(new RezeptZutat());
            umgebung.Zutat.Setup(s => s.UpdateZutatAsync(1, It.IsAny<int>(), It.IsAny<UpdateZutatRequest>()))
                .Callback(() => aufrufe.Add("Update"))
                .ReturnsAsync(new RezeptZutat());
            umgebung.Zutat.Setup(s => s.DeleteZutatAsync(1, It.IsAny<int>()))
                .Callback(() => aufrufe.Add("Delete"))
                .Returns(Task.CompletedTask);
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-2']").Click();
            cut.Find("[data-testid='input-menge-1']").Change("999");
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Equal(new[] { "Add", "Update", "Delete" }, aufrufe), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beim_Speichern_Nur_Geaenderte_Zeilen_Updaten()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>
            {
                CreateZutat(1, 0, "Mehl"),
                CreateZutat(2, 1, "Zucker")
            });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-menge-1']").Change("350");
            cut.Find("[data-testid='select-einheit-1']").Change("ml");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                umgebung.Zutat.Verify(s => s.UpdateZutatAsync(1, 1, It.IsAny<UpdateZutatRequest>()), Times.Never);
                umgebung.Zutat.Verify(s => s.UpdateZutatAsync(
                    1,
                    2,
                    It.Is<UpdateZutatRequest>(r => r.Menge == 350 && r.Einheit == "ml")), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Geleerte_Notiz_Als_Leerstring_Statt_Null_Speichern()
        {
            // Arrange
            RezeptZutat zutat = CreateZutat(1, 0, "Mehl");
            zutat.Notizen = "fein gesiebt";
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { zutat });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-notizen-0']").Change("");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
                umgebung.Zutat.Verify(s => s.UpdateZutatAsync(
                    1,
                    1,
                    It.Is<UpdateZutatRequest>(r => r.Notizen == "")), Times.Once), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neue_Zeile_Mit_Allen_Feldern_An_AddZutat_Uebergeben()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            FügeZeileHinzu(cut, lebensmittelId: "12", menge: "2.5", einheit: "TL", notiz: "");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
                umgebung.Zutat.Verify(s => s.AddZutatAsync(
                    1,
                    It.Is<AddZutatRequest>(r => r.LebensmittelId == 12 && r.Menge == 2.5 && r.Einheit == "TL" && r.Notizen == "")), Times.Once),
                TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nach_Erfolgreichem_Speichern_Neu_Laden_Und_Modus_Verlassen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-menge-0']").Change("300");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                umgebung.Zutat.Verify(s => s.GetZutatenAsync(1), Times.Exactly(2));
                Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten-bearbeiten']"));
                Assert.Single(cut.FindAll("[data-testid='tabelle-zutaten']"));
                Assert.Single(cut.FindAll("[data-testid='btn-zutaten-bearbeiten']"));
                Assert.Empty(cut.FindAll("[data-testid='alert-fehler']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beim_Abbrechen_Aenderungen_Verwerfen_Und_Nichts_Schreiben()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>
            {
                CreateZutat(1, 0, "Mehl"),
                CreateZutat(2, 1, "Zucker")
            });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='input-menge-0']").Change("999");
            cut.Find("[data-testid='btn-entfernen-1']").Click();
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");

            // Act
            cut.Find("[data-testid='btn-zutaten-abbrechen']").Click();

            // Assert
            Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten-bearbeiten']"));
            List<string> namen = cut.FindAll("[data-testid^='zeile-zutat-'] td:nth-child(1)")
                .Select(td => td.TextContent.Trim())
                .ToList();
            Assert.Equal(new[] { "Mehl", "Zucker" }, namen);
            Assert.Equal("200", cut.Find("[data-testid='zeile-zutat-1'] td:nth-child(2)").TextContent.Trim());
            VerifiziereKeineSchreibaufrufe(umgebung);
        }

        [Fact]
        public void Sollte_Beim_Erneuten_Oeffnen_Nach_Abbrechen_Den_DB_Stand_Zeigen()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-0']").Click();
            cut.Find("[data-testid='btn-zutaten-abbrechen']").Click();

            // Act
            cut.Find("[data-testid='btn-zutaten-bearbeiten']").Click();

            // Assert
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid^='edit-zeile-']")), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Beim_Speichern_Leerer_Liste_Modal_Zeigen_Ohne_Service_Aufruf()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-0']").Click();

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            IElement modal = cut.Find("[data-testid='modal-rezept-löschen']");
            Assert.Contains("Das Rezept hat keine Zutaten mehr. Wenn du das Speichern bestätigst, wird das Rezept gelöscht.", modal.TextContent);
            VerifiziereKeineSchreibaufrufe(umgebung);
            umgebung.Rezept.Verify(s => s.DeleteRezeptAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Rezept_Loeschen_Und_Zur_Liste_Navigieren_Wenn_Modal_Bestaetigt()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-0']").Click();
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Act
            cut.Find("[data-testid='btn-rezept-löschen']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                umgebung.Rezept.Verify(s => s.DeleteRezeptAsync(1), Times.Once);
                FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();
                Assert.EndsWith("/rezepte", nav.Uri);
            }, TimeSpan.FromSeconds(2));
            umgebung.Zutat.Verify(s => s.DeleteZutatAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Modal_Schliessen_Und_Modus_Offen_Lassen_Beim_Zurueck()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-0']").Click();
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Act
            cut.Find("[data-testid='btn-modal-zurück']").Click();
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");

            // Assert
            Assert.Empty(cut.FindAll("[data-testid='modal-rezept-löschen']"));
            Assert.Single(cut.FindAll("[data-testid='edit-zeile-0']"));
            Assert.Single(cut.FindAll("[data-testid='btn-zutaten-speichern']"));
            umgebung.Rezept.Verify(s => s.DeleteRezeptAsync(It.IsAny<int>()), Times.Never);
            VerifiziereKeineSchreibaufrufe(umgebung);
        }

        [Fact]
        public void Sollte_Fehler_Zeigen_Neu_Laden_Und_Modus_Verlassen_Wenn_Lebensmittel_Nicht_Gefunden()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            umgebung.Zutat.Setup(s => s.AddZutatAsync(1, It.IsAny<AddZutatRequest>()))
                .ThrowsAsync(new FoodDatabase.App.Services.Exceptions.NotFoundException("Lebensmittel 11 nicht gefunden."));
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Lebensmittel 11 nicht gefunden.", cut.Find("[data-testid='alert-fehler']").TextContent);
                umgebung.Zutat.Verify(s => s.GetZutatenAsync(1), Times.Exactly(2));
                Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten-bearbeiten']"));
                Assert.Single(cut.FindAll("[data-testid='tabelle-zutaten']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Service_ValidationException_Beim_Speichern_Als_Fehler_Zeigen_Und_Abbrechen()
        {
            // Arrange: Der Add schlägt fehl, daher dürfen Update und Delete nicht mehr laufen.
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>
            {
                CreateZutat(1, 0, "Mehl"),
                CreateZutat(2, 1, "Zucker")
            });
            umgebung.Zutat.Setup(s => s.AddZutatAsync(1, It.IsAny<AddZutatRequest>()))
                .ThrowsAsync(new FoodDatabase.App.Services.Exceptions.ValidationException("Menge muss zwischen 0.01 und 10000 liegen."));
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.Find("[data-testid='btn-entfernen-1']").Click();
            FügeZeileHinzu(cut, lebensmittelId: "11", menge: "5", einheit: "g", notiz: "");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Menge muss zwischen 0.01 und 10000 liegen.", cut.Find("[data-testid='alert-fehler']").TextContent);
                Assert.Empty(cut.FindAll("[data-testid='tabelle-zutaten-bearbeiten']"));
                umgebung.Zutat.Verify(s => s.GetZutatenAsync(1), Times.Exactly(2));
            }, TimeSpan.FromSeconds(2));
            umgebung.Zutat.Verify(s => s.DeleteZutatAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Sollte_Naehrwerte_Nach_Erfolgreichem_Speichern_Erneut_Laden()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat> { CreateZutat(1, 0, "Mehl") });
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();
            cut.WaitForAssertion(() =>
                umgebung.Naehrwert.Verify(s => s.GetRezeptNährwerteAsync(1), Times.Once), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='input-menge-0']").Change("300");

            // Act
            cut.Find("[data-testid='btn-zutaten-speichern']").Click();

            // Assert
            cut.WaitForAssertion(() =>
                umgebung.Naehrwert.Verify(s => s.GetRezeptNährwerteAsync(1), Times.Exactly(2)), TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Hinweis_Mit_Link_Zeigen_Wenn_Lebensmittel_Katalog_Leer()
        {
            // Arrange
            Umgebung umgebung = ErstelleUmgebung(new List<RezeptZutat>(), new List<LebensmittelKatalog>());

            // Act
            IRenderedComponent<RezeptDetail> cut = ÖffneBearbeitungsmodus();

            // Assert
            Assert.Empty(cut.FindAll("[data-testid='select-lebensmittel-neu']"));
            Assert.Single(cut.FindAll("[data-testid='hinweis-katalog-leer']"));
            Assert.Equal("/lebensmittel/neu", cut.Find("[data-testid='link-lebensmittel-neu']").GetAttribute("href"));
        }

        // ============ Hilfen für den Bearbeitungsmodus ============

        /// <summary>Alle Mocks einer Testumgebung, damit Tests Aufrufe prüfen und Verhalten nachrüsten können.</summary>
        private class Umgebung
        {
            public Mock<IRezeptService> Rezept { get; } = new();
            public Mock<IRezeptZutatService> Zutat { get; } = new();
            public Mock<IRezeptNährwertService> Naehrwert { get; } = new();
            public Mock<ILebensmittelService> Lebensmittel { get; } = new();
        }

        private Umgebung ErstelleUmgebung(List<RezeptZutat> zutaten, List<LebensmittelKatalog>? katalog = null)
        {
            // Der Katalog kommt absichtlich unsortiert, damit der Sortier-Test etwas beweist.
            katalog ??= new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 12, Name = "Zucker" },
                new LebensmittelKatalog { Id = 10, Name = "Mehl" },
                new LebensmittelKatalog { Id = 11, Name = "Apfel" }
            };

            Umgebung umgebung = new();
            umgebung.Rezept.Setup(s => s.GetRezeptByIdAsync(1)).ReturnsAsync(CreateRezept(1));
            umgebung.Zutat.Setup(s => s.GetZutatenAsync(1)).ReturnsAsync(zutaten);
            umgebung.Zutat.Setup(s => s.AddZutatAsync(1, It.IsAny<AddZutatRequest>())).ReturnsAsync(new RezeptZutat());
            umgebung.Zutat.Setup(s => s.UpdateZutatAsync(1, It.IsAny<int>(), It.IsAny<UpdateZutatRequest>())).ReturnsAsync(new RezeptZutat());
            umgebung.Zutat.Setup(s => s.DeleteZutatAsync(1, It.IsAny<int>())).Returns(Task.CompletedTask);
            umgebung.Naehrwert.Setup(s => s.GetRezeptNährwerteAsync(It.IsAny<int>())).ReturnsAsync(CreateNaehrwerte());
            umgebung.Lebensmittel.Setup(s => s.GetAllLebensmittelAsync()).ReturnsAsync(katalog);

            RegistriereMocks(umgebung.Rezept, umgebung.Zutat, umgebung.Naehrwert, umgebung.Lebensmittel);
            return umgebung;
        }

        private IRenderedComponent<RezeptDetail> ÖffneBearbeitungsmodus()
        {
            IRenderedComponent<RezeptDetail> cut = RenderComponent<RezeptDetail>(parameters => parameters.Add(p => p.Id, 1));
            cut.WaitForElement("[data-testid='btn-zutaten-bearbeiten']", TimeSpan.FromSeconds(2)).Click();
            cut.WaitForElement("[data-testid='btn-zutaten-speichern']", TimeSpan.FromSeconds(2));
            return cut;
        }

        private static void FügeZeileHinzu(IRenderedComponent<RezeptDetail> cut, string lebensmittelId, string menge, string einheit, string notiz)
        {
            cut.Find("[data-testid='select-lebensmittel-neu']").Change(lebensmittelId);
            cut.Find("[data-testid='input-menge-neu']").Change(menge);
            cut.Find("[data-testid='select-einheit-neu']").Change(einheit);
            cut.Find("[data-testid='input-notizen-neu']").Change(notiz);
            cut.Find("[data-testid='btn-hinzufügen']").Click();
        }

        private static void VerifiziereKeineSchreibaufrufe(Umgebung umgebung)
        {
            umgebung.Zutat.Verify(s => s.AddZutatAsync(It.IsAny<int>(), It.IsAny<AddZutatRequest>()), Times.Never);
            umgebung.Zutat.Verify(s => s.UpdateZutatAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<UpdateZutatRequest>()), Times.Never);
            umgebung.Zutat.Verify(s => s.DeleteZutatAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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

        private void RegistriereMocks(
            Mock<IRezeptService> rezeptMock,
            Mock<IRezeptZutatService> zutatMock,
            Mock<IRezeptNährwertService>? naehrwertMock = null,
            Mock<ILebensmittelService>? lebensmittelMock = null)
        {
            // Nährwert-Service immer registrieren: RezeptNährwertAnzeige wird ggf. eingebettet.
            if (naehrwertMock is null)
            {
                naehrwertMock = new();
                naehrwertMock.Setup(s => s.GetRezeptNährwerteAsync(It.IsAny<int>()))
                    .ReturnsAsync(CreateNaehrwerte());
            }

            // Lebensmittel-Service immer registrieren: RezeptDetail injiziert ihn für den Bearbeitungsmodus.
            if (lebensmittelMock is null)
            {
                lebensmittelMock = new();
                lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                    .ReturnsAsync(new List<LebensmittelKatalog>());
            }

            Services.AddSingleton<IRezeptService>(rezeptMock.Object);
            Services.AddSingleton<IRezeptZutatService>(zutatMock.Object);
            Services.AddSingleton<IRezeptNährwertService>(naehrwertMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
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

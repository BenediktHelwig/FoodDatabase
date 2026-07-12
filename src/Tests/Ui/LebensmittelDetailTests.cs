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
    /// bUnit Tests für LebensmittelDetail.razor Komponente (UC1 + UC3).
    /// Testet Funktionalität: Detail-Anzeige mit Nährwert-Integration.
    /// </summary>
    public class LebensmittelDetailTests : TestContext
    {
        public LebensmittelDetailTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Lebensmittel_Information_Anzeigen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .WithKategorie("Getreide")
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .WithStandardMengeEinheit("g")
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Mehl"));
                Assert.True(cut.Markup.Contains("Getreide"));
                Assert.True(cut.Markup.Contains("g"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Lebensmittel_Name_Im_Heading_Anzeigen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var heading = cut.Find("h2");
                Assert.Contains("Mehl", heading.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Button_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var button = cut.Find("[data-testid='btn-bearbeiten']");
                Assert.NotNull(button);
                Assert.Contains("Bearbeiten", button.TextContent);
                Assert.Contains("/lebensmittel/1/bearbeiten", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Zurück_Button_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var button = cut.Find("[data-testid='btn-zurück']");
                Assert.NotNull(button);
                Assert.Contains("Zurück", button.TextContent);
                Assert.Contains("/lebensmittel", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Formularfelder_Rendern()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.NotNull(cut.Find("[data-testid='input-kalorien']"));
                Assert.NotNull(cut.Find("[data-testid='input-fett']"));
                Assert.NotNull(cut.Find("[data-testid='input-gesättigte-fettsäuren']"));
                Assert.NotNull(cut.Find("[data-testid='input-kohlenhydrate']"));
                Assert.NotNull(cut.Find("[data-testid='input-zucker']"));
                Assert.NotNull(cut.Find("[data-testid='input-protein']"));
                Assert.NotNull(cut.Find("[data-testid='input-ballaststoffe']"));
                Assert.NotNull(cut.Find("[data-testid='input-salz']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Standard_Einheit_Select_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var select = cut.Find("[data-testid='select-standard-einheit']");
                Assert.NotNull(select);
                Assert.Contains("Gramm (g)", select.Markup);
                Assert.Contains("Milliliter (ml)", select.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Speichern_Button_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var button = cut.Find("[data-testid='btn-nährwert-speichern']");
                Assert.NotNull(button);
                Assert.Contains("Speichern", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Abbrechen_Button_Haben()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var button = cut.Find("[data-testid='btn-nährwert-abbrechen']");
                Assert.NotNull(button);
                Assert.Contains("Abbrechen", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestehenden_Nährwert_Aktualisieren()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Input("400");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                nährwertMock.Verify(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neuen_Nährwert_Erstellen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var newNährwert = new Nährwert
            {
                Id = 0,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "g"
            };

            var createdNährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "g"
            };

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync((Nährwert?)null);
            nährwertMock.Setup(s => s.CreateNährwertAsync(It.IsAny<Nährwert>()))
                .ReturnsAsync(createdNährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Kein Nährwert definiert"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Wenn_Lebensmittel_Nicht_Geladen()
        {
            // Arrange
            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(999))
                .ThrowsAsync(new Exception("Lebensmittel nicht gefunden"));

            var nährwertMock = new Mock<INährwertService>();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 999));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Laden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Beim_Nährwert_Laden_Anzeigen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ThrowsAsync(new Exception("Nährwert Service Error"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Argument_Exception_Bei_Nährwert_Speichern_Anzeigen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ThrowsAsync(new ArgumentException("Ungültige Einheit"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Input("400");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Validierungsfehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Invalid_Operation_Exception_Bei_Nährwert_Speichern_Anzeigen()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ThrowsAsync(new InvalidOperationException("Operation fehlgeschlagen"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Input("400");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Lebensmittel_Information_Kartentext_Rendern()
        {
            // Arrange
            var lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .WithKategorie("Getreide")
                .Build();

            var nährwert = NährwertTestDataBuilder.CreateNährwert()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            var nährwertMock = new Mock<INährwertService>();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Lebensmittel-Information"));
                Assert.True(cut.Markup.Contains("Einheit"));
                Assert.True(cut.Markup.Contains("Kategorie"));
                Assert.True(cut.Markup.Contains("Erstellt"));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

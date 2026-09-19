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
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .WithKategorie("Getreide")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .WithStandardMengeEinheit("g")
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

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
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement heading = cut.Find("h2");
                Assert.Contains("Mehl", heading.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bearbeiten_Button_Haben()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-bearbeiten']");
                Assert.NotNull(button);
                Assert.Contains("Bearbeiten", button.TextContent);
                Assert.Contains("/lebensmittel/1/bearbeiten", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Zurück_Button_Haben()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-zurück']");
                Assert.NotNull(button);
                Assert.Contains("Zurück", button.TextContent);
                Assert.Contains("/lebensmittel", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Formularfelder_Rendern()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

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
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement select = cut.Find("[data-testid='select-standard-einheit']");
                Assert.NotNull(select);
                Assert.Contains("Gramm (g)", select.OuterHtml);
                Assert.Contains("Milliliter (ml)", select.OuterHtml);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Speichern_Button_Haben()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-nährwert-speichern']");
                Assert.NotNull(button);
                Assert.Contains("Speichern", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Nährwert_Abbrechen_Button_Haben()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-nährwert-abbrechen']");
                Assert.NotNull(button);
                Assert.Contains("Abbrechen", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestehenden_Nährwert_Aktualisieren()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Change("400");
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
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert newNährwert = new Nährwert
            {
                Id = 0,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "g"
            };

            Nährwert createdNährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 100,
                StandardMengeEinheit = "g"
            };

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync((Nährwert?)null);
            nährwertMock.Setup(s => s.CreateNährwertAsync(It.IsAny<Nährwert>()))
                .ReturnsAsync(createdNährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

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
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(999))
                .ThrowsAsync(new Exception("Lebensmittel nicht gefunden"));

            Mock<INährwertService> nährwertMock = new();

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 999));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Laden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Beim_Nährwert_Laden_Anzeigen()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ThrowsAsync(new Exception("Nährwert Service Error"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Argument_Exception_Bei_Nährwert_Speichern_Anzeigen()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ThrowsAsync(new ArgumentException("Ungültige Einheit"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Change("400");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Validierungsfehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Invalid_Operation_Exception_Bei_Nährwert_Speichern_Anzeigen()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);
            nährwertMock.Setup(s => s.UpdateNährwertAsync(It.IsAny<Nährwert>()))
                .ThrowsAsync(new InvalidOperationException("Operation fehlgeschlagen"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-kalorien']").Change("400");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler-nährwert']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Lebensmittel_Information_Kartentext_Rendern()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .WithKategorie("Getreide")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

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

        [Fact]
        public void Sollte_Packungsliste_Mit_Produktinstanzen_Rendern()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            List<ProduktInstanz> packungen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(7),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
                },
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 1,
                    Menge = 1000,
                    Verfallsdatum = DateTime.Today.AddDays(30),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Pantry"
                }
            };

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(1))
                .ReturnsAsync(packungen);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);
            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-packung-']");
                Assert.Equal(2, rows.Count);
                Assert.Contains("Kühlschrank", cut.Markup);
                Assert.Contains("Pantry", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Packungsliste_Nach_Verfallsdatum_Sortieren()
        {
            // Arrange: unsortierte Liste zurückgeben, um zu sichern, dass UI sortiert
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            // Unsortiert: Id 3 (30 Tage), Id 1 (10 Tage), Id 2 (20 Tage)
            List<ProduktInstanz> unsortiertPackungen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 3,
                    LebensmittelKatalogId = 1,
                    Menge = 300,
                    Verfallsdatum = DateTime.Today.AddDays(30),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Pantry"
                },
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(10),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
                },
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 1,
                    Menge = 1000,
                    Verfallsdatum = DateTime.Today.AddDays(20),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Pantry"
                }
            };

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(1))
                .ReturnsAsync(unsortiertPackungen);

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);
            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert: Reihenfolge sollte nach Verfallsdatum sortiert sein (1, 2, 3)
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-packung-']");
                Assert.Equal(3, rows.Count);
                Assert.Contains("data-testid=\"zeile-packung-1\"", rows[0].OuterHtml);
                Assert.Contains("data-testid=\"zeile-packung-2\"", rows[1].OuterHtml);
                Assert.Contains("data-testid=\"zeile-packung-3\"", rows[2].OuterHtml);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Packungsfehler_Nicht_Lebensmittel_Verdecken()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(1))
                .ThrowsAsync(new Exception("Fehler beim Laden der Packungen"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);
            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert: Lebensmittel sollte sichtbar sein, Packungsfehler sollte separate Benachrichtigung sein
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Mehl", cut.Markup); // Lebensmittel noch sichtbar
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler-packungen']");
                Assert.NotEmpty(errorAlert);
                Assert.Contains("Fehler beim Laden der Packungen", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Packungsfehler_Nicht_Nährwert_Verdecken()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .WithStandardMehlValues()
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(1))
                .ThrowsAsync(new Exception("Fehler beim Laden der Packungen"));

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);
            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert: Nährwert sollte sichtbar sein, trotz Packungsfehler
            cut.WaitForAssertion(() =>
            {
                Assert.NotNull(cut.Find("[data-testid='input-kalorien']")); // Nährwert noch sichtbar
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler-packungen']");
                Assert.NotEmpty(errorAlert);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Leerzustand_Wenn_Keine_Packungen_Vorhanden()
        {
            // Arrange
            LebensmittelKatalog lebensmittel = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .Build();

            Nährwert nährwert = new NährwertTestDataBuilder()
                .WithId(1)
                .WithLebensmittelId(1)
                .Build();

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetLebensmittelByIdAsync(1))
                .ReturnsAsync(lebensmittel);

            Mock<INährwertService> nährwertMock = new();
            nährwertMock.Setup(s => s.GetNährwertByLebensmittelIdAsync(1))
                .ReturnsAsync(nährwert);

            Mock<IProduktInstanzService> produktInstanzMock = new();
            produktInstanzMock.Setup(s => s.GetByLebensmittelAsync(1))
                .ReturnsAsync(new List<ProduktInstanz>());

            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<INährwertService>(nährwertMock.Object);
            Services.AddSingleton<IProduktInstanzService>(produktInstanzMock.Object);

            // Act
            IRenderedComponent<LebensmittelDetail> cut = RenderComponent<LebensmittelDetail>(
                parameters => parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Keine Packungen vorhanden", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }
    }
}




using Bunit;
using FoodDatabase.App.Components.Pages.Lebensmittel;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class LebensmittelDetailTests : TestContext
    {
        public LebensmittelDetailTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersLebensmittelDetail_WithAllInformation()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "Mehl",
                Einheit = "g",
                Kategorie = "Getreide",
                ErstelltAm = DateTime.UtcNow
            };

            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 364,
                Fett = 1.3,
                GesättigteFettsäuren = 0.2,
                Kohlenhydrate = 77,
                Zucker = 0.3,
                Protein = 10,
                Ballaststoffe = 2.7,
                Salz = 0.01,
                StandardMengeEinheit = "g"
            };

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
                Assert.True(cut.Markup.Contains("364"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void RenderNährwertForm_WithAllEightFields()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" };
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 364,
                Fett = 1.3,
                GesättigteFettsäuren = 0.2,
                Kohlenhydrate = 77,
                Zucker = 0.3,
                Protein = 10,
                Ballaststoffe = 2.7,
                Salz = 0.01,
                StandardMengeEinheit = "g"
            };

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
        public void UpdateNährwert_CallsUpdateService()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" };
            var nährwert = new Nährwert
            {
                Id = 1,
                LebensmittelId = 1,
                Kalorien = 364,
                Fett = 1.3,
                GesättigteFettsäuren = 0.2,
                Kohlenhydrate = 77,
                Zucker = 0.3,
                Protein = 10,
                Ballaststoffe = 2.7,
                Salz = 0.01,
                StandardMengeEinheit = "g"
            };

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
        public void ShowsErrorMessage_OnNährwertServiceException()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" };

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
                var errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsErrorMessage_WhenLebensmittelNotFound()
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
    }
}

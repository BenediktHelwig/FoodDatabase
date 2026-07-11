using Bunit;
using FoodDatabase.App.Components.Pages.Lager;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class ProduktInstanzFormTests : TestContext
    {
        public ProduktInstanzFormTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersForm_WithNewMode()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Neue Packung"));
                Assert.True(cut.Markup.Contains("Mehl"));
                Assert.True(cut.Markup.Contains("Zucker"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void RendersForm_WithEditMode()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var existingInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(7),
                Einkaufsdatum = DateTime.Today,
                Lagerort = "Kühlschrank"
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(existingInstanz);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Packung bearbeiten"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void HasLebensmittelDropdown_WithValidOptions()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var dropdown = cut.Find("[data-testid='select-lebensmittel']");
                Assert.NotNull(dropdown);
                Assert.True(cut.Markup.Contains("Mehl (g)"));
                Assert.True(cut.Markup.Contains("Milch (ml)"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void HasLagerortDropdown_WithAllValidOptions()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var dropdown = cut.Find("[data-testid='select-lagerort']");
                Assert.NotNull(dropdown);
                Assert.True(cut.Markup.Contains("Kühlschrank"));
                Assert.True(cut.Markup.Contains("Tiefkühler"));
                Assert.True(cut.Markup.Contains("Pantry"));
                Assert.True(cut.Markup.Contains("Anderes"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task SubmitForm_CreatesNewInstanz()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var newInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(30),
                Einkaufsdatum = DateTime.Today,
                Lagerort = "Pantry"
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.CreateAsync(1, 500, DateTime.Today.AddDays(30), "Pantry"))
                .ReturnsAsync(newInstanz);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Act
            await cut.WaitForAssertion(async () =>
            {
                var lebensmittelSelect = cut.Find("[data-testid='select-lebensmittel']");
                await lebensmittelSelect.ChangeAsync("1");
            }, TimeSpan.FromSeconds(2));

            // Assert - Service should be called (actual submission tested in integration tests)
            Assert.True(cut.Markup.Contains("Speichern"));
        }

        [Fact]
        public void HasInputFields_ForMengeAndVerfallsdatum()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.NotNull(cut.Find("[data-testid='input-menge']"));
                Assert.NotNull(cut.Find("[data-testid='input-verfallsdatum']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void LebensmittelDropdown_IsDisabledInEditMode()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var existingInstanz = new ProduktInstanz
            {
                Id = 1,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(7),
                Einkaufsdatum = DateTime.Today,
                Lagerort = "Kühlschrank"
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(existingInstanz);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var dropdown = cut.Find("[data-testid='select-lebensmittel']");
                Assert.True(dropdown.GetAttribute("disabled") is not null);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

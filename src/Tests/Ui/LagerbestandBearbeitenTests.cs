using Bunit;
using FoodDatabase.App.Components.Pages.Lager;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class LagerbestandBearbeitenTests : TestContext
    {
        public LagerbestandBearbeitenTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersTable_WithAllProduktInstanzen()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
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
                    LebensmittelKatalogId = 2,
                    Menge = 1000,
                    Verfallsdatum = DateTime.Today.AddDays(30),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Pantry"
                }
            };

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(2, rows.Count);
                Assert.True(cut.Markup.Contains("Mehl"));
                Assert.True(cut.Markup.Contains("Zucker"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsNoItems_WhenListIsEmpty()
        {
            // Arrange
            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Kein Lagerbestand vorhanden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void HighlightsExpiredItems_WithRedBackground()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(-1),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                }
            };

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var row = cut.Find("[data-testid='zeile-1']");
                Assert.True(row.ClassList.Contains("table-danger"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void DeleteButton_Renders()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(7),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
                }
            };

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var deleteBtn = cut.Find("[data-testid='btn-löschen-1']");
                Assert.NotNull(deleteBtn);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void HasNewButton()
        {
            // Arrange
            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            Assert.True(cut.Markup.Contains("btn-neu"));
        }
    }
}

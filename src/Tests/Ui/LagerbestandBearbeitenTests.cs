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
                    Verfallsdatum = DateTime.Today.AddDays(-1), // Abgelaufen
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
        public async Task DeleteButton_ShowsConfirmationModal()
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

            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Act
            await cut.WaitForAssertion(async () =>
            {
                var deleteBtn = cut.Find("[data-testid='btn-löschen-1']");
                deleteBtn.Click();
                cut.Render();
            }, TimeSpan.FromSeconds(2));

            // Assert
            var modal = cut.Find("[data-testid='modal-bestätigung']");
            Assert.NotNull(modal);
            Assert.True(modal.ClassList.Contains("show"));
        }

        [Fact]
        public async Task ConfirmDelete_CallsDeleteService()
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
                .ReturnsAsync(instanzen.Take(1).ToList())
                .Then.ReturnsAsync(new List<ProduktInstanz>()); // Nach Delete: leere Liste

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Act
            await cut.WaitForAssertion(async () =>
            {
                var deleteBtn = cut.Find("[data-testid='btn-löschen-1']");
                deleteBtn.Click();
                cut.Render();
            }, TimeSpan.FromSeconds(2));

            var confirmBtn = cut.Find("[data-testid='btn-löschen-bestätigt']");
            confirmBtn.Click();

            // Assert
            await cut.WaitForAssertion(() =>
            {
                instanzMock.Verify(s => s.DeleteAsync(1), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

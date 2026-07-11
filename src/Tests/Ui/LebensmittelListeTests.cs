using Bunit;
using FoodDatabase.App.Components.Pages.Lebensmittel;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class LebensmittelListeTests : TestContext
    {
        public LebensmittelListeTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersList_WithAllLebensmittel()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" },
                new LebensmittelKatalog { Id = 3, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                Assert.True(cut.Markup.Contains("Mehl"));
                Assert.True(cut.Markup.Contains("Zucker"));
                Assert.True(cut.Markup.Contains("Milch"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsNoItems_WhenListIsEmpty()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Keine Lebensmittel gefunden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void SearchLebensmittel_CallsSearchService()
        {
            // Arrange
            var allItems = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" }
            };
            var searchResults = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(allItems);
            mock.Setup(s => s.SearchLebensmittelAsync("Mehl"))
                .ReturnsAsync(searchResults);

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-suche']").Input("Mehl");
                cut.Find("[data-testid='btn-suche']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.SearchLebensmittelAsync("Mehl"), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void DeleteButton_ShowsConfirmationDialog()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
            }, TimeSpan.FromSeconds(2));

            // Assert
            var modal = cut.FindAll("[data-testid='modal-bestätigung']");
            Assert.NotEmpty(modal);
        }

        [Fact]
        public void ConfirmDelete_CallsDeleteService()
        {
            // Arrange
            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);
            mock.Setup(s => s.DeleteLebensmittelAsync(1))
                .ReturnsAsync(true);

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Act
            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='btn-löschen-1']").Click();
            }, TimeSpan.FromSeconds(2));

            cut.Find("[data-testid='btn-löschen-bestätigt']").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.DeleteLebensmittelAsync(1), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsErrorMessage_OnServiceException()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetAllLebensmittelAsync())
                .ThrowsAsync(new Exception("Service Error"));

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

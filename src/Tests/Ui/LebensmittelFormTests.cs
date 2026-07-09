using Bunit;
using Bunit.TestDoubles;
using FoodDatabase.App.Components.Pages.Lebensmittel;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class LebensmittelFormTests : TestContext
    {
        public LebensmittelFormTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Renders_FormFields_ForNewLebensmittel()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            Assert.NotNull(cut.Find("[data-testid='input-name']"));
            Assert.NotNull(cut.Find("[data-testid='select-einheit']"));
            Assert.NotNull(cut.Find("[data-testid='input-kategorie']"));
            Assert.NotNull(cut.Find("[data-testid='btn-speichern']"));
            Assert.True(cut.Markup.Contains("Neues Lebensmittel"));
        }

        [Fact]
        public void SubmitNewLebensmittel_CallsCreateLebensmittelAsync()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            var createdItem = new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" };
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(createdItem);

            Services.AddSingleton<ILebensmittelService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();

            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Act
            cut.Find("[data-testid='input-name']").Input("Mehl");
            cut.Find("[data-testid='select-einheit']").Change("g");
            cut.Find("[data-testid='input-kategorie']").Input("Getreide");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()), Times.Once);
                Assert.EndsWith("/lebensmittel", nav.Uri);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ShowsErrorMessage_OnArgumentException()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new ArgumentException("Name ist erforderlich"));

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Act
            cut.Find("[data-testid='input-name']").Input("Test");
            cut.Find("[data-testid='select-einheit']").Change("g");
            cut.Find("[data-testid='input-kategorie']").Input("Test");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Eingabefehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void LoadsExistingLebensmittel_WhenIdProvided()
        {
            // Arrange
            var existingItem = new LebensmittelKatalog { Id = 5, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" };
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetLebensmittelByIdAsync(5))
                .ReturnsAsync(existingItem);

            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 5));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Lebensmittel bearbeiten"));
                mock.Verify(s => s.GetLebensmittelByIdAsync(5), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

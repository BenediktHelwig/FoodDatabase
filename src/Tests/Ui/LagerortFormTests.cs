using AngleSharp.Dom;
using Bunit;
using FoodDatabase.App.Components.Pages.Lager;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für LagerortForm.razor Komponente (UC9).
    /// Testet Funktionalität: Formular-Rendering, Service-Aufruf, Fehlerbehandlung.
    /// </summary>
    public class LagerortFormTests : TestContext
    {
        public LagerortFormTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Formular_Rendern()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            Services.AddSingleton<ILagerortService>(mock.Object);

            // Act
            IRenderedComponent<LagerortForm> cut = RenderComponent<LagerortForm>();

            // Assert
            IElement inputName = cut.Find("[data-testid='input-name']");
            Assert.NotNull(inputName);

            IElement buttonSpeichern = cut.Find("[data-testid='btn-speichern']");
            Assert.NotNull(buttonSpeichern);
            Assert.Contains("Speichern", buttonSpeichern.TextContent);
        }

        [Fact]
        public void Sollte_Service_Aufrufen_Beim_Speichern()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetOrCreateAsync("Keller"))
                .ReturnsAsync(new Lagerort { Id = 1, Name = "Keller", CreatedAt = DateTime.UtcNow, IsArchived = false });
            Services.AddSingleton<ILagerortService>(mock.Object);

            IRenderedComponent<LagerortForm> cut = RenderComponent<LagerortForm>();

            // Act
            cut.WaitForAssertion(() =>
            {
                IElement input = cut.Find("[data-testid='input-name']");
                input.Change("Keller");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.GetOrCreateAsync("Keller"), Times.Once);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Fehler_Anzeigen_Bei_Ungültigem_Namen()
        {
            // Arrange
            Mock<ILagerortService> mock = new();
            mock.Setup(s => s.GetOrCreateAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Nur Buchstaben (A-Z, a-z) erlaubt."));
            Services.AddSingleton<ILagerortService>(mock.Object);

            IRenderedComponent<LagerortForm> cut = RenderComponent<LagerortForm>();

            // Act
            cut.WaitForAssertion(() =>
            {
                IElement input = cut.Find("[data-testid='input-name']");
                input.Change("Keller 2");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Eingabefehler"));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

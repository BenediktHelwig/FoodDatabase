using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FoodDatabase.App.Components.Pages.Lebensmittel;
using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.Tests.Ui.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    /// <summary>
    /// bUnit Tests für LebensmittelForm.razor Komponente (UC1).
    /// Testet Funktionalität: Neu/Bearbeiten-Modus, Formularvalidierung, Speichern.
    /// </summary>
    public class LebensmittelFormTests : TestContext
    {
        public LebensmittelFormTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Sollte_Formularfelder_Im_Neu_Modus_Rendern()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.NotNull(cut.Find("[data-testid='input-name']"));
                Assert.NotNull(cut.Find("[data-testid='select-einheit']"));
                Assert.NotNull(cut.Find("[data-testid='input-kategorie']"));
                Assert.NotNull(cut.Find("[data-testid='btn-speichern']"));
                Assert.True(cut.Markup.Contains("Neues Lebensmittel"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Korrekte_Title_Im_Neu_Modus_Anzeigen()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement heading = cut.Find("h2");
                Assert.Contains("Neues Lebensmittel", heading.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Einheit_Select_Mit_Optionen_Haben()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement select = cut.Find("[data-testid='select-einheit']");
                Assert.Contains("Gramm (g)", select.OuterHtml);
                Assert.Contains("Milliliter (ml)", select.OuterHtml);
                Assert.Contains("Stück", select.OuterHtml);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Speichern_Button_Im_Neu_Modus_Haben()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-speichern']");
                Assert.NotNull(button);
                Assert.Contains("Speichern", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Abbrechen_Button_Haben()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IElement button = cut.Find("[data-testid='btn-abbrechen']");
                Assert.NotNull(button);
                Assert.Contains("Abbrechen", button.TextContent);
                Assert.Contains("/lebensmittel", button.GetAttribute("href"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Neues_Lebensmittel_Erstellen()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            LebensmittelKatalog createdItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(1)
                .WithName("Mehl")
                .WithEinheit("g")
                .WithKategorie("Getreide")
                .Build();
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(createdItem);
            Services.AddSingleton<ILebensmittelService>(mock.Object);
            FakeNavigationManager nav = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-name']").Input("Mehl");
                cut.Find("[data-testid='select-einheit']").Change("g");
                cut.Find("[data-testid='input-kategorie']").Input("Getreide");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()), Times.Once);
                Assert.EndsWith("/lebensmittel", nav.Uri);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestehend_Lebensmittel_Laden()
        {
            // Arrange
            LebensmittelKatalog existingItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(5)
                .WithName("Zucker")
                .WithEinheit("g")
                .WithKategorie("Süßstoffe")
                .Build();
            Mock<ILebensmittelService> mock = new();
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

        [Fact]
        public void Sollte_Korrekte_Title_Im_Bearbeiten_Modus_Anzeigen()
        {
            // Arrange
            var existingItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(5)
                .Build();
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
                IElement heading = cut.Find("h2");
                Assert.Contains("Lebensmittel bearbeiten", heading.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Bestehend_Lebensmittel_Aktualisieren()
        {
            // Arrange
            LebensmittelKatalog existingItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(5)
                .WithName("Zucker")
                .WithEinheit("g")
                .WithKategorie("Süßstoffe")
                .Build();
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetLebensmittelByIdAsync(5))
                .ReturnsAsync(existingItem);
            mock.Setup(s => s.UpdateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ReturnsAsync(existingItem);
            Services.AddSingleton<ILebensmittelService>(mock.Object);
            var nav = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 5));

            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-name']").Input("Zucker Updated");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                mock.Verify(s => s.UpdateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()), Times.Once);
                Assert.EndsWith("/lebensmittel", nav.Uri);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Argument_Exception_Anzeigen()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new ArgumentException("Name ist erforderlich"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-name']").Input("Test");
                cut.Find("[data-testid='select-einheit']").Change("g");
                cut.Find("[data-testid='input-kategorie']").Input("Test");
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

        [Fact]
        public void Sollte_Validation_Exception_Anzeigen()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new ValidationException("Validierung fehlgeschlagen"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-name']").Input("Test");
                cut.Find("[data-testid='select-einheit']").Change("g");
                cut.Find("[data-testid='input-kategorie']").Input("Test");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Validierungsfehler"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Generische_Exception_Anzeigen()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.CreateLebensmittelAsync(It.IsAny<LebensmittelKatalog>()))
                .ThrowsAsync(new Exception("Unerwarteter Fehler"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            cut.WaitForAssertion(() =>
            {
                cut.Find("[data-testid='input-name']").Input("Test");
                cut.Find("[data-testid='select-einheit']").Change("g");
                cut.Find("[data-testid='input-kategorie']").Input("Test");
                cut.Find("form").Submit();
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Speichern"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Laden_Fehler_Anzeigen()
        {
            // Arrange
            var mock = new Mock<ILebensmittelService>();
            mock.Setup(s => s.GetLebensmittelByIdAsync(5))
                .ThrowsAsync(new Exception("Lebensmittel nicht gefunden"));
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 5));

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> errorAlert = cut.FindAll("[data-testid='alert-fehler']");
                Assert.NotEmpty(errorAlert);
                Assert.True(cut.Markup.Contains("Fehler beim Laden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Speichern_Button_Im_Bearbeiten_Modus_Haben()
        {
            // Arrange
            var existingItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(5)
                .Build();
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
                IElement button = cut.Find("[data-testid='btn-speichern']");
                Assert.NotNull(button);
                Assert.Contains("Speichern", button.TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Leer_Formular_Im_Neu_Modus_Anzeigen()
        {
            // Arrange
            Mock<ILebensmittelService> mock = new();
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 0));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var nameInput = cut.Find("[data-testid='input-name']");
                var value = nameInput.GetAttribute("value") ?? "";
                // In Blazor Forms ist der Wert bei neuem Formular leer
                Assert.True(string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Sollte_Formular_Mit_Existierendem_Lebensmittel_Füllen()
        {
            // Arrange
            LebensmittelKatalog existingItem = LebensmittelTestDataBuilder.CreateLebensmittel()
                .WithId(5)
                .WithName("Zucker")
                .WithEinheit("g")
                .WithKategorie("Süßstoffe")
                .Build();
            Mock<ILebensmittelService> mock = new();
            mock.Setup(s => s.GetLebensmittelByIdAsync(5))
                .ReturnsAsync(existingItem);
            Services.AddSingleton<ILebensmittelService>(mock.Object);

            // Act
            IRenderedComponent<LebensmittelForm> cut = RenderComponent<LebensmittelForm>(
                parameters => parameters.Add(p => p.Id, 5));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Zucker"));
            }, TimeSpan.FromSeconds(2));
        }
    }
}

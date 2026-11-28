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

        private static ILagerortService ErstelleLagerortService()
        {
            Mock<ILagerortService> lagerortMock = new();
            lagerortMock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(new List<Lagerort>
                {
                    new Lagerort { Id = 1, Name = "Kühlschrank" },
                    new Lagerort { Id = 2, Name = "Tiefkühler" },
                    new Lagerort { Id = 3, Name = "Pantry" }
                });
            return lagerortMock.Object;
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
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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
                LagerortId = 1
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(existingInstanz);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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
        public void HasLagerortDropdown_WithOptionsFromService()
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
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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
            }, TimeSpan.FromSeconds(2));
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
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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
                LagerortId = 1
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(existingInstanz);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

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

        [Fact]
        public void HasSpeichernButton()
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
            Services.AddSingleton<ILagerortService>(ErstelleLagerortService());

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderComponent<ProduktInstanzForm>(parameters =>
                parameters.Add(p => p.Id, 0));

            // Assert
            Assert.True(cut.Markup.Contains("btn-speichern"));
        }

        // === UC9→UC10: Lagerort als Fremdschlüssel (Dropdown, „Neuer Lagerort…“, Feldfehler) ===

        private IRenderedComponent<ProduktInstanzForm> RenderForm(
            int id,
            Mock<IProduktInstanzService> instanzMock,
            Mock<ILagerortService> lagerortMock)
        {
            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>
                {
                    new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
                });

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);
            Services.AddSingleton<ILagerortService>(lagerortMock.Object);

            return RenderComponent<ProduktInstanzForm>(parameters => parameters.Add(p => p.Id, id));
        }

        private static Mock<ILagerortService> ErstelleLagerortMock(params Lagerort[] lagerorte)
        {
            Mock<ILagerortService> lagerortMock = new();
            lagerortMock.Setup(s => s.GetAlleLagerorte())
                .ReturnsAsync(lagerorte.ToList());
            return lagerortMock;
        }

        private static ProduktInstanz ErstelleBestehendePackung(int lagerortId)
        {
            return new ProduktInstanz
            {
                Id = 5,
                LebensmittelKatalogId = 1,
                Menge = 500,
                Verfallsdatum = DateTime.Today.AddDays(7),
                LagerortId = lagerortId
            };
        }

        [Fact]
        public void LagerortDropdown_ListsLagerorteSortedByNameFollowedByNeu()
        {
            // Arrange
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(
                new Lagerort { Id = 3, Name = "Tiefkühler" },
                new Lagerort { Id = 1, Name = "Kühlschrank" },
                new Lagerort { Id = 2, Name = "Pantry" });

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, new Mock<IProduktInstanzService>(), lagerortMock);

            // Assert
            cut.WaitForAssertion(() =>
            {
                List<string> optionen = cut.FindAll("[data-testid='select-lagerort'] option")
                    .Select(o => o.TextContent.Trim())
                    .ToList();
                Assert.Equal(
                    new List<string> { "-- Lagerort wählen --", "Kühlschrank", "Pantry", "Tiefkühler", "Neuer Lagerort…" },
                    optionen);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void NewMode_WithoutLagerorte_PreselectsNeuAndShowsTextfield()
        {
            // Arrange
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock();

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, new Mock<IProduktInstanzService>(), lagerortMock);

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("neu", cut.Find("[data-testid='select-lagerort']").GetAttribute("value"));
                Assert.NotNull(cut.Find("[data-testid='input-neuer-lagerort']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void NewMode_WithLagerorte_PreselectsPlaceholderAndHidesTextfield()
        {
            // Arrange
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 1, Name = "Kühlschrank" });

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, new Mock<IProduktInstanzService>(), lagerortMock);

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal("", cut.Find("[data-testid='select-lagerort']").GetAttribute("value"));
                Assert.Empty(cut.FindAll("[data-testid='input-neuer-lagerort']"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void LagerortDropdown_SwitchingToNeuAndBack_TogglesTextfield()
        {
            // Arrange
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 1, Name = "Kühlschrank" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, new Mock<IProduktInstanzService>(), lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='select-lagerort']"), TimeSpan.FromSeconds(2));

            // Act & Assert: Neu gewählt, Textfeld erscheint
            cut.Find("[data-testid='select-lagerort']").Change("neu");
            Assert.NotNull(cut.Find("[data-testid='input-neuer-lagerort']"));

            // Act & Assert: Bestehenden gewählt, Textfeld verschwindet
            cut.Find("[data-testid='select-lagerort']").Change("1");
            Assert.Empty(cut.FindAll("[data-testid='input-neuer-lagerort']"));
        }

        [Fact]
        public void Save_WithExistingLagerort_CallsCreateWithItsIdAndNotGetOrCreate()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(
                new Lagerort { Id = 1, Name = "Kühlschrank" },
                new Lagerort { Id = 2, Name = "Pantry" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='select-lagerort']"), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lebensmittel']").Change("1");
            cut.Find("[data-testid='select-lagerort']").Change("2");

            // Act
            cut.Find("form").Submit();

            // Assert
            instanzMock.Verify(s => s.CreateAsync(1, It.IsAny<decimal>(), It.IsAny<DateTime>(), 2), Times.Once);
            lagerortMock.Verify(s => s.GetOrCreateAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Save_WithNeuAndName_CallsGetOrCreateThenCreateWithReturnedId()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 1, Name = "Kühlschrank" });
            lagerortMock.Setup(s => s.GetOrCreateAsync("Keller"))
                .ReturnsAsync(new Lagerort { Id = 42, Name = "Keller" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='select-lagerort']"), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lebensmittel']").Change("1");
            cut.Find("[data-testid='select-lagerort']").Change("neu");
            cut.Find("[data-testid='input-neuer-lagerort']").Change("Keller");

            // Act
            cut.Find("form").Submit();

            // Assert
            lagerortMock.Verify(s => s.GetOrCreateAsync("Keller"), Times.Once);
            instanzMock.Verify(s => s.CreateAsync(1, It.IsAny<decimal>(), It.IsAny<DateTime>(), 42), Times.Once);
        }

        [Fact]
        public void Save_WithNeuAndExistingNameInOtherSpelling_UsesTheIdOfTheExistingLagerort()
        {
            // Arrange: UC9 liefert für „kühlschrank“ den bestehenden Lagerort zurück, keinen zweiten
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 1, Name = "Kühlschrank" });
            lagerortMock.Setup(s => s.GetOrCreateAsync("kühlschrank"))
                .ReturnsAsync(new Lagerort { Id = 1, Name = "Kühlschrank" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='select-lagerort']"), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lebensmittel']").Change("1");
            cut.Find("[data-testid='select-lagerort']").Change("neu");
            cut.Find("[data-testid='input-neuer-lagerort']").Change("kühlschrank");

            // Act
            cut.Find("form").Submit();

            // Assert
            instanzMock.Verify(s => s.CreateAsync(1, It.IsAny<decimal>(), It.IsAny<DateTime>(), 1), Times.Once);
        }

        [Fact]
        public void Save_WithoutLagerortSelection_ShowsFieldErrorAndCallsNoService()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 1, Name = "Kühlschrank" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='select-lagerort']"), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(cut.Find("[data-testid='lagerort-fehler']").TextContent));
            instanzMock.Verify(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            lagerortMock.Verify(s => s.GetOrCreateAsync(It.IsAny<string>()), Times.Never);
            Assert.Empty(cut.FindAll("[data-testid='alert-fehler']"));
        }

        [Fact]
        public void Save_WithNeuAndEmptyName_ShowsFieldErrorAndCallsNoService()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock();
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='input-neuer-lagerort']"), TimeSpan.FromSeconds(2));

            // Act
            cut.Find("form").Submit();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(cut.Find("[data-testid='lagerort-fehler']").TextContent));
            instanzMock.Verify(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            lagerortMock.Verify(s => s.GetOrCreateAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Save_WhenGetOrCreateRejectsName_ShowsFieldErrorKeepsButtonEnabledAndCreatesNothing()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock();
            lagerortMock.Setup(s => s.GetOrCreateAsync("Keller1"))
                .ThrowsAsync(new ArgumentException("Lagerortname darf nur Buchstaben enthalten."));
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='input-neuer-lagerort']"), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lebensmittel']").Change("1");
            cut.Find("[data-testid='input-neuer-lagerort']").Change("Keller1");

            // Act
            cut.Find("form").Submit();

            // Assert
            Assert.Contains("nur Buchstaben", cut.Find("[data-testid='lagerort-fehler']").TextContent);
            instanzMock.Verify(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            Assert.Empty(cut.FindAll("[data-testid='alert-fehler']"));
            Assert.Null(cut.Find("[data-testid='btn-speichern']").GetAttribute("disabled"));
        }

        [Fact]
        public void Save_AfterCorrectingTheLagerortName_Succeeds()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock();
            lagerortMock.Setup(s => s.GetOrCreateAsync("Keller1"))
                .ThrowsAsync(new ArgumentException("Lagerortname darf nur Buchstaben enthalten."));
            lagerortMock.Setup(s => s.GetOrCreateAsync("Keller"))
                .ReturnsAsync(new Lagerort { Id = 7, Name = "Keller" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(0, instanzMock, lagerortMock);
            cut.WaitForAssertion(() => cut.Find("[data-testid='input-neuer-lagerort']"), TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lebensmittel']").Change("1");
            cut.Find("[data-testid='input-neuer-lagerort']").Change("Keller1");
            cut.Find("form").Submit();

            // Act
            cut.Find("[data-testid='input-neuer-lagerort']").Change("Keller");
            cut.Find("form").Submit();

            // Assert
            instanzMock.Verify(s => s.CreateAsync(1, It.IsAny<decimal>(), It.IsAny<DateTime>(), 7), Times.Once);
        }

        [Fact]
        public void EditMode_PreselectsTheLagerortOfThePackung()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(ErstelleBestehendePackung(2));
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(
                new Lagerort { Id = 1, Name = "Kühlschrank" },
                new Lagerort { Id = 2, Name = "Pantry" });

            // Act
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(5, instanzMock, lagerortMock);

            // Assert
            cut.WaitForAssertion(() =>
                Assert.Equal("2", cut.Find("[data-testid='select-lagerort']").GetAttribute("value")),
                TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void EditMode_Save_CallsUpdateWithTheChosenLagerortId()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(ErstelleBestehendePackung(2));
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(
                new Lagerort { Id = 1, Name = "Kühlschrank" },
                new Lagerort { Id = 2, Name = "Pantry" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(5, instanzMock, lagerortMock);
            cut.WaitForAssertion(() =>
                Assert.Equal("2", cut.Find("[data-testid='select-lagerort']").GetAttribute("value")),
                TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lagerort']").Change("1");

            // Act
            cut.Find("form").Submit();

            // Assert
            instanzMock.Verify(s => s.UpdateAsync(5, 500m, It.IsAny<DateTime>(), 1), Times.Once);
        }

        [Fact]
        public void EditMode_Save_WithNewLagerort_CallsGetOrCreateThenUpdateWithReturnedId()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(ErstelleBestehendePackung(2));
            Mock<ILagerortService> lagerortMock = ErstelleLagerortMock(new Lagerort { Id = 2, Name = "Pantry" });
            lagerortMock.Setup(s => s.GetOrCreateAsync("Keller"))
                .ReturnsAsync(new Lagerort { Id = 9, Name = "Keller" });
            IRenderedComponent<ProduktInstanzForm> cut = RenderForm(5, instanzMock, lagerortMock);
            cut.WaitForAssertion(() =>
                Assert.Equal("2", cut.Find("[data-testid='select-lagerort']").GetAttribute("value")),
                TimeSpan.FromSeconds(2));
            cut.Find("[data-testid='select-lagerort']").Change("neu");
            cut.Find("[data-testid='input-neuer-lagerort']").Change("Keller");

            // Act
            cut.Find("form").Submit();

            // Assert
            instanzMock.Verify(s => s.UpdateAsync(5, 500m, It.IsAny<DateTime>(), 9), Times.Once);
        }
    }
}

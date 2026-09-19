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

        [Fact]
        public void StatusFilter_Ruft_GetVerfallenenAsync_Auf()
        {
            // Arrange
            List<ProduktInstanz> abgelaufeneInstanzen = new()
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

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(abgelaufeneInstanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Milch", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));

            instanzMock.Verify(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact]
        public void LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf()
        {
            // Arrange
            List<ProduktInstanz> lagerortInstanzen = new()
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

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetByLagerortAsync("Kühlschrank"))
                .ReturnsAsync(lagerortInstanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));

            instanzMock.Verify(s => s.GetByLagerortAsync("Kühlschrank"), Times.Once);
        }

        [Fact]
        public void TodayExpiredItem_DoesNotAppear_InExpiredFilter()
        {
            // Arrange: heute ablaufende Packung (DateTime.Today)
            List<ProduktInstanz> instanzenMitHeute = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today, // Heute ablaufend
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            // Service liefert die heute ablaufende Packung
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(instanzenMitHeute);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            // Assert: Die Tabelle ist leer, weil die UI auf < DateTime.Today filtert
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Empty(rows); // Keine Tabellenzeilen
            }, TimeSpan.FromSeconds(2));

            Assert.Contains("Keine Treffer für diesen Filter", cut.Markup);
        }

        [Fact]
        public void FilteredList_IsSortedByVerfallsdatum()
        {
            // Arrange: unsortierte Liste zurückgeben um sicherzustellen, dass UI sortiert
            List<ProduktInstanz> unsortierteLagerort = new()
            {
                new ProduktInstanz
                {
                    Id = 3,
                    LebensmittelKatalogId = 3,
                    Menge = 100,
                    Verfallsdatum = DateTime.Today.AddDays(30),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
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
                    LebensmittelKatalogId = 2,
                    Menge = 250,
                    Verfallsdatum = DateTime.Today.AddDays(20),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Butter", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 2, Name = "Käse", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 3, Name = "Joghurt", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetByLagerortAsync("Kühlschrank"))
                .ReturnsAsync(unsortierteLagerort); // Unsortiert zurückgeben

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert: Die Reihenfolge sollte aufsteigend nach Verfallsdatum sein
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                // ID-Reihenfolge sollte 1, 2, 3 sein (aufsteigend nach Verfallsdatum)
                Assert.Contains("data-testid=\"zeile-1\"", rows[0].OuterHtml);
                Assert.Contains("data-testid=\"zeile-2\"", rows[1].OuterHtml);
                Assert.Contains("data-testid=\"zeile-3\"", rows[2].OuterHtml);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void WithoutFilter_CallsGetNachVerfallsdatumSortiertAsync()
        {
            // Arrange
            List<ProduktInstanz> instanzen = new()
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

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert: Ohne Filter sollte GetNachVerfallsdatumSortiertAsync aufgerufen werden
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));

            instanzMock.Verify(s => s.GetNachVerfallsdatumSortiertAsync(), Times.Once);
        }

        [Fact]
        public void MhdColumn_ShowsFutureDays()
        {
            // Arrange
            List<ProduktInstanz> instanzen = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(5),
                    Einkaufsdatum = DateTime.Today,
                    Lagerort = "Kühlschrank"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("5 Tage verbleibend", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MhdColumn_ShowsPastDays()
        {
            // Arrange
            List<ProduktInstanz> instanzen = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(-3),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("vor 3 Tagen abgelaufen", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MhdColumn_ShowsTodayExpiring()
        {
            // Arrange
            List<ProduktInstanz> instanzen = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today,
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(instanzen);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Heute ablaufend", cut.Markup);
                IElement row = cut.Find("[data-testid='zeile-1']");
                Assert.DoesNotContain("table-danger", row.ClassList);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void CombinedFilters_WorksCorrectly()
        {
            // Arrange: Abgelaufene Packungen von verschiedenen Lagerorten
            List<ProduktInstanz> abgelaufeneAlle = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(-1),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                },
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 2,
                    Menge = 1000,
                    Verfallsdatum = DateTime.Today.AddDays(-2),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Pantry"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(abgelaufeneAlle);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            IElement lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert: Nur die Packung von Kühlschrank sollte angezeigt werden
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void EmptyState_WithFilter_ShowsDifferentMessage()
        {
            // Arrange
            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<ProduktInstanz>()); // Keine abgelaufenen

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");

            cut.WaitForAssertion(() =>
            {
                statusFilter.Change("expired");
            }, TimeSpan.FromSeconds(2));

            // Assert: Sollte "Keine Treffer für diesen Filter" zeigen, nicht "Kein Lagerbestand vorhanden"
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Keine Treffer für diesen Filter", cut.Markup);
                Assert.DoesNotContain("Kein Lagerbestand vorhanden", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ExpiredFilter_IsCaseSensitiveForLagerort()
        {
            // Arrange: Zwei abgelaufene Packungen mit unterschiedlichem Case in Lagerort.
            // Das ist wichtig, weil der Lagerort-Filter nur im Expired-Pfad case-sensitive verglichen wird.
            List<ProduktInstanz> abgelaufeneAlle = new()
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(-2),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "Kühlschrank"
                },
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 2,
                    Menge = 250,
                    Verfallsdatum = DateTime.Today.AddDays(-1),
                    Einkaufsdatum = DateTime.Today.AddDays(-10),
                    Lagerort = "kühlschrank"  // Kleinbuchstabe
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Butter", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 2, Name = "Käse", Einheit = "g", Kategorie = "Milchprodukte" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(abgelaufeneAlle);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            IElement lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");

            cut.WaitForAssertion(() =>
            {
                lagerortFilter.Change("Kühlschrank");
            }, TimeSpan.FromSeconds(2));

            // Assert: Mit case-sensitive Vergleich darf nur ID=1 angezeigt werden
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("data-testid=\"zeile-1\"", rows[0].OuterHtml);
                Assert.DoesNotContain("Käse", cut.Markup);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ExpiredFilter_IsSortedByVerfallsdatum()
        {
            // Arrange: Unsortierte abgelaufene Packungen — sorgt dafür, dass die Sortierung in der UI erfolgt
            List<ProduktInstanz> unsortiertAbgelaufene = new()
            {
                new ProduktInstanz
                {
                    Id = 3,
                    LebensmittelKatalogId = 3,
                    Menge = 100,
                    Verfallsdatum = DateTime.Today.AddDays(-30),
                    Einkaufsdatum = DateTime.Today.AddDays(-40),
                    Lagerort = "Pantry"
                },
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 500,
                    Verfallsdatum = DateTime.Today.AddDays(-10),
                    Einkaufsdatum = DateTime.Today.AddDays(-15),
                    Lagerort = "Kühlschrank"
                },
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 2,
                    Menge = 250,
                    Verfallsdatum = DateTime.Today.AddDays(-15),
                    Einkaufsdatum = DateTime.Today.AddDays(-20),
                    Lagerort = "Pantry"
                }
            };

            List<LebensmittelKatalog> lebensmittel = new()
            {
                new LebensmittelKatalog { Id = 1, Name = "Butter", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 2, Name = "Käse", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 3, Name = "Joghurt", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            Mock<IProduktInstanzService> instanzMock = new();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(unsortiertAbgelaufene);

            Mock<ILebensmittelService> lebensmittelMock = new();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            IElement statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            // Assert: Reihenfolge sollte nach Verfallsdatum sortiert sein (ID 3, 2, 1 vom ältesten zum neuesten)
            cut.WaitForAssertion(() =>
            {
                IReadOnlyList<IElement> rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Equal(3, rows.Count);
                Assert.Contains("data-testid=\"zeile-3\"", rows[0].OuterHtml);
                Assert.Contains("data-testid=\"zeile-2\"", rows[1].OuterHtml);
                Assert.Contains("data-testid=\"zeile-1\"", rows[2].OuterHtml);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

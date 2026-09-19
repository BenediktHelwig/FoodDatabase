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
            var abgelaufeneInstanzen = new List<ProduktInstanz>
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
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(abgelaufeneInstanzen);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            var statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            // Assert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Milch", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf()
        {
            // Arrange
            var lagerortInstanzen = new List<ProduktInstanz>
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
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetByLagerortAsync("Kühlschrank"))
                .ReturnsAsync(lagerortInstanzen);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            var lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void TodayExpiredItem_DoesNotAppear_InExpiredFilter()
        {
            // Arrange: heute ablaufende Packung (DateTime.Today)
            var instanzenMitHeute = new List<ProduktInstanz>
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

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Milch", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            // Service liefert die heute ablaufende Packung
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(instanzenMitHeute);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            var statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            // Assert: Die Tabelle ist leer, weil die UI auf < DateTime.Today filtert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Empty(rows); // Keine Tabellenzeilen
            }, TimeSpan.FromSeconds(2));

            Assert.Contains("Keine Treffer für diesen Filter", cut.Markup);
        }

        [Fact]
        public void FilteredList_IsSortedByVerfallsdatum()
        {
            // Arrange: unsortierte Liste zurückgeben um sicherzustellen, dass UI sortiert
            var unsortierteLagerort = new List<ProduktInstanz>
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

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Butter", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 2, Name = "Käse", Einheit = "g", Kategorie = "Milchprodukte" },
                new LebensmittelKatalog { Id = 3, Name = "Joghurt", Einheit = "ml", Kategorie = "Milchprodukte" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetByLagerortAsync("Kühlschrank"))
                .ReturnsAsync(unsortierteLagerort); // Unsortiert zurückgeben

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            var lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert: Die Reihenfolge sollte aufsteigend nach Verfallsdatum sein
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
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

            // Assert: Ohne Filter sollte GetNachVerfallsdatumSortiertAsync aufgerufen werden
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MhdColumn_ShowsFutureDays()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
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
                Assert.True(cut.Markup.Contains("5 Tage verbleibend"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MhdColumn_ShowsPastDays()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
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
                Assert.True(cut.Markup.Contains("vor 3 Tagen abgelaufen"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MhdColumn_ShowsTodayExpiring()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>
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
                Assert.True(cut.Markup.Contains("Heute ablaufend"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void CombinedFilters_WorksCorrectly()
        {
            // Arrange: Abgelaufene Packungen von verschiedenen Lagerorten
            var abgelaufeneAlle = new List<ProduktInstanz>
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

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" },
                new LebensmittelKatalog { Id = 2, Name = "Zucker", Einheit = "g", Kategorie = "Süßstoffe" }
            };

            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(abgelaufeneAlle);

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            var statusFilter = cut.Find("[data-testid='select-status-filter']");
            statusFilter.Change("expired");

            var lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
            lagerortFilter.Change("Kühlschrank");

            // Assert: Nur die Packung von Kühlschrank sollte angezeigt werden
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void EmptyState_WithFilter_ShowsDifferentMessage()
        {
            // Arrange
            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            instanzMock.Setup(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<ProduktInstanz>()); // Keine abgelaufenen

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(new List<LebensmittelKatalog>());

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            cut.WaitForAssertion(() =>
            {
                var statusFilter = cut.Find("[data-testid='select-status-filter']");
                statusFilter.Change("expired");
            }, TimeSpan.FromSeconds(2));

            // Assert: Sollte "Keine Treffer für diesen Filter" zeigen, nicht "Kein Lagerbestand vorhanden"
            cut.WaitForAssertion(() =>
            {
                Assert.True(cut.Markup.Contains("Keine Treffer für diesen Filter"));
                Assert.False(cut.Markup.Contains("Kein Lagerbestand vorhanden"));
            }, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void LagerortFilter_IsCaseSensitive()
        {
            // Arrange: mock mit kleingeschriebenem Wert
            var instanzMock = new Mock<IProduktInstanzService>();
            instanzMock.Setup(s => s.GetNachVerfallsdatumSortiertAsync())
                .ReturnsAsync(new List<ProduktInstanz>());
            // Setup für exakt "Kühlschrank" (mit Großbuchstaben)
            instanzMock.Setup(s => s.GetByLagerortAsync("Kühlschrank"))
                .ReturnsAsync(new List<ProduktInstanz>
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
                });

            var lebensmittel = new List<LebensmittelKatalog>
            {
                new LebensmittelKatalog { Id = 1, Name = "Mehl", Einheit = "g", Kategorie = "Getreide" }
            };

            var lebensmittelMock = new Mock<ILebensmittelService>();
            lebensmittelMock.Setup(s => s.GetAllLebensmittelAsync())
                .ReturnsAsync(lebensmittel);

            Services.AddSingleton<IProduktInstanzService>(instanzMock.Object);
            Services.AddSingleton<ILebensmittelService>(lebensmittelMock.Object);

            // Act
            IRenderedComponent<LagerbestandBearbeiten> cut = RenderComponent<LagerbestandBearbeiten>();

            cut.WaitForAssertion(() =>
            {
                var lagerortFilter = cut.Find("[data-testid='select-lagerort-filter']");
                lagerortFilter.Change("Kühlschrank");
            }, TimeSpan.FromSeconds(2));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var rows = cut.FindAll("[data-testid^='zeile-']");
                Assert.Single(rows);
                Assert.Contains("Mehl", rows[0].TextContent);
            }, TimeSpan.FromSeconds(2));
        }
    }
}

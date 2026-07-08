using Xunit;
using Moq;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDatabase.Tests.Unit.Services
{
    /// <summary>
    /// UC8: Einkaufslistenservice Tests (TDD Rot-Phase).
    /// Tests für on-demand Einkaufslisten-Generierung basierend auf Mindestbestand-Unterschreitung.
    /// Trigger: Gesamtmenge <= MindestbestandMenge (aggregiert über alle Instanzen pro Lebensmittel).
    /// </summary>
    public class EinkaufslistenServiceTests
    {
        private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
        private readonly EinkaufslistenService _service;

        public EinkaufslistenServiceTests()
        {
            _mockRepository = new Mock<IRepository<ProduktInstanz>>();
            _service = new EinkaufslistenService(_mockRepository.Object);
        }

        // === GetEinkaufslisteAsync Tests (17) ===

        [Fact]
        public async Task GetEinkaufslisteAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            var instanzen = new List<ProduktInstanz>();
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithNullFromRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync((List<ProduktInstanz>)null);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithAllMengenUeberMindestbestand_ReturnsEmptyList()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "Mehl",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 200,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 12, 31),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMengeUnterMindestbestand_ReturnsEintrag()
        {
            // Arrange
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 1,
                Name = "Milch",
                Einheit = "ml"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 1,
                    LebensmittelKatalogId = 1,
                    Menge = 300,
                    MindestbestandMenge = 1000,
                    Verfallsdatum = new DateTime(2026, 7, 15),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].LebensmittelKatalogId);
            Assert.Equal("Milch", result[0].LebensmittelName);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMengeGleichMindestbestand_ReturnsEintrag()
        {
            // Arrange: Boundary-Test für <= Vergleich (nicht < wie UC2)
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 2,
                Name = "Zucker",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 2,
                    LebensmittelKatalogId = 2,
                    Menge = 500,
                    MindestbestandMenge = 500, // <= Boundary: Menge == Mindestbestand
                    Verfallsdatum = new DateTime(2027, 1, 1),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(500, result[0].AktuelleGesamtmenge);
            Assert.Equal(500, result[0].MindestbestandMenge);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMengeKnappUeberMindestbestand_ReturnsEmptyList()
        {
            // Arrange: 100.01 > 100 → sollte nicht in Liste
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 3,
                Name = "Olivenöl",
                Einheit = "ml"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 3,
                    LebensmittelKatalogId = 3,
                    Menge = 100.01m,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2028, 6, 30),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMultipleInstanzenDesselbenLebensmittels_AggregiertGesamtmenge()
        {
            // Arrange: 30 + 30 = 60 <= 100 → sollte in Liste; Aggregation zu 1 Eintrag
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 4,
                Name = "Butter",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 4,
                    LebensmittelKatalogId = 4,
                    Menge = 30,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 8, 15),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                },
                new ProduktInstanz
                {
                    Id = 5,
                    LebensmittelKatalogId = 4,
                    Menge = 30,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 9, 1),
                    Einkaufsdatum = new DateTime(2026, 7, 5),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(4, result[0].LebensmittelKatalogId);
            Assert.Equal(60, result[0].AktuelleGesamtmenge);
            Assert.Equal(100, result[0].MindestbestandMenge);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithSummeUeberMindestbestand_ReturnsEmptyList()
        {
            // Arrange: 60 + 60 = 120 > 100 → sollte nicht in Liste
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 5,
                Name = "Käse",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 6,
                    LebensmittelKatalogId = 5,
                    Menge = 60,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 8, 20),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                },
                new ProduktInstanz
                {
                    Id = 7,
                    LebensmittelKatalogId = 5,
                    Menge = 60,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 8, 25),
                    Einkaufsdatum = new DateTime(2026, 7, 5),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMehrerenUnterschrittenenLebensmitteln_ReturnsEinEintragProLebensmittel()
        {
            // Arrange: 2 verschiedene Lebensmittel, beide unterschritten
            var lebensmittel1 = new LebensmittelKatalog
            {
                Id = 6,
                Name = "Brot",
                Einheit = "Stück"
            };
            var lebensmittel2 = new LebensmittelKatalog
            {
                Id = 7,
                Name = "Salat",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 8,
                    LebensmittelKatalogId = 6,
                    Menge = 1,
                    MindestbestandMenge = 3,
                    Verfallsdatum = new DateTime(2026, 7, 12),
                    Einkaufsdatum = new DateTime(2026, 7, 5),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel1
                },
                new ProduktInstanz
                {
                    Id = 9,
                    LebensmittelKatalogId = 7,
                    Menge = 100,
                    MindestbestandMenge = 500,
                    Verfallsdatum = new DateTime(2026, 7, 15),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel2
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMixedLebensmitteln_ReturnsOnlyUnterschritteneEintraege()
        {
            // Arrange: Mix aus OK und unterschritten
            var lebensmittel1 = new LebensmittelKatalog
            {
                Id = 8,
                Name = "Apfel",
                Einheit = "Stück"
            };
            var lebensmittel2 = new LebensmittelKatalog
            {
                Id = 9,
                Name = "Banane",
                Einheit = "Stück"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 10,
                    LebensmittelKatalogId = 8,
                    Menge = 10, // > 5 (Mindestbestand)
                    MindestbestandMenge = 5,
                    Verfallsdatum = new DateTime(2026, 7, 20),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel1
                },
                new ProduktInstanz
                {
                    Id = 11,
                    LebensmittelKatalogId = 9,
                    Menge = 2, // < 5 (Mindestbestand)
                    MindestbestandMenge = 5,
                    Verfallsdatum = new DateTime(2026, 7, 18),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel2
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(9, result[0].LebensmittelKatalogId);
            Assert.Equal("Banane", result[0].LebensmittelName);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithUnterschreitung_BerechnetFehlmengeKorrekt()
        {
            // Arrange: Mindestbestand 100, Gesamtmenge 40 → Fehlmenge = 60
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 10,
                Name = "Tomaten",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 12,
                    LebensmittelKatalogId = 10,
                    Menge = 40,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 7, 14),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(60, result[0].Fehlmenge); // 100 - 40 = 60
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMengeGleichMindestbestand_ReturnsFehlmengeZero()
        {
            // Arrange: Menge == Mindestbestand → Fehlmenge = 0 (aber sollte trotzdem in Liste, weil <=)
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 11,
                Name = "Paprika",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 13,
                    LebensmittelKatalogId = 11,
                    Menge = 250,
                    MindestbestandMenge = 250,
                    Verfallsdatum = new DateTime(2026, 7, 25),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Kühlschrank",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(0, result[0].Fehlmenge); // 250 - 250 = 0
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithLebensmittelKatalogNavigation_MapsNameUndEinheit()
        {
            // Arrange: Teste dass Name und Einheit aus LebensmittelKatalog korrekt gemappt werden
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 12,
                Name = "Knoblauch",
                Einheit = "Zehe"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 14,
                    LebensmittelKatalogId = 12,
                    Menge = 2,
                    MindestbestandMenge = 10,
                    Verfallsdatum = new DateTime(2026, 10, 1),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Knoblauch", result[0].LebensmittelName);
            Assert.Equal("Zehe", result[0].Einheit);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithNullLebensmittelKatalog_UsesFallbackName()
        {
            // Arrange: Navigation Property ist null → Fallback-Name verwenden
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 15,
                    LebensmittelKatalogId = 99,
                    Menge = 50,
                    MindestbestandMenge = 200,
                    Verfallsdatum = new DateTime(2026, 8, 30),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = null // null Navigation Property
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Lebensmittel #99", result[0].LebensmittelName); // Fallback
            Assert.Equal(string.Empty, result[0].Einheit); // Fallback für Einheit
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMindestbestandZero_ReturnsEmptyList()
        {
            // Arrange: Mindestbestand = 0 → Gesamtmenge 100 > 0 → nicht in Liste (> ist nicht <=)
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 13,
                Name = "Wasser",
                Einheit = "ml"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 16,
                    LebensmittelKatalogId = 13,
                    Menge = 100,
                    MindestbestandMenge = 0,
                    Verfallsdatum = new DateTime(2027, 12, 31),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithMehrerenEintraegen_SortsByLebensmittelName()
        {
            // Arrange: Mehrere Einträge → müssen nach Name sortiert sein
            var lebensmittel1 = new LebensmittelKatalog
            {
                Id = 14,
                Name = "Zwiebel",
                Einheit = "g"
            };
            var lebensmittel2 = new LebensmittelKatalog
            {
                Id = 15,
                Name = "Anis",
                Einheit = "g"
            };
            var lebensmittel3 = new LebensmittelKatalog
            {
                Id = 16,
                Name = "Muskatnuss",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 17,
                    LebensmittelKatalogId = 14,
                    Menge = 10,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 7, 20),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel1
                },
                new ProduktInstanz
                {
                    Id = 18,
                    LebensmittelKatalogId = 15,
                    Menge = 2,
                    MindestbestandMenge = 50,
                    Verfallsdatum = new DateTime(2026, 7, 21),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel2
                },
                new ProduktInstanz
                {
                    Id = 19,
                    LebensmittelKatalogId = 16,
                    Menge = 5,
                    MindestbestandMenge = 100,
                    Verfallsdatum = new DateTime(2026, 7, 22),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel3
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Anis", result[0].LebensmittelName); // Alphabetisch zuerst
            Assert.Equal("Muskatnuss", result[1].LebensmittelName);
            Assert.Equal("Zwiebel", result[2].LebensmittelName); // Alphabetisch zuletzt
        }

        [Fact]
        public async Task GetEinkaufslisteAsync_WithEintrag_MapsLebensmittelKatalogIdUndMindestbestand()
        {
            // Arrange: Teste dass LebensmittelKatalogId und MindestbestandMenge korrekt gemappt werden
            var lebensmittel = new LebensmittelKatalog
            {
                Id = 17,
                Name = "Pfeffer",
                Einheit = "g"
            };
            var instanzen = new List<ProduktInstanz>
            {
                new ProduktInstanz
                {
                    Id = 20,
                    LebensmittelKatalogId = 17,
                    Menge = 5,
                    MindestbestandMenge = 75,
                    Verfallsdatum = new DateTime(2027, 5, 15),
                    Einkaufsdatum = new DateTime(2026, 7, 1),
                    Lagerort = "Pantry",
                    LebensmittelKatalog = lebensmittel
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instanzen);

            // Act
            var result = await _service.GetEinkaufslisteAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(17, result[0].LebensmittelKatalogId);
            Assert.Equal(75, result[0].MindestbestandMenge);
            Assert.Equal(5, result[0].AktuelleGesamtmenge);
        }
    }
}

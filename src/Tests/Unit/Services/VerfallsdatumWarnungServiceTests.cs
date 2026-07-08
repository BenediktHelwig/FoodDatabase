using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Classes;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Interfaces;
using FoodDatabase.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace FoodDatabase.Tests.Unit.Services;

/// <summary>
/// Tests für UC7: Verfallsdatum-Warnungen (on-demand berechnet, keine Persistierung).
/// </summary>
public class VerfallsdatumWarnungServiceTests
{
    private readonly Mock<IRepository<ProduktInstanz>> _mockRepository;
    private readonly VerfallsdatumWarnungService _service;
    private readonly FixedTimeProvider _timeProvider;

    public VerfallsdatumWarnungServiceTests()
    {
        _mockRepository = new Mock<IRepository<ProduktInstanz>>();
        _timeProvider = new FixedTimeProvider(new DateTime(2026, 7, 8));
        _service = new VerfallsdatumWarnungService(_mockRepository.Object, _timeProvider);
    }

    // === BerechneStatus Tests (7) ===

    [Fact]
    public void BerechneStatus_VerfallsdatumGestern_ReturnsAbgelaufen()
    {
        // Arrange
        DateTime verfallsdatum = new DateTime(2026, 7, 7);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.Abgelaufen, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumHeute_ReturnsBaldAblaufend()
    {
        // Arrange
        DateTime verfallsdatum = new DateTime(2026, 7, 8);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.BaldAblaufend, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumInEinemTag_ReturnsBaldAblaufend()
    {
        // Arrange
        DateTime verfallsdatum = new DateTime(2026, 7, 9);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.BaldAblaufend, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumInDreiTagen_ReturnsBaldAblaufend()
    {
        // Arrange: +3 Tage ist inklusiv (Boundary)
        DateTime verfallsdatum = new DateTime(2026, 7, 11);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.BaldAblaufend, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumInVierTagen_ReturnsOk()
    {
        // Arrange: +4 Tage ist exklusiv (außerhalb 0..3)
        DateTime verfallsdatum = new DateTime(2026, 7, 12);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.Ok, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumWeitInVergangenheit_ReturnsAbgelaufen()
    {
        // Arrange
        DateTime verfallsdatum = new DateTime(2026, 1, 1);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.Abgelaufen, result);
    }

    [Fact]
    public void BerechneStatus_VerfallsdatumWeitInZukunft_ReturnsOk()
    {
        // Arrange
        DateTime verfallsdatum = new DateTime(2027, 1, 1);
        DateTime heute = new DateTime(2026, 7, 8);

        // Act
        VerfallsdatumStatus result = VerfallsdatumWarnungService.BerechneStatus(verfallsdatum, heute);

        // Assert
        Assert.Equal(VerfallsdatumStatus.Ok, result);
    }

    // === GetWarnungenAsync Tests (12) ===

    [Fact]
    public async Task GetWarnungenAsync_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProduktInstanz>());

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithNullFromRepository_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ProduktInstanz>)null!);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithOnlyOkProdukten_ReturnsEmptyList()
    {
        // Arrange: Alle MHD > +3 Tage
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz { Id = 1, Verfallsdatum = new DateTime(2026, 7, 15), LebensmittelKatalogId = 1 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithAbgelaufenemProdukt_ReturnsAbgelaufenWarnung()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 5),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Milch" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(VerfallsdatumStatus.Abgelaufen, result[0].Status);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithBaldAblaufendemProdukt_ReturnsBaldAblaufendWarnung()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 10),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Käse" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(VerfallsdatumStatus.BaldAblaufend, result[0].Status);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithMixedStatus_FiltersOutOkProdukte()
    {
        // Arrange: Ok + Gelb + Rot → nur 2 zurückgeben
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 20),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Ok" }
            },
            new ProduktInstanz
            {
                Id = 2,
                Verfallsdatum = new DateTime(2026, 7, 10),
                LebensmittelKatalogId = 2,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 2, Name = "Gelb" }
            },
            new ProduktInstanz
            {
                Id = 3,
                Verfallsdatum = new DateTime(2026, 7, 5),
                LebensmittelKatalogId = 3,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 3, Name = "Rot" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, w => w.Status == VerfallsdatumStatus.Ok);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithMultipleWarnungen_SortsAbgelaufeneZuerstDannNachDatumAufsteigend()
    {
        // Arrange: Daten +2, -5, 0, -1 → erwartet -5, -1, 0, +2
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz { Id = 1, Verfallsdatum = new DateTime(2026, 7, 10), LebensmittelKatalogId = 1 },
            new ProduktInstanz { Id = 2, Verfallsdatum = new DateTime(2026, 7, 3), LebensmittelKatalogId = 2 },
            new ProduktInstanz { Id = 3, Verfallsdatum = new DateTime(2026, 7, 8), LebensmittelKatalogId = 3 },
            new ProduktInstanz { Id = 4, Verfallsdatum = new DateTime(2026, 7, 7), LebensmittelKatalogId = 4 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.True(result[0].Verfallsdatum <= result[1].Verfallsdatum);
        Assert.True(result[1].Verfallsdatum <= result[2].Verfallsdatum);
        Assert.True(result[2].Verfallsdatum <= result[3].Verfallsdatum);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithVerfallsdatumHeute_ReturnsTageBisAblaufZero()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 8),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Test" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].TageBisAblauf);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithAbgelaufenemProdukt_ReturnsNegativeTageBisAblauf()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 5),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Test" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(-3, result[0].TageBisAblauf);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithLebensmittelKatalogNavigation_MapsLebensmittelName()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 10),
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Käsekuchen" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Käsekuchen", result[0].LebensmittelName);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithNullLebensmittelKatalog_UsesFallbackName()
    {
        // Arrange
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 1,
                Verfallsdatum = new DateTime(2026, 7, 10),
                LebensmittelKatalogId = 99,
                LebensmittelKatalog = null
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Contains("99", result[0].LebensmittelName);
    }

    [Fact]
    public async Task GetWarnungenAsync_WithWarnung_MapsProduktInstanzIdAndVerfallsdatum()
    {
        // Arrange
        DateTime mhd = new DateTime(2026, 7, 10);
        List<ProduktInstanz> instanzen = new()
        {
            new ProduktInstanz
            {
                Id = 42,
                Verfallsdatum = mhd,
                LebensmittelKatalogId = 1,
                LebensmittelKatalog = new LebensmittelKatalog { Id = 1, Name = "Test" }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(instanzen);

        // Act
        List<VerfallsdatumWarnungDto> result = await _service.GetWarnungenAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(42, result[0].ProduktInstanzId);
        Assert.Equal(mhd, result[0].Verfallsdatum);
    }
}

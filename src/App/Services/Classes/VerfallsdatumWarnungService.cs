using FoodDatabase.App.Models;
using FoodDatabase.App.Services.Dtos;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Services.Classes;

/// <summary>
/// Service für Verfallsdatum-Warnungen (UC7).
/// Berechnet Warnstufen on-demand ohne Persistierung.
/// </summary>
public class VerfallsdatumWarnungService : IVerfallsdatumWarnungService
{
    private readonly IRepository<ProduktInstanz> _repository;
    private readonly TimeProvider _timeProvider;

    public VerfallsdatumWarnungService(IRepository<ProduktInstanz> repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<List<VerfallsdatumWarnungDto>> GetWarnungenAsync()
    {
        IEnumerable<ProduktInstanz> allInstanzen = await _repository.GetAllAsync();
        if (allInstanzen is null)
            return new List<VerfallsdatumWarnungDto>();

        DateTime heute = _timeProvider.GetLocalNow().Date;

        List<VerfallsdatumWarnungDto> warnungen = allInstanzen
            .Select(i => new VerfallsdatumWarnungDto
            {
                ProduktInstanzId = i.Id,
                LebensmittelName = i.LebensmittelKatalog?.Name ?? $"Lebensmittel #{i.LebensmittelKatalogId}",
                Verfallsdatum = i.Verfallsdatum,
                TageBisAblauf = (i.Verfallsdatum.Date - heute).Days,
                Status = BerechneStatus(i.Verfallsdatum, heute)
            })
            .Where(w => w.Status != VerfallsdatumStatus.Ok)
            .OrderBy(w => w.Verfallsdatum)
            .ToList();

        return warnungen;
    }

    /// <summary>
    /// Berechnet die Warnstufe für ein Verfallsdatum.
    /// Reine statische Funktion für direkte Testbarkeit.
    /// Grenzen: < 0 = Abgelaufen; 0..3 = BaldAblaufend; > 3 = Ok.
    /// </summary>
    public static VerfallsdatumStatus BerechneStatus(DateTime verfallsdatum, DateTime heute)
    {
        int tage = (verfallsdatum.Date - heute).Days;

        if (tage < 0)
            return VerfallsdatumStatus.Abgelaufen;

        if (tage <= 3)
            return VerfallsdatumStatus.BaldAblaufend;

        return VerfallsdatumStatus.Ok;
    }
}

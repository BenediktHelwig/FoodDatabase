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
        throw new NotImplementedException("TDD Red Phase — Implementierung folgt.");
    }

    /// <summary>
    /// Berechnet die Warnstufe für ein Verfallsdatum.
    /// Reine statische Funktion für direkte Testbarkeit.
    /// </summary>
    public static VerfallsdatumStatus BerechneStatus(DateTime verfallsdatum, DateTime heute)
    {
        throw new NotImplementedException("TDD Red Phase — Implementierung folgt.");
    }
}

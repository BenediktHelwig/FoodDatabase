namespace FoodDatabase.Tests.Unit.Helpers;

/// <summary>
/// Manueller TimeProvider-Fake für Tests mit festem Datum.
/// Macht GetLocalNow() maschinenunabhängig (UTC).
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedNow;

    public FixedTimeProvider(DateTime fixedDate)
        => _fixedNow = new DateTimeOffset(DateTime.SpecifyKind(fixedDate, DateTimeKind.Utc));

    public override DateTimeOffset GetUtcNow() => _fixedNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

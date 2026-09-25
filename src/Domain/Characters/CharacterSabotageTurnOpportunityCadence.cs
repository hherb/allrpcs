namespace Allrpcs.Domain.Characters;

/// <summary>Defines a bounded cadence in authoritative meaningful-world-event occurrences.</summary>
public sealed record CharacterSabotageTurnOpportunityCadence
{
    /// <summary>Gets the largest supported number of meaningful events between opportunities.</summary>
    public const int MaximumMeaningfulEventInterval = 100;

    /// <summary>Creates one cadence from a positive bounded meaningful-event interval.</summary>
    public CharacterSabotageTurnOpportunityCadence(int meaningfulEventInterval)
    {
        if (meaningfulEventInterval is < 1 or > MaximumMeaningfulEventInterval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(meaningfulEventInterval),
                $"A sabotage cadence must be between 1 and {MaximumMeaningfulEventInterval} events.");
        }

        MeaningfulEventInterval = meaningfulEventInterval;
    }

    /// <summary>Gets the number of meaningful events between sabotage opportunities.</summary>
    public int MeaningfulEventInterval { get; }
}

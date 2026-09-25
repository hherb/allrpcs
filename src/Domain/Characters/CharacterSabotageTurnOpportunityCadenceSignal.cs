using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one claimed cadence boundary and the meaningful event that reached it.</summary>
public sealed record CharacterSabotageTurnOpportunityCadenceSignal
{
    /// <summary>Creates one immutable signal without deciding whether its provenance is current.</summary>
    public CharacterSabotageTurnOpportunityCadenceSignal(
        CharacterSabotageTurnOpportunityCadence cadence,
        WorldEvent worldEvent,
        int occurredMeaningfulEventCount)
    {
        ArgumentNullException.ThrowIfNull(cadence);
        ArgumentNullException.ThrowIfNull(worldEvent);

        if (occurredMeaningfulEventCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurredMeaningfulEventCount),
                "A sabotage cadence signal must retain a positive meaningful-event count.");
        }

        Cadence = cadence;
        WorldEvent = worldEvent;
        OccurredMeaningfulEventCount = occurredMeaningfulEventCount;
    }

    /// <summary>Gets the exact cadence claimed by this signal.</summary>
    public CharacterSabotageTurnOpportunityCadence Cadence { get; }

    /// <summary>Gets the exact meaningful world event claimed by this signal.</summary>
    public WorldEvent WorldEvent { get; }

    /// <summary>Gets the claimed authoritative meaningful-event count including this event.</summary>
    public int OccurredMeaningfulEventCount { get; }
}

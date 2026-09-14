namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact move destination to discoverable evidence for the same occurrence.</summary>
public sealed class CharacterObjectSabotageMoveCandidate
{
    /// <summary>Creates one immutable move candidate without applying either state change.</summary>
    public CharacterObjectSabotageMoveCandidate(
        CharacterObjectSabotageMoveDestination destination,
        CharacterObjectSabotageEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(evidence);

        if (!ReferenceEquals(destination.Occurrence, evidence.Occurrence))
        {
            throw new ArgumentException(
                "Move destination and evidence must retain the same exact occurrence.",
                nameof(evidence));
        }

        Destination = destination;
        Evidence = evidence;
    }

    /// <summary>Gets the exact occurrence-bound authored destination.</summary>
    public CharacterObjectSabotageMoveDestination Destination { get; }

    /// <summary>Gets the exact occurrence-bound discoverable evidence.</summary>
    public CharacterObjectSabotageEvidence Evidence { get; }
}

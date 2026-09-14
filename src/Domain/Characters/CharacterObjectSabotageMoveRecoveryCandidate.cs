using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact move candidate and its exact pre-move source placement.</summary>
public sealed class CharacterObjectSabotageMoveRecoveryCandidate
{
    /// <summary>Creates one immutable recovery candidate without applying recovery.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate(
        CharacterObjectSabotageMoveCandidate candidate,
        WorldObjectPlacement sourcePlacement)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(sourcePlacement);

        if (!ReferenceEquals(
                candidate.Destination.Occurrence.Target,
                sourcePlacement.WorldObject))
        {
            throw new ArgumentException(
                "The source placement must retain the move occurrence's exact target.",
                nameof(sourcePlacement));
        }

        if (sourcePlacement.Location.Identity.Id
            == candidate.Destination.Destination.Identity.Id)
        {
            throw new ArgumentException(
                "The source placement must differ from the move destination.",
                nameof(sourcePlacement));
        }

        Candidate = candidate;
        SourcePlacement = sourcePlacement;
    }

    /// <summary>Gets the exact move candidate.</summary>
    public CharacterObjectSabotageMoveCandidate Candidate { get; }

    /// <summary>Gets the exact pre-move source placement.</summary>
    public WorldObjectPlacement SourcePlacement { get; }
}

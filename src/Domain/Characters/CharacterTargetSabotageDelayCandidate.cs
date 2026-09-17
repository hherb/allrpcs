using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact eligible delay occurrence to one exact advancing world event.</summary>
public sealed class CharacterTargetSabotageDelayCandidate
{
    /// <summary>Creates one immutable delay candidate without applying its event.</summary>
    public CharacterTargetSabotageDelayCandidate(
        CharacterTargetSabotageOccurrence occurrence,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(worldEvent);

        Occurrence = occurrence;
        WorldEvent = worldEvent;
    }

    /// <summary>Gets the exact eligible character-targeted occurrence.</summary>
    public CharacterTargetSabotageOccurrence Occurrence { get; }

    /// <summary>Gets the exact caller-authored world event candidate.</summary>
    public WorldEvent WorldEvent { get; }
}

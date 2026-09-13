using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact task activity to one proposed authoritative world occurrence.</summary>
public sealed record CharacterIndependentTaskWorldEventOccurrence
{
    /// <summary>Creates one immutable occurrence candidate without applying either value.</summary>
    public CharacterIndependentTaskWorldEventOccurrence(
        CharacterIndependentTaskActivity activity,
        CharacterTurnWorldEventOccurrence occurrence)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(occurrence);

        Activity = activity;
        Occurrence = occurrence;
    }

    /// <summary>Gets the exact independent-task activity that proposes the occurrence.</summary>
    public CharacterIndependentTaskActivity Activity { get; }

    /// <summary>Gets the exact proposed world-event and transcript occurrence.</summary>
    public CharacterTurnWorldEventOccurrence Occurrence { get; }
}

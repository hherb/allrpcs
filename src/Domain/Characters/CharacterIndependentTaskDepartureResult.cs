namespace Allrpcs.Domain.Characters;

/// <summary>Contains the exact active task and party state after one bounded departure.</summary>
public sealed record CharacterIndependentTaskDepartureResult
{
    /// <summary>Creates one immutable atomic departure result.</summary>
    public CharacterIndependentTaskDepartureResult(
        CharacterIndependentTaskActivity activeActivity,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(activeActivity);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(party);

        ActiveActivity = activeActivity;
        Activities = activities;
        Party = party;
    }

    /// <summary>Gets the exact newly active independent-task activity.</summary>
    public CharacterIndependentTaskActivity ActiveActivity { get; }

    /// <summary>Gets the resulting independent-task activity snapshot.</summary>
    public CharacterIndependentTaskActivitySnapshot Activities { get; }

    /// <summary>Gets the resulting party without the assigned character.</summary>
    public CharacterPartySnapshot Party { get; }
}

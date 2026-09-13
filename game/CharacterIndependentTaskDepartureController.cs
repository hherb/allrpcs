using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact scene task and party snapshots across bounded departures.</summary>
public partial class CharacterIndependentTaskDepartureController : Node
{
    /// <summary>Gets the initialized exact independent-task activities, if any.</summary>
    public CharacterIndependentTaskActivitySnapshot? Activities { get; private set; }

    /// <summary>Gets the initialized exact party membership, if any.</summary>
    public CharacterPartySnapshot? Party { get; private set; }

    /// <summary>Gets the exact activity started by the latest successful departure, if any.</summary>
    public CharacterIndependentTaskActivity? ActiveActivity { get; private set; }

    /// <summary>Atomically initializes exact task and party state once.</summary>
    public bool TryInitialize(
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(party);

        if (Activities is not null || Party is not null)
        {
            return false;
        }

        Activities = activities;
        Party = party;
        return true;
    }

    /// <summary>
    /// Starts one eligible task and separates its member only from exact expected current state.
    /// </summary>
    public bool TryStartAndSeparate(
        CharacterId characterId,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        CharacterPartySnapshot expectedParty)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(expectedParty);

        if (!ReferenceEquals(Activities, expectedActivities)
            || !ReferenceEquals(Party, expectedParty))
        {
            return false;
        }

        CharacterIndependentTaskDepartureResult result;
        try
        {
            result = CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                characterId,
                expectedActivities,
                expectedParty);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Activities = result.Activities;
        Party = result.Party;
        ActiveActivity = result.ActiveActivity;
        return true;
    }

    /// <summary>Installs one exact prevalidated task completion from current scene state.</summary>
    internal bool TryInstallCompletion(
        CharacterIndependentTaskActivity expectedActiveActivity,
        CharacterIndependentTaskActivitySnapshot expectedActivities,
        CharacterIndependentTaskWorldEventOccurrenceResult result)
    {
        ArgumentNullException.ThrowIfNull(expectedActiveActivity);
        ArgumentNullException.ThrowIfNull(expectedActivities);
        ArgumentNullException.ThrowIfNull(result);

        if (!ReferenceEquals(ActiveActivity, expectedActiveActivity)
            || !ReferenceEquals(Activities, expectedActivities)
            || !ReferenceEquals(result.Candidate.Activity, expectedActiveActivity))
        {
            return false;
        }

        Activities = result.Activities;
        ActiveActivity = null;
        return true;
    }
}

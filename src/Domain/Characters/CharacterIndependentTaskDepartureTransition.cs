using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically starts one assigned task and separates its character from the party.</summary>
public static class CharacterIndependentTaskDepartureTransition
{
    /// <summary>Returns fresh task and party snapshots plus the exact newly active activity.</summary>
    public static CharacterIndependentTaskDepartureResult StartAndSeparate(
        CharacterId characterId,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(party);

        CharacterIndependentTaskActivity? selected =
            CharacterIndependentTaskSelectionPolicy.SelectNext(characterId, activities);
        if (selected is null || !CharacterPartySeparationPolicy.CanSeparate(characterId, party))
        {
            throw new InvalidOperationException(
                "A party member requires an eligible assigned task before departure.");
        }

        CharacterIndependentTaskActivitySnapshot resultingActivities =
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                activities,
                selected,
                CharacterIndependentTaskActivityState.Active);
        CharacterIndependentTaskActivity activeActivity = resultingActivities.Activities.Single(
            activity => ReferenceEquals(activity.Assignment, selected.Assignment));
        CharacterPartySnapshot resultingParty =
            CharacterPartySeparationTransition.Separate(characterId, party);

        return new CharacterIndependentTaskDepartureResult(
            activeActivity,
            resultingActivities,
            resultingParty);
    }
}

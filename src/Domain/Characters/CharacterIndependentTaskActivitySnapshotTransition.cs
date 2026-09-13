namespace Allrpcs.Domain.Characters;

/// <summary>Transitions one exact contained task activity without reordering the snapshot.</summary>
public static class CharacterIndependentTaskActivitySnapshotTransition
{
    /// <summary>Returns a snapshot replacing only the exact activity through a legal transition.</summary>
    public static CharacterIndependentTaskActivitySnapshot Transition(
        CharacterIndependentTaskActivitySnapshot snapshot,
        CharacterIndependentTaskActivity activity,
        CharacterIndependentTaskActivityState nextState)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(nextState);

        if (!snapshot.Activities.Any(candidate => ReferenceEquals(candidate, activity)))
        {
            throw new InvalidOperationException(
                "Only an exact activity retained by the snapshot can transition.");
        }

        CharacterIndependentTaskActivity transitioned =
            CharacterIndependentTaskLifecycleTransition.Transition(activity, nextState);

        return new CharacterIndependentTaskActivitySnapshot(snapshot.Activities.Select(candidate =>
            ReferenceEquals(candidate, activity) ? transitioned : candidate));
    }
}

using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects the next assigned independent task for one character.</summary>
public static class CharacterIndependentTaskSelectionPolicy
{
    /// <summary>
    /// Returns the first exact assigned activity unless the character already has active work.
    /// </summary>
    public static CharacterIndependentTaskActivity? SelectNext(
        CharacterId characterId,
        CharacterIndependentTaskActivitySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Activities.Any(activity =>
                activity.Assignment.CharacterId == characterId
                && activity.State == CharacterIndependentTaskActivityState.Active))
        {
            return null;
        }

        return snapshot.Activities.FirstOrDefault(activity =>
            activity.Assignment.CharacterId == characterId
            && activity.State == CharacterIndependentTaskActivityState.Assigned);
    }
}

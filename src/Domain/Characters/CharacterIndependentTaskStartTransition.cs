using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Starts the first eligible independent task for one character.</summary>
public static class CharacterIndependentTaskStartTransition
{
    /// <summary>Returns a snapshot with the selected exact activity advanced to active.</summary>
    public static CharacterIndependentTaskActivitySnapshot StartNext(
        CharacterId characterId,
        CharacterIndependentTaskActivitySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(snapshot);

        CharacterIndependentTaskActivity? selected =
            CharacterIndependentTaskSelectionPolicy.SelectNext(characterId, snapshot);
        if (selected is null)
        {
            throw new InvalidOperationException(
                "The character has no eligible assigned independent task.");
        }

        return CharacterIndependentTaskActivitySnapshotTransition.Transition(
            snapshot,
            selected,
            CharacterIndependentTaskActivityState.Active);
    }
}

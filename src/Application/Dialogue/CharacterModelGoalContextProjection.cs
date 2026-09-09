using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects only one character's authored goals into provider-neutral model context.
/// </summary>
public static class CharacterModelGoalContextProjection
{
    /// <summary>
    /// Returns fresh goal entries in stable authored order with exact description text.
    /// </summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        List<CharacterModelContextEntry> entries = new();
        foreach (CharacterGoal goal in character.Goals.Entries)
        {
            entries.Add(new CharacterModelContextEntry(
                CharacterModelContextEntryKind.Goal,
                goal.Description));
        }

        return entries.AsReadOnly();
    }
}

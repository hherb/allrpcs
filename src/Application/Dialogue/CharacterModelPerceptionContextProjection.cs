using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects only identified world-object descriptions visible to one character into model context.
/// </summary>
public static class CharacterModelPerceptionContextProjection
{
    /// <summary>
    /// Returns fresh filtered-perception entries in stable perception order with exact authored text.
    /// </summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        Character character,
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (perception.CharacterId != character.Identity.Id)
        {
            throw new ArgumentException(
                "The perception snapshot must belong to the supplied character.",
                nameof(perception));
        }

        List<CharacterModelContextEntry> entries = new();
        foreach (WorldObjectVisibility visibility in perception.Entries)
        {
            if (visibility.VisibilityState != VisibilityState.Identified)
            {
                continue;
            }

            WorldObject worldObject = WorldObjectLookup.FindById(visibility.ObjectId, worldObjects)
                ?? throw new ArgumentException(
                    "Every identified object must exist in the authoritative world-object snapshot.",
                    nameof(worldObjects));

            if (worldObject.Description.Text.Length > 0)
            {
                entries.Add(new CharacterModelContextEntry(
                    CharacterModelContextEntryKind.FilteredPerception,
                    worldObject.Description.Text));
            }
        }

        return entries.AsReadOnly();
    }
}

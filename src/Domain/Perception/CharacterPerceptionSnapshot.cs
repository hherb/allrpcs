using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Perception;

/// <summary>
/// Associates one character with an ordered snapshot of world-object visibility entries.
/// </summary>
public sealed class CharacterPerceptionSnapshot
{
    /// <summary>
    /// Creates a character perception snapshot. An empty sequence explicitly means the snapshot
    /// contains no world-object visibility entries.
    /// </summary>
    public CharacterPerceptionSnapshot(
        CharacterId characterId,
        IEnumerable<WorldObjectVisibility> entries)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(entries);

        List<WorldObjectVisibility> orderedEntries = new();
        HashSet<ObjectId> objectIds = new();

        foreach (WorldObjectVisibility entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Perception-snapshot entries cannot contain null values.",
                    nameof(entries));
            }

            if (!objectIds.Add(entry.ObjectId))
            {
                throw new ArgumentException(
                    "Perception-snapshot entries cannot contain duplicate object identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        CharacterId = characterId;
        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets the stable identifier of the character whose perception the snapshot represents.
    /// </summary>
    public CharacterId CharacterId { get; }

    /// <summary>
    /// Gets world-object visibility entries in their stable supplied order.
    /// </summary>
    public IReadOnlyList<WorldObjectVisibility> Entries { get; }
}

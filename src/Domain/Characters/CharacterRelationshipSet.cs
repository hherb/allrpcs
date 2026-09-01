using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains an ordered set of relationships with distinct related characters.
/// </summary>
public sealed class CharacterRelationshipSet
{
    /// <summary>
    /// Creates a relationship set. An empty sequence explicitly means no relationships are authored.
    /// </summary>
    public CharacterRelationshipSet(IEnumerable<CharacterRelationship> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<CharacterRelationship> orderedEntries = new();
        HashSet<CharacterId> relatedCharacterIds = new();

        foreach (CharacterRelationship entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Relationship entries cannot contain null values.",
                    nameof(entries));
            }

            if (!relatedCharacterIds.Add(entry.RelatedCharacterId))
            {
                throw new ArgumentException(
                    "Relationship entries cannot contain duplicate related character identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets relationships in their stable authored order.
    /// </summary>
    public IReadOnlyList<CharacterRelationship> Entries { get; }
}

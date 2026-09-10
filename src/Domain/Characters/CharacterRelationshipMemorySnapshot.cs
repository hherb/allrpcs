using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded ordered relationship memories for one character.</summary>
public sealed class CharacterRelationshipMemorySnapshot
{
    /// <summary>Gets the maximum related characters retained per character.</summary>
    public const int MaximumRelationshipCount = 64;

    /// <summary>Creates one immutable per-character relationship-memory snapshot.</summary>
    public CharacterRelationshipMemorySnapshot(
        CharacterId characterId,
        IEnumerable<CharacterRelationshipMemory> relationships)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(relationships);

        List<CharacterRelationshipMemory> retainedRelationships = new();
        HashSet<CharacterId> relatedCharacterIds = new();
        foreach (CharacterRelationshipMemory relationship in relationships)
        {
            if (relationship is null)
            {
                throw new ArgumentException(
                    "Relationship-memory snapshots cannot contain null values.",
                    nameof(relationships));
            }

            if (relationship.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every relationship must belong to the snapshot character.",
                    nameof(relationships));
            }

            if (!relatedCharacterIds.Add(relationship.RelatedCharacterId))
            {
                throw new ArgumentException(
                    "A character can have only one relationship memory for another character.",
                    nameof(relationships));
            }

            retainedRelationships.Add(relationship);
            if (retainedRelationships.Count > MaximumRelationshipCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(relationships),
                    retainedRelationships.Count,
                    $"A character accepts at most {MaximumRelationshipCount} relationships.");
            }
        }

        CharacterId = characterId;
        Relationships = retainedRelationships.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact relationship-memory instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterRelationshipMemory> Relationships { get; }
}

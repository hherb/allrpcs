using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded multidimensional relationship state and its recorded local reasons.</summary>
public sealed record CharacterRelationshipMemory
{
    /// <summary>Gets the maximum recorded reasons retained for one relationship.</summary>
    public const int MaximumReasonCount = 64;

    /// <summary>Creates one immutable character-to-character relationship memory.</summary>
    public CharacterRelationshipMemory(
        CharacterId characterId,
        CharacterId relatedCharacterId,
        IEnumerable<CharacterRelationshipDimensionValue> dimensions,
        IEnumerable<CharacterRelationshipReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(relatedCharacterId);
        ArgumentNullException.ThrowIfNull(dimensions);
        ArgumentNullException.ThrowIfNull(reasons);
        if (characterId == relatedCharacterId)
        {
            throw new ArgumentException(
                "A character relationship must refer to a different character.",
                nameof(relatedCharacterId));
        }

        List<CharacterRelationshipDimensionValue> retainedDimensions = new();
        HashSet<CharacterRelationshipDimension> dimensionSet = new();
        foreach (CharacterRelationshipDimensionValue dimension in dimensions)
        {
            if (dimension is null || !dimensionSet.Add(dimension.Dimension))
            {
                throw new ArgumentException(
                    "Relationship dimensions must be non-null and unique.",
                    nameof(dimensions));
            }

            retainedDimensions.Add(dimension);
        }

        if (retainedDimensions.Count == 0)
        {
            throw new ArgumentException(
                "A relationship memory requires at least one dimension.",
                nameof(dimensions));
        }

        List<CharacterRelationshipReason> retainedReasons = new();
        HashSet<RelationshipReasonId> reasonIds = new();
        foreach (CharacterRelationshipReason reason in reasons)
        {
            if (reason is null || !reasonIds.Add(reason.ReasonId))
            {
                throw new ArgumentException(
                    "Relationship reasons must be non-null and have unique identities.",
                    nameof(reasons));
            }

            if (!dimensionSet.Contains(reason.Dimension))
            {
                throw new ArgumentException(
                    "Every relationship reason must concern a retained dimension.",
                    nameof(reasons));
            }

            retainedReasons.Add(reason);
            if (retainedReasons.Count > MaximumReasonCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reasons),
                    retainedReasons.Count,
                    $"A relationship accepts at most {MaximumReasonCount} recorded reasons.");
            }
        }

        CharacterId = characterId;
        RelatedCharacterId = relatedCharacterId;
        Dimensions = retainedDimensions.AsReadOnly();
        Reasons = retainedReasons.AsReadOnly();
    }

    /// <summary>Gets the exact character retaining the relationship.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact other character in the relationship.</summary>
    public CharacterId RelatedCharacterId { get; }

    /// <summary>Gets exact dimension values in stable supplied order.</summary>
    public IReadOnlyList<CharacterRelationshipDimensionValue> Dimensions { get; }

    /// <summary>Gets exact recorded reasons in stable supplied order.</summary>
    public IReadOnlyList<CharacterRelationshipReason> Reasons { get; }
}

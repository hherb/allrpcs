namespace Allrpcs.Domain.Characters;

/// <summary>Applies one eligible bounded exchange change to immutable relationship memory.</summary>
public static class CharacterRelationshipChangeTransition
{
    /// <summary>Returns a new relationship with one changed dimension and exact appended reason.</summary>
    public static CharacterRelationshipMemory Apply(
        CharacterRelationshipMemory relationship,
        CharacterRelationshipChange change)
    {
        if (!CharacterRelationshipChangePolicy.CanApply(relationship, change))
        {
            throw new InvalidOperationException(
                "Only one eligible bounded relationship change may apply per source exchange.");
        }

        IEnumerable<CharacterRelationshipDimensionValue> dimensions = relationship.Dimensions.Select(
            dimension => dimension.Dimension == change.Reason.Dimension
                ? new CharacterRelationshipDimensionValue(
                    dimension.Dimension,
                    dimension.Value + change.Delta)
                : dimension);
        return new CharacterRelationshipMemory(
            relationship.CharacterId,
            relationship.RelatedCharacterId,
            dimensions,
            relationship.Reasons
                .Skip(Math.Max(0, relationship.Reasons.Count - CharacterRelationshipMemory.MaximumReasonCount + 1))
                .Append(change.Reason));
    }
}

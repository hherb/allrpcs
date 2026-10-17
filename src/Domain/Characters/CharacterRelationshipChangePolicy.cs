namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether one bounded exchange change can update a relationship.</summary>
public static class CharacterRelationshipChangePolicy
{
    /// <summary>Returns whether the exact change can be applied once without clamping.</summary>
    public static bool CanApply(
        CharacterRelationshipMemory relationship,
        CharacterRelationshipChange change)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(change);

        if (relationship.CharacterId != change.CharacterId
            || relationship.RelatedCharacterId != change.RelatedCharacterId
            || relationship.Reasons.Any(reason =>
                reason.ReasonId == change.Reason.ReasonId
                || reason.SourceTurnId == change.Reason.SourceTurnId))
        {
            return false;
        }

        CharacterRelationshipDimensionValue? current = relationship.Dimensions.FirstOrDefault(
            dimension => dimension.Dimension == change.Reason.Dimension);
        if (current is null)
        {
            return false;
        }

        int resultingValue = current.Value + change.Delta;
        return resultingValue is >= CharacterRelationshipDimensionValue.MinimumValue
            and <= CharacterRelationshipDimensionValue.MaximumValue;
    }
}

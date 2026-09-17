namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted recovery selection or preserves abstention.</summary>
public static class CharacterTargetSabotageInfluenceRecoveryConsiderationTransition
{
    /// <summary>Returns fresh relationship state for a selection, or null for abstention.</summary>
    public static CharacterTargetSabotageInfluenceRecoveryResult? Apply(
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageInfluenceRecoveryCandidate? selectedCandidate =
            considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        return CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
            considerationResult.Request.Relationship,
            selectedCandidate);
    }
}

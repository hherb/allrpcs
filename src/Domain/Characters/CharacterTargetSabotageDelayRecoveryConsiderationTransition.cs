namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted delay-recovery selection or preserves abstention.</summary>
public static class CharacterTargetSabotageDelayRecoveryConsiderationTransition
{
    /// <summary>Returns fresh authoritative world state for a selection, or null for abstention.</summary>
    public static CharacterTargetSabotageDelayRecoveryResult? Apply(
        CharacterTargetSabotageDelayRecoveryConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageDelayRecoveryCandidate? selectedCandidate =
            considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        return CharacterTargetSabotageDelayRecoveryTransition.Recover(
            considerationResult.Request.WorldEvents,
            considerationResult.Request.EventLog,
            considerationResult.Request.WorldTime,
            selectedCandidate);
    }
}

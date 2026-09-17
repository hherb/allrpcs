namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted target-delay selection or preserves abstention.</summary>
public static class CharacterTargetSabotageDelayConsiderationTransition
{
    /// <summary>Returns fresh world state for a selection, or null for abstention.</summary>
    public static CharacterTargetSabotageDelayResult? Apply(
        CharacterTargetSabotageDelayConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageDelayCandidate? selectedCandidate =
            considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterTargetSabotageDelayConsiderationRequest request =
            considerationResult.Request;
        return CharacterTargetSabotageDelayTransition.Apply(
            request.Actor,
            request.InteractionAccess,
            request.WorldEvents,
            request.EventLog,
            request.WorldTime,
            selectedCandidate);
    }
}

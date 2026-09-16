namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted influence selection or preserves abstention.</summary>
public static class CharacterTargetSabotageInfluenceConsiderationTransition
{
    /// <summary>Returns fresh relationship state for a selection, or null for abstention.</summary>
    public static CharacterTargetSabotageInfluenceResult? Apply(
        CharacterTargetSabotageInfluenceConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterTargetSabotageInfluenceCandidate? selectedCandidate =
            considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterTargetSabotageInfluenceConsiderationRequest request =
            considerationResult.Request;
        return CharacterTargetSabotageInfluenceTransition.Apply(
            request.Actor,
            request.InteractionAccess,
            request.Relationship,
            selectedCandidate);
    }
}

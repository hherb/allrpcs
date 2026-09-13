namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted concealment selection or preserves abstention.</summary>
public static class CharacterObjectSabotageConcealmentConsiderationTransition
{
    /// <summary>Returns fresh concealment state for a selection, or null for abstention.</summary>
    public static CharacterObjectSabotageConcealmentResult? Apply(
        CharacterObjectSabotageConcealmentConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageEvidence? selectedCandidate = considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterObjectSabotageConcealmentConsiderationRequest request = considerationResult.Request;
        return CharacterObjectSabotageConcealmentTransition.Apply(
            request.Character,
            request.PhysicalAccess,
            request.WorldObjects,
            request.Concealment,
            request.Evidence,
            selectedCandidate);
    }
}

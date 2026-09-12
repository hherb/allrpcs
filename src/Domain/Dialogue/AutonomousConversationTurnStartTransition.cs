namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Starts one already-approved autonomous follow-up without advancing its conversation budget.
/// </summary>
public static class AutonomousConversationTurnStartTransition
{
    /// <summary>Produces an awaiting activity for the exact eligible follow-up request.</summary>
    public static CharacterTurnActivity Start(
        AutonomousConversationBudget budget,
        CharacterTurnParticipationDecision decision,
        CharacterTurnRequest request,
        CharacterTurnActivity? currentActivity)
    {
        if (!AutonomousConversationTurnStartPolicy.CanStart(
                budget,
                decision,
                request,
                currentActivity))
        {
            throw new InvalidOperationException(
                "Only an exact speaking decision in an open autonomous conversation may start.");
        }

        return CharacterTurnStartTransition.Start(currentActivity, request);
    }
}

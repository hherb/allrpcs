namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Determines whether one already-selected autonomous follow-up turn may start.
/// </summary>
public static class AutonomousConversationTurnStartPolicy
{
    /// <summary>
    /// Returns true only for an exact speaking decision in an open chain and activity slot.
    /// </summary>
    public static bool CanStart(
        AutonomousConversationBudget budget,
        CharacterTurnParticipationDecision decision,
        CharacterTurnRequest request,
        CharacterTurnActivity? currentActivity)
    {
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(request);

        bool isSpeakingDecision = decision.Kind == CharacterTurnParticipationKind.Respond
            || decision.Kind == CharacterTurnParticipationKind.Interject;

        return AutonomousConversationContinuationPolicy.CanContinue(budget)
            && isSpeakingDecision
            && ReferenceEquals(decision.ScheduledCharacterId, request.RespondingCharacterId)
            && CharacterTurnStartPolicy.CanStart(currentActivity);
    }
}

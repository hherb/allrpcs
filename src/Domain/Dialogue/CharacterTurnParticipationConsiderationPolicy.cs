namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Validates one participation choice against the exact request that prompted its consideration.
/// </summary>
public static class CharacterTurnParticipationConsiderationPolicy
{
    /// <summary>
    /// Returns true when the exact scheduled character may make the supplied explicit choice.
    /// </summary>
    public static bool CanAccept(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        return CharacterTurnParticipationDecisionPolicy.CanAccept(
            decision,
            request.ScheduledCharacter,
            request.IsDirectlyAddressed);
    }
}

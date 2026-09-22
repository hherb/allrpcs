namespace Allrpcs.Domain.Characters;

/// <summary>Validates one exact request-member sabotage-kind selection or abstention.</summary>
public static class CharacterSabotageActionDecisionPolicy
{
    /// <summary>Returns whether the exact actor made a request-member selection or abstention.</summary>
    public static bool CanAccept(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        return ReferenceEquals(decision.ActorId, request.Actor.Identity.Id)
            && (decision.SelectedActionKind is null
                || request.ActionKinds.Any(actionKind =>
                    ReferenceEquals(actionKind, decision.SelectedActionKind)));
    }
}

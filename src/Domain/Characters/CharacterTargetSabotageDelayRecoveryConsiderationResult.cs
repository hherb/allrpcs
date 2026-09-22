namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted delay-recovery decision to its exact consideration request.</summary>
public sealed record CharacterTargetSabotageDelayRecoveryConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationResult(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CharacterTargetSabotageDelayRecoveryDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The delay-recovery decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedCandidate;
    }

    /// <summary>Gets the exact request whose target delay recovery was considered.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted delay-recovery decision.</summary>
    public CharacterTargetSabotageDelayRecoveryDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? SelectedCandidate { get; }
}

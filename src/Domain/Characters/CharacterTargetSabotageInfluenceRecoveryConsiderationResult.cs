namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted recovery decision to its exact consideration request.</summary>
public sealed record CharacterTargetSabotageInfluenceRecoveryConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationResult(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        CharacterTargetSabotageInfluenceRecoveryDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The influence-recovery decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedCandidate;
    }

    /// <summary>Gets the exact request whose influence recovery was considered.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted influence-recovery decision.</summary>
    public CharacterTargetSabotageInfluenceRecoveryDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterTargetSabotageInfluenceRecoveryCandidate? SelectedCandidate { get; }
}

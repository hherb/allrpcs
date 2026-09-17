namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted target-delay decision to its exact consideration request.</summary>
public sealed record CharacterTargetSabotageDelayConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterTargetSabotageDelayConsiderationResult(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CharacterTargetSabotageDelayDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterTargetSabotageDelayDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The target-delay decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedCandidate;
    }

    /// <summary>Gets the exact request whose target delay was considered.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted target-delay decision.</summary>
    public CharacterTargetSabotageDelayDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterTargetSabotageDelayCandidate? SelectedCandidate { get; }
}

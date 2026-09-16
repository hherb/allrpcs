namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted influence decision to its exact consideration request.</summary>
public sealed record CharacterTargetSabotageInfluenceConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterTargetSabotageInfluenceConsiderationResult(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        CharacterTargetSabotageInfluenceDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The influence decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedCandidate;
    }

    /// <summary>Gets the exact request whose influence was considered.</summary>
    public CharacterTargetSabotageInfluenceConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted influence decision.</summary>
    public CharacterTargetSabotageInfluenceDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterTargetSabotageInfluenceCandidate? SelectedCandidate { get; }
}

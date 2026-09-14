namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted move decision to its exact consideration request.</summary>
public sealed record CharacterObjectSabotageMoveConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterObjectSabotageMoveConsiderationResult(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterObjectSabotageMoveDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The move decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedEvidenceId is null
            ? null
            : request.Candidates.Single(candidate =>
                ReferenceEquals(candidate.Evidence.Evidence.EvidenceId, decision.SelectedEvidenceId));
    }

    /// <summary>Gets the exact request whose move was considered.</summary>
    public CharacterObjectSabotageMoveConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted move decision.</summary>
    public CharacterObjectSabotageMoveDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterObjectSabotageMoveCandidate? SelectedCandidate { get; }
}

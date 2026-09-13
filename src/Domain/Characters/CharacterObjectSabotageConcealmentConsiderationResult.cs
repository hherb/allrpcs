namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted concealment decision to its exact consideration request.</summary>
public sealed record CharacterObjectSabotageConcealmentConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterObjectSabotageConcealmentConsiderationResult(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CharacterObjectSabotageConcealmentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterObjectSabotageConcealmentDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The concealment decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedEvidenceId is null
            ? null
            : request.Candidates.Single(candidate =>
                ReferenceEquals(candidate.Evidence.EvidenceId, decision.SelectedEvidenceId));
    }

    /// <summary>Gets the exact request whose concealment was considered.</summary>
    public CharacterObjectSabotageConcealmentConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted concealment decision.</summary>
    public CharacterObjectSabotageConcealmentDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterObjectSabotageEvidence? SelectedCandidate { get; }
}

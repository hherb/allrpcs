namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted contamination decision to its exact consideration request.</summary>
public sealed record CharacterObjectSabotageContaminationConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterObjectSabotageContaminationConsiderationResult(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        CharacterObjectSabotageContaminationDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The contamination decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedEvidenceId is null
            ? null
            : request.Candidates.Single(candidate =>
                ReferenceEquals(candidate.Evidence.EvidenceId, decision.SelectedEvidenceId));
    }

    /// <summary>Gets the exact request whose contamination was considered.</summary>
    public CharacterObjectSabotageContaminationConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted contamination decision.</summary>
    public CharacterObjectSabotageContaminationDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterObjectSabotageEvidence? SelectedCandidate { get; }
}

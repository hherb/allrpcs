namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted substitution decision to its exact consideration request.</summary>
public sealed record CharacterObjectSabotageSubstitutionConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationResult(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CharacterObjectSabotageSubstitutionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The substitution decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedCandidate = decision.SelectedEvidenceId is null
            ? null
            : request.Candidates.Single(candidate =>
                ReferenceEquals(
                    candidate.Evidence.Evidence.EvidenceId,
                    decision.SelectedEvidenceId));
    }

    /// <summary>Gets the exact request whose substitution was considered.</summary>
    public CharacterObjectSabotageSubstitutionConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted substitution decision.</summary>
    public CharacterObjectSabotageSubstitutionDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterObjectSabotageSubstitutionCandidate? SelectedCandidate { get; }
}

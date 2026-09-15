namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted misquotation-evidence decision to its exact request.</summary>
public sealed record CharacterDialogueMisquotationEvidenceConsiderationResult
{
    /// <summary>Creates one immutable result only for an exact currently accepted decision.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationResult(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request,
        CharacterDialogueMisquotationEvidenceDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The misquotation-evidence decision must be valid for the exact consideration request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
        SelectedEvidence = decision.SelectedEvidence;
    }

    /// <summary>Gets the exact request whose evidence was considered.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted misquotation-evidence decision.</summary>
    public CharacterDialogueMisquotationEvidenceDecision Decision { get; }

    /// <summary>Gets the exact selected request candidate, or null for abstention.</summary>
    public CharacterDialogueMisquotationEvidence? SelectedEvidence { get; }
}

namespace Allrpcs.Domain.Investigations;

/// <summary>Retains open questions and conclusions after one atomic confirmation.</summary>
public sealed class InvestigationConclusionConfirmationResult
{
    /// <summary>Creates an immutable confirmation result from existing snapshots.</summary>
    public InvestigationConclusionConfirmationResult(
        OpenInvestigationQuestionSnapshot openQuestions,
        InvestigationConclusionSnapshot conclusions)
    {
        ArgumentNullException.ThrowIfNull(openQuestions);
        ArgumentNullException.ThrowIfNull(conclusions);

        OpenQuestions = openQuestions;
        Conclusions = conclusions;
    }

    /// <summary>Gets the exact remaining open-question snapshot.</summary>
    public OpenInvestigationQuestionSnapshot OpenQuestions { get; }

    /// <summary>Gets the exact resulting conclusion snapshot.</summary>
    public InvestigationConclusionSnapshot Conclusions { get; }
}

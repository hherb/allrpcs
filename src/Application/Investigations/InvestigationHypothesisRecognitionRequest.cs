using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Investigations;

/// <summary>Supplies exact natural-language and authored candidate state to one interpreter.</summary>
public sealed class InvestigationHypothesisRecognitionRequest
{
    /// <summary>Creates one immutable provider-neutral recognition request.</summary>
    public InvestigationHypothesisRecognitionRequest(
        DialogueUtterance sourceUtterance,
        OpenInvestigationQuestionSnapshot openQuestions,
        InvestigationHypothesisSnapshot hypotheses)
    {
        ArgumentNullException.ThrowIfNull(sourceUtterance);
        ArgumentNullException.ThrowIfNull(openQuestions);
        ArgumentNullException.ThrowIfNull(hypotheses);

        SourceUtterance = sourceUtterance;
        OpenQuestions = openQuestions;
        Hypotheses = hypotheses;
    }

    /// <summary>Gets the exact in-world natural-language utterance.</summary>
    public DialogueUtterance SourceUtterance { get; }

    /// <summary>Gets the exact currently open authored questions.</summary>
    public OpenInvestigationQuestionSnapshot OpenQuestions { get; }

    /// <summary>Gets the exact authored possible hypotheses.</summary>
    public InvestigationHypothesisSnapshot Hypotheses { get; }
}

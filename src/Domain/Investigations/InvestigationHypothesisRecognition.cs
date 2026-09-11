using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Investigations;

/// <summary>Retains one bounded hypothesis match without promoting it to a conclusion or fact.</summary>
public sealed class InvestigationHypothesisRecognition
{
    /// <summary>Creates one immutable recognition from exact source and authored instances.</summary>
    public InvestigationHypothesisRecognition(
        DialogueUtterance sourceUtterance,
        InvestigationHypothesis hypothesis)
    {
        ArgumentNullException.ThrowIfNull(sourceUtterance);
        ArgumentNullException.ThrowIfNull(hypothesis);

        SourceUtterance = sourceUtterance;
        Hypothesis = hypothesis;
    }

    /// <summary>Gets the exact natural-language utterance that was interpreted.</summary>
    public DialogueUtterance SourceUtterance { get; }

    /// <summary>Gets the exact authored possible answer that was matched.</summary>
    public InvestigationHypothesis Hypothesis { get; }
}

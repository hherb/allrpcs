using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Investigations;

/// <summary>Canonicalizes one interpreter proposal without promoting it to authoritative state.</summary>
public static class InvestigationHypothesisRecognitionController
{
    /// <summary>Returns only an authored open-question match for the exact source utterance.</summary>
    public static async Task<InvestigationHypothesisRecognition?> TryRecognizeAsync(
        IInvestigationHypothesisInterpreter interpreter,
        InvestigationHypothesisRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(interpreter);
        ArgumentNullException.ThrowIfNull(request);

        InvestigationHypothesisProposal? proposal = await interpreter
            .InterpretAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (proposal is null)
        {
            return null;
        }

        return InvestigationHypothesisRecognitionPolicy.TryRecognize(
            request.SourceUtterance,
            proposal.QuestionId,
            proposal.HypothesisId,
            request.OpenQuestions,
            request.Hypotheses);
    }
}

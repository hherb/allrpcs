namespace Allrpcs.Application.Investigations;

/// <summary>Interprets natural language into at most one provider-neutral hypothesis proposal.</summary>
public interface IInvestigationHypothesisInterpreter
{
    /// <summary>Returns a proposed authored identity pair or no match while observing cancellation.</summary>
    ValueTask<InvestigationHypothesisProposal?> InterpretAsync(
        InvestigationHypothesisRecognitionRequest request,
        CancellationToken cancellationToken);
}

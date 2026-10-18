namespace Allrpcs.Application.Characters;

/// <summary>Obtains a bounded, validated decision for one exact caller-owned request.</summary>
public interface IStructuredDecisionClient<in TRequest, TDecision>
    where TRequest : class
    where TDecision : class
{
    /// <summary>Returns only a complete accepted decision, preserving caller cancellation.</summary>
    ValueTask<TDecision> DecideAsync(TRequest request, CancellationToken cancellationToken);
}

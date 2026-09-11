namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Transports one bounded hypothesis-recognition document without owning provider state.</summary>
public interface IInvestigationHypothesisTransport
{
    /// <summary>Returns one complete serialized response while observing cancellation.</summary>
    ValueTask<string> CompleteAsync(
        string requestDocument,
        CancellationToken cancellationToken);
}

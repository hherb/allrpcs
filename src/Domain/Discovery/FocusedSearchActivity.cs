namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Denotes that one existing focused-search request is active without defining search lifecycle
/// or execution behavior.
/// </summary>
public sealed record FocusedSearchActivity
{
    /// <summary>
    /// Creates an immutable active focused search from an existing request.
    /// </summary>
    public FocusedSearchActivity(FocusedSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Request = request;
    }

    /// <summary>
    /// Gets the request represented by this active focused search.
    /// </summary>
    public FocusedSearchRequest Request { get; }
}

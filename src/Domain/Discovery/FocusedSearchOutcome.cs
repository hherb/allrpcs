using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one focused-search result without producing it or applying it to perception.
/// </summary>
public sealed record FocusedSearchOutcome
{
    /// <summary>
    /// Creates an immutable outcome for an existing request and resulting visibility state.
    /// </summary>
    public FocusedSearchOutcome(
        FocusedSearchRequest request,
        VisibilityState resultingVisibilityState)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(resultingVisibilityState);

        Request = request;
        ResultingVisibilityState = resultingVisibilityState;
    }

    /// <summary>
    /// Gets the request represented by this outcome.
    /// </summary>
    public FocusedSearchRequest Request { get; }

    /// <summary>
    /// Gets the visibility state resulting from the focused search.
    /// </summary>
    public VisibilityState ResultingVisibilityState { get; }
}

using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Produces one explicitly temporary deterministic outcome for an existing focused-search request.
/// </summary>
public partial class TemporaryFocusedSearchOutcomeSource : Node
{
    private readonly VisibilityState resultingVisibilityState = VisibilityState.Identified;

    /// <summary>
    /// Creates a distinct outcome retaining the request and the identified visibility singleton.
    /// </summary>
    public FocusedSearchOutcome CreateOutcome(FocusedSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new FocusedSearchOutcome(request, resultingVisibilityState);
    }
}

using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Produces one explicitly temporary deterministic outcome for an existing focused-search request.
/// </summary>
public partial class TemporaryFocusedSearchOutcomeSource : Node
{
    private readonly VisibilityState resultingVisibilityState = VisibilityState.Identified;

    /// <summary>
    /// Creates a distinct identified outcome when the supplied world object matches the request.
    /// </summary>
    public FocusedSearchOutcome? CreateOutcome(
        FocusedSearchRequest request,
        WorldObject? worldObject)
    {
        ArgumentNullException.ThrowIfNull(request);

        return worldObject is null || worldObject.Identity.Id != request.TargetObjectId
            ? null
            : new FocusedSearchOutcome(request, resultingVisibilityState);
    }
}

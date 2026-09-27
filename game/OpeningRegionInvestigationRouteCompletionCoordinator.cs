using Allrpcs.Application.Investigations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicitly observed exact opening-region route-completion tokens.</summary>
public partial class OpeningRegionInvestigationRouteCompletionCoordinator : Node
{
    private OpeningRegionInvestigationRouteProgressController progress = null!;

    /// <summary>Gets accepted exact completion tokens in observation order.</summary>
    public IReadOnlyList<OpeningRegionInvestigationRouteCompletion> AcceptedCompletions
    {
        get;
        private set;
    } = Array.Empty<OpeningRegionInvestigationRouteCompletion>();

    /// <inheritdoc />
    public override void _Ready()
    {
        progress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
    }

    /// <summary>Observes one exact current route completion once.</summary>
    public bool TryObserve(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        OpeningRegionInvestigationRoute route)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(route);

        if (!ReferenceEquals(progress.Progress, expectedProgress)
            || AcceptedCompletions.Any(completion => ReferenceEquals(completion.Route, route)))
        {
            return false;
        }

        OpeningRegionInvestigationRouteCompletion? completion =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(expectedProgress, route);
        if (completion is null)
        {
            return false;
        }

        AcceptedCompletions = AcceptedCompletions
            .Append(completion)
            .ToList()
            .AsReadOnly();
        return true;
    }

    /// <summary>Installs preflighted fresh restored completion tokens atomically.</summary>
    internal void InstallRestored(
        OpeningRegionInvestigationRouteProgressSnapshot restoredProgress,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> restored)
    {
        ArgumentNullException.ThrowIfNull(restoredProgress);
        ArgumentNullException.ThrowIfNull(restored);
        _ = InvestigationRouteSaveStateCaptureProjection.Capture(restoredProgress, restored, null);
        AcceptedCompletions = restored;
    }
}

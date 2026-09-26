using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Installs explicit authority-bound route progress through exact scene state.</summary>
public partial class OpeningRegionInvestigationRouteAuthorityCoordinator : Node
{
    private SeekerMapController map = null!;
    private OpeningRegionInvestigationRouteProgressController progress = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        map = GetNode<SeekerMapController>("../SeekerMapController");
        progress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
    }

    /// <summary>Records one exact location only from the exact current verified map.</summary>
    public bool TryRecordVerifiedLocation(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        OpeningRegionInvestigationRoute route,
        SeekerMapSnapshot expectedMap,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(map.Map, expectedMap))
        {
            return false;
        }

        try
        {
            _ = OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                expectedProgress,
                route,
                expectedMap,
                location);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        return progress.TryRecordLocation(expectedProgress, route, location.Id);
    }

    /// <summary>Records one exact evidence identity from one explicit filtered character view.</summary>
    public bool TryRecordVisibleEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        OpeningRegionInvestigationRoute route,
        CharacterEvidenceViewSnapshot evidenceView,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(evidenceId);

        try
        {
            _ = OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                expectedProgress,
                route,
                evidenceView,
                evidenceId);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        return progress.TryRecordEvidence(expectedProgress, route, evidenceId);
    }
}

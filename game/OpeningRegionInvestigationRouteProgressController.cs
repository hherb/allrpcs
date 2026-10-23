using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns explicit ID-only investigation-route progress without activating any route.</summary>
public partial class OpeningRegionInvestigationRouteProgressController : Node
{
    /// <summary>Gets the exact initialized route catalogue, or null before explicit initialization.</summary>
    public OpeningRegionInvestigationRouteCatalogue? Catalogue { get; private set; }

    /// <summary>Gets exact current route progress, or null before explicit initialization.</summary>
    public OpeningRegionInvestigationRouteProgressSnapshot? Progress { get; private set; }

    /// <summary>Initializes one validated exact catalogue with empty progress once.</summary>
    public bool TryInitialize(
        OpeningRegionInvestigationRouteCatalogue catalogue,
        OpeningRegionMapLocationCatalogue locations,
        DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(scenario);

        if (Catalogue is not null
            || Progress is not null
            || !OpeningRegionInvestigationRouteValidationPolicy.IsValid(
                catalogue,
                locations,
                scenario))
        {
            return false;
        }

        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(catalogue);
        Catalogue = catalogue;
        Progress = progress;
        return true;
    }

    /// <summary>Installs one completely preflighted fresh restored route-progress graph.</summary>
    internal void InstallRestored(OpeningRegionInvestigationRouteProgressSnapshot restored)
    {
        ArgumentNullException.ThrowIfNull(restored);
        _ = InvestigationRouteSaveStateCaptureProjection.Capture(restored, [], null);
        Catalogue = restored.Catalogue;
        Progress = restored;
    }

    /// <summary>Returns to the uninitialized state; routes initialize again when the map is next opened.</summary>
    internal void ResetToUnopened()
    {
        Catalogue = null;
        Progress = null;
    }

    internal void InstallPreparedRestoration(OpeningRegionInvestigationRouteProgressSnapshot restored)
    {
        Catalogue = restored.Catalogue;
        Progress = restored;
    }

    /// <summary>Records one exact route location through compare-and-swap.</summary>
    public bool TryRecordLocation(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        OpeningRegionInvestigationRoute route,
        LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(locationId);

        if (!ReferenceEquals(Progress, expectedProgress))
        {
            return false;
        }

        try
        {
            Progress = OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                expectedProgress,
                route,
                locationId);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Records one exact route evidence identity through compare-and-swap.</summary>
    public bool TryRecordEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        OpeningRegionInvestigationRoute route,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(evidenceId);

        if (!ReferenceEquals(Progress, expectedProgress))
        {
            return false;
        }

        try
        {
            Progress = OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                expectedProgress,
                route,
                evidenceId);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

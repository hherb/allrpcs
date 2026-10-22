using Allrpcs.Application.Investigations;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the stream residue is noticed only by arriving at the stream, never at boot.</summary>
public partial class OpeningRegionStreamResiduePassiveNoticingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_STREAM_RESIDUE_ROUTE_CHECK";
    private const string MigratedSaveMode = "migrated";
    private const string ActiveRouteMode = "route";
    private const string LateRoutesMode = "late-routes";
    private const string StreamRouteKey = "stream-laundry";

    private OpeningRegionStreamResiduePassiveNoticingCoordinator coordinator = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator completions = null!;
    private string mode = string.Empty;

    /// <inheritdoc />
    public override void _Ready()
    {
        mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool enabled = mode is MigratedSaveMode or ActiveRouteMode or LateRoutesMode;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        coordinator = GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
            "../OpeningRegionStreamResiduePassiveNoticingCoordinator");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        completions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        OpeningRegionOldShrineSceneSource source = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        if (!ReferenceEquals(coordinator.Scenario, source.DiscoveryScenario)
            || coordinator.ResidueEvidence.EvidenceId.Value != "residue"
            || !ReferenceEquals(coordinator.ResidueEvidence.ObjectId, coordinator.StreamResidue.Identity.Id)
            || !ReferenceEquals(coordinator.ResidueEvidence.MinimumVisibilityState, VisibilityState.Noticed))
        {
            Fail("The stream-residue coordinator lost exact scenario provenance.");
            return;
        }

        LocationId stream = new("stream");
        if (mode != ActiveRouteMode)
        {
            ProveCatchUpOnMapOpen(stream, initializeRoutesBeforeOpen: mode == MigratedSaveMode);
            return;
        }

        MainFlowCheckSteps.InitializeMap(GetParent());
        OpeningRegionInvestigationRouteProgressSnapshot opened = routeProgress.Progress
            ?? throw new InvalidOperationException("Opening the map did not create route progress.");
        if (StreamEntry(opened).RecordedEvidenceIds.Count != 0
            || coordinator.TryNoticeOnArrival(new LocationId("old-shrine"))
            || !ReferenceEquals(routeProgress.Progress, opened))
        {
            Fail("The residue was noticed at boot or by arriving somewhere other than the stream.");
            return;
        }

        // Laundry first, so the residue noticed on the stream arrival is what completes the route.
        if (!TryVisit(new LocationId("abandoned-laundry")))
        {
            return;
        }

        if (completions.AcceptedCompletions.Count != 0)
        {
            Fail("The laundry visit completed a route before the stream residue was noticed.");
            return;
        }

        if (!TryVisit(stream))
        {
            return;
        }

        OpeningRegionInvestigationRouteProgressSnapshot noticed = routeProgress.Progress!;
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> accepted = completions.AcceptedCompletions;
        if (!HasOnlyStreamResidue(noticed, stream, "Arriving at the stream")
            || accepted.Count != 1
            || accepted[0].Route.Key != StreamRouteKey)
        {
            Fail("Arriving at the stream after the laundry did not complete exactly the stream route.");
            return;
        }

        if (coordinator.TryNoticeOnArrival(stream)
            || !ReferenceEquals(routeProgress.Progress, noticed)
            || !ReferenceEquals(completions.AcceptedCompletions, accepted))
        {
            Fail("A repeated stream arrival changed the recorded residue.");
            return;
        }

        Pass("nothing at boot or elsewhere, stream arrival records the residue and completes the route, repeat inert");
    }

    // The stream is verified while route progress lacks it: either no routes exist yet (as after a
    // failed first open) or empty routes exist (as in a save migrated to route state). The next map
    // open must catch up both the stream location and its residue.
    private void ProveCatchUpOnMapOpen(LocationId stream, bool initializeRoutesBeforeOpen)
    {
        Main main = GetParent<Main>();
        SeekerMapController map = main.GetNode<SeekerMapController>("SeekerMapController");
        OpeningRegionOldShrineSceneSource source = main.GetNode<OpeningRegionOldShrineSceneSource>(
            "OpeningRegionOldShrineSceneSource");
        if (!map.TryInitialize(
                main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource").Character.Identity.Id)
            || !TryVisit(stream)
            || routeProgress.Progress is not null
            || (initializeRoutesBeforeOpen
                && !routeProgress.TryInitialize(
                    RepresentativeOpeningRegionInvestigationRouteSource.Create(map.Catalogue!, source.DiscoveryScenario),
                    map.Catalogue!,
                    source.DiscoveryScenario)))
        {
            Fail("The catch-up scenario could not verify the stream before it was routed.");
            return;
        }

        if (initializeRoutesBeforeOpen && StreamEntry(routeProgress.Progress!).RecordedLocationIds.Count != 0)
        {
            Fail("The migrated-save scenario routed the stream before the map opened.");
            return;
        }

        MainFlowCheckSteps.InitializeMap(main);
        if (routeProgress.Progress is not { } caughtUp
            || !HasOnlyStreamResidue(caughtUp, stream, "The first map open after an unrouted stream visit"))
        {
            return;
        }

        MainFlowCheckSteps.InitializeMap(main);
        if (!ReferenceEquals(routeProgress.Progress, caughtUp))
        {
            Fail("A second map open changed already caught-up route progress.");
            return;
        }

        Pass($"{mode}: an unrouted stream visit is routed with its residue on the next map open, once");
    }

    // Walks the Seeker onto the location's anchor and lets the real visit verifier run.
    private bool TryVisit(LocationId location)
    {
        OpeningRegionAnchor anchor = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource")
            .Projection.Anchors.Single(candidate => candidate.Location.Identity.Id.Value == location.Value);
        GetNode<Node2D>("../Seeker").Position = new Vector2(anchor.Position.X, anchor.Position.Y);
        if (!GetNode<OpeningRegionMapVisitVerificationCoordinator>(
                "../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit())
        {
            Fail($"The Seeker's {location.Value} visit was not verified.");
            return false;
        }

        return true;
    }

    private bool HasOnlyStreamResidue(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        LocationId stream,
        string scenario)
    {
        OpeningRegionInvestigationRouteProgress streamEntry = StreamEntry(progress);
        if (streamEntry.RecordedEvidenceIds.Count != 1
            || !ReferenceEquals(streamEntry.RecordedEvidenceIds[0], coordinator.ResidueEvidence.EvidenceId)
            || !streamEntry.RecordedLocationIds.Any(locationId => locationId.Value == stream.Value)
            || progress.Entries.Any(entry => entry.Route.Key != StreamRouteKey && entry.RecordedEvidenceIds.Count != 0))
        {
            Fail($"{scenario} did not record exactly the stream and its residue on the stream route.");
            return false;
        }

        return true;
    }

    private static OpeningRegionInvestigationRouteProgress StreamEntry(
        OpeningRegionInvestigationRouteProgressSnapshot progress) =>
        progress.Entries.Single(entry => entry.Route.Key == StreamRouteKey);

    private void Pass(string detail)
    {
        GD.Print($"Opening-region stream-residue route smoke passed: {detail}.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

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
    private const string MissingProgressMode = "missing";
    private const string ActiveRouteMode = "route";
    private const string LateRoutesMode = "late-routes";
    private const string StreamRouteKey = "stream-laundry";

    private OpeningRegionStreamResiduePassiveNoticingCoordinator coordinator = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private bool activeRouteMode;
    private bool lateRoutesMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        string mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool enabled = mode is MissingProgressMode or ActiveRouteMode or LateRoutesMode;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        activeRouteMode = mode == ActiveRouteMode;
        lateRoutesMode = mode == LateRoutesMode;
        coordinator = GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
            "../OpeningRegionStreamResiduePassiveNoticingCoordinator");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
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
        if (lateRoutesMode)
        {
            ProveLateRoutesCatchUp(stream);
            return;
        }

        if (!activeRouteMode)
        {
            // Without routes (the map was never opened) an arrival records nothing.
            if (coordinator.TryNoticeOnArrival(stream) || routeProgress.Progress is not null)
            {
                Fail("Stream-residue noticing changed state before route progress existed.");
                return;
            }

            Pass("no route progress, so an arrival is inert");
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

        if (!TryVisitStream(stream))
        {
            return;
        }

        OpeningRegionInvestigationRouteProgressSnapshot noticed = routeProgress.Progress!;
        if (!HasOnlyStreamResidue(noticed, stream, "Arriving at the stream"))
        {
            return;
        }

        if (coordinator.TryNoticeOnArrival(stream) || !ReferenceEquals(routeProgress.Progress, noticed))
        {
            Fail("A repeated stream arrival changed the recorded residue.");
            return;
        }

        Pass("nothing at boot or elsewhere, residue recorded on stream arrival, repeat arrival inert");
    }

    // The stream is verified while the map exists without routes (as after a failed first open);
    // the next open must catch up both the stream location and its residue.
    private void ProveLateRoutesCatchUp(LocationId stream)
    {
        Main main = GetParent<Main>();
        if (!main.GetNode<SeekerMapController>("SeekerMapController").TryInitialize(
                main.GetNode<TemporarySeekerCharacterSource>("TemporarySeekerCharacterSource").Character.Identity.Id)
            || !TryVisitStream(stream)
            || routeProgress.Progress is not null)
        {
            Fail("The late-routes scenario could not verify the stream before routes existed.");
            return;
        }

        MainFlowCheckSteps.InitializeMap(main);
        if (routeProgress.Progress is not { } caughtUp
            || !HasOnlyStreamResidue(caughtUp, stream, "The first map open after a routeless stream visit"))
        {
            return;
        }

        Pass("a stream visit verified before routes existed is routed with its residue on the next map open");
    }

    // Walks the Seeker onto the stream anchor and lets the real visit verifier run.
    private bool TryVisitStream(LocationId stream)
    {
        OpeningRegionAnchor anchor = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource")
            .Projection.Anchors.Single(candidate => candidate.Location.Identity.Id.Value == stream.Value);
        GetNode<Node2D>("../Seeker").Position = new Vector2(anchor.Position.X, anchor.Position.Y);
        if (!GetNode<OpeningRegionMapVisitVerificationCoordinator>(
                "../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit())
        {
            Fail("The Seeker's stream visit was not verified.");
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

using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class InvestigationRouteSaveStateProjectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CapturesAndRestoresPartialCompleteAcceptedAndOptionallyConsumedLifecycle(
        bool consume)
    {
        (OpeningRegionMapLocationCatalogue locations, DiscoveryScenario scenario,
            OpeningRegionInvestigationRouteCatalogue catalogue) = Sources();
        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(catalogue);
        OpeningRegionInvestigationRoute first = catalogue.Routes[0];
        OpeningRegionInvestigationRoute second = catalogue.Routes[1];
        progress = Complete(progress, first);
        OpeningRegionInvestigationRouteCompletion firstCompletion =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(progress, first)!;
        progress = OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
            progress,
            second,
            second.LocationIds[0]);

        InvestigationRouteSaveState saved = InvestigationRouteSaveStateCaptureProjection.Capture(
            progress,
            [firstCompletion],
            consume ? firstCompletion : null);
        InvestigationRouteSaveStateRestoration restored =
            InvestigationRouteSaveStateRestorationProjection.Restore(saved, locations, scenario);

        Assert.Equal(["stream-laundry", "shrine", "road-wagon"],
            saved.Progress.Select(entry => entry.RouteKey));
        Assert.Equal("stream-laundry", Assert.Single(saved.AcceptedCompletionRouteKeys));
        Assert.Equal(consume ? "stream-laundry" : null, saved.ConsumedCompletionRouteKey);
        Assert.NotSame(progress, restored.Progress);
        Assert.NotSame(catalogue, restored.Progress.Catalogue);
        Assert.Equal(2, restored.Progress.Entries[0].RecordedLocationIds.Count);
        Assert.Single(restored.Progress.Entries[0].RecordedEvidenceIds);
        Assert.Single(restored.Progress.Entries[1].RecordedLocationIds);
        Assert.Empty(restored.Progress.Entries[1].RecordedEvidenceIds);
        OpeningRegionInvestigationRouteCompletion restoredCompletion =
            Assert.Single(restored.AcceptedCompletions);
        Assert.Same(restored.Progress, restoredCompletion.ProgressSnapshot);
        if (consume)
        {
            Assert.Same(restoredCompletion, restored.ConsumedCompletion);
        }
        else
        {
            Assert.Null(restored.ConsumedCompletion);
        }
    }

    [Fact]
    public void CaptureRejectsForeignCompletionAndConsumption()
    {
        (_, _, OpeningRegionInvestigationRouteCatalogue catalogue) = Sources();
        OpeningRegionInvestigationRouteProgressSnapshot complete = Complete(
            OpeningRegionInvestigationRouteProgressTransition.Initialize(catalogue),
            catalogue.Routes[0]);
        OpeningRegionInvestigationRouteCompletion accepted =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(complete, catalogue.Routes[0])!;
        (_, _, OpeningRegionInvestigationRouteCatalogue foreignCatalogue) = Sources();
        OpeningRegionInvestigationRouteProgressSnapshot foreignProgress = Complete(
            OpeningRegionInvestigationRouteProgressTransition.Initialize(foreignCatalogue),
            foreignCatalogue.Routes[0]);
        OpeningRegionInvestigationRouteCompletion foreign =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
                foreignProgress,
                foreignCatalogue.Routes[0])!;

        Assert.Throws<ArgumentException>(() =>
            InvestigationRouteSaveStateCaptureProjection.Capture(complete, [foreign], null));
        Assert.Throws<ArgumentException>(() =>
            InvestigationRouteSaveStateCaptureProjection.Capture(complete, [accepted], foreign));
    }

    [Theory]
    [InlineData("unknown", "stream", "residue", "", "")]
    [InlineData("stream-laundry", "unknown", "residue", "", "")]
    [InlineData("stream-laundry", "stream", "unknown", "", "")]
    [InlineData("stream-laundry", "stream", "residue", "shrine", "")]
    [InlineData("stream-laundry", "stream", "residue", "stream-laundry", "shrine")]
    public void RestorationRejectsUnknownIncompleteOrUnacceptedReferences(
        string routeKey,
        string locationId,
        string evidenceId,
        string acceptedKey,
        string consumedKey)
    {
        (OpeningRegionMapLocationCatalogue locations, DiscoveryScenario scenario, _) = Sources();
        InvestigationRouteSaveState state = new(
            [
                new(routeKey, [new LocationId(locationId)], [new EvidenceId(evidenceId)]),
                new("shrine", [], []),
                new("road-wagon", [], []),
            ],
            string.IsNullOrEmpty(acceptedKey) ? [] : [acceptedKey],
            string.IsNullOrEmpty(consumedKey) ? null : consumedKey);

        Assert.Throws<ArgumentException>(() =>
            InvestigationRouteSaveStateRestorationProjection.Restore(state, locations, scenario));
    }

    private static OpeningRegionInvestigationRouteProgressSnapshot Complete(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route)
    {
        foreach (LocationId locationId in route.LocationIds)
        {
            progress = OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                progress,
                route,
                locationId);
        }

        foreach (EvidenceId evidenceId in route.EvidenceIds)
        {
            progress = OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                progress,
                route,
                evidenceId);
        }

        return progress;
    }

    private static (OpeningRegionMapLocationCatalogue, DiscoveryScenario,
        OpeningRegionInvestigationRouteCatalogue) Sources()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        return (locations, scenario,
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario));
    }
}

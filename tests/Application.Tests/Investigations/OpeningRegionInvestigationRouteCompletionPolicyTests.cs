using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteCompletionPolicyTests
{
    [Fact]
    public void AuthorizesOneExactCompleteRouteWithoutPrescribingRecordOrder()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRouteProgressSnapshot withEvidence =
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                initial,
                route,
                route.EvidenceIds[0]);
        OpeningRegionInvestigationRouteProgressSnapshot withSecondLocation =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                withEvidence,
                route,
                route.LocationIds[1]);
        OpeningRegionInvestigationRouteProgressSnapshot complete =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                withSecondLocation,
                route,
                route.LocationIds[0]);

        OpeningRegionInvestigationRouteCompletion? completion =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(complete, route);

        Assert.NotNull(completion);
        Assert.Same(complete, completion.ProgressSnapshot);
        Assert.Same(complete.Entries[0], completion.RouteProgress);
        Assert.Same(route, completion.Route);
        Assert.Equal(
            [route.LocationIds[1], route.LocationIds[0]],
            completion.RouteProgress.RecordedLocationIds);
    }

    [Fact]
    public void RejectsPartialAndCrossRoutePooledProgress()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute first = context.Routes.Routes[0];
        OpeningRegionInvestigationRoute second = context.Routes.Routes[1];
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRouteProgressSnapshot firstLocation =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                first,
                first.LocationIds[0]);
        OpeningRegionInvestigationRouteProgressSnapshot differentRouteEvidence =
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                firstLocation,
                second,
                second.EvidenceIds[0]);
        OpeningRegionInvestigationRouteProgressSnapshot differentRouteComplete =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                differentRouteEvidence,
                second,
                second.LocationIds[0]);

        Assert.Null(OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
            differentRouteComplete,
            first));
        Assert.Null(OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
            differentRouteComplete,
            context.Routes.Routes[2]));
    }

    [Fact]
    public void RejectsForeignAndRebuiltRoutesWithoutChangingProgress()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[1];
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRouteProgressSnapshot withLocation =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                route,
                route.LocationIds[0]);
        OpeningRegionInvestigationRouteProgressSnapshot complete =
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                withLocation,
                route,
                route.EvidenceIds[0]);
        Context foreign = CreateContext();
        OpeningRegionInvestigationRoute rebuilt = new(
            route.Key,
            route.LocationIds,
            route.EvidenceIds);

        Assert.Null(OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
            complete,
            foreign.Routes.Routes[1]));
        Assert.Null(OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
            complete,
            rebuilt));
        Assert.Same(route, complete.Entries[1].Route);
    }

    [Fact]
    public void RejectsNullsAndExposesNoCompletionBoolean()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
                null!,
                context.Routes.Routes[0]));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(progress, null!));
        Assert.True(typeof(OpeningRegionInvestigationRouteCompletion).IsSealed);
        Assert.DoesNotContain(
            typeof(OpeningRegionInvestigationRouteCompletion).GetProperties(),
            property => property.PropertyType == typeof(bool));
        Assert.All(
            typeof(OpeningRegionInvestigationRouteCompletion).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.Empty(typeof(OpeningRegionInvestigationRouteCompletion).GetEvents());
        Assert.Empty(typeof(OpeningRegionInvestigationRouteCompletion).GetMethods().Where(
            method => method.DeclaringType
                == typeof(OpeningRegionInvestigationRouteCompletion)
                && !method.IsSpecialName));
    }

    private static Context CreateContext()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        return new Context(routes);
    }

    private sealed record Context(OpeningRegionInvestigationRouteCatalogue Routes);
}

using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteCompletionPresentationProjectionTests
{
    [Theory]
    [InlineData("stream-laundry", "Stream and abandoned laundry route complete.")]
    [InlineData("shrine", "Old shrine route complete.")]
    [InlineData("road-wagon", "Southern road and abandoned wagon route complete.")]
    public void ProjectReturnsOnlyClosedAuthoredCopy(string routeKey, string expectedContent)
    {
        OpeningRegionInvestigationRouteCompletion completion = Complete(routeKey);

        OpeningRegionInvestigationRouteCompletionPresentationSnapshot result =
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(completion);

        Assert.Equal(
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Title,
            result.Title);
        Assert.Equal(expectedContent, result.Content);
        Assert.All(
            typeof(OpeningRegionInvestigationRouteCompletionPresentationSnapshot).GetProperties(),
            property =>
            {
                Assert.Equal(typeof(string), property.PropertyType);
                Assert.False(property.CanWrite);
            });
    }

    [Fact]
    public void ProjectRejectsNullAndUnknownRoutes()
    {
        OpeningRegionInvestigationRouteCompletion known = Complete("stream-laundry");
        OpeningRegionInvestigationRoute unknown = new(
            "unknown-route",
            known.Route.LocationIds,
            known.Route.EvidenceIds);
        OpeningRegionInvestigationRouteCompletion completion =
            (OpeningRegionInvestigationRouteCompletion)Activator.CreateInstance(
                typeof(OpeningRegionInvestigationRouteCompletion),
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                [known.ProgressSnapshot, known.RouteProgress, unknown],
                culture: null)!;

        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(completion));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(null!));
    }

    [Fact]
    public void SurfaceIsClosedAndSnapshotsAreFresh()
    {
        OpeningRegionInvestigationRouteCompletion completion = Complete("shrine");

        OpeningRegionInvestigationRouteCompletionPresentationSnapshot first =
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(completion);
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot second =
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(completion);
        System.Reflection.MethodInfo method = Assert.Single(
            typeof(OpeningRegionInvestigationRouteCompletionPresentationProjection).GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.NotSame(first, second);
        Assert.Equal(
            nameof(OpeningRegionInvestigationRouteCompletionPresentationProjection.Project),
            method.Name);
        Assert.Equal(
            typeof(OpeningRegionInvestigationRouteCompletionPresentationSnapshot),
            method.ReturnType);
        Assert.True(typeof(OpeningRegionInvestigationRouteCompletionPresentationProjection).IsSealed);
        Assert.True(typeof(OpeningRegionInvestigationRouteCompletionPresentationProjection).IsAbstract);
        Assert.True(typeof(OpeningRegionInvestigationRouteCompletionPresentationSnapshot).IsSealed);
    }

    private static OpeningRegionInvestigationRouteCompletion Complete(string routeKey)
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue catalogue =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute route = catalogue.Routes.Single(candidate =>
            string.Equals(candidate.Key, routeKey, StringComparison.Ordinal));
        OpeningRegionInvestigationRouteProgressSnapshot snapshot = CompleteProgress(
            catalogue,
            route);
        return OpeningRegionInvestigationRouteCompletionPolicy.Authorize(snapshot, route)!;
    }

    private static OpeningRegionInvestigationRouteProgressSnapshot CompleteProgress(
        OpeningRegionInvestigationRouteCatalogue catalogue,
        OpeningRegionInvestigationRoute route)
    {
        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(catalogue);
        foreach (Allrpcs.Domain.Identifiers.LocationId locationId in route.LocationIds)
        {
            progress = OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                progress,
                route,
                locationId);
        }

        foreach (Allrpcs.Domain.Identifiers.EvidenceId evidenceId in route.EvidenceIds)
        {
            progress = OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                progress,
                route,
                evidenceId);
        }

        return progress;
    }
}

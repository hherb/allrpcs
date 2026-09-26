using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteProgressTransitionTests
{
    [Fact]
    public void InitializesOneEmptyExactEntryPerRoute()
    {
        Context context = CreateContext();

        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        Assert.Same(context.Routes, progress.Catalogue);
        Assert.Equal(context.Routes.Routes.Count, progress.Entries.Count);
        for (int index = 0; index < progress.Entries.Count; index++)
        {
            Assert.Same(context.Routes.Routes[index], progress.Entries[index].Route);
            Assert.Empty(progress.Entries[index].RecordedLocationIds);
            Assert.Empty(progress.Entries[index].RecordedEvidenceIds);
        }
    }

    [Fact]
    public void RecordsExactLocationAndEvidenceWithoutInferringCompletion()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        OpeningRegionInvestigationRouteProgressSnapshot withLocation =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                route,
                route.LocationIds[1]);
        OpeningRegionInvestigationRouteProgressSnapshot withEvidence =
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                withLocation,
                route,
                route.EvidenceIds[0]);

        Assert.Same(context.Routes, withEvidence.Catalogue);
        Assert.Same(route, withEvidence.Entries[0].Route);
        Assert.Equal([route.LocationIds[1]], withEvidence.Entries[0].RecordedLocationIds);
        Assert.Equal([route.EvidenceIds[0]], withEvidence.Entries[0].RecordedEvidenceIds);
        Assert.Same(initial.Entries[1], withLocation.Entries[1]);
        Assert.Same(withLocation.Entries[1], withEvidence.Entries[1]);
        Assert.DoesNotContain(
            typeof(OpeningRegionInvestigationRouteProgress).GetProperties(),
            property => property.Name.Contains("Complete", StringComparison.Ordinal));
    }

    [Fact]
    public void RejectsStaleForeignRebuiltRepeatedAndNonmemberInputs()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        LocationId location = route.LocationIds[0];
        EvidenceId evidence = route.EvidenceIds[0];
        OpeningRegionInvestigationRouteProgressSnapshot changed =
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                route,
                location);
        Context foreign = CreateContext();

        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                changed,
                route,
                location));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                foreign.Routes.Routes[0],
                location));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial,
                route,
                new LocationId(location.Value)));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                initial,
                route,
                new EvidenceId(evidence.Value)));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                initial,
                route,
                foreign.Routes.Routes[0].EvidenceIds[0]));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableProgressState()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.Initialize(null!));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                null!, route, route.LocationIds[0]));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial, null!, route.LocationIds[0]));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                initial, route, null!));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                null!, route, route.EvidenceIds[0]));

        AssertImmutable(typeof(OpeningRegionInvestigationRouteProgress));
        AssertImmutable(typeof(OpeningRegionInvestigationRouteProgressSnapshot));
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

    private static void AssertImmutable(Type type)
    {
        Assert.True(type.IsSealed);
        Assert.All(type.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(type.GetEvents());
        Assert.Empty(type.GetMethods().Where(
            method => method.DeclaringType == type && !method.IsSpecialName));
    }

    private sealed record Context(OpeningRegionInvestigationRouteCatalogue Routes);
}

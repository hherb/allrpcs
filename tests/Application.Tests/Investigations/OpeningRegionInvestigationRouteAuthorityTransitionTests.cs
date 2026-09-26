using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class OpeningRegionInvestigationRouteAuthorityTransitionTests
{
    [Fact]
    public void RecordsOnlyAnExactVerifiedRouteLocation()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        MapLocationDefinition second = Location(context.Locations, route.LocationIds[1]);
        MapLocationDefinition first = Location(context.Locations, route.LocationIds[0]);
        SeekerMapSnapshot map = Map(context.Locations, first, second);
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        OpeningRegionInvestigationRouteProgressSnapshot withSecond =
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                initial,
                route,
                map,
                second);
        OpeningRegionInvestigationRouteProgressSnapshot withBoth =
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                withSecond,
                route,
                map,
                first);

        Assert.Same(context.Routes, withBoth.Catalogue);
        Assert.Equal([second.Id, first.Id], withBoth.Entries[0].RecordedLocationIds);
        Assert.Same(withSecond.Entries[1], withBoth.Entries[1]);
        Assert.Equal([first, second], map.VerifiedLocations);
    }

    [Fact]
    public void RejectsUnverifiedRebuiltForeignNonmemberRepeatedAndMismatchedLocations()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        MapLocationDefinition member = Location(context.Locations, route.LocationIds[0]);
        MapLocationDefinition nonmember = context.Locations.Locations[0];
        SeekerMapSnapshot verified = Map(context.Locations, member, nonmember);
        SeekerMapSnapshot unverified = Map(context.Locations);
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRouteProgressSnapshot changed =
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                initial,
                route,
                verified,
                member);
        Context foreign = CreateContext();
        MapLocationDefinition foreignMember = Location(
            foreign.Locations,
            foreign.Routes.Routes[0].LocationIds[0]);

        AssertRejected(initial, route, unverified, member);
        AssertRejected(initial, route, verified, new MapLocationDefinition(
            new LocationId(member.Id.Value), member.DisplayLabel));
        AssertRejected(initial, route, verified, foreignMember);
        AssertRejected(initial, route, verified, nonmember);
        AssertRejected(changed, route, verified, member);
        AssertRejected(initial, foreign.Routes.Routes[0], verified, member);
        AssertRejected(initial, route, Map(foreign.Locations, foreignMember), foreignMember);
        Assert.Empty(initial.Entries[0].RecordedLocationIds);
        Assert.Equal([nonmember, member], verified.VerifiedLocations);
    }

    [Fact]
    public void RecordsOnlyAnExactVisibleRouteEvidenceIdentity()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        EvidenceId evidenceId = route.EvidenceIds[0];
        CharacterEvidenceViewSnapshot view = new(context.SeekerId, [evidenceId]);
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        OpeningRegionInvestigationRouteProgressSnapshot changed =
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                initial,
                route,
                view,
                evidenceId);

        Assert.Same(context.Routes, changed.Catalogue);
        Assert.Equal([evidenceId], changed.Entries[0].RecordedEvidenceIds);
        Assert.Same(initial.Entries[1], changed.Entries[1]);
        Assert.Equal([evidenceId], view.EvidenceIds);
    }

    [Fact]
    public void RejectsUnseenRebuiltForeignNonmemberRepeatedAndMismatchedEvidence()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        EvidenceId evidenceId = route.EvidenceIds[0];
        EvidenceId nonmember = context.Routes.Routes[1].EvidenceIds[0];
        CharacterEvidenceViewSnapshot visible = new(context.SeekerId, [evidenceId, nonmember]);
        CharacterEvidenceViewSnapshot unseen = new(context.SeekerId, []);
        OpeningRegionInvestigationRouteProgressSnapshot initial =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);
        OpeningRegionInvestigationRouteProgressSnapshot changed =
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                initial,
                route,
                visible,
                evidenceId);
        Context foreign = CreateContext();
        EvidenceId foreignEvidence = foreign.Routes.Routes[0].EvidenceIds[0];

        AssertRejected(initial, route, unseen, evidenceId);
        AssertRejected(initial, route, visible, new EvidenceId(evidenceId.Value));
        AssertRejected(initial, route, visible, foreignEvidence);
        AssertRejected(initial, route, visible, nonmember);
        AssertRejected(changed, route, visible, evidenceId);
        AssertRejected(initial, foreign.Routes.Routes[0], visible, evidenceId);
        AssertRejected(
            initial,
            route,
            new CharacterEvidenceViewSnapshot(context.SeekerId, [foreignEvidence]),
            foreignEvidence);
        Assert.Empty(initial.Entries[0].RecordedEvidenceIds);
        Assert.Equal([evidenceId, nonmember], visible.EvidenceIds);
    }

    [Fact]
    public void RejectsNullAuthorityInputs()
    {
        Context context = CreateContext();
        OpeningRegionInvestigationRoute route = context.Routes.Routes[0];
        MapLocationDefinition location = Location(context.Locations, route.LocationIds[0]);
        SeekerMapSnapshot map = Map(context.Locations, location);
        EvidenceId evidenceId = route.EvidenceIds[0];
        CharacterEvidenceViewSnapshot view = new(context.SeekerId, [evidenceId]);
        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(context.Routes);

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                null!, route, map, location));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                progress, null!, map, location));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                progress, route, null!, location));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                progress, route, map, null!));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                null!, route, view, evidenceId));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                progress, null!, view, evidenceId));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                progress, route, null!, evidenceId));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                progress, route, view, null!));
    }

    private static void AssertRejected(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        SeekerMapSnapshot map,
        MapLocationDefinition location) => Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                progress,
                route,
                map,
                location));

    private static void AssertRejected(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        CharacterEvidenceViewSnapshot view,
        EvidenceId evidenceId) => Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                progress,
                route,
                view,
                evidenceId));

    private static SeekerMapSnapshot Map(
        OpeningRegionMapLocationCatalogue locations,
        params MapLocationDefinition[] verified) => SeekerMapSnapshotProjection.Project(
            locations,
            new CharacterId("seeker"),
            [],
            verified);

    private static MapLocationDefinition Location(
        OpeningRegionMapLocationCatalogue locations,
        LocationId locationId) => locations.Locations.Single(location =>
            ReferenceEquals(location.Id, locationId));

    private static Context CreateContext()
    {
        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        return new Context(locations, routes, new CharacterId("seeker"));
    }

    private sealed record Context(
        OpeningRegionMapLocationCatalogue Locations,
        OpeningRegionInvestigationRouteCatalogue Routes,
        CharacterId SeekerId);
}

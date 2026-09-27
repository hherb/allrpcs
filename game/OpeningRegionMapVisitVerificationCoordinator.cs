using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Coordinates one explicit exact live opening-region map visit verification.</summary>
public partial class OpeningRegionMapVisitVerificationCoordinator : Node
{
    private const float VisitRadiusPixels = 24.0f;

    private SeekerMapController map = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteObservationRoutingCoordinator routing = null!;
    private OpeningRegionProjectionSource projectionSource = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    private Seeker seeker = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        map = GetNode<SeekerMapController>("../SeekerMapController");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routing = GetNode<OpeningRegionInvestigationRouteObservationRoutingCoordinator>(
            "../OpeningRegionInvestigationRouteObservationRoutingCoordinator");
        projectionSource = GetNode<OpeningRegionProjectionSource>(
            "../OpeningRegionProjectionSource");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        seeker = GetNode<Seeker>("../Seeker");
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        _ = TryVerifyCurrentVisit();
    }

    /// <summary>Verifies the first not-yet-verified anchor containing the live Seeker.</summary>
    internal bool TryVerifyCurrentVisit()
    {
        if (map.Map is not { } currentMap
            || map.Catalogue is not { } catalogue)
        {
            return false;
        }

        NumericsVector2 seekerPosition = ToNumerics(seeker.Position);
        foreach (OpeningRegionAnchor anchor in projectionSource.Projection.Anchors)
        {
            MapLocationDefinition? location = catalogue.Locations.SingleOrDefault(candidate =>
                string.Equals(
                    candidate.Id.Value,
                    anchor.Location.Identity.Id.Value,
                    StringComparison.Ordinal));
            if (location is null
                || currentMap.VerifiedLocations.Any(candidate => ReferenceEquals(candidate, location))
                || NumericsVector2.Distance(seekerPosition, anchor.Position) > VisitRadiusPixels)
            {
                continue;
            }

            return TryVerifyVisit(
                currentMap,
                seekerCharacter.Character.Identity.Id,
                seekerPosition,
                projectionSource.Projection,
                anchor,
                location);
        }

        return false;
    }

    /// <summary>Verifies one explicitly supplied exact current anchor visit.</summary>
    public bool TryVerifyVisit(
        SeekerMapSnapshot expectedMap,
        CharacterId seekerId,
        NumericsVector2 seekerPosition,
        OpeningRegionProjection projection,
        OpeningRegionAnchor anchor,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(map.Map, expectedMap)
            || !ReferenceEquals(seekerCharacter.Character.Identity.Id, seekerId)
            || !ReferenceEquals(projectionSource.Projection, projection)
            || ToNumerics(seeker.Position) != seekerPosition)
        {
            return false;
        }

        OpeningRegionLocationVisit? visit;
        try
        {
            visit = OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                seekerPosition,
                VisitRadiusPixels);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (visit is null || !map.TryVerifyVisit(expectedMap, projection, visit, location))
        {
            return false;
        }

        if (routeProgress.Progress is { } expectedRouteProgress
            && map.Map is { } verifiedMap)
        {
            _ = routing.TryRouteVerifiedLocation(
                expectedRouteProgress,
                verifiedMap,
                location);
        }

        return true;
    }

    private static NumericsVector2 ToNumerics(Godot.Vector2 position) =>
        new(position.X, position.Y);
}

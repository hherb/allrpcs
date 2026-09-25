using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionProjectionTests
{
    private const float MinimumPathClearancePixels = 24.0f;
    private const float MinimumLandmarkClearancePixels = 120.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly Vector2[] Positions =
    [
        new(920.0f, 820.0f),
        new(880.0f, 620.0f),
        new(880.0f, 400.0f),
        new(650.0f, 400.0f),
        new(808.0f, 260.0f),
        new(700.0f, 60.0f),
        new(470.0f, 70.0f),
        new(160.0f, 70.0f),
        new(620.0f, 620.0f),
        new(720.0f, 820.0f),
        new(120.0f, 620.0f),
    ];

    [Fact]
    public void CreatesStableProjectionWithExactTopologyProvenance()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = Anchors(topology);

        OpeningRegionProjection projection = new(
            topology,
            WorldBounds,
            MinimumPathClearancePixels,
            MinimumLandmarkClearancePixels,
            anchors.Reverse());

        Assert.Same(topology, projection.Topology);
        Assert.Equal(WorldBounds, projection.WorldBounds);
        Assert.Equal(MinimumPathClearancePixels, projection.MinimumPathClearancePixels);
        Assert.Equal(MinimumLandmarkClearancePixels, projection.MinimumLandmarkClearancePixels);
        Assert.Equal(topology.Locations.Count, projection.Anchors.Count);
        for (int index = 0; index < topology.Locations.Count; index++)
        {
            Assert.Same(topology.Locations[index], projection.Anchors[index].Location);
            Assert.Same(anchors[index], projection.Anchors[index]);
        }

        Assert.Equal(topology.Connections.Count, projection.Connections.Count);
        for (int index = 0; index < topology.Connections.Count; index++)
        {
            OpeningRegionProjectedConnection projected = projection.Connections[index];
            OpeningRegionConnection authored = topology.Connections[index];
            Assert.Same(authored, projected.TopologyConnection);
            Assert.Same(authored.First, projected.FirstAnchor.Location);
            Assert.Same(authored.Second, projected.SecondAnchor.Location);
        }
    }

    [Fact]
    public void RejectsMissingDuplicateForeignAndRebuiltAnchors()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = Anchors(topology);
        Location foreign = Location("foreign");
        Location rebuilt = Location(topology.Locations[^1].Identity.Id.Value);

        Assert.Throws<ArgumentException>(() => Projection(topology, anchors[..^1]));
        Assert.Throws<ArgumentException>(() => Projection(
            topology,
            anchors[..^1].Append(new OpeningRegionAnchor(topology.Locations[0], Positions[^1]))));
        Assert.Throws<ArgumentException>(() => Projection(
            topology,
            anchors[..^1].Append(new OpeningRegionAnchor(foreign, Positions[^1]))));
        Assert.Throws<ArgumentException>(() => Projection(
            topology,
            anchors[..^1].Append(new OpeningRegionAnchor(rebuilt, Positions[^1]))));
    }

    [Fact]
    public void RejectsBoundsAndClearanceViolations()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = Anchors(topology);

        Assert.Throws<ArgumentOutOfRangeException>(() => Projection(
            topology,
            ReplacePosition(anchors, 0, new Vector2(-1.0f, 820.0f))));
        Assert.Throws<ArgumentOutOfRangeException>(() => Projection(
            topology,
            ReplacePosition(anchors, 0, new Vector2(12.0f, 820.0f))));
        Assert.Throws<ArgumentException>(() => Projection(
            topology,
            ReplacePosition(anchors, 1, anchors[0].Position + new Vector2(60.0f, 0.0f))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionProjection(
            topology,
            WorldBounds,
            0.0f,
            MinimumLandmarkClearancePixels,
            anchors));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionProjection(
            topology,
            WorldBounds,
            MinimumPathClearancePixels,
            float.PositiveInfinity,
            anchors));
    }

    [Fact]
    public void AnchorRejectsNullAndNonFiniteInputs()
    {
        Location location = Location("valid");

        Assert.Throws<ArgumentNullException>(() => new OpeningRegionAnchor(null!, Vector2.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpeningRegionAnchor(location, new Vector2(float.NaN, 0.0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpeningRegionAnchor(location, new Vector2(0.0f, float.PositiveInfinity)));
    }

    private static OpeningRegionProjection Projection(
        OpeningRegionSnapshot topology,
        IEnumerable<OpeningRegionAnchor> anchors) => new(
            topology,
            WorldBounds,
            MinimumPathClearancePixels,
            MinimumLandmarkClearancePixels,
            anchors);

    private static OpeningRegionAnchor[] Anchors(OpeningRegionSnapshot topology) =>
        topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();

    private static OpeningRegionAnchor[] ReplacePosition(
        OpeningRegionAnchor[] anchors,
        int index,
        Vector2 position) => anchors
            .Select((anchor, candidateIndex) => candidateIndex == index
                ? new OpeningRegionAnchor(anchor.Location, position)
                : anchor)
            .ToArray();

    private static Location Location(string id) => new(
        new LocationIdentity(new LocationId(id), id),
        new LocationDescription($"The authored {id} location."));
}

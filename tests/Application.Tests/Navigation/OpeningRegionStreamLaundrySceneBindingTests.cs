using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionStreamLaundrySceneBindingTests
{
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds StreamBounds =
        new(752.0f, 80.0f, 864.0f, 286.0f);
    private static readonly NavigationWorldBounds LaundryBounds =
        new(620.0f, 24.0f, 748.0f, 104.0f);
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
    public void BindsExactStreamAndLaundryAnchorsToDistinctBoundedAreas()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor stream = projection.Anchors[4];
        OpeningRegionAnchor laundry = projection.Anchors[5];

        OpeningRegionStreamLaundrySceneBinding binding = new(
            projection,
            stream,
            StreamBounds,
            laundry,
            LaundryBounds);

        Assert.Same(projection, binding.Projection);
        Assert.Same(stream, binding.StreamAnchor);
        Assert.Equal(StreamBounds, binding.StreamBounds);
        Assert.Same(laundry, binding.AbandonedLaundryAnchor);
        Assert.Equal(LaundryBounds, binding.AbandonedLaundryBounds);
    }

    [Fact]
    public void RejectsMissingRebuiltForeignAndSwappedAnchors()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor stream = projection.Anchors[4];
        OpeningRegionAnchor laundry = projection.Anchors[5];
        OpeningRegionAnchor rebuiltStream = new(stream.Location, stream.Position);
        OpeningRegionProjection foreignProjection = Projection();

        Assert.Throws<ArgumentNullException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            null!,
            StreamBounds,
            laundry,
            LaundryBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            rebuiltStream,
            StreamBounds,
            laundry,
            LaundryBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            stream,
            StreamBounds,
            foreignProjection.Anchors[5],
            LaundryBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            laundry,
            StreamBounds,
            stream,
            LaundryBounds));
    }

    [Fact]
    public void RejectsOverlappingOutOfBoundsAndInsufficientClearanceAreas()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor stream = projection.Anchors[4];
        OpeningRegionAnchor laundry = projection.Anchors[5];

        Assert.Throws<ArgumentException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            stream,
            StreamBounds,
            laundry,
            new NavigationWorldBounds(700.0f, 24.0f, 800.0f, 104.0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            stream,
            new NavigationWorldBounds(752.0f, 80.0f, 1601.0f, 286.0f),
            laundry,
            LaundryBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionStreamLaundrySceneBinding(
            projection,
            stream,
            new NavigationWorldBounds(790.0f, 80.0f, 864.0f, 286.0f),
            laundry,
            LaundryBounds));
    }

    private static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        return new OpeningRegionProjection(topology, WorldBounds, 24.0f, 120.0f, anchors);
    }
}

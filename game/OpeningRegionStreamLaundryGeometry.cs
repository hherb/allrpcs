using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored stream/laundry geometry shared by scene and route owners.</summary>
internal static class OpeningRegionStreamLaundryGeometry
{
    internal const float OpeningWidthPixels = 96.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float StreamLeft = 1736.0f;
    internal const float StreamTop = 452.0f;
    internal const float StreamRight = 1864.0f;
    internal const float StreamBottom = 548.0f;
    internal const float LaundryLeft = 1420.0f;
    internal const float LaundryTop = 120.0f;
    internal const float LaundryRight = 1580.0f;
    internal const float LaundryBottom = 360.0f;
    internal const float LaundryOpeningY = 220.0f;

    internal static NavigationWorldBounds StreamBounds => new(
        StreamLeft,
        StreamTop,
        StreamRight,
        StreamBottom);

    internal static NavigationWorldBounds LaundryBounds => new(
        LaundryLeft,
        LaundryTop,
        LaundryRight,
        LaundryBottom);

    internal static NumericsVector2 VillageEdgeApproach => new(1700.0f, 900.0f);

    internal static NumericsVector2 BridgeNorthApproach => new(1748.5714f, StreamTop);

    internal static IReadOnlyList<NumericsVector2> StreamWater { get; } =
    [
        new(1600.0f, StreamTop),
        new(2000.0f, StreamTop),
        new(2000.0f, StreamBottom),
        new(1600.0f, StreamBottom),
    ];

    internal static IReadOnlyList<NumericsVector2> WestStreamBank { get; } =
    [
        new(1600.0f, StreamTop),
        new(1700.0f, StreamTop),
        new(1700.0f, StreamBottom),
        new(1600.0f, StreamBottom),
    ];

    internal static IReadOnlyList<NumericsVector2> EastStreamBank { get; } =
    [
        new(1900.0f, StreamTop),
        new(2000.0f, StreamTop),
        new(2000.0f, StreamBottom),
        new(1900.0f, StreamBottom),
    ];
}

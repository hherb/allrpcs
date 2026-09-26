using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored stream/laundry geometry shared by scene and route owners.</summary>
internal static class OpeningRegionStreamLaundryGeometry
{
    internal const float OpeningWidthPixels = 64.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float StreamLeft = 752.0f;
    internal const float StreamTop = 80.0f;
    internal const float StreamRight = 864.0f;
    internal const float StreamBottom = 286.0f;
    internal const float LaundryLeft = 620.0f;
    internal const float LaundryTop = 24.0f;
    internal const float LaundryRight = 748.0f;
    internal const float LaundryBottom = 104.0f;
    internal const float LaundryOpeningY = 64.0f;

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

    internal static NumericsVector2 VillageEdgeApproach => new(650.0f, 400.0f);

    internal static NumericsVector2 BridgeNorthApproach => new(808.0f, LaundryOpeningY);

    internal static IReadOnlyList<NumericsVector2> WestStreamBank { get; } =
    [
        new(0.0f, 148.0f),
        new(320.0f, 180.0f),
        new(624.0f, 100.0f),
        new(StreamLeft, 117.3f),
        new(StreamLeft, 189.3f),
        new(624.0f, 172.0f),
        new(320.0f, 252.0f),
        new(0.0f, 220.0f),
    ];

    internal static IReadOnlyList<NumericsVector2> EastStreamBank { get; } =
    [
        new(StreamRight, 132.4f),
        new(920.0f, 140.0f),
        new(1248.0f, 76.0f),
        new(1600.0f, 108.0f),
        new(1600.0f, 180.0f),
        new(1248.0f, 148.0f),
        new(920.0f, 212.0f),
        new(StreamRight, 204.4f),
    ];
}

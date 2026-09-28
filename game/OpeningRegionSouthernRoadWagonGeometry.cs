using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored southern-road and inert wagon-shell geometry.</summary>
internal static class OpeningRegionSouthernRoadWagonGeometry
{
    internal const float RoadHalfWidthPixels = 48.0f;
    internal const float BoundaryThicknessPixels = 8.0f;
    internal const float WagonOpeningWidthPixels = 96.0f;
    internal const float WagonLeft = 1720.0f;
    internal const float WagonTop = 1580.0f;
    internal const float WagonRight = 2040.0f;
    internal const float WagonBottom = 1768.0f;
    internal const float WagonOpeningX = 1800.0f;

    internal static NavigationWorldBounds WagonBounds => new(
        WagonLeft,
        WagonTop,
        WagonRight,
        WagonBottom);

    internal static NumericsVector2 WagonOpening => new(WagonOpeningX, WagonTop);
}

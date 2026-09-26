using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored southern-road and inert wagon-shell geometry.</summary>
internal static class OpeningRegionSouthernRoadWagonGeometry
{
    internal const float RoadHalfWidthPixels = 24.0f;
    internal const float BoundaryThicknessPixels = 8.0f;
    internal const float WagonOpeningWidthPixels = 48.0f;
    internal const float WagonLeft = 648.0f;
    internal const float WagonTop = 744.0f;
    internal const float WagonRight = 776.0f;
    internal const float WagonBottom = 876.0f;
    internal const float WagonOpeningX = 690.0f;

    internal static NavigationWorldBounds WagonBounds => new(
        WagonLeft,
        WagonTop,
        WagonRight,
        WagonBottom);

    internal static NumericsVector2 WagonOpening => new(WagonOpeningX, WagonTop);
}

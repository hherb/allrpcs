using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored old-shrine geometry shared by scene and route owners.</summary>
internal static class OpeningRegionOldShrineGeometry
{
    internal const float OpeningWidthPixels = 96.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float ShrineLeft = 240.0f;
    internal const float ShrineTop = 180.0f;
    internal const float ShrineRight = 620.0f;
    internal const float ShrineBottom = 460.0f;
    internal const float SouthernRoadOpeningX = 542.9091f;

    internal static NavigationWorldBounds ShrineBounds => new(
        ShrineLeft,
        ShrineTop,
        ShrineRight,
        ShrineBottom);

    internal static NumericsVector2 SouthernRoadOpening => new(
        SouthernRoadOpeningX,
        ShrineBottom);

    internal static NumericsVector2 LayeredInscriptionPosition => new(380.0f, 300.0f);
}

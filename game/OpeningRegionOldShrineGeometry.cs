using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored old-shrine geometry shared by scene and route owners.</summary>
internal static class OpeningRegionOldShrineGeometry
{
    internal const float OpeningWidthPixels = 48.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float ShrineLeft = 24.0f;
    internal const float ShrineTop = 24.0f;
    internal const float ShrineRight = 216.0f;
    internal const float ShrineBottom = 136.0f;
    internal const float SouthernRoadOpeningX = 192.0f;

    internal static NavigationWorldBounds ShrineBounds => new(
        ShrineLeft,
        ShrineTop,
        ShrineRight,
        ShrineBottom);

    internal static NumericsVector2 SouthernRoadOpening => new(
        SouthernRoadOpeningX,
        ShrineBottom);

    internal static NumericsVector2 LayeredInscriptionPosition => new(110.0f, 90.0f);
}

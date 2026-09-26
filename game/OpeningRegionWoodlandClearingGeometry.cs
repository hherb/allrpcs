using Allrpcs.Application.Navigation;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Names the authored woodland-clearing geometry shared by scene and route owners.</summary>
internal static class OpeningRegionWoodlandClearingGeometry
{
    internal const float OpeningWidthPixels = 48.0f;
    internal const float WallThicknessPixels = 8.0f;
    internal const float ClearingLeft = 240.0f;
    internal const float ClearingTop = 24.0f;
    internal const float ClearingRight = 536.0f;
    internal const float ClearingBottom = 120.0f;
    internal const float VillageEdgeOpeningX = 497.0f;
    internal const float ClosedForestPathOpeningX = 438.0f;

    internal static NavigationWorldBounds ClearingBounds => new(
        ClearingLeft,
        ClearingTop,
        ClearingRight,
        ClearingBottom);

    internal static NumericsVector2 VillageEdgeOpening => new(
        VillageEdgeOpeningX,
        ClearingBottom);

    internal static NumericsVector2 ClosedForestPathOpening => new(
        ClosedForestPathOpeningX,
        ClearingBottom);

    internal static NumericsVector2 RareSpecimenPosition => new(450.0f, 90.0f);
}

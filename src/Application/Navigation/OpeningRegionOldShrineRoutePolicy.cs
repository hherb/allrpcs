using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts bounded routes only through the shrine's authored openings.</summary>
public static class OpeningRegionOldShrineRoutePolicy
{
    /// <summary>Checks one route against the collision-backed shrine shell.</summary>
    public static bool IsAcceptable(
        OpeningRegionOldShrineLayout layout,
        IReadOnlyList<Vector2> route)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count < 2
            || route.Any(point => !Inside(layout.SceneBinding.Projection.WorldBounds, point)))
        {
            return false;
        }

        NavigationWorldBounds bounds = layout.SceneBinding.OldShrineBounds;
        float half = layout.OpeningWidthPixels / 2.0f;
        for (int index = 0; index < route.Count - 1; index++)
        {
            Vector2 start = route[index];
            Vector2 end = route[index + 1];
            if (Horizontal(start, end, bounds.Top, bounds.Left, bounds.Right)
                || Vertical(start, end, bounds.Left, bounds.Top, bounds.Bottom)
                || Vertical(
                    start,
                    end,
                    bounds.Right,
                    bounds.Top,
                    layout.WoodlandClearingOpening.Y - half)
                || Vertical(
                    start,
                    end,
                    bounds.Right,
                    layout.WoodlandClearingOpening.Y + half,
                    bounds.Bottom - half)
                || Horizontal(
                    start,
                    end,
                    bounds.Bottom,
                    bounds.Left,
                    layout.SouthernRoadOpening.X - half))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Inside(NavigationWorldBounds bounds, Vector2 point) =>
        float.IsFinite(point.X)
        && float.IsFinite(point.Y)
        && point.X >= bounds.Left
        && point.X <= bounds.Right
        && point.Y >= bounds.Top
        && point.Y <= bounds.Bottom;

    private static bool Horizontal(
        Vector2 start,
        Vector2 end,
        float wallY,
        float wallLeft,
        float wallRight)
    {
        if (start.Y == end.Y)
        {
            return start.Y == wallY
                && MathF.Max(MathF.Min(start.X, end.X), wallLeft)
                    <= MathF.Min(MathF.Max(start.X, end.X), wallRight);
        }

        float progress = (wallY - start.Y) / (end.Y - start.Y);
        float crossing = start.X + ((end.X - start.X) * progress);
        return progress >= 0.0f && progress <= 1.0f
            && crossing >= wallLeft && crossing <= wallRight;
    }

    private static bool Vertical(
        Vector2 start,
        Vector2 end,
        float wallX,
        float wallTop,
        float wallBottom)
    {
        if (start.X == end.X)
        {
            return start.X == wallX
                && MathF.Max(MathF.Min(start.Y, end.Y), wallTop)
                    <= MathF.Min(MathF.Max(start.Y, end.Y), wallBottom);
        }

        float progress = (wallX - start.X) / (end.X - start.X);
        float crossing = start.Y + ((end.Y - start.Y) * progress);
        return progress >= 0.0f && progress <= 1.0f
            && crossing >= wallTop && crossing <= wallBottom;
    }
}

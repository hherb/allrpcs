using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts bounded routes only through the clearing's authored openings.</summary>
public static class OpeningRegionWoodlandClearingRoutePolicy
{
    /// <summary>Checks one route against the collision-backed clearing shell.</summary>
    public static bool IsAcceptable(
        OpeningRegionWoodlandClearingLayout layout,
        IReadOnlyList<Vector2> route)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count < 2
            || route.Any(point => !IsInside(layout.SceneBinding.Projection.WorldBounds, point)))
        {
            return false;
        }

        for (int index = 0; index < route.Count - 1; index++)
        {
            if (CrossesTreeBoundary(layout, route[index], route[index + 1]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CrossesTreeBoundary(
        OpeningRegionWoodlandClearingLayout layout,
        Vector2 start,
        Vector2 end)
    {
        NavigationWorldBounds bounds = layout.SceneBinding.WoodlandClearingBounds;
        float halfOpening = layout.OpeningWidthPixels / 2.0f;
        float sideTop = layout.LaundryOpening.Y - halfOpening;
        float sideBottom = layout.LaundryOpening.Y + halfOpening;
        bool closedPathIsNorth = layout.ClosedForestPathOpening.Y == bounds.Top;
        float villageOpeningLeft = layout.VillageEdgeOpening.X - halfOpening;
        float villageOpeningRight = layout.VillageEdgeOpening.X + halfOpening;
        bool crossesTop = closedPathIsNorth
            ? CrossesHorizontalWall(
                start,
                end,
                bounds.Top,
                bounds.Left,
                layout.ClosedForestPathOpening.X - halfOpening)
                || CrossesHorizontalWall(
                    start,
                    end,
                    bounds.Top,
                    layout.ClosedForestPathOpening.X + halfOpening,
                    bounds.Right)
            : CrossesHorizontalWall(start, end, bounds.Top, bounds.Left, bounds.Right);
        bool crossesBottom = closedPathIsNorth
            ? CrossesHorizontalWall(start, end, bounds.Bottom, bounds.Left, villageOpeningLeft)
                || CrossesHorizontalWall(
                    start,
                    end,
                    bounds.Bottom,
                    villageOpeningRight,
                    bounds.Right)
            : CrossesHorizontalWall(
                start,
                end,
                bounds.Bottom,
                bounds.Left,
                layout.ClosedForestPathOpening.X - halfOpening)
                || CrossesHorizontalWall(
                    start,
                    end,
                    bounds.Bottom,
                    layout.ClosedForestPathOpening.X + halfOpening,
                    villageOpeningLeft)
                || CrossesHorizontalWall(
                    start,
                    end,
                    bounds.Bottom,
                    villageOpeningRight,
                    bounds.Right);
        return crossesTop
            || CrossesVerticalWall(start, end, bounds.Left, bounds.Top, sideTop)
            || CrossesVerticalWall(start, end, bounds.Left, sideBottom, bounds.Bottom)
            || CrossesVerticalWall(start, end, bounds.Right, bounds.Top, sideTop)
            || CrossesVerticalWall(start, end, bounds.Right, sideBottom, bounds.Bottom)
            || crossesBottom;
    }

    private static bool IsInside(NavigationWorldBounds bounds, Vector2 point) =>
        float.IsFinite(point.X)
        && float.IsFinite(point.Y)
        && point.X >= bounds.Left
        && point.X <= bounds.Right
        && point.Y >= bounds.Top
        && point.Y <= bounds.Bottom;

    private static bool CrossesHorizontalWall(
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
        if (progress < 0.0f || progress > 1.0f)
        {
            return false;
        }

        float crossingX = start.X + ((end.X - start.X) * progress);
        return crossingX >= wallLeft && crossingX <= wallRight;
    }

    private static bool CrossesVerticalWall(
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
        if (progress < 0.0f || progress > 1.0f)
        {
            return false;
        }

        float crossingY = start.Y + ((end.Y - start.Y) * progress);
        return crossingY >= wallTop && crossingY <= wallBottom;
    }
}

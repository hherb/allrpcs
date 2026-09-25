using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts bounded village routes only when every area boundary uses an opening.</summary>
public static class OpeningRegionVillageTransitionRoutePolicy
{
    /// <summary>Checks one route against the exact square and edge layout boundaries.</summary>
    public static bool IsAcceptable(
        OpeningRegionVillageTransitionLayout layout,
        IReadOnlyList<Vector2> route)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count < 2
            || route.Any(point => !IsInside(layout.SceneBinding.Projection.WorldBounds, point)))
        {
            return false;
        }

        foreach ((Vector2 start, Vector2 end) in Segments(route))
        {
            if (CrossesSquareWall(layout, start, end)
                || CrossesEdgeWall(layout, start, end))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<(Vector2 Start, Vector2 End)> Segments(
        IReadOnlyList<Vector2> route)
    {
        for (int index = 0; index < route.Count - 1; index++)
        {
            yield return (route[index], route[index + 1]);
        }
    }

    private static bool CrossesSquareWall(
        OpeningRegionVillageTransitionLayout layout,
        Vector2 start,
        Vector2 end)
    {
        NavigationWorldBounds bounds = layout.SceneBinding.VillageSquareBounds;
        float halfOpening = layout.OpeningWidthPixels / 2.0f;
        return CrossesHorizontalWall(start, end, bounds.Top, bounds.Left, bounds.Right)
            || CrossesHorizontalWall(
                start,
                end,
                bounds.Bottom,
                bounds.Left,
                layout.SquareGardenOpening.X - halfOpening)
            || CrossesHorizontalWall(
                start,
                end,
                bounds.Bottom,
                layout.SquareGardenOpening.X + halfOpening,
                bounds.Right)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Left,
                bounds.Top,
                layout.SquareEdgeOpening.Y - halfOpening)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Left,
                layout.SquareEdgeOpening.Y + halfOpening,
                bounds.Bottom)
            || CrossesVerticalWall(start, end, bounds.Right, bounds.Top, bounds.Bottom);
    }

    private static bool CrossesEdgeWall(
        OpeningRegionVillageTransitionLayout layout,
        Vector2 start,
        Vector2 end)
    {
        NavigationWorldBounds bounds = layout.SceneBinding.VillageEdgeBounds;
        float halfOpening = layout.OpeningWidthPixels / 2.0f;
        return CrossesHorizontalWall(
                start,
                end,
                bounds.Top,
                bounds.Left,
                layout.EdgeStreamOpening.X - halfOpening)
            || CrossesHorizontalWall(
                start,
                end,
                bounds.Top,
                layout.EdgeStreamOpening.X + halfOpening,
                bounds.Right)
            || CrossesHorizontalWall(start, end, bounds.Bottom, bounds.Left, bounds.Right)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Left,
                bounds.Top,
                layout.EdgeWoodlandOpening.Y - halfOpening)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Left,
                layout.EdgeWoodlandOpening.Y + halfOpening,
                bounds.Bottom)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Right,
                bounds.Top,
                layout.EdgeSquareOpening.Y - halfOpening)
            || CrossesVerticalWall(
                start,
                end,
                bounds.Right,
                layout.EdgeSquareOpening.Y + halfOpening,
                bounds.Bottom);
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

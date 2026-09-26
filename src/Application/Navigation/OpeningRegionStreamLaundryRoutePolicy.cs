using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts bounded stream/laundry routes only through authored clear openings.</summary>
public static class OpeningRegionStreamLaundryRoutePolicy
{
    private const float IntersectionTolerance = 0.0001f;

    /// <summary>Checks one route against both stream banks and the laundry shell.</summary>
    public static bool IsAcceptable(
        OpeningRegionStreamLaundryLayout layout,
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
            if (CrossesLaundryWall(layout, start, end)
                || CrossesPolygonInterior(start, end, layout.WestStreamBank)
                || CrossesPolygonInterior(start, end, layout.EastStreamBank))
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

    private static bool CrossesLaundryWall(
        OpeningRegionStreamLaundryLayout layout,
        Vector2 start,
        Vector2 end)
    {
        NavigationWorldBounds bounds = layout.SceneBinding.AbandonedLaundryBounds;
        float halfOpening = layout.OpeningWidthPixels / 2.0f;
        float openingTop = layout.LaundryStreamOpening.Y - halfOpening;
        float openingBottom = layout.LaundryStreamOpening.Y + halfOpening;
        return CrossesHorizontalWall(start, end, bounds.Top, bounds.Left, bounds.Right)
            || CrossesHorizontalWall(start, end, bounds.Bottom, bounds.Left, bounds.Right)
            || CrossesVerticalWall(start, end, bounds.Left, bounds.Top, openingTop)
            || CrossesVerticalWall(start, end, bounds.Left, openingBottom, bounds.Bottom)
            || CrossesVerticalWall(start, end, bounds.Right, bounds.Top, openingTop)
            || CrossesVerticalWall(start, end, bounds.Right, openingBottom, bounds.Bottom);
    }

    private static bool CrossesPolygonInterior(
        Vector2 start,
        Vector2 end,
        IReadOnlyList<Vector2> polygon)
    {
        if (IsStrictlyInsidePolygon(start, polygon) || IsStrictlyInsidePolygon(end, polygon))
        {
            return true;
        }

        List<float> intersections = [0.0f, 1.0f];
        for (int index = 0; index < polygon.Count; index++)
        {
            Vector2 edgeStart = polygon[index];
            Vector2 edgeEnd = polygon[(index + 1) % polygon.Count];
            if (TryFindSegmentIntersection(start, end, edgeStart, edgeEnd, out float progress))
            {
                intersections.Add(progress);
            }
        }

        intersections.Sort();
        for (int index = 0; index < intersections.Count - 1; index++)
        {
            float midpoint = (intersections[index] + intersections[index + 1]) / 2.0f;
            if (IsStrictlyInsidePolygon(Vector2.Lerp(start, end, midpoint), polygon))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryFindSegmentIntersection(
        Vector2 start,
        Vector2 end,
        Vector2 edgeStart,
        Vector2 edgeEnd,
        out float progress)
    {
        Vector2 route = end - start;
        Vector2 edge = edgeEnd - edgeStart;
        float denominator = Cross(route, edge);
        if (MathF.Abs(denominator) <= IntersectionTolerance)
        {
            progress = 0.0f;
            return false;
        }

        Vector2 offset = edgeStart - start;
        float routeProgress = Cross(offset, edge) / denominator;
        float edgeProgress = Cross(offset, route) / denominator;
        progress = routeProgress;
        return routeProgress >= 0.0f
            && routeProgress <= 1.0f
            && edgeProgress >= 0.0f
            && edgeProgress <= 1.0f;
    }

    private static bool IsStrictlyInsidePolygon(
        Vector2 point,
        IReadOnlyList<Vector2> polygon)
    {
        bool inside = false;
        for (int first = 0, second = polygon.Count - 1; first < polygon.Count; second = first++)
        {
            Vector2 current = polygon[first];
            Vector2 previous = polygon[second];
            bool crossesRay = (current.Y > point.Y) != (previous.Y > point.Y)
                && point.X < ((previous.X - current.X) * (point.Y - current.Y)
                    / (previous.Y - current.Y)) + current.X;
            if (crossesRay)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static float Cross(Vector2 first, Vector2 second) =>
        (first.X * second.Y) - (first.Y * second.X);

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

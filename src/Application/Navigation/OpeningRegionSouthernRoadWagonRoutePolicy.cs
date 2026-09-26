using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts routes that stay inside the southern road and wagon collision boundaries.</summary>
public static class OpeningRegionSouthernRoadWagonRoutePolicy
{
    /// <summary>Checks one route against every authored road and wagon boundary.</summary>
    public static bool IsAcceptable(
        OpeningRegionSouthernRoadWagonLayout layout,
        IReadOnlyList<Vector2> route)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count < 2
            || route.Any(point => !Inside(layout.SceneBinding.Projection.WorldBounds, point)))
        {
            return false;
        }

        IEnumerable<OpeningRegionBoundarySegment> boundaries =
            layout.RoadBoundarySegments.Concat(layout.WagonBoundarySegments);
        for (int index = 0; index < route.Count - 1; index++)
        {
            if (boundaries.Any(boundary => Intersects(
                route[index],
                route[index + 1],
                boundary.Start,
                boundary.End)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Intersects(
        Vector2 firstStart,
        Vector2 firstEnd,
        Vector2 secondStart,
        Vector2 secondEnd)
    {
        float firstSecondStart = Cross(firstStart, firstEnd, secondStart);
        float firstSecondEnd = Cross(firstStart, firstEnd, secondEnd);
        float secondFirstStart = Cross(secondStart, secondEnd, firstStart);
        float secondFirstEnd = Cross(secondStart, secondEnd, firstEnd);
        if (StrictlyOpposite(firstSecondStart, firstSecondEnd)
            && StrictlyOpposite(secondFirstStart, secondFirstEnd))
        {
            return true;
        }

        return (firstSecondStart == 0.0f && OnSegment(firstStart, firstEnd, secondStart))
            || (firstSecondEnd == 0.0f && OnSegment(firstStart, firstEnd, secondEnd))
            || (secondFirstStart == 0.0f && OnSegment(secondStart, secondEnd, firstStart))
            || (secondFirstEnd == 0.0f && OnSegment(secondStart, secondEnd, firstEnd));
    }

    private static float Cross(Vector2 start, Vector2 end, Vector2 point) =>
        ((end.X - start.X) * (point.Y - start.Y))
        - ((end.Y - start.Y) * (point.X - start.X));

    private static bool StrictlyOpposite(float first, float second) =>
        (first < 0.0f && second > 0.0f) || (first > 0.0f && second < 0.0f);

    private static bool OnSegment(Vector2 start, Vector2 end, Vector2 point) =>
        point.X >= MathF.Min(start.X, end.X)
        && point.X <= MathF.Max(start.X, end.X)
        && point.Y >= MathF.Min(start.Y, end.Y)
        && point.Y <= MathF.Max(start.Y, end.Y);

    private static bool Inside(NavigationWorldBounds bounds, Vector2 point) =>
        float.IsFinite(point.X)
        && float.IsFinite(point.Y)
        && point.X >= bounds.Left
        && point.X <= bounds.Right
        && point.Y >= bounds.Top
        && point.Y <= bounds.Bottom;
}

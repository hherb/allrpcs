using System.Numerics;

namespace Allrpcs.Application.Navigation;

/// <summary>Accepts bounded sickhouse routes only when no segment crosses a wall.</summary>
public static class OpeningRegionSickhouseRoutePolicy
{
    /// <summary>Checks one route against the exact room bounds and single entrance.</summary>
    public static bool IsAcceptable(
        OpeningRegionSickhouseLayout layout,
        IReadOnlyList<Vector2> route)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(route);
        if (route.Count < 2
            || route.Any(point => !IsInside(layout.FirstPatientBinding.Projection.WorldBounds, point)))
        {
            return false;
        }

        NavigationWorldBounds room = layout.RoomBounds;
        float entranceLeft = layout.EntranceCenter.X - (layout.EntranceWidthPixels / 2.0f);
        float entranceRight = layout.EntranceCenter.X + (layout.EntranceWidthPixels / 2.0f);
        for (int index = 0; index < route.Count - 1; index++)
        {
            Vector2 start = route[index];
            Vector2 end = route[index + 1];
            if (CrossesHorizontalWall(start, end, room.Top, room.Left, entranceLeft)
                || CrossesHorizontalWall(start, end, room.Top, entranceRight, room.Right)
                || CrossesHorizontalWall(start, end, room.Bottom, room.Left, room.Right)
                || CrossesVerticalWall(start, end, room.Left, room.Top, room.Bottom)
                || CrossesVerticalWall(start, end, room.Right, room.Top, room.Bottom))
            {
                return false;
            }
        }

        return true;
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

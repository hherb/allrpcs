using System.Numerics;

namespace Allrpcs.Application.Actions;

/// <summary>Checks this action's inclusive abstract reach and complete supported straight segment.</summary>
public static class CaskCoverHandlingAccessPolicy
{
    /// <summary>The specifically authorized reach from actual Seeker to current cask marker centre.</summary>
    public const float ReachPixels = 32;
    /// <summary>The maximum spacing of support observations, including both endpoints.</summary>
    public const float MaximumSampleSpacingPixels = 4;
    /// <summary>Requires finite in-range positions and every sampled support observation.</summary>
    public static bool CanReach(Vector2 actor, Vector2 target, Func<Vector2, bool> supported)
    {
        ArgumentNullException.ThrowIfNull(supported);
        if (!float.IsFinite(actor.X) || !float.IsFinite(actor.Y)
            || !float.IsFinite(target.X) || !float.IsFinite(target.Y)) { return false; }
        float distance = Vector2.Distance(actor, target);
        if (distance > ReachPixels) { return false; }
        int intervals = Math.Max(1, (int)MathF.Ceiling(distance / MaximumSampleSpacingPixels));
        for (int index = 0; index <= intervals; index++)
        { if (!supported(Vector2.Lerp(actor, target, (float)index / intervals))) { return false; } }
        return true;
    }
}

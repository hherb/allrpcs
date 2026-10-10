using System.Numerics;
using Allrpcs.Domain.Actions;

namespace Allrpcs.Application.Actions;

/// <summary>Checks the approved dry footprint and hand reach, independently of Godot.</summary>
public static class StreamWaterContactAccessPolicy
{
    /// <summary>The newly authorized inclusive hand reach at this one site.</summary>
    public const float ReachPixels = 32;
    /// <summary>The authorized standing clearance from all crossing edges.</summary>
    public const float StandingInsetPixels = 12;
    /// <summary>The sole approved site key, separate from discovery object identity.</summary>
    public const string SiteKey = "stream-west-bridge-contact";

    /// <summary>Requires exact authored crossing/site coordinates and supported finite dry position.</summary>
    public static bool CanReach(Vector2 position, bool supported, StreamWaterContactSite site,
        Vector2 crossingMinimum, Vector2 crossingMaximum)
    {
        ArgumentNullException.ThrowIfNull(site);
        if (!supported || !float.IsFinite(position.X) || !float.IsFinite(position.Y)
            || crossingMinimum != new Vector2(1736, 452) || crossingMaximum != new Vector2(1864, 548)
            || site.Key != SiteKey || site.Position != crossingMinimum + new Vector2(-2, 32)) { return false; }
        Vector2 low = crossingMinimum + new Vector2(StandingInsetPixels);
        Vector2 high = crossingMaximum - new Vector2(StandingInsetPixels);
        return position.X >= low.X && position.X <= high.X && position.Y >= low.Y && position.Y <= high.Y
            && Vector2.DistanceSquared(position, site.Position) <= ReachPixels * ReachPixels;
    }
}

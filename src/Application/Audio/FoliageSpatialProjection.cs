using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>Presentation admission for the one fixed garden source, without world hearing authority.</summary>
public static class FoliageSpatialProjection
{
    /// <summary>The accepted planting's unchanged world origin.</summary>
    public static Vector2 EmitterPosition => new(2232, 1328);
    /// <summary>Exclusive native player range in world pixels.</summary>
    public const float RangePixels = 192;
    /// <summary>A listener discontinuity larger than this cancels presentation.</summary>
    public const float TeleportPixels = 96;
    /// <summary>Anonymous wording shared by caption and independently owned history.</summary>
    public const string Phrase = "Leaves rustling";

    /// <summary>Admits a finite supported exterior listener independently of output and caption preferences.</summary>
    public static bool Admitted(Vector2 listener, bool sourceValid, bool listenerSupported) =>
        sourceValid && Exterior(listener, listenerSupported)
        && Vector2.DistanceSquared(EmitterPosition, listener) < RangePixels * RangePixels;

    /// <summary>Checks only retained bounds and inclusive room exclusions; never creates a material.</summary>
    public static bool Exterior(Vector2 listener, bool supported) => supported
        && OpeningRegionPositionalAudioProjection.Finite(listener)
        && listener.X is >= 0 and <= 3200 && listener.Y is >= 0 and <= 1800
        && !Inside(listener, 2480, 1310, 2800, 1502)
        && !Inside(listener, 1420, 120, 1580, 360);

    /// <summary>Captures the retained screen-space bearing with inclusive visible viewport edges.</summary>
    public static string Caption(Vector2 screenPosition, Vector2 viewport) =>
        OpeningRegionPositionalAudioProjection.Caption(Phrase, screenPosition, viewport);

    private static bool Inside(Vector2 p, float left, float top, float right, float bottom) =>
        p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom;
}

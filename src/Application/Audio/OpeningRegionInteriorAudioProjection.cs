using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>The two authorized listener profiles, without world indoor-state authority.</summary>
public enum InteriorAudioRoom { None, Sickhouse, Laundry }

/// <summary>Immutable validated live doorway coordinates for the exact approved footprints.</summary>
public sealed class InteriorAudioLayout
{
    /// <summary>Retains live openings only when they match the scoped structures.</summary>
    public InteriorAudioLayout(Vector2 sickhouseEntrance, float sickhouseWidth,
        Vector2 laundryWest, Vector2 laundryEast, float laundryWidth)
    {
        if (!Near(sickhouseEntrance, new(2528, 1310)) || !Near(sickhouseWidth, 64)
            || !Near(laundryWest, new(1420, 240.8f)) || !Near(laundryEast, new(1580, 294.66666f))
            || !Near(laundryWidth, 96))
        {
            throw new ArgumentException("Interior audio requires the exact approved live doorways.");
        }
        SickhouseEntrance = sickhouseEntrance;
        SickhouseWidth = sickhouseWidth;
        LaundryWest = laundryWest;
        LaundryEast = laundryEast;
        LaundryWidth = laundryWidth;
    }

    /// <summary>The live north entrance centre.</summary>
    public Vector2 SickhouseEntrance { get; }
    /// <summary>The live north entrance width.</summary>
    public float SickhouseWidth { get; }
    /// <summary>The live west laundry opening.</summary>
    public Vector2 LaundryWest { get; }
    /// <summary>The live east laundry opening.</summary>
    public Vector2 LaundryEast { get; }
    /// <summary>The shared live laundry opening width.</summary>
    public float LaundryWidth { get; }

    private static bool Near(Vector2 actual, Vector2 expected) => Near(actual.X, expected.X) && Near(actual.Y, expected.Y);
    private static bool Near(float actual, float expected) => float.IsFinite(actual) && Math.Abs(actual - expected) <= 0.01f;
}

/// <summary>One transient target, without persisted location, perception or event state.</summary>
public sealed record InteriorAudioSnapshot(InteriorAudioRoom Room, double Blend, FootstepMaterial? Material)
{
    /// <summary>Neutral exterior or unavailable presentation.</summary>
    public static InteriorAudioSnapshot Neutral { get; } = new(InteriorAudioRoom.None, 0, null);
    /// <summary>Whether the target is a valid bounded enclosure profile.</summary>
    public bool Valid => Enum.IsDefined(Room) && double.IsFinite(Blend) && Blend is >= 0 and <= 1;
    /// <summary>Amplitude multiplier for one existing bed; sheltered rain remains unity.</summary>
    public double Gain(int bed)
    {
        if (bed is < 0 or > 3) { throw new ArgumentOutOfRangeException(nameof(bed)); }
        double attenuation = Room switch { InteriorAudioRoom.Sickhouse => -9, InteriorAudioRoom.Laundry => -3, _ => 0 };
        return !Valid || bed == 0 ? 1 : Math.Pow(10, attenuation * Blend / 20);
    }
}

/// <summary>Audio-only conservative floors and distance-to-live-opening enclosure targets.</summary>
public static class OpeningRegionInteriorAudioProjection
{
    /// <summary>Depth at which the proposed listener profile reaches full enclosure.</summary>
    public const double FullDepthPixels = 32;

    /// <summary>Resolves one supported local point without inferring geometry from art.</summary>
    public static InteriorAudioSnapshot Select(InteriorAudioLayout? layout, Vector2 p, bool supported)
    {
        if (layout is null || !supported || !Finite(p)) { return InteriorAudioSnapshot.Neutral; }
        bool sickhouse = Strict(p, 2488, 1318, 2792, 1494)
            || (p.Y >= 1310 && p.Y <= 1318 && Math.Abs(p.X - layout.SickhouseEntrance.X) < layout.SickhouseWidth / 2);
        if (sickhouse)
        {
            double distance = Distance(p, layout.SickhouseEntrance, layout.SickhouseWidth, horizontal: true);
            return new(InteriorAudioRoom.Sickhouse, Math.Clamp(distance / FullDepthPixels, 0, 1), FootstepMaterial.Timber);
        }
        bool laundry = Strict(p, 1428, 128, 1572, 352)
            || (p.X >= 1420 && p.X <= 1428 && p.Y > layout.LaundryWest.Y - layout.LaundryWidth / 2
                && p.Y < layout.LaundryWest.Y + layout.LaundryWidth / 2)
            || (p.X >= 1572 && p.X <= 1580 && p.Y > layout.LaundryEast.Y - layout.LaundryWidth / 2
                && p.Y < layout.LaundryEast.Y + layout.LaundryWidth / 2);
        if (laundry)
        {
            double distance = Math.Min(Distance(p, layout.LaundryWest, layout.LaundryWidth, false),
                Distance(p, layout.LaundryEast, layout.LaundryWidth, false));
            return new(InteriorAudioRoom.Laundry, Math.Clamp(distance / FullDepthPixels, 0, 1), FootstepMaterial.Earth);
        }
        return InteriorAudioSnapshot.Neutral;
    }

    /// <summary>Overrides only eligible interiors; all other positions use the retained exterior selector.</summary>
    public static FootstepMaterial? MaterialAt(InteriorAudioLayout? layout, Vector2 p, bool supported) =>
        Select(layout, p, supported).Material ?? OpeningRegionFootstepMaterialProjection.Select(p, supported);

    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
    private static bool Strict(Vector2 p, float left, float top, float right, float bottom) =>
        p.X > left && p.X < right && p.Y > top && p.Y < bottom;
    private static double Distance(Vector2 p, Vector2 center, float width, bool horizontal)
    {
        Vector2 axis = horizontal ? Vector2.UnitX : Vector2.UnitY;
        Vector2 start = center - axis * (width / 2);
        Vector2 segment = axis * width;
        float t = Math.Clamp(Vector2.Dot(p - start, segment) / segment.LengthSquared(), 0, 1);
        return Vector2.Distance(p, start + segment * t);
    }
}

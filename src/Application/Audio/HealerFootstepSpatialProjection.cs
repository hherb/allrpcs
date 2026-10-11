using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>Pure admission for the new anonymous exterior contact, without gameplay hearing authority.</summary>
public static class HealerFootstepSpatialProjection
{
    /// <summary>Exclusive world-pixel contact presentation range.</summary>
    public const float RangePixels = 192;
    /// <summary>Lifetime of the independently owned current caption.</summary>
    public const double CaptionSeconds = 0.75;
    /// <summary>Largest presentation-time increment admitted by this owner.</summary>
    public const double MaximumDeltaSeconds = 0.1;

    /// <summary>Admits only known materials with supported exterior endpoints and finite in-range positions.</summary>
    public static bool Admitted(FootstepContact? contact, Vector2 listener, bool sourceSupported, bool listenerSupported) =>
        contact is not null && Enum.IsDefined(contact.Leg) && contact.Variant is >= 1 and <= 6
        && OpeningRegionFootstepMaterialProjection.ClipId(contact.Material) is not null
        && sourceSupported && listenerSupported
        && OpeningRegionPositionalAudioProjection.Finite(contact.Position)
        && OpeningRegionPositionalAudioProjection.Finite(listener)
        && Vector2.DistanceSquared(contact.Position, listener) < RangePixels * RangePixels;

    /// <summary>Allows bounded active presentation time without advancing a world clock.</summary>
    public static bool ValidDelta(double seconds) => double.IsFinite(seconds) && seconds is > 0 and <= MaximumDeltaSeconds;

    /// <summary>Formats only the generic material and optional screen bearing captured at contact time.</summary>
    public static string Caption(FootstepContact contact, Vector2 screenPosition, Vector2 viewport) =>
        OpeningRegionPositionalAudioProjection.Caption(
            OpeningRegionFootstepMaterialProjection.Caption(contact.Material), screenPosition, viewport);
}

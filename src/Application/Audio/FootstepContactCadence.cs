using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>Fixed logical identity for modeled contacts, without anatomical sample attribution.</summary>
public enum FootstepLeg { Left, Right }

/// <summary>A transient measurement of one completed physics movement, in common world-pixel coordinates.</summary>
public sealed record FootstepMotionSample(long Tick, Vector2 Start, Vector2 End, double Delta, double SpeedLimit);

/// <summary>One threshold crossing, with interpolated support position and deterministic variant.</summary>
public sealed record FootstepContact(FootstepMaterial Material, FootstepLeg Leg, int Variant, Vector2 Position);

/// <summary>Optional contact and explicit cancellation of any owned transient presentation.</summary>
public sealed record FootstepCadenceResult(FootstepContact? Contact, bool Cancelled);

/// <summary>Counts actual displacement on supported ground; it never controls movement, art or world time.</summary>
public sealed class FootstepContactCadence
{
    /// <summary>Authored half-cycle displacement at the default 240-pixel/second movement speed.</summary>
    public const double ContactDistancePixels = 86.4;
    private const double MinimumIntervalSeconds = 0.180;
    private const double MaximumDeltaSeconds = 0.1;
    private const double SupportSampleSpacingPixels = 4;
    private const double MaximumWorldDisplacementPixels = 3672;
    private readonly int[] variants = new int[4];
    private bool initialized;
    private Vector2 previous;
    private long lastTick = long.MinValue;
    private double distance;
    private double eligibleTime;
    private FootstepLeg nextLeg;

    /// <summary>Cancels cadence and rebases the next measurement without producing a startup cue.</summary>
    public void Reset()
    {
        initialized = false;
        lastTick = long.MinValue;
        ClearCadence();
    }

    /// <summary>Observes one completed movement and consumes all crossings without a stale queue.</summary>
    public FootstepCadenceResult Advance(FootstepMotionSample sample, Func<Vector2, FootstepMaterial?> materialAt)
    {
        ArgumentNullException.ThrowIfNull(sample);
        ArgumentNullException.ThrowIfNull(materialAt);
        if (sample.Tick <= lastTick) { return Cancel(); }
        lastTick = sample.Tick;
        double measured = Vector2.Distance(sample.Start, sample.End);
        bool continuous = !initialized || Vector2.DistanceSquared(previous, sample.Start) <= 1;
        previous = sample.End;
        bool wasInitialized = initialized;
        initialized = Finite(sample.End);
        if (!Finite(sample.Start) || !Finite(sample.End) || !double.IsFinite(sample.Delta)
            || sample.Delta <= 0 || sample.Delta > MaximumDeltaSeconds
            || !double.IsFinite(sample.SpeedLimit) || sample.SpeedLimit <= 0
            || !double.IsFinite(measured) || measured > MaximumWorldDisplacementPixels || !continuous || measured > sample.SpeedLimit * sample.Delta + 1)
        {
            return Cancel();
        }
        if (!wasInitialized) { return Cancel(); }
        if (measured <= 0.01) { return Cancel(); }
        int subdivisions = (int)Math.Ceiling(measured / SupportSampleSpacingPixels);
        for (int index = 0; index <= subdivisions; index++)
        {
            FootstepMaterial? material = materialAt(Vector2.Lerp(sample.Start, sample.End, (float)index / subdivisions));
            if (material is null || OpeningRegionFootstepMaterialProjection.ClipId(material.Value) is null) { return Cancel(); }
        }

        eligibleTime += sample.Delta;
        double accumulated = distance + measured;
        int crossings = (int)Math.Floor((accumulated + 0.001) / ContactDistancePixels);
        FootstepContact? emitted = null;
        for (int index = 0; index < crossings; index++)
        {
            float progress = (float)Math.Clamp((ContactDistancePixels * (index + 1) - distance) / measured, 0, 1);
            Vector2 position = Vector2.Lerp(sample.Start, sample.End, progress);
            FootstepMaterial? material = materialAt(position);
            if (material is null || OpeningRegionFootstepMaterialProjection.ClipId(material.Value) is null) { return Cancel(); }
            int slot = (int)material.Value;
            int variant = variants[slot] + 1;
            variants[slot] = variant % 6;
            if (emitted is null && eligibleTime + 0.000001 >= MinimumIntervalSeconds)
            {
                emitted = new(material.Value, nextLeg, variant, position);
                eligibleTime = 0;
            }
            nextLeg = nextLeg == FootstepLeg.Left ? FootstepLeg.Right : FootstepLeg.Left;
        }
        distance = Math.Max(0, accumulated - crossings * ContactDistancePixels);
        return new(emitted, false);
    }

    private FootstepCadenceResult Cancel() { ClearCadence(); return new(null, true); }
    private void ClearCadence()
    {
        distance = eligibleTime = 0;
        nextLeg = FootstepLeg.Left;
        Array.Clear(variants);
    }
    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
}

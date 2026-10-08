namespace Allrpcs.Application.Audio;

/// <summary>One bounded four-gain slew, independent of bus processing and world time.</summary>
public sealed class InteriorAudioRamp
{
    /// <summary>Maximum time needed to traverse one amplitude unit.</summary>
    public const double SecondsPerAmplitudeUnit = 0.75;
    /// <summary>Largest active presentation frame; suspended time never catches up.</summary>
    public const double MaximumDeltaSeconds = 0.25;
    private readonly double[] gains = [1, 1, 1, 1];

    /// <summary>Reads one bed's current multiplier.</summary>
    public double Gain(int bed) => gains[bed];
    /// <summary>Clears transient attenuation immediately.</summary>
    public void Reset() => Array.Fill(gains, 1);
    /// <summary>Advances once per active frame; invalid time or target resets to neutral.</summary>
    public bool Advance(InteriorAudioSnapshot target, double delta)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!target.Valid || !double.IsFinite(delta) || delta < 0 || delta > MaximumDeltaSeconds)
        {
            Reset(); return false;
        }
        double maximum = delta / SecondsPerAmplitudeUnit;
        for (int bed = 0; bed < gains.Length; bed++)
        {
            gains[bed] += Math.Clamp(target.Gain(bed) - gains[bed], -maximum, maximum);
        }
        return true;
    }
}

namespace Allrpcs.Application.Audio;

/// <summary>Authorized presentation foreground kinds, without creating world events.</summary>
public enum AudioForeground { Speech, ConsequentialCue, Reading }

/// <summary>Tracks overlapping foreground leases and deterministic gain envelopes.</summary>
public sealed class AudioDuckingEnvelope
{
    private readonly Dictionary<long, AudioForeground> active = [];
    private readonly double[] reduction = new double[6];
    private readonly double[] peak = new double[6];
    private readonly double[] attackSeconds = new double[6];
    private long nextToken;
    private double holdRemaining;
    private double releaseSeconds = 0.4;

    /// <summary>Starts one foreground lease; its token must be ended on success, failure or cancellation.</summary>
    public long Begin(AudioForeground foreground)
    {
        if (!Enum.IsDefined(foreground))
        {
            throw new ArgumentOutOfRangeException(nameof(foreground));
        }
        long token = ++nextToken;
        active.Add(token, foreground);
        releaseSeconds = Math.Max(releaseSeconds, foreground == AudioForeground.ConsequentialCue ? 0.4 : 0.6);
        holdRemaining = Math.Max(holdRemaining, foreground == AudioForeground.ConsequentialCue ? 0.1 : 0.15);
        foreach (AudioCategory category in Enum.GetValues<AudioCategory>())
        {
            int index = (int)category;
            double target = Target(category, foreground);
            if (target > peak[index])
            {
                peak[index] = target;
                attackSeconds[index] = foreground == AudioForeground.ConsequentialCue ? 0.05 : 0.1;
            }
        }
        return token;
    }

    /// <summary>Ends an exact lease; repeated or unknown endings cannot disturb another foreground.</summary>
    public bool End(long token) => active.Remove(token);

    /// <summary>Advances attack, hold and release using elapsed presentation seconds.</summary>
    public void Advance(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }
        if (active.Count > 0)
        {
            for (int index = 0; index < reduction.Length; index++)
            {
                if (peak[index] > 0)
                {
                    reduction[index] = Math.Min(peak[index], reduction[index] + peak[index] * seconds / attackSeconds[index]);
                }
            }
            return;
        }
        double releaseTime = Math.Max(0, seconds - holdRemaining);
        holdRemaining = Math.Max(0, holdRemaining - seconds);
        for (int index = 0; index < reduction.Length; index++)
        {
            reduction[index] = Math.Max(0, reduction[index] - peak[index] * releaseTime / releaseSeconds);
        }
        if (reduction.All(value => value == 0) && holdRemaining == 0)
        {
            Array.Clear(peak);
            releaseSeconds = 0.4;
        }
    }

    /// <summary>Returns attenuation relative to the user's chosen gain.</summary>
    public double Gain(AudioCategory category) => Math.Pow(10, -ReductionDb(category) / 20);
    /// <summary>Returns the current non-additive reduction for diagnostics.</summary>
    public double ReductionDb(AudioCategory category) => reduction[(int)category];
    /// <summary>Immediately clears all foreground and reductions during scene teardown.</summary>
    public void Clear()
    {
        active.Clear();
        Array.Clear(reduction);
        Array.Clear(peak);
        holdRemaining = 0;
        releaseSeconds = 0.4;
    }

    private static double Target(AudioCategory category, AudioForeground foreground) => (category, foreground) switch
    {
        (AudioCategory.Music, AudioForeground.Speech) => 6,
        (AudioCategory.Ambience or AudioCategory.Effects, AudioForeground.Speech) => 3,
        (AudioCategory.Music, _) => 3,
        (AudioCategory.Ambience, _) => 2,
        _ => 0,
    };
}

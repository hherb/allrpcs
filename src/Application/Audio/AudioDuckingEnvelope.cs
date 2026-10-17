namespace Allrpcs.Application.Audio;

/// <summary>Authorized presentation foreground kinds, without creating world events.</summary>
public enum AudioForeground { Speech, ConsequentialCue, Reading }

/// <summary>Tracks overlapping foreground leases and deterministic gain envelopes.</summary>
public sealed class AudioDuckingEnvelope
{
    private readonly Dictionary<long, AudioForeground> active = [];
    private readonly int[] foregroundCounts = new int[3];
    private readonly double[] reduction = new double[6];
    private readonly double[] peak = new double[6];
    private long nextToken;
    private double holdRemaining;
    private double releaseSeconds = 0.4;
    private bool endedThisFrame;

    /// <summary>Starts one foreground lease; its token must be ended on success, failure or cancellation.</summary>
    public long Begin(AudioForeground foreground)
    {
        if (!Enum.IsDefined(foreground))
        {
            throw new ArgumentOutOfRangeException(nameof(foreground));
        }
        long token = ++nextToken;
        active.Add(token, foreground);
        foregroundCounts[(int)foreground]++;
        return token;
    }

    /// <summary>Ends an exact lease; repeated or unknown endings cannot disturb another foreground.</summary>
    public bool End(long token)
    {
        if (!active.Remove(token, out AudioForeground foreground)) { return false; }
        foregroundCounts[(int)foreground]--;
        // The ending foreground's policy timing releases from the current level, never a historical peak;
        // leases ending in the same frame keep the longest timing, independent of their End order.
        Array.Copy(reduction, peak, reduction.Length);
        double release = foreground == AudioForeground.ConsequentialCue ? 0.4 : 0.6;
        double hold = foreground == AudioForeground.ConsequentialCue ? 0.1 : 0.15;
        releaseSeconds = endedThisFrame ? Math.Max(releaseSeconds, release) : release;
        holdRemaining = endedThisFrame ? Math.Max(holdRemaining, hold) : hold;
        endedThisFrame = true;
        return true;
    }

    /// <summary>Advances attack, hold and release using elapsed presentation seconds.</summary>
    public void Advance(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds));
        }
        endedThisFrame = false;
        if (active.Count > 0)
        {
            for (int index = 0; index < reduction.Length; index++)
            {
                (double target, double attack) = ActiveTarget((AudioCategory)index);
                reduction[index] = reduction[index] < target
                    ? Math.Min(target, reduction[index] + target * seconds / attack)
                    : Math.Max(target, reduction[index] - peak[index] * seconds / releaseSeconds);
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
        Array.Clear(foregroundCounts);
        Array.Clear(reduction);
        Array.Clear(peak);
        holdRemaining = 0;
        releaseSeconds = 0.4;
        endedThisFrame = false;
    }

    private (double Target, double Attack) ActiveTarget(AudioCategory category)
    {
        double target = 0;
        double attack = 0.1;
        for (int index = 0; index < foregroundCounts.Length; index++)
        {
            if (foregroundCounts[index] == 0) { continue; }
            AudioForeground foreground = (AudioForeground)index;
            double candidate = Target(category, foreground);
            double candidateAttack = foreground == AudioForeground.ConsequentialCue ? 0.05 : 0.1;
            if (candidate > target || (candidate == target && candidateAttack < attack))
            {
                target = candidate;
                attack = candidateAttack;
            }
        }
        return (target, attack);
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

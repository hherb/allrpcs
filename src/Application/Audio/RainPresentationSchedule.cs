namespace Allrpcs.Application.Audio;

/// <summary>A deterministic presentation clock with no world-time, random or deferred replay authority.</summary>
public sealed class RainPresentationSchedule
{
    /// <summary>First wait and interval between consumed scheduled starts.</summary>
    public const double IntervalSeconds = 20;
    /// <summary>Maximum lifetime of one admitted passage.</summary>
    public const double PassageSeconds = 4;
    /// <summary>Largest finite active increment; stalls cancel and rebase.</summary>
    public const double MaximumDeltaSeconds = 0.1;
    private double untilStart = IntervalSeconds;
    private double remaining;

    /// <summary>Whether the current scheduled passage has remaining active presentation time.</summary>
    public bool InPassage => remaining > 0;

    /// <summary>Consumes starts regardless of range/output; callers decide admission only at each start.</summary>
    public bool Advance(double seconds, bool blocked)
    {
        if (blocked || !ValidDelta(seconds)) { Reset(); return false; }
        remaining = Math.Max(0, remaining - seconds);
        if (remaining < 0.000000001) { remaining = 0; }
        untilStart -= seconds;
        if (untilStart > 0.000000001) { return false; }
        untilStart = IntervalSeconds;
        remaining = PassageSeconds;
        return true;
    }

    /// <summary>Stops a current passage without changing the next start or creating a late replay.</summary>
    public void CancelPassage() => remaining = 0;

    /// <summary>Pause/overlay/stall/load resets wait and current passage; owns no accessible history.</summary>
    public void Reset() { untilStart = IntervalSeconds; remaining = 0; }

    /// <summary>Rejects nonfinite, nonpositive and stalled deltas.</summary>
    public static bool ValidDelta(double seconds) =>
        double.IsFinite(seconds) && seconds is > 0 and <= MaximumDeltaSeconds;
}

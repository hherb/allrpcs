namespace Allrpcs.Application.Audio;

/// <summary>Independent anonymous transient footstep history; cannot mutate reply or world history.</summary>
public sealed class HealerFootstepCueHistory
{
    /// <summary>Maximum accessible entries before oldest-first eviction.</summary>
    public const int Capacity = 16;
    /// <summary>Minimum active presentation time between recorded contacts.</summary>
    public const double IntervalSeconds = 1;
    private readonly List<string> entries = [];
    private long sequence;
    private double elapsed = IntervalSeconds;

    /// <summary>A detached insertion-ordered view.</summary>
    public IReadOnlyList<string> Entries => entries.ToArray();

    /// <summary>Advances only during active presentation; rejects invalid or stalled frames.</summary>
    public bool Advance(double seconds)
    {
        if (!HealerFootstepSpatialProjection.ValidDelta(seconds)) { return false; }
        elapsed = Math.Min(IntervalSeconds, elapsed + seconds);
        return true;
    }

    /// <summary>Records one bounded generic phrase, discarding rate-limited contacts without a queue.</summary>
    public bool Record(string caption)
    {
        string[] phrases = ["Footsteps on earth", "Footsteps on gravel", "Footsteps in leaves", "Footsteps on timber"];
        string[] directions = ["above", "above/right", "right", "below/right", "below", "below/left", "left", "above/left"];
        bool generic = phrases.Any(p => caption == p || directions.Any(d => caption == p + " " + d));
        if (!generic || elapsed + 0.000001 < IntervalSeconds || sequence == long.MaxValue) { return false; }
        entries.Add($"{++sequence}. {caption}");
        if (entries.Count > Capacity) { entries.RemoveAt(0); }
        elapsed = 0;
        return true;
    }

    /// <summary>Load/teardown clears only this history and its limiter, without replay.</summary>
    public void Clear() { entries.Clear(); sequence = 0; elapsed = IntervalSeconds; }
}

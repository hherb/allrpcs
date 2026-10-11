namespace Allrpcs.Application.Audio;

/// <summary>Anonymous insertion-ordered transient foliage records, independent of all prior cue owners.</summary>
public sealed class FoliageCueHistory
{
    /// <summary>Oldest-first bounded accessible capacity.</summary>
    public const int Capacity = 16;
    /// <summary>Minimum active time between records; rejected starts are discarded.</summary>
    public const double IntervalSeconds = 12;
    private static readonly string[] Bearings =
        ["above", "above/right", "right", "below/right", "below", "below/left", "left", "above/left"];
    private readonly List<string> entries = [];
    private double elapsed = IntervalSeconds;
    private long sequence;

    /// <summary>A detached read-only snapshot that cannot modify future views.</summary>
    public IReadOnlyList<string> Entries => Array.AsReadOnly(entries.ToArray());

    /// <summary>Advances only the local limiter when presentation is active.</summary>
    public bool Advance(double seconds)
    {
        if (!FoliagePresentationSchedule.ValidDelta(seconds)) { return false; }
        elapsed = Math.Min(IntervalSeconds, elapsed + seconds);
        return true;
    }

    /// <summary>Records the first generic admitted start immediately, then no more than every twelve seconds.</summary>
    public bool Record(string caption)
    {
        bool generic = caption == FoliageSpatialProjection.Phrase
            || Bearings.Any(b => caption == FoliageSpatialProjection.Phrase + " " + b);
        if (!generic || elapsed + 0.000000001 < IntervalSeconds || sequence == long.MaxValue) { return false; }
        entries.Add($"{++sequence}. {caption}");
        if (entries.Count > Capacity) { entries.RemoveAt(0); }
        elapsed = 0;
        return true;
    }

    /// <summary>Successful load/teardown clears only this history, sequence and limiter.</summary>
    public void Clear() { entries.Clear(); sequence = 0; elapsed = IntervalSeconds; }
}

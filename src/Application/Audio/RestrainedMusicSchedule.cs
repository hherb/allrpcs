using System.Numerics;

namespace Allrpcs.Application.Audio;

/// <summary>Two accepted optional passages, without a world event or rest-state claim.</summary>
public enum MusicPassage { Exploration, Treatment }

/// <summary>Finite presentation-only admission, silence and interruption; owns no gameplay clock.</summary>
public sealed class RestrainedMusicSchedule
{
    /// <summary>Continuous quiet and regional dwell before exploration admission.</summary>
    public const double QuietSeconds = 12;
    /// <summary>Actual supported traversal required after a regional transition.</summary>
    public const double TravelPixels = 24;
    /// <summary>Deadline for one temporary opportunity; expiry wins an admission tie.</summary>
    public const double OpportunitySeconds = 45;
    /// <summary>Foreground reservation for an existing committed action passage.</summary>
    public const double ActionHoldSeconds = 8;
    /// <summary>Foreground reservation for an already admitted reply caption.</summary>
    public const double ReplyHoldSeconds = 3;
    /// <summary>Current-amplitude fade to silence before explicit foreground stop.</summary>
    public const double FadeSeconds = 0.6;
    /// <summary>Minimum silence after actual completion, stop or failed start.</summary>
    public const double CooldownSeconds = 180;
    /// <summary>Long-frame threshold; suspended time never counts as quiet time.</summary>
    public const double MaximumDeltaSeconds = 0.25;
    /// <summary>World position discontinuity that rebases transient presentation.</summary>
    public const double TeleportPixels = 96;
    private readonly HashSet<object> treatments = new(ReferenceEqualityComparer.Instance);
    private string? region;
    private double time;
    private double created;
    private double travel;
    private double quiet;
    private double holdUntil;
    private double cooldownUntil;
    private double fadeElapsed;
    private long lastMotionTick = long.MinValue;
    private Vector2? previousMotion;

    /// <summary>The sole temporary opportunity, if any.</summary>
    public MusicPassage? Pending { get; private set; }
    /// <summary>The sole active passage, including its terminal fade.</summary>
    public MusicPassage? Active { get; private set; }
    /// <summary>Whether the active passage is irreversibly yielding to foreground.</summary>
    public bool Fading { get; private set; }
    /// <summary>Transient player amplitude only; user gain and ducking remain on the bus.</summary>
    public double Gain => Active is null ? 0 : Fading ? Math.Max(0, 1 - fadeElapsed / FadeSeconds) : 1;
    /// <summary>Remaining silence; expiry itself produces no new opportunity.</summary>
    public double CooldownRemaining => Math.Max(0, cooldownUntil - time);

    /// <summary>Observes only a known changed regional clip; startup/null return is inert.</summary>
    public void ObserveRegion(string? clip, bool audible)
    {
        if (clip is not ("B01" or "B02" or "B03" or "B04"))
        {
            region = null;
            if (Pending == MusicPassage.Exploration) { Pending = null; }
            return;
        }
        if (region == clip) { return; }
        bool transition = region is not null;
        region = clip;
        if (Pending == MusicPassage.Exploration) { Pending = null; }
        if (transition && CanOffer(audible) && Pending != MusicPassage.Treatment) { Offer(MusicPassage.Exploration); }
    }

    /// <summary>Admits only a fresh exact live binding and reserves its existing action passage.</summary>
    public void ObserveTreatment(object? binding, object? currentBinding, bool audible)
    {
        if (binding is null || !ReferenceEquals(binding, currentBinding) || !treatments.Add(binding)) { return; }
        Foreground(ActionHoldSeconds);
        if (CanOffer(audible)) { Offer(MusicPassage.Treatment); }
    }

    /// <summary>Protects committed action or heard reply independently of output settings.</summary>
    public void Foreground(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) { return; }
        holdUntil = Math.Max(holdUntil, time + seconds);
        quiet = 0;
        if (Active is not null && !Fading) { Fading = true; fadeElapsed = 0; }
    }

    /// <summary>Uses valid post-collision travel; duplicates and impossible travel never admit music.</summary>
    public void ObserveMotion(FootstepMotionSample sample)
    {
        ArgumentNullException.ThrowIfNull(sample);
        if (sample.Tick <= lastMotionTick) { return; }
        lastMotionTick = sample.Tick;
        if (!Finite(sample.Start) || !Finite(sample.End)) { Reset(); return; }
        double distance = Vector2.Distance(sample.Start, sample.End);
        bool teleport = distance > TeleportPixels
            || previousMotion is { } previous && Vector2.Distance(previous, sample.Start) > TeleportPixels;
        previousMotion = sample.End;
        if (teleport) { Reset(); previousMotion = sample.End; return; }
        if (!double.IsFinite(sample.Delta) || sample.Delta <= 0 || sample.Delta > MaximumDeltaSeconds
            || !double.IsFinite(sample.SpeedLimit) || sample.SpeedLimit <= 0
            || distance > sample.SpeedLimit * sample.Delta + 1) { return; }
        if (Pending == MusicPassage.Exploration) { travel = Math.Min(TravelPixels, travel + distance); }
    }

    /// <summary>Advances one bounded presentation interval and returns a fresh start at most once.</summary>
    public MusicPassage? Advance(double seconds, bool blocked, bool audible, bool focused = true)
    {
        if (!double.IsFinite(seconds) || seconds < 0) { return null; }
        if (seconds > MaximumDeltaSeconds || !focused) { Reset(); return null; }
        time += seconds;
        if (!audible)
        {
            Pending = null;
            quiet = 0;
            if (Active is not null) { Finished(); }
            return null;
        }
        if (blocked || time < holdUntil) { Foreground(0); }
        else { quiet = Math.Min(QuietSeconds, quiet + Math.Min(seconds, Math.Max(0, time - holdUntil))); }
        if (Fading)
        {
            fadeElapsed += seconds;
            if (fadeElapsed >= FadeSeconds) { Finished(); }
        }
        if (Pending is not null && time - created >= OpportunitySeconds) { Pending = null; }
        if (Pending is null || Active is not null || CooldownRemaining > 0 || blocked || time < holdUntil
            || quiet + 1e-9 < QuietSeconds || Pending == MusicPassage.Exploration
                && (time - created + 1e-9 < QuietSeconds || travel < TravelPixels)) { return null; }
        Active = Pending;
        Pending = null;
        Fading = false;
        return Active;
    }

    /// <summary>Handles natural finish, explicit stop or failed start without relying on an engine signal.</summary>
    public void Finished()
    {
        Active = Pending = null;
        Fading = false;
        fadeElapsed = quiet = 0;
        cooldownUntil = time + CooldownSeconds;
    }

    /// <summary>Clears transient ownership after load/discontinuity; restored history never creates a cue.</summary>
    public void Reset()
    {
        Finished();
        region = null;
        treatments.Clear();
        travel = 0;
        holdUntil = time;
        previousMotion = null;
        lastMotionTick = long.MinValue;
    }

    private bool CanOffer(bool audible) => audible && Active is null && CooldownRemaining == 0;
    private void Offer(MusicPassage passage) { Pending = passage; created = time; quiet = travel = 0; }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}

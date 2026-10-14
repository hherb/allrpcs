namespace Allrpcs.Application.Input;

/// <summary>Separates a native tap from one voice hold using a monotonic clock.</summary>
[Flags]
public enum TapHoldEffect { None = 0, Tap = 1, BeginVoice = 2, EndVoice = 4 }

/// <summary>Arbitrates one physical press; cancellation requires release before another hold.</summary>
public sealed class TapHoldGesture
{
    /// <summary>Minimum physical hold duration before voice starts.</summary>
    public const double HoldMilliseconds = 300;
    private enum Phase { Idle, Pending, Voice, Fenced }
    private Phase phase;
    private double began;
    /// <summary>Gets whether the original press still awaits a tap/hold decision.</summary>
    public bool Pending => phase == Phase.Pending;
    /// <summary>Gets whether this press has already started voice.</summary>
    public bool Voicing => phase == Phase.Voice;
    /// <summary>Gets whether a press or cancellation fence owns Space releases.</summary>
    public bool Active => phase != Phase.Idle;
    /// <summary>Begins a new physical press, ignoring repeats and fenced presses.</summary>
    public void Press(double milliseconds)
    {
        Validate(milliseconds);
        if (phase == Phase.Idle) { began = milliseconds; phase = Phase.Pending; }
    }
    /// <summary>Starts voice exactly once when a pending press reaches the threshold.</summary>
    public TapHoldEffect Tick(double milliseconds)
    {
        Validate(milliseconds);
        if (phase != Phase.Pending || Elapsed(milliseconds) < HoldMilliseconds) { return TapHoldEffect.None; }
        phase = Phase.Voice; return TapHoldEffect.BeginVoice;
    }
    /// <summary>Returns a native tap or finishes voice; delayed ticks never leak a long-held space.</summary>
    public TapHoldEffect Release(double milliseconds)
    {
        Validate(milliseconds);
        TapHoldEffect result = phase switch
        {
            Phase.Pending => Elapsed(milliseconds) < HoldMilliseconds ? TapHoldEffect.Tap
                : TapHoldEffect.BeginVoice | TapHoldEffect.EndVoice,
            Phase.Voice => TapHoldEffect.EndVoice,
            _ => TapHoldEffect.None
        };
        phase = Phase.Idle; return result;
    }
    /// <summary>Fences an outstanding press after Stop, close, load or application focus loss.</summary>
    public void Cancel() { if (phase != Phase.Idle) { phase = Phase.Fenced; } }
    private double Elapsed(double milliseconds) => milliseconds >= began ? milliseconds - began
        : throw new ArgumentOutOfRangeException(nameof(milliseconds), "Clock moved backwards.");
    private static void Validate(double milliseconds)
    { if (!double.IsFinite(milliseconds) || milliseconds < 0) { throw new ArgumentOutOfRangeException(nameof(milliseconds)); } }
}

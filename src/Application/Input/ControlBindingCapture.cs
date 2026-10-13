namespace Allrpcs.Application.Input;

/// <summary>Finite device-specific capture with initiator release, neutral axes and cancellation.</summary>
public sealed class ControlBindingCapture
{
    /// <summary>Maximum capture time in seconds.</summary>
    public const double TimeoutSeconds = 10;
    /// <summary>Neutral threshold independent of installed deadzones.</summary>
    public const double NeutralThreshold = 0.2;
    /// <summary>Capture crossing independent of installed deadzones.</summary>
    public const double CaptureThreshold = 0.75;
    private readonly HashSet<(int Device, int Axis)> neutral = [];
    private double elapsed;
    private bool armed;
    /// <summary>Whether capture owns incoming events.</summary>
    public bool Active { get; private set; }
    /// <summary>Selected device class.</summary>
    public string DeviceClass { get; private set; } = "";
    /// <summary>Selected/first controller connection, never persisted.</summary>
    public int? Device { get; private set; }
    /// <summary>Captured candidate, independent of subsequent edit validation.</summary>
    public ControlBinding? Candidate { get; private set; }
    /// <summary>Path-free status for presentation.</summary>
    public string Status { get; private set; } = "Ready";
    /// <summary>Begins a fresh capture; caller arms after the initiating press is released.</summary>
    public void Begin(string deviceClass, int? device = null)
    {
        if (deviceClass is not ("Keyboard" or "Mouse" or "Controller")) { throw new ArgumentException("Unknown capture device."); }
        DeviceClass = deviceClass; Device = device; elapsed = 0; armed = false; neutral.Clear();
        Candidate = null; Active = true; Status = "Release the initiating control, then press an input. Escape cancels.";
    }
    /// <summary>Arms only after release; no input is guessed from the initiating held press.</summary>
    public void ArmAfterRelease() { if (Active) { armed = true; Status = "Press an input. Escape cancels."; } }
    /// <summary>Consumes button/key observations; ignores release, echo and other device classes.</summary>
    public ControlBinding? Observe(ControlBinding binding, bool pressed, bool echo = false, int device = 0)
    {
        if (!Active || !armed || !pressed || echo || binding.DeviceClass != DeviceClass || binding.Kind == "joyAxis") { return null; }
        if (binding.Kind == "key" && binding.Code == 4194305) { Cancel("Capture cancelled"); return null; }
        if (DeviceClass == "Controller" && !SelectDevice(device)) { return null; }
        return Complete(binding);
    }
    /// <summary>Requires per-axis neutral followed by a deliberate crossing on the selected controller.</summary>
    public ControlBinding? ObserveAxis(int axis, double value, int device)
    {
        if (!Active || !armed || DeviceClass != "Controller" || !double.IsFinite(value) || Math.Abs(value) > 1
            || axis < 0 || axis > 5 || !SelectDevice(device)) { return null; }
        if (Math.Abs(value) <= NeutralThreshold) { neutral.Add((device, axis)); return null; }
        if (!neutral.Contains((device, axis)) || Math.Abs(value) < CaptureThreshold) { return null; }
        return Complete(new ControlBinding("joyAxis", axis, sign: Math.Sign(value)));
    }
    /// <summary>Advances finite elapsed time without accepting negative/nonfinite time.</summary>
    public void Advance(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) { throw new ArgumentOutOfRangeException(nameof(delta)); }
        if (Active) { elapsed += delta; if (elapsed >= TimeoutSeconds) { Cancel("Capture timed out"); } }
    }
    /// <summary>Cancels only when the selected connection disappears.</summary>
    public void Disconnect(int device) { if (Active && Device == device) { Cancel("Controller disconnected"); } }
    /// <summary>Ends capture for close, focus loss or explicit cancellation.</summary>
    public void Cancel(string reason = "Capture cancelled") { Active = false; Candidate = null; Status = reason; }
    private bool SelectDevice(int device) { Device ??= device; return Device == device; }
    private ControlBinding Complete(ControlBinding binding) { Candidate = binding; Active = false; Status = "Input captured"; return binding; }
}

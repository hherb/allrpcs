using System;
using System.Globalization;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Autoload that ends a smoke launch with exit code 124 when it outlives its frame budget.
/// It frees itself (and is therefore inert in production) unless this is a debug build and
/// <c>ALLRPCS_CHECK_DEADLINE_FRAMES</c> holds a positive integer; scripts/smoke-lib.sh sets it.
/// </summary>
public sealed partial class CheckDeadline : Node
{
    public const string EnvironmentVariable = "ALLRPCS_CHECK_DEADLINE_FRAMES";
    public const int ExitCode = 124;

    // Teardown allocates far less; exceeding it only ends the region early.
    private const long NoGCRegionBytes = 64L * 1024 * 1024;

    private bool armed;
    private int deadlineFrames;
    private int processFrames;

    public override void _Ready()
    {
        if (!OS.IsDebugBuild()
            || !int.TryParse(
                System.Environment.GetEnvironmentVariable(EnvironmentVariable),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out deadlineFrames)
            || deadlineFrames <= 0)
        {
            SetProcess(false);
            QueueFree();
            return;
        }

        ProcessMode = ProcessModeEnum.Always;
        armed = true;
    }

    public override void _ExitTree()
    {
        // An armed autoload leaves the tree only at shutdown, after the scene. A GC during the
        // native teardown that follows can collect engine-resource wrappers before Godot's
        // shutdown disposal reaches them; their late finalizers then hit the debug CRASH_COND in
        // csharp_script.cpp (exit 134, #476). Drain finalizers and hold collections off instead.
        if (armed && GC.TryStartNoGCRegion(NoGCRegionBytes))
        {
            GC.WaitForPendingFinalizers();
        }
    }

    public override void _Process(double delta)
    {
        processFrames++;
        if (processFrames > deadlineFrames)
        {
            GD.PrintErr(string.Create(
                CultureInfo.InvariantCulture,
                $"FAIL: check deadline of {deadlineFrames} frames reached"));
            SetProcess(false);
            GetTree().Quit(ExitCode);
        }
    }
}

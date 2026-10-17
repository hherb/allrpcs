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

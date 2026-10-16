using System;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Smoke harness probe: an <c>async void</c> continuation throws after awaiting a process frame,
/// and the probe then quits with exit code 0, proving that the smoke wrapper also fails on an
/// exception that escapes an awaited continuation rather than <c>_Ready</c> itself.
/// </summary>
public sealed partial class SmokeHarnessAsyncThrowProbe : Node
{
    private const int QuitFrame = 5;

    private int processFrames;

    public override void _Ready()
    {
        ThrowAfterFrame();
    }

    public override void _Process(double delta)
    {
        processFrames++;
        if (processFrames == QuitFrame)
        {
            GetTree().Quit();
        }
    }

    private async void ThrowAfterFrame()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        throw new InvalidOperationException("smoke harness async probe");
    }
}

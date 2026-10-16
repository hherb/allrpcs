using System;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Smoke harness probe: throws an unhandled exception from <c>_Ready</c> and then quits
/// with exit code 0, proving that the smoke wrapper fails on the exception text alone.
/// </summary>
public sealed partial class SmokeHarnessThrowProbe : Node
{
    private const int QuitFrame = 3;

    private int processFrames;

    public override void _Ready()
    {
        throw new InvalidOperationException("smoke harness probe");
    }

    public override void _Process(double delta)
    {
        processFrames++;
        if (processFrames == QuitFrame)
        {
            GetTree().Quit();
        }
    }
}

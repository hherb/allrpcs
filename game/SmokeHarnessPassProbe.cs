using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Smoke harness probe: quits with exit code 0 on its first processed frame.
/// </summary>
public sealed partial class SmokeHarnessPassProbe : Node
{
    public override void _Process(double delta)
    {
        GetTree().Quit();
    }
}

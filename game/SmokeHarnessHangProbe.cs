using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Smoke harness probe: never quits, standing in for a stalled check that only the
/// smoke deadline can end.
/// </summary>
public sealed partial class SmokeHarnessHangProbe : Node
{
}

using Godot;

namespace Allrpcs.Game;

/// <summary>Limits synthetic model and disposable storage overrides to debug builds.</summary>
internal static class DebugTestEnvironment
{
    /// <summary>Reads a test override only when both the assembly and engine are debug builds.</summary>
    internal static string GetEnvironment(string name)
    {
#if DEBUG
        return OS.IsDebugBuild() ? OS.GetEnvironment(name) : string.Empty;
#else
        return string.Empty;
#endif
    }
}

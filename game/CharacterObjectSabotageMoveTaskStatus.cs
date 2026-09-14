namespace Allrpcs.Game;

/// <summary>Defines the closed lifecycle of one scene-owned move-decision task.</summary>
public enum CharacterObjectSabotageMoveTaskStatus
{
    /// <summary>No decision is active and no terminal outcome is retained.</summary>
    Idle = 0,

    /// <summary>One exact decision is active.</summary>
    Running = 1,

    /// <summary>One exact accepted decision was resolved against current scene state.</summary>
    Completed = 2,

    /// <summary>The caller requested cancellation before resolution.</summary>
    Cancelled = 3,

    /// <summary>The decision failed or could not resolve against current scene state.</summary>
    Failed = 4,
}

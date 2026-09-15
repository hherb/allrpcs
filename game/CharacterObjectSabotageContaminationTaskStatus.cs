namespace Allrpcs.Game;

/// <summary>Defines the bounded lifecycle of one scene-owned contamination decision.</summary>
public enum CharacterObjectSabotageContaminationTaskStatus
{
    /// <summary>No decision is active and no terminal outcome is retained.</summary>
    Idle,

    /// <summary>One exact decision is currently being obtained.</summary>
    Running,

    /// <summary>One exact accepted result was installed into current scene state.</summary>
    Completed,

    /// <summary>Caller cancellation ended the active decision without a result.</summary>
    Cancelled,

    /// <summary>The model failed or its result could not resolve against current state.</summary>
    Failed,
}

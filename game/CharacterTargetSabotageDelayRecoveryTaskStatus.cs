namespace Allrpcs.Game;

/// <summary>Defines the bounded lifecycle of one scene-owned delay-recovery decision.</summary>
public enum CharacterTargetSabotageDelayRecoveryTaskStatus
{
    /// <summary>No decision is active and no terminal outcome is retained.</summary>
    Idle,

    /// <summary>One exact delay-recovery decision is currently being obtained.</summary>
    Running,

    /// <summary>One exact accepted result is available on the main thread.</summary>
    Completed,

    /// <summary>Caller cancellation ended the active decision without a result.</summary>
    Cancelled,

    /// <summary>The model failed or returned a decision that the exact request rejected.</summary>
    Failed,
}

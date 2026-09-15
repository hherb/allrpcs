namespace Allrpcs.Game;

/// <summary>Lists the closed lifecycle states of one scene-owned substitution decision task.</summary>
public enum CharacterObjectSabotageSubstitutionTaskStatus
{
    /// <summary>No substitution decision task has started.</summary>
    Idle,

    /// <summary>The exact substitution decision is being obtained.</summary>
    Running,

    /// <summary>The accepted decision result was resolved into exact scene state.</summary>
    Completed,

    /// <summary>The caller cancelled the active decision task.</summary>
    Cancelled,

    /// <summary>The model failed or its accepted result could not resolve exact scene state.</summary>
    Failed,
}

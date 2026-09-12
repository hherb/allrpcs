namespace Allrpcs.Game;

/// <summary>Identifies one bounded scene-owned participation-round task state.</summary>
public enum CharacterTurnParticipationRoundTaskStatus
{
    /// <summary>No participation round has started, or the prior terminal state was reset.</summary>
    Idle,

    /// <summary>One complete participation round is being decided.</summary>
    Running,

    /// <summary>One complete successful participation round is available.</summary>
    Completed,

    /// <summary>The scene caller cancelled the active participation round.</summary>
    Cancelled,

    /// <summary>The shared participation-round deadline elapsed.</summary>
    TimedOut,

    /// <summary>The participation round failed without exposing provider details.</summary>
    Failed,
}

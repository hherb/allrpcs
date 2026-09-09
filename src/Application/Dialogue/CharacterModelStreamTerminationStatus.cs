namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Identifies why a character-model response stream terminated.
/// </summary>
public enum CharacterModelStreamTerminationStatus
{
    /// <summary>The model completed generation normally.</summary>
    Completed,

    /// <summary>The caller cancelled generation.</summary>
    Cancelled,

    /// <summary>The configured generation deadline elapsed.</summary>
    TimedOut,

    /// <summary>Generation failed for a bounded provider-neutral reason.</summary>
    Failed,
}

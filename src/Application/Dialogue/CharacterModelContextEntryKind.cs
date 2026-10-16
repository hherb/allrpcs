namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Identifies the trusted origin and role of one provider-neutral model-context entry.
/// </summary>
public enum CharacterModelContextEntryKind
{
    /// <summary>World state already filtered to the responding character's perception.</summary>
    FilteredPerception,

    /// <summary>Knowledge already selected as relevant and available to the character.</summary>
    RelevantKnowledge,

    /// <summary>One current character goal.</summary>
    Goal,

    /// <summary>One previously retained character memory.</summary>
    Memory,

    /// <summary>One recent in-world spoken statement.</summary>
    RecentSpeech,

    /// <summary>One action currently valid for the character to attempt.</summary>
    ValidAffordance,

    /// <summary>Exact player dialogue treated as untrusted in-world content.</summary>
    UntrustedPlayerDialogue,

    /// <summary>An authoritative completed action observation owned by this character.</summary>
    ActionOutcome,
}

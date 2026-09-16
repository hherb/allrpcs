using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Declares ordinary-interaction, capability, and knowledge access to one character.</summary>
public sealed record CharacterTargetAccessRequirement
{
    /// <summary>Creates one immutable character-targeted access requirement.</summary>
    public CharacterTargetAccessRequirement(
        CharacterId targetCharacterId,
        string requiredCapabilityKey,
        KnowledgeId requiredKnowledgeId)
    {
        ArgumentNullException.ThrowIfNull(targetCharacterId);
        ArgumentNullException.ThrowIfNull(requiredKnowledgeId);

        TargetCharacterId = targetCharacterId;
        RequiredCapabilityKey = CanonicalIdentifierText.RequireCanonical(
            requiredCapabilityKey,
            nameof(requiredCapabilityKey));
        RequiredKnowledgeId = requiredKnowledgeId;
    }

    /// <summary>Gets the exact target character that must be reachable.</summary>
    public CharacterId TargetCharacterId { get; }

    /// <summary>Gets the canonical authored capability key required for the action.</summary>
    public string RequiredCapabilityKey { get; }

    /// <summary>Gets the exact knowledge reference required for informational access.</summary>
    public KnowledgeId RequiredKnowledgeId { get; }
}

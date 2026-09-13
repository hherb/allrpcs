using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Declares explicit physical, capability, and knowledge access required by an action.</summary>
public sealed record CharacterActionAccessRequirement
{
    /// <summary>Creates one immutable targeted action-access requirement.</summary>
    public CharacterActionAccessRequirement(
        ObjectId targetObjectId,
        string requiredCapabilityKey,
        KnowledgeId requiredKnowledgeId)
    {
        ArgumentNullException.ThrowIfNull(targetObjectId);
        ArgumentNullException.ThrowIfNull(requiredKnowledgeId);

        TargetObjectId = targetObjectId;
        RequiredCapabilityKey = CanonicalIdentifierText.RequireCanonical(
            requiredCapabilityKey,
            nameof(requiredCapabilityKey));
        RequiredKnowledgeId = requiredKnowledgeId;
    }

    /// <summary>Gets the exact target object that must be physically accessible.</summary>
    public ObjectId TargetObjectId { get; }

    /// <summary>Gets the canonical authored capability key required for the action.</summary>
    public string RequiredCapabilityKey { get; }

    /// <summary>Gets the exact knowledge reference required for informational access.</summary>
    public KnowledgeId RequiredKnowledgeId { get; }
}

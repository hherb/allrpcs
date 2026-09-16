using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Proposes one access-bound delay or influence action against a character.</summary>
public sealed class CharacterTargetSabotageProposal
{
    /// <summary>Creates one immutable character-targeted sabotage proposal.</summary>
    public CharacterTargetSabotageProposal(
        CharacterId actorId,
        CharacterTargetAccessRequirement accessRequirement,
        CharacterSabotageActionKind actionKind)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(accessRequirement);
        ArgumentNullException.ThrowIfNull(actionKind);

        if (actorId == accessRequirement.TargetCharacterId)
        {
            throw new ArgumentException(
                "Character-targeted sabotage proposals cannot target their actor.",
                nameof(accessRequirement));
        }

        if (!IsCharacterTargeted(actionKind))
        {
            throw new ArgumentException(
                "The sabotage action kind must be delay or influence.",
                nameof(actionKind));
        }

        ActorId = actorId;
        AccessRequirement = accessRequirement;
        ActionKind = actionKind;
    }

    /// <summary>Gets the exact character proposing the action.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact interaction, capability, and knowledge requirement.</summary>
    public CharacterTargetAccessRequirement AccessRequirement { get; }

    /// <summary>Gets the exact closed character-targeted sabotage kind.</summary>
    public CharacterSabotageActionKind ActionKind { get; }

    private static bool IsCharacterTargeted(CharacterSabotageActionKind actionKind) =>
        actionKind == CharacterSabotageActionKind.Delay
        || actionKind == CharacterSabotageActionKind.Influence;
}

using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Proposes one access-bound sabotage action against a world object.</summary>
public sealed record CharacterObjectSabotageProposal
{
    /// <summary>Creates one immutable object-targeted sabotage proposal.</summary>
    public CharacterObjectSabotageProposal(
        CharacterId actorId,
        CharacterActionAccessRequirement accessRequirement,
        CharacterSabotageActionKind actionKind)
    {
        ArgumentNullException.ThrowIfNull(actorId);
        ArgumentNullException.ThrowIfNull(accessRequirement);
        ArgumentNullException.ThrowIfNull(actionKind);

        if (!IsObjectTargeted(actionKind))
        {
            throw new ArgumentException(
                "The sabotage action kind must target a world object.",
                nameof(actionKind));
        }

        ActorId = actorId;
        AccessRequirement = accessRequirement;
        ActionKind = actionKind;
    }

    /// <summary>Gets the exact character proposing the action.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact physical, capability, and knowledge requirement.</summary>
    public CharacterActionAccessRequirement AccessRequirement { get; }

    /// <summary>Gets the exact closed object-targeted sabotage kind.</summary>
    public CharacterSabotageActionKind ActionKind { get; }

    private static bool IsObjectTargeted(CharacterSabotageActionKind actionKind) =>
        actionKind == CharacterSabotageActionKind.Conceal
        || actionKind == CharacterSabotageActionKind.Move
        || actionKind == CharacterSabotageActionKind.Substitute
        || actionKind == CharacterSabotageActionKind.Contaminate;
}

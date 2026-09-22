using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral sabotage-kind selection or abstention.</summary>
public sealed record CharacterSabotageActionDecision
{
    /// <summary>Creates one explicit action-kind selection or null-kind abstention.</summary>
    public CharacterSabotageActionDecision(
        CharacterId actorId,
        CharacterSabotageActionKind? selectedActionKind)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        ActorId = actorId;
        SelectedActionKind = selectedActionKind;
    }

    /// <summary>Gets the exact deciding actor identifier.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact selected action kind, or null to abstain.</summary>
    public CharacterSabotageActionKind? SelectedActionKind { get; }
}

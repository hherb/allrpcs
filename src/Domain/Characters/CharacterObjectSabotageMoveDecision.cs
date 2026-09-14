using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral decision to select move evidence or abstain.</summary>
public sealed record CharacterObjectSabotageMoveDecision
{
    /// <summary>Creates one explicit selection or null-evidence abstention.</summary>
    public CharacterObjectSabotageMoveDecision(
        CharacterId characterId,
        EvidenceId? selectedEvidenceId)
    {
        ArgumentNullException.ThrowIfNull(characterId);

        CharacterId = characterId;
        SelectedEvidenceId = selectedEvidenceId;
    }

    /// <summary>Gets the exact deciding character identifier.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact selected move-candidate evidence identifier, or null to abstain.</summary>
    public EvidenceId? SelectedEvidenceId { get; }
}

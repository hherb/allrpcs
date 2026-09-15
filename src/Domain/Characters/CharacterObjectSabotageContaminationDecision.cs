using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral decision to select contamination evidence or abstain.</summary>
public sealed record CharacterObjectSabotageContaminationDecision
{
    /// <summary>Creates one explicit selection or null-evidence abstention.</summary>
    public CharacterObjectSabotageContaminationDecision(
        CharacterId characterId,
        EvidenceId? selectedEvidenceId)
    {
        ArgumentNullException.ThrowIfNull(characterId);

        CharacterId = characterId;
        SelectedEvidenceId = selectedEvidenceId;
    }

    /// <summary>Gets the exact deciding character identifier.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact selected candidate evidence identifier, or null to abstain.</summary>
    public EvidenceId? SelectedEvidenceId { get; }
}

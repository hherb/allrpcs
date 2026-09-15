using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral decision to select substitution evidence or abstain.</summary>
public sealed record CharacterObjectSabotageSubstitutionDecision
{
    /// <summary>Creates one explicit selection or null-evidence abstention.</summary>
    public CharacterObjectSabotageSubstitutionDecision(
        CharacterId characterId,
        EvidenceId? selectedEvidenceId)
    {
        ArgumentNullException.ThrowIfNull(characterId);

        CharacterId = characterId;
        SelectedEvidenceId = selectedEvidenceId;
    }

    /// <summary>Gets the exact deciding character identifier.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact selected substitution-candidate evidence identifier, or null.</summary>
    public EvidenceId? SelectedEvidenceId { get; }
}

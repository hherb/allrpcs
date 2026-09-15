using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one provider-neutral exact evidence selection or abstention.</summary>
public sealed record CharacterDialogueMisquotationEvidenceDecision
{
    /// <summary>Creates one explicit selection or null-evidence abstention.</summary>
    public CharacterDialogueMisquotationEvidenceDecision(
        CharacterId actorId,
        CharacterDialogueMisquotationEvidence? selectedEvidence)
    {
        ArgumentNullException.ThrowIfNull(actorId);

        ActorId = actorId;
        SelectedEvidence = selectedEvidence;
    }

    /// <summary>Gets the exact deciding actor identifier.</summary>
    public CharacterId ActorId { get; }

    /// <summary>Gets the exact selected evidence candidate, or null to abstain.</summary>
    public CharacterDialogueMisquotationEvidence? SelectedEvidence { get; }
}

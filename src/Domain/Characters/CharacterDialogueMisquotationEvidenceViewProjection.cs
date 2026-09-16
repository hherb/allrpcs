using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Projects exposed misquotation evidence visible to one exact witness.</summary>
public static class CharacterDialogueMisquotationEvidenceViewProjection
{
    /// <summary>Returns exact witness-owned entries in stable exposure order.</summary>
    public static CharacterDialogueMisquotationEvidenceViewSnapshot ForWitness(
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure,
        CharacterId witnessCharacterId)
    {
        ArgumentNullException.ThrowIfNull(exposure);
        ArgumentNullException.ThrowIfNull(witnessCharacterId);

        return new CharacterDialogueMisquotationEvidenceViewSnapshot(
            witnessCharacterId,
            exposure.Entries.Where(evidence =>
                ReferenceEquals(evidence.WitnessCharacterId, witnessCharacterId)));
    }
}

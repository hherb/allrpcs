using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one exact non-actor witness who heard both sides of a misquotation.</summary>
public sealed class CharacterDialogueMisquotationEvidence
{
    /// <summary>Creates immutable witness evidence without changing either dialogue event.</summary>
    public CharacterDialogueMisquotationEvidence(
        CharacterDialogueMisquotationOccurrence occurrence,
        CharacterId witnessCharacterId)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(witnessCharacterId);

        Occurrence = occurrence;
        WitnessCharacterId = witnessCharacterId;
    }

    /// <summary>Gets the exact misquotation occurrence observed by the witness.</summary>
    public CharacterDialogueMisquotationOccurrence Occurrence { get; }

    /// <summary>Gets the exact identifier of the candidate witness.</summary>
    public CharacterId WitnessCharacterId { get; }
}

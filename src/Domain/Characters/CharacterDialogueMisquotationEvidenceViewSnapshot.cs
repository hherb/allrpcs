using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded exact exposed misquotation evidence visible to one exact witness.</summary>
public sealed class CharacterDialogueMisquotationEvidenceViewSnapshot
{
    /// <summary>Gets the maximum number of evidence entries retained in one view.</summary>
    public const int MaximumEvidenceCount =
        CharacterDialogueMisquotationEvidenceExposureSnapshot.MaximumEvidenceCount;

    /// <summary>Creates an immutable stable copy of exact witness-visible evidence.</summary>
    public CharacterDialogueMisquotationEvidenceViewSnapshot(
        CharacterId witnessCharacterId,
        IEnumerable<CharacterDialogueMisquotationEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(witnessCharacterId);
        ArgumentNullException.ThrowIfNull(evidence);

        List<CharacterDialogueMisquotationEvidence> retainedEvidence = [];
        HashSet<DialogueEvent> deliveredEvents = new(ReferenceEqualityComparer.Instance);
        foreach (CharacterDialogueMisquotationEvidence? entry in evidence)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "A misquotation evidence view cannot contain null entries.",
                    nameof(evidence));
            }

            if (!ReferenceEquals(entry.WitnessCharacterId, witnessCharacterId))
            {
                throw new ArgumentException(
                    "A misquotation evidence view can contain only its exact witness's evidence.",
                    nameof(evidence));
            }

            if (!deliveredEvents.Add(entry.Occurrence.DeliveredEvent))
            {
                throw new ArgumentException(
                    "A misquotation evidence view cannot duplicate an exact delivered event.",
                    nameof(evidence));
            }

            retainedEvidence.Add(entry);
            if (retainedEvidence.Count > MaximumEvidenceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(evidence),
                    retainedEvidence.Count,
                    $"A misquotation evidence view accepts at most {MaximumEvidenceCount} entries.");
            }
        }

        WitnessCharacterId = witnessCharacterId;
        Entries = retainedEvidence.AsReadOnly();
    }

    /// <summary>Gets the exact witness whose identity produced this view.</summary>
    public CharacterId WitnessCharacterId { get; }

    /// <summary>Gets exact visible evidence in stable exposure order.</summary>
    public IReadOnlyList<CharacterDialogueMisquotationEvidence> Entries { get; }
}

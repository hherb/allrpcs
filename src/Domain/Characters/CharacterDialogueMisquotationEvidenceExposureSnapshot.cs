using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded exact exposed misquotation evidence in stable order.</summary>
public sealed class CharacterDialogueMisquotationEvidenceExposureSnapshot
{
    /// <summary>Gets the maximum number of evidence entries retained in one snapshot.</summary>
    public const int MaximumEvidenceCount = 64;

    /// <summary>Creates an immutable stable copy of exact exposed evidence.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot(
        IEnumerable<CharacterDialogueMisquotationEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        List<CharacterDialogueMisquotationEvidence> retainedEvidence = [];
        HashSet<DialogueEvent> deliveredEvents = new(ReferenceEqualityComparer.Instance);
        foreach (CharacterDialogueMisquotationEvidence? entry in evidence)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Exposed misquotation evidence cannot contain null entries.",
                    nameof(evidence));
            }

            if (!deliveredEvents.Add(entry.Occurrence.DeliveredEvent))
            {
                throw new ArgumentException(
                    "Exposed misquotation evidence cannot duplicate an exact delivered event.",
                    nameof(evidence));
            }

            retainedEvidence.Add(entry);
            if (retainedEvidence.Count > MaximumEvidenceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(evidence),
                    retainedEvidence.Count,
                    $"Exposure state accepts at most {MaximumEvidenceCount} evidence entries.");
            }
        }

        Entries = retainedEvidence.AsReadOnly();
    }

    /// <summary>Gets exact exposed evidence in stable supplied order.</summary>
    public IReadOnlyList<CharacterDialogueMisquotationEvidence> Entries { get; }
}

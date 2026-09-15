using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact observable misquotation-evidence candidate without changing it.</summary>
public static class CharacterDialogueMisquotationEvidenceSelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first observable candidate in stable caller order, or null.</summary>
    public static CharacterDialogueMisquotationEvidence? SelectFirstObservable(
        Character actor,
        IEnumerable<CharacterDialogueMisquotationEvidence> candidates)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterDialogueMisquotationEvidence> retainedCandidates = [];
        HashSet<DialogueEvent> deliveredEvents = new(ReferenceEqualityComparer.Instance);

        foreach (CharacterDialogueMisquotationEvidence? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Misquotation-evidence candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!deliveredEvents.Add(candidate.Occurrence.DeliveredEvent))
            {
                throw new ArgumentException(
                    "Misquotation-evidence candidates cannot duplicate an exact delivered event.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Misquotation-evidence selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, candidate));
    }
}

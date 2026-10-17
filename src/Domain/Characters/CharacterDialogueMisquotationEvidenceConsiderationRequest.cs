using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded candidates for one provider-neutral evidence choice.</summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterDialogueMisquotationEvidenceConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or exposing evidence.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationRequest(
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
                    "Misquotation-evidence consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!deliveredEvents.Add(candidate.Occurrence.DeliveredEvent))
            {
                throw new ArgumentException(
                    "Misquotation-evidence consideration candidates cannot duplicate an exact delivered event.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Misquotation-evidence consideration exceeded the bounded candidate count.");
            }
        }

        Actor = actor;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering whether to expose evidence.</summary>
    public Character Actor { get; }

    /// <summary>Gets the stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterDialogueMisquotationEvidence> Candidates { get; }
}

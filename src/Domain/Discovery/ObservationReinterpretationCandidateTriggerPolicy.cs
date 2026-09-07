using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one observation-reinterpretation candidate's knowledge trigger is satisfied
/// by one complete character without changing either input.
/// </summary>
public static class ObservationReinterpretationCandidateTriggerPolicy
{
    /// <summary>
    /// Returns true when the character is the candidate observer and references its trigger knowledge.
    /// </summary>
    public static bool CanTrigger(
        ObservationReinterpretationCandidate candidate,
        Character character)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(character);

        return character.Identity.Id == candidate.ObserverCharacterId
            && character.Knowledge.KnowledgeReferences.Contains(candidate.TriggerKnowledgeId);
    }
}

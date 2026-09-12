using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Selects the first speaking choice from scheduled, already-accepted participation decisions.
/// </summary>
public static class CharacterTurnParticipationSelectionPolicy
{
    /// <summary>
    /// Returns the exact first response or interjection, or no choice when everyone is silent.
    /// </summary>
    public static CharacterTurnParticipationDecision? SelectNext(
        IEnumerable<CharacterTurnParticipationDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(decisions);

        HashSet<CharacterId> scheduledCharacterIds = [];
        CharacterTurnParticipationDecision? firstSpeakingDecision = null;
        int decisionCount = 0;

        foreach (CharacterTurnParticipationDecision decision in decisions)
        {
            if (decision is null)
            {
                throw new ArgumentException(
                    "Participation decisions cannot contain null values.",
                    nameof(decisions));
            }

            if (!scheduledCharacterIds.Add(decision.ScheduledCharacterId))
            {
                throw new ArgumentException(
                    "Participation decisions require distinct scheduled character identifiers.",
                    nameof(decisions));
            }

            decisionCount++;
            if (decisionCount > DialogueAudienceSnapshot.MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decisions),
                    decisionCount,
                    "Participation selection accepts at most "
                        + $"{DialogueAudienceSnapshot.MaximumListenerCount} decisions.");
            }

            if (firstSpeakingDecision is null
                && decision.Kind != CharacterTurnParticipationKind.RemainSilent)
            {
                firstSpeakingDecision = decision;
            }
        }

        return firstSpeakingDecision;
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Validates one complete scheduled participation round before selecting its first speaker.
/// </summary>
public static class CharacterTurnParticipationRoundPolicy
{
    /// <summary>
    /// Returns the exact first accepted speaking decision or no selection when all remain silent.
    /// </summary>
    public static CharacterTurnParticipationDecision? SelectNext(
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterTurnParticipationDecision> decisions,
        IEnumerable<CharacterId> directlyAddressedCharacterIds)
    {
        List<Character> retainedCharacters = RetainCharacters(scheduledCharacters);
        List<CharacterTurnParticipationDecision> retainedDecisions = RetainDecisions(decisions);
        HashSet<CharacterId> addressedCharacterIds = RetainAddresses(
            directlyAddressedCharacterIds);

        if (retainedCharacters.Count != retainedDecisions.Count)
        {
            throw new ArgumentException(
                "Every scheduled character requires one positionally aligned decision.",
                nameof(decisions));
        }

        for (int index = 0; index < retainedCharacters.Count; index++)
        {
            Character character = retainedCharacters[index];
            CharacterTurnParticipationDecision decision = retainedDecisions[index];
            bool isDirectlyAddressed = addressedCharacterIds.Contains(character.Identity.Id);

            if (!CharacterTurnParticipationDecisionPolicy.CanAccept(
                    decision,
                    character,
                    isDirectlyAddressed))
            {
                throw new ArgumentException(
                    "Every participation decision must be valid for its scheduled character.",
                    nameof(decisions));
            }
        }

        return CharacterTurnParticipationSelectionPolicy.SelectNext(retainedDecisions);
    }

    private static List<Character> RetainCharacters(IEnumerable<Character> scheduledCharacters)
    {
        ArgumentNullException.ThrowIfNull(scheduledCharacters);

        List<Character> retained = [];
        HashSet<CharacterId> characterIds = [];

        foreach (Character character in scheduledCharacters)
        {
            if (character is null)
            {
                throw new ArgumentException(
                    "Scheduled characters cannot contain null values.",
                    nameof(scheduledCharacters));
            }

            if (!characterIds.Add(character.Identity.Id))
            {
                throw new ArgumentException(
                    "Scheduled characters require distinct identifiers.",
                    nameof(scheduledCharacters));
            }

            retained.Add(character);
            EnforceMaximum(retained.Count, nameof(scheduledCharacters));
        }

        return retained;
    }

    private static List<CharacterTurnParticipationDecision> RetainDecisions(
        IEnumerable<CharacterTurnParticipationDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(decisions);

        List<CharacterTurnParticipationDecision> retained = [];
        HashSet<CharacterId> characterIds = [];

        foreach (CharacterTurnParticipationDecision decision in decisions)
        {
            if (decision is null)
            {
                throw new ArgumentException(
                    "Participation decisions cannot contain null values.",
                    nameof(decisions));
            }

            if (!characterIds.Add(decision.ScheduledCharacterId))
            {
                throw new ArgumentException(
                    "Participation decisions require distinct scheduled character identifiers.",
                    nameof(decisions));
            }

            retained.Add(decision);
            EnforceMaximum(retained.Count, nameof(decisions));
        }

        return retained;
    }

    private static HashSet<CharacterId> RetainAddresses(
        IEnumerable<CharacterId> directlyAddressedCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterIds);

        HashSet<CharacterId> retained = [];
        int addressCount = 0;

        foreach (CharacterId characterId in directlyAddressedCharacterIds)
        {
            if (characterId is null)
            {
                throw new ArgumentException(
                    "Directly addressed character identifiers cannot contain null values.",
                    nameof(directlyAddressedCharacterIds));
            }

            if (!retained.Add(characterId))
            {
                throw new ArgumentException(
                    "Directly addressed character identifiers must be distinct.",
                    nameof(directlyAddressedCharacterIds));
            }

            addressCount++;
            EnforceMaximum(addressCount, nameof(directlyAddressedCharacterIds));
        }

        return retained;
    }

    private static void EnforceMaximum(int count, string parameterName)
    {
        if (count > DialogueAudienceSnapshot.MaximumListenerCount)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                count,
                "Participation rounds accept at most "
                    + $"{DialogueAudienceSnapshot.MaximumListenerCount} entries per input.");
        }
    }
}

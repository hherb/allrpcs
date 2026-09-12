using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Creates one stable bounded round of exact participation-consideration requests.
/// </summary>
public static class CharacterTurnParticipationConsiderationRoundTransition
{
    /// <summary>
    /// Preflights all scheduled listeners, then creates one request per character in supplied order.
    /// </summary>
    public static IReadOnlyList<CharacterTurnParticipationConsiderationRequest> CreateRequests(
        IEnumerable<Character> scheduledCharacters,
        DialogueEvent heardDialogueEvent,
        IEnumerable<CharacterId> directlyAddressedCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(scheduledCharacters);
        ArgumentNullException.ThrowIfNull(heardDialogueEvent);
        ArgumentNullException.ThrowIfNull(directlyAddressedCharacterIds);

        List<Character> characters = RetainCharacters(scheduledCharacters);
        List<CharacterId> directAddresses = RetainDirectAddresses(directlyAddressedCharacterIds);

        foreach (Character character in characters)
        {
            if (!heardDialogueEvent.Audience.ListenerCharacterIds.Contains(character.Identity.Id))
            {
                throw new ArgumentException(
                    "Every scheduled character must be a listener in the heard dialogue audience.",
                    nameof(scheduledCharacters));
            }
        }

        List<CharacterTurnParticipationConsiderationRequest> requests = characters
            .Select(character => new CharacterTurnParticipationConsiderationRequest(
                character,
                heardDialogueEvent,
                directAddresses.Contains(character.Identity.Id)))
            .ToList();

        return requests.AsReadOnly();
    }

    private static List<Character> RetainCharacters(IEnumerable<Character> scheduledCharacters)
    {
        List<Character> characters = new();
        HashSet<CharacterId> identifiers = new();
        foreach (Character character in scheduledCharacters)
        {
            if (character is null)
            {
                throw new ArgumentException(
                    "Scheduled characters cannot contain null values.",
                    nameof(scheduledCharacters));
            }

            if (!identifiers.Add(character.Identity.Id))
            {
                throw new ArgumentException(
                    "Scheduled characters must have distinct identifiers.",
                    nameof(scheduledCharacters));
            }

            characters.Add(character);
            if (characters.Count > DialogueAudienceSnapshot.MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scheduledCharacters),
                    characters.Count,
                    $"Participation rounds accept at most "
                        + $"{DialogueAudienceSnapshot.MaximumListenerCount} scheduled characters.");
            }
        }

        return characters;
    }

    private static List<CharacterId> RetainDirectAddresses(
        IEnumerable<CharacterId> directlyAddressedCharacterIds)
    {
        List<CharacterId> directAddresses = new();
        HashSet<CharacterId> identifiers = new();
        foreach (CharacterId identifier in directlyAddressedCharacterIds)
        {
            if (identifier is null)
            {
                throw new ArgumentException(
                    "Direct-address identifiers cannot contain null values.",
                    nameof(directlyAddressedCharacterIds));
            }

            if (!identifiers.Add(identifier))
            {
                throw new ArgumentException(
                    "Direct-address identifiers must be distinct.",
                    nameof(directlyAddressedCharacterIds));
            }

            directAddresses.Add(identifier);
            if (directAddresses.Count > DialogueAudienceSnapshot.MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(directlyAddressedCharacterIds),
                    directAddresses.Count,
                    $"Participation rounds accept at most "
                        + $"{DialogueAudienceSnapshot.MaximumListenerCount} direct addresses.");
            }
        }

        return directAddresses;
    }
}

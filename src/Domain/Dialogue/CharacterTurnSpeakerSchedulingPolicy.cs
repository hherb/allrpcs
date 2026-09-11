using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>Orders eligible dialogue speakers with direct addresses first.</summary>
public static class CharacterTurnSpeakerSchedulingPolicy
{
    /// <summary>
    /// Returns exact eligible identifiers in stable priority partitions without starting turns.
    /// </summary>
    public static IReadOnlyList<CharacterId> Order(
        IEnumerable<CharacterId> eligibleSpeakerIds,
        IEnumerable<CharacterId> directlyAddressedSpeakerIds)
    {
        List<CharacterId> eligible = RetainDistinctBounded(
            eligibleSpeakerIds,
            nameof(eligibleSpeakerIds));
        List<CharacterId> addressed = RetainDistinctBounded(
            directlyAddressedSpeakerIds,
            nameof(directlyAddressedSpeakerIds));
        HashSet<CharacterId> addressedIds = new(addressed);
        List<CharacterId> ordered = new(eligible.Count);

        foreach (CharacterId candidate in eligible)
        {
            if (addressedIds.Contains(candidate))
            {
                ordered.Add(candidate);
            }
        }

        foreach (CharacterId candidate in eligible)
        {
            if (!addressedIds.Contains(candidate))
            {
                ordered.Add(candidate);
            }
        }

        return ordered.AsReadOnly();
    }

    private static List<CharacterId> RetainDistinctBounded(
        IEnumerable<CharacterId> characterIds,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(characterIds, parameterName);

        List<CharacterId> retained = [];
        HashSet<CharacterId> identifiers = [];

        foreach (CharacterId characterId in characterIds)
        {
            if (characterId is null)
            {
                throw new ArgumentException(
                    "Speaker identifiers cannot contain null values.",
                    parameterName);
            }

            if (!identifiers.Add(characterId))
            {
                throw new ArgumentException(
                    "Speaker identifiers cannot contain duplicates.",
                    parameterName);
            }

            retained.Add(characterId);

            if (retained.Count > DialogueAudienceSnapshot.MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    retained.Count,
                    $"Speaker scheduling accepts at most "
                    + $"{DialogueAudienceSnapshot.MaximumListenerCount} identifiers per input.");
            }
        }

        return retained;
    }
}

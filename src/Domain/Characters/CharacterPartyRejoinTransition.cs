using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Appends one absent character to immutable bounded party membership.</summary>
public static class CharacterPartyRejoinTransition
{
    /// <summary>Returns a party retaining stable members followed by the exact rejoining character.</summary>
    public static CharacterPartySnapshot Rejoin(
        CharacterId characterId,
        CharacterPartySnapshot party)
    {
        if (!CharacterPartyRejoinPolicy.CanRejoin(characterId, party))
        {
            throw new InvalidOperationException(
                "Only an absent character can rejoin a party with capacity.");
        }

        return new CharacterPartySnapshot(party.MemberCharacterIds.Append(characterId));
    }
}

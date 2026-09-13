using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Removes one existing character from immutable party membership.</summary>
public static class CharacterPartySeparationTransition
{
    /// <summary>Returns a party without the value-equal member and without changing input order.</summary>
    public static CharacterPartySnapshot Separate(
        CharacterId characterId,
        CharacterPartySnapshot party)
    {
        if (!CharacterPartySeparationPolicy.CanSeparate(characterId, party))
        {
            throw new InvalidOperationException(
                "Only a current party member can be separated.");
        }

        return new CharacterPartySnapshot(
            party.MemberCharacterIds.Where(member => member != characterId));
    }
}

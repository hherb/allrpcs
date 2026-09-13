using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether one character can leave the current player party.</summary>
public static class CharacterPartySeparationPolicy
{
    /// <summary>Returns true when the value-equal character is currently a party member.</summary>
    public static bool CanSeparate(CharacterId characterId, CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(party);

        return party.MemberCharacterIds.Contains(characterId);
    }
}

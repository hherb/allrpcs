using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether one character can join the current bounded player party.</summary>
public static class CharacterPartyRejoinPolicy
{
    /// <summary>Returns true when the character is absent and the party has capacity.</summary>
    public static bool CanRejoin(CharacterId characterId, CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(party);

        return party.MemberCharacterIds.Count < CharacterPartySnapshot.MaximumMemberCount
            && !party.MemberCharacterIds.Contains(characterId);
    }
}

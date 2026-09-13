using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains the bounded stable set of characters currently in the player party.</summary>
public sealed class CharacterPartySnapshot
{
    /// <summary>Gets the maximum retained party-member count.</summary>
    public const int MaximumMemberCount = 32;

    /// <summary>Creates one immutable party snapshot in supplied order.</summary>
    public CharacterPartySnapshot(IEnumerable<CharacterId> memberCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(memberCharacterIds);

        List<CharacterId> retainedMembers = new();
        HashSet<CharacterId> identifiers = new();
        foreach (CharacterId memberCharacterId in memberCharacterIds)
        {
            if (memberCharacterId is null)
            {
                throw new ArgumentException(
                    "Party-member identifiers cannot contain null values.",
                    nameof(memberCharacterIds));
            }

            if (!identifiers.Add(memberCharacterId))
            {
                throw new ArgumentException(
                    "Party-member identifiers must be unique.",
                    nameof(memberCharacterIds));
            }

            retainedMembers.Add(memberCharacterId);
            if (retainedMembers.Count > MaximumMemberCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(memberCharacterIds),
                    retainedMembers.Count,
                    $"A party accepts at most {MaximumMemberCount} members.");
            }
        }

        MemberCharacterIds = retainedMembers.AsReadOnly();
    }

    /// <summary>Gets exact member identifiers in stable supplied order.</summary>
    public IReadOnlyList<CharacterId> MemberCharacterIds { get; }
}

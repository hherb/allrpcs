using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Projects exact independent-task party presence onto one scene body.</summary>
public partial class CharacterIndependentTaskBodyPresentationController : Node
{
    private CanvasItem body = null!;

    /// <summary>Gets the exact character identifier represented by the body, if initialized.</summary>
    public CharacterId? CharacterId { get; private set; }

    /// <summary>Gets the exact most recently presented party snapshot, if initialized.</summary>
    public CharacterPartySnapshot? LastPresentedParty { get; private set; }

    /// <summary>Initializes one exact body and immediately presents its party presence once.</summary>
    public bool TryInitialize(
        CharacterId characterId,
        CanvasItem characterBody,
        CharacterPartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(characterBody);
        ArgumentNullException.ThrowIfNull(party);

        if (CharacterId is not null || LastPresentedParty is not null)
        {
            return false;
        }

        CharacterId = characterId;
        body = characterBody;
        body.Visible = ContainsCharacter(party, characterId);
        LastPresentedParty = party;
        return true;
    }

    /// <summary>Projects one fresh exact party snapshot only from the last presented snapshot.</summary>
    public bool TryPresent(
        CharacterPartySnapshot expectedParty,
        CharacterPartySnapshot currentParty)
    {
        ArgumentNullException.ThrowIfNull(expectedParty);
        ArgumentNullException.ThrowIfNull(currentParty);

        if (CharacterId is not { } characterId
            || !ReferenceEquals(LastPresentedParty, expectedParty)
            || ReferenceEquals(expectedParty, currentParty))
        {
            return false;
        }

        body.Visible = ContainsCharacter(currentParty, characterId);
        LastPresentedParty = currentParty;
        return true;
    }

    private static bool ContainsCharacter(
        CharacterPartySnapshot party,
        CharacterId characterId) =>
        party.MemberCharacterIds.Any(memberCharacterId => memberCharacterId == characterId);
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors one temporary complete character for the existing scene Seeker.
/// </summary>
public partial class TemporarySeekerCharacterSource : Node
{
    private const string SeekerDisplayName = "SEEKER";

    /// <summary>Gets the single retained temporary Seeker character.</summary>
    public Character Character { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterId seekerCharacterId = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").SearcherCharacterId;

        Character = CreateCharacter(seekerCharacterId);
    }

    private static Character CreateCharacter(CharacterId characterId)
    {
        ArgumentNullException.ThrowIfNull(characterId);

        return new Character(
            new CharacterIdentity(characterId, SeekerDisplayName),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }
}

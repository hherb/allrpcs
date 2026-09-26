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
    private const string TriggerKnowledgeIdText = "stone-marker-meaning";

    /// <summary>Gets the single retained temporary Seeker character.</summary>
    public Character Character { get; private set; } = null!;

    /// <summary>Gets the exact installed customization application, or null before installation.</summary>
    public SeekerCustomizedCharacterApplication? CustomizationApplication { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterId seekerCharacterId = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").SearcherCharacterId;

        Character = CreateCharacter(seekerCharacterId);
    }

    /// <summary>Atomically installs one application bound to the exact current character.</summary>
    public bool TryApplyCustomization(SeekerCustomizedCharacterApplication? application)
    {
        if (CustomizationApplication is not null
            || application is null
            || !ReferenceEquals(application.CurrentCharacter, Character)
            || ReferenceEquals(application.CustomizedCharacter, Character))
        {
            return false;
        }

        CustomizationApplication = application;
        Character = application.CustomizedCharacter;
        return true;
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
            new CharacterKnowledge([new KnowledgeId(TriggerKnowledgeIdText)]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }
}

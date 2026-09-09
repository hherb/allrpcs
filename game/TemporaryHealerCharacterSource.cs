using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one stable complete temporary LLM-controlled healer character.</summary>
public partial class TemporaryHealerCharacterSource : Node
{
    private const string HealerDisplayName = "HEALER";
    private const string PublicBackgroundText =
        "A village healer who assists the Seeker with cautious practical care.";
    private const string CapabilityKey = "examine-local-remedies";
    private const string CapabilityText =
        "Can examine visible local remedies before deciding whether they are safe to collect.";
    private const string LimitationKey = "requires-observed-evidence";
    private const string LimitationText =
        "Cannot claim facts about objects that are absent from current filtered perception.";
    private const string KnowledgeIdText = "healer-herb-care";
    private const string GoalKey = "help-without-hidden-claims";
    private const string GoalText = "Help the Seeker without claiming unobserved facts.";

    /// <summary>Gets the single retained temporary healer character.</summary>
    public Character Character { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterId healerId = GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource").Activity.Request.RespondingCharacterId;

        Character = new Character(
            new CharacterIdentity(healerId, HealerDisplayName),
            new PublicCharacterBackground(PublicBackgroundText),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([new CharacterCapability(CapabilityKey, CapabilityText)]),
            new CharacterLimitationSet([new CharacterLimitation(LimitationKey, LimitationText)]),
            new CharacterControllerAssignment(healerId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([new KnowledgeId(KnowledgeIdText)]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal(GoalKey, GoalText)]));
    }
}

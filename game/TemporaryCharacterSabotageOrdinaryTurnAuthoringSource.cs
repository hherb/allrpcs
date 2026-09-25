using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns exact current target-action inputs for ordinary-turn sabotage authoring.</summary>
public partial class TemporaryCharacterSabotageOrdinaryTurnAuthoringSource : Node
{
    private const string RequiredCapabilityKey = "assess-local-influence";

    /// <summary>Gets the exact enemy actor shared with the ordinary authoring boundary.</summary>
    public Character Actor { get; private set; } = null!;

    /// <summary>Gets the exact current target-delay input.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest DelayRequest { get; private set; } = null!;

    /// <summary>Gets the exact current target-influence input.</summary>
    public CharacterTargetSabotageInfluenceConsiderationRequest InfluenceRequest
    {
        get;
        private set;
    } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Actor = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource").Character;
        Character target = RepresentativeSageCharacterSource.Create();
        DelayRequest = CreateDelayRequest(Actor, target);
        InfluenceRequest = CreateInfluenceRequest(Actor, target);
    }

    private static CharacterTargetSabotageDelayConsiderationRequest CreateDelayRequest(
        Character actor,
        Character target)
    {
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent worldEvent = new(
            new WorldEventId("ordinary-turn-enemy-delay"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        return new CharacterTargetSabotageDelayConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            [candidate]);
    }

    private static CharacterTargetSabotageInfluenceConsiderationRequest CreateInfluenceRequest(
        Character actor,
        Character target)
    {
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    RequiredCapabilityKey,
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            target);
        CharacterTargetSabotageInfluenceCandidate candidate = new(
            occurrence,
            new CharacterRelationshipChange(
                target.Identity.Id,
                actor.Identity.Id,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId("ordinary-turn-influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("ordinary-turn-influence-attempt"),
                    "The enemy agent attempted to gain influence.")));
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        return new CharacterTargetSabotageInfluenceConsiderationRequest(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            relationship,
            [candidate]);
    }
}

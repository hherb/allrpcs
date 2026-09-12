using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Authors the pure representative enemy-agent role without sabotage mechanics.</summary>
public static class RepresentativeEnemyAgentCharacterSource
{
    /// <summary>Creates one fresh LLM-controlled agent with a private conflicted objective.</summary>
    public static Character Create()
    {
        CharacterId agentId = new("enemy-agent");

        return new Character(
            new CharacterIdentity(agentId, "VALE"),
            new PublicCharacterBackground(
                "A displaced courier who offers local news while seeking safe passage."),
            new PrivateCharacterBackground(
                "Vale serves the southern crown and was ordered to prolong the outbreak until "
                + "the rival village accepts political concessions."),
            new CharacterCapabilitySet(
                [
                    new CharacterCapability(
                        "maintain-cover",
                        "Can present an ordinary courier identity while withholding private orders."),
                    new CharacterCapability(
                        "assess-local-influence",
                        "Can assess people and information that are available through ordinary interaction."),
                ]),
            new CharacterLimitationSet(
                [
                    new CharacterLimitation(
                        "must-use-authoritative-actions",
                        "Cannot bypass the same authoritative action rules that constrain every character."),
                    new CharacterLimitation(
                        "requires-physical-and-informational-access",
                        "Cannot affect objects or claims without the required physical and informational access."),
                    new CharacterLimitation(
                        "cannot-remove-all-recovery-routes",
                        "Cannot knowingly eliminate every route by which the outbreak can be investigated."),
                ]),
            new CharacterControllerAssignment(agentId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([new KnowledgeId("agent-national-orders")]),
            new CharacterBeliefSet(
                [
                    new CharacterBelief(
                        "civilian-harm-can-outweigh-orders",
                        "Mass civilian harm can justify a choice to change allegiance despite prior orders."),
                ]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [
                    new CharacterGoal(
                        "delay-rival-recovery",
                        "Delay the rival village's recovery until the southern crown gains concessions."),
                    new CharacterGoal(
                        "avoid-mass-civilian-harm",
                        "Avoid allowing the mission to cause mass civilian harm."),
                ]));
    }
}

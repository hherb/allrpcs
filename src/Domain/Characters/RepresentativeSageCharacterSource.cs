using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Authors the pure representative evidence-consultant sage role.</summary>
public static class RepresentativeSageCharacterSource
{
    /// <summary>Creates one fresh LLM-controlled sage with bounded consultant state.</summary>
    public static Character Create()
    {
        CharacterId sageId = new("village-sage");

        return new Character(
            new CharacterIdentity(sageId, "ORIN"),
            new PublicCharacterBackground(
                "A village sage who compares accounts and questions premature conclusions."),
            new PrivateCharacterBackground(
                "Orin wants the village to reason from evidence instead of inheriting his authority."),
            new CharacterCapabilitySet(
                [
                    new CharacterCapability(
                        "compare-presented-evidence",
                        "Can compare evidence and testimony that has been presented in conversation."),
                    new CharacterCapability(
                        "challenge-unsupported-assumptions",
                        "Can identify assumptions that are unsupported by the presented evidence."),
                ]),
            new CharacterLimitationSet(
                [
                    new CharacterLimitation(
                        "requires-presented-evidence",
                        "Cannot reason from evidence that has not been presented or privately known."),
                    new CharacterLimitation(
                        "cannot-author-authoritative-truth",
                        "Cannot turn advice into authoritative world truth or guarantee a conclusion."),
                ]),
            new CharacterControllerAssignment(sageId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([new KnowledgeId("sage-shrine-oral-history")]),
            new CharacterBeliefSet(
                [
                    new CharacterBelief(
                        "contradictions-reveal-assumptions",
                        "Contradictions between accounts often expose an unsupported assumption."),
                ]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [
                    new CharacterGoal(
                        "strengthen-investigation-reasoning",
                        "Help investigators test their conclusions without supplying omniscient answers."),
                ]));
    }
}

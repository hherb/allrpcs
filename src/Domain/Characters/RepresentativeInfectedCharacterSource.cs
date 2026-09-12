using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Authors two pure representative infected roles with dormant controllers.</summary>
public static class RepresentativeInfectedCharacterSource
{
    /// <summary>Creates two fresh dormant infected characters in stable authored order.</summary>
    public static IReadOnlyList<Character> Create() => Array.AsReadOnly(
        new[]
        {
            CreateInfected(
                "sleeping-forester",
                "ROWAN",
                "A village forester afflicted by the deep sleeping sickness.",
                "Rowan remembers woodland trails that have disappeared from ordinary maps.",
                "track-woodland-paths",
                "Can navigate and interpret familiar woodland trails while responsive.",
                "forester-hidden-trails",
                "return-to-woodland-patrol",
                "Recover and return to protecting travellers on the woodland paths."),
            CreateInfected(
                "sleeping-scribe",
                "MIRA",
                "A village scribe afflicted by the deep sleeping sickness.",
                "Mira remembers discrepancies in village records from before the outbreak.",
                "interpret-village-records",
                "Can compare and interpret familiar village records while responsive.",
                "scribe-village-records",
                "preserve-outbreak-records",
                "Recover and preserve an accurate account of the outbreak."),
        });

    private static Character CreateInfected(
        string id,
        string displayName,
        string publicBackground,
        string privateBackground,
        string capabilityKey,
        string capabilityDescription,
        string knowledgeId,
        string goalKey,
        string goalDescription)
    {
        CharacterId characterId = new(id);

        return new Character(
            new CharacterIdentity(characterId, displayName),
            new PublicCharacterBackground(publicBackground),
            new PrivateCharacterBackground(privateBackground),
            new CharacterCapabilitySet(
                [new CharacterCapability(capabilityKey, capabilityDescription)]),
            new CharacterLimitationSet(
                [
                    new CharacterLimitation(
                        "unresponsive-while-dormant",
                        "Cannot converse or attempt actions while assigned a dormant controller."),
                ]),
            new CharacterControllerAssignment(characterId, ControllerType.Dormant),
            new CharacterConditionSet(
                [
                    new CharacterCondition(
                        "afflicted-deep-sleep",
                        "Unresponsive while afflicted by the deep sleeping sickness."),
                ]),
            new CharacterInventory([]),
            new CharacterKnowledge([new KnowledgeId(knowledgeId)]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal(goalKey, goalDescription)]));
    }
}

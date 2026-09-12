using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Authors the two pure representative hermit character roles.</summary>
public static class RepresentativeHermitCharacterSource
{
    private const string WoodlandHermitId = "woodland-hermit";
    private const string RiversideHermitId = "riverside-hermit";

    /// <summary>Creates two fresh LLM-controlled hermits in stable authored order.</summary>
    public static IReadOnlyList<Character> Create() => Array.AsReadOnly(
        new[]
        {
            CreateHermit(
                WoodlandHermitId,
                "ELIAN",
                "A reclusive herbalist who tends old-growth plants beyond the village paths.",
                "Elian hides the last undisturbed remedy grove after seeing outsiders strip others bare.",
                "woodland-hermit-old-growth-remedies",
                "outsiders-overharvest-remedies",
                "Outsiders who learn every remedy location will exhaust the woodland.",
                "protect-old-growth-remedies",
                "Protect the old-growth remedy grove from destructive harvesting."),
            CreateHermit(
                RiversideHermitId,
                "SERA",
                "A solitary washer and observer who lives downstream from the abandoned laundry.",
                "Sera quietly records when sickness follows changes in the laundry runoff.",
                "riverside-hermit-laundry-history",
                "laundry-runoff-spreads-sickness",
                "The abandoned laundry runoff carries the cause of the sleeping sickness downstream.",
                "trace-sickness-through-water",
                "Prove how the sickness travels through the water before accusing the village."),
        });

    private static Character CreateHermit(
        string id,
        string displayName,
        string publicBackground,
        string privateBackground,
        string knowledgeId,
        string beliefKey,
        string beliefStatement,
        string goalKey,
        string goalDescription)
    {
        CharacterId characterId = new(id);

        return new Character(
            new CharacterIdentity(characterId, displayName),
            new PublicCharacterBackground(publicBackground),
            new PrivateCharacterBackground(privateBackground),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([new KnowledgeId(knowledgeId)]),
            new CharacterBeliefSet([new CharacterBelief(beliefKey, beliefStatement)]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([new CharacterGoal(goalKey, goalDescription)]));
    }
}

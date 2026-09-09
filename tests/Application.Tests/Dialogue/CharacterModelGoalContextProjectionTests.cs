using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGoalContextProjectionTests
{
    [Fact]
    public void ProjectsFreshGoalEntriesInStableAuthoredOrderWithExactDescriptions()
    {
        string firstDescription = "  Keep the patient comfortable.  ";
        string secondDescription = "Find a safe remedy.";
        CharacterGoal first = new("keep-patient-comfortable", firstDescription);
        CharacterGoal second = new("find-safe-remedy", secondDescription);
        Character character = CreateCharacter(new CharacterGoalSet([first, second]));

        CharacterModelContextEntry[] entries =
            CharacterModelGoalContextProjection.Project(character).ToArray();

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Goal, entry.Kind);
                Assert.Same(firstDescription, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Goal, entry.Kind);
                Assert.Same(secondDescription, entry.Text);
            });
        Assert.NotSame(entries[0], entries[1]);
        Assert.Collection(
            character.Goals.Entries,
            goal => Assert.Same(first, goal),
            goal => Assert.Same(second, goal));
    }

    [Fact]
    public void ProjectsNoOtherCharacterDataAndAcceptsNoGoals()
    {
        Character character = CreateCharacter(new CharacterGoalSet([]));

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelGoalContextProjection.Project(character);

        Assert.Empty(entries);
        Assert.Empty(character.Goals.Entries);
    }

    [Fact]
    public void RejectsANullCharacter()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelGoalContextProjection.Project(null!));
    }

    internal static Character CreateCharacter(
        CharacterGoalSet goals,
        string characterIdText = "healer",
        CharacterKnowledge? knowledge = null)
    {
        CharacterId characterId = new(characterIdText);

        return new Character(
            new CharacterIdentity(characterId, "Healer"),
            new PublicCharacterBackground("Public history that must not be projected."),
            new PrivateCharacterBackground("Private history that must not be projected."),
            new CharacterCapabilitySet([new CharacterCapability("medicine", "Practises medicine.")]),
            new CharacterLimitationSet([new CharacterLimitation("fatigued", "Needs rest.")]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([new CharacterCondition("well", "Appears well.")]),
            new CharacterInventory([new ObjectId("healing-herb")]),
            knowledge ?? new CharacterKnowledge([new KnowledgeId("herbal-remedies")]),
            new CharacterBeliefSet([new CharacterBelief("patient-can-recover", "The patient can recover.")]),
            new CharacterRelationshipSet([]),
            goals);
    }
}

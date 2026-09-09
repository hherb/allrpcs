using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelContextAssemblerTests
{
    [Fact]
    public void AssemblesOwnedAndTypedSuppliedContextInRoadmapOrder()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        string perceptionText = "Visible herb patch.";
        WorldObject perceivedObject = CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
            "herb-patch", "Herb patch", perceptionText);
        string knowledgeText = "The herb is safe when prepared correctly.";
        CharacterGoal goal = new("help-safely", "Help when it is safe.");
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([goal]));
        DialogueEvent recentSpeech = CharacterModelRecentSpeechContextProjectionTests.CreateEvent(
            "healer", "I will help safely.", "seeker");
        CharacterPerceptionSnapshot perception = new(
            character.Identity.Id,
            [new WorldObjectVisibility(new ObjectId("herb-patch"), VisibilityState.Identified)]);
        WorldObjectSnapshot worldObjects = new([perceivedObject]);
        KnowledgeSnapshot knowledge = new(
            [new KnowledgeEntry(new KnowledgeId("herbal-remedies"), knowledgeText)]);
        CharacterModelContextEntry memory = new(CharacterModelContextEntryKind.Memory, "A memory.");
        CharacterModelContextEntry affordance = new(
            CharacterModelContextEntryKind.ValidAffordance,
            "examine-object herb-patch");
        CharacterModelContextEntry[] memories = [memory];
        CharacterModelContextEntry[] affordances = [affordance];

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            worldObjects,
            knowledge,
            memories,
            [recentSpeech],
            affordances);
        memories[0] = new(CharacterModelContextEntryKind.Memory, "Changed caller memory.");
        affordances[0] = new(CharacterModelContextEntryKind.ValidAffordance, "Changed affordance.");

        Assert.Same(request, result.TurnRequest);
        Assert.Collection(
            result.Context.Entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.FilteredPerception, entry.Kind);
                Assert.Same(perceptionText, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RelevantKnowledge, entry.Kind);
                Assert.Same(knowledgeText, entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Goal, entry.Kind);
                Assert.Same(goal.Description, entry.Text);
            },
            entry => Assert.Same(memory, entry),
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RecentSpeech, entry.Kind);
                Assert.Same(recentSpeech.Utterance.Text, entry.Text);
            },
            entry => Assert.Same(affordance, entry),
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.UntrustedPlayerDialogue, entry.Kind);
                Assert.Same(request.DialogueEvent.Utterance.Text, entry.Text);
            });
    }

    [Fact]
    public void AcceptsEmptySuppliedContext()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        CharacterModelGenerationRequest result = Assemble(
            request, character, CreateEmptyPerception(character), [], [], []);

        Assert.Equal(2, result.Context.Entries.Count);
    }

    [Fact]
    public void RejectsNullInputsAndMismatchedAuthoritativeIdentity()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterPerceptionSnapshot perception = CreateEmptyPerception(character);
        WorldObjectSnapshot worldObjects = new([]);
        KnowledgeSnapshot knowledge = CreateKnowledge(character);
        Character wrongCharacter = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]), "someone-else");
        CharacterPerceptionSnapshot wrongPerception = new(new CharacterId("someone-else"), []);

        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            null!, character, perception, worldObjects, knowledge, [], [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, null!, perception, worldObjects, knowledge, [], [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, null!, worldObjects, knowledge, [], [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, null!, knowledge, [], [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, null!, [], [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, null!, [], []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, [], null!, []));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, [], [], null!));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request, wrongCharacter, perception, worldObjects, knowledge, [], [], []));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request, character, wrongPerception, worldObjects, knowledge, [], [], []));
    }

    public static TheoryData<CharacterModelContextEntryKind> NonMemoryKinds =>
        KindsExcept(CharacterModelContextEntryKind.Memory);

    [Theory]
    [MemberData(nameof(NonMemoryKinds))]
    public void RejectsNullAndEveryNonMemoryKindFromMemoryInput(CharacterModelContextEntryKind kind)
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        Assert.Throws<ArgumentException>(() => Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            [new CharacterModelContextEntry(kind, "Spoofed memory.")],
            [],
            []));
        Assert.Throws<ArgumentException>(() => Assemble(
            request, character, CreateEmptyPerception(character), [null!], [], []));
    }

    public static TheoryData<CharacterModelContextEntryKind> NonAffordanceKinds =>
        KindsExcept(CharacterModelContextEntryKind.ValidAffordance);

    [Theory]
    [MemberData(nameof(NonAffordanceKinds))]
    public void RejectsNullAndEveryNonAffordanceKindFromAffordanceInput(
        CharacterModelContextEntryKind kind)
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        Assert.Throws<ArgumentException>(() => Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            [],
            [],
            [new CharacterModelContextEntry(kind, "Spoofed affordance.")]));
        Assert.Throws<ArgumentException>(() => Assemble(
            request, character, CreateEmptyPerception(character), [], [], [null!]));
    }

    [Fact]
    public void RejectsExcessiveContextWithoutChangingInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterModelContextEntry memory = new(CharacterModelContextEntryKind.Memory, "memory");
        CharacterModelContextEntry[] memories = Enumerable
            .Repeat(memory, CharacterModelContextSnapshot.MaximumEntryCount)
            .ToArray();
        CharacterModelContextEntry affordance = new(
            CharacterModelContextEntryKind.ValidAffordance,
            "affordance");
        CharacterModelContextEntry[] affordances = [affordance];

        Assert.Throws<ArgumentException>(() => Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            memories,
            [],
            affordances));
        Assert.All(memories, retained => Assert.Same(memory, retained));
        Assert.Same(affordance, affordances[0]);
    }

    private static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        Character character,
        CharacterPerceptionSnapshot perception,
        IEnumerable<CharacterModelContextEntry> memories,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        IEnumerable<CharacterModelContextEntry> validAffordances) =>
        CharacterModelContextAssembler.Assemble(
            request,
            character,
            perception,
            new WorldObjectSnapshot([]),
            CreateKnowledge(character),
            memories,
            recentDialogueEvents,
            validAffordances);

    private static CharacterPerceptionSnapshot CreateEmptyPerception(Character character) =>
        new(character.Identity.Id, []);

    private static TheoryData<CharacterModelContextEntryKind> KindsExcept(
        CharacterModelContextEntryKind excludedKind)
    {
        TheoryData<CharacterModelContextEntryKind> kinds = new();
        foreach (CharacterModelContextEntryKind kind in Enum.GetValues<CharacterModelContextEntryKind>())
        {
            if (kind != excludedKind)
            {
                kinds.Add(kind);
            }
        }

        return kinds;
    }

    private static KnowledgeSnapshot CreateKnowledge(Character character) =>
        new(character.Knowledge.KnowledgeReferences.Select(
            reference => new KnowledgeEntry(reference, $"Knowledge for {reference.Value}.")));
}

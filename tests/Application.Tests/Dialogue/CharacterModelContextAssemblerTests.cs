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
        CharacterIntentionAffordance affordance = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("herb-patch"));
        CharacterModelContextEntry[] memories = [memory];
        CharacterIntentionAffordanceSnapshot affordances = new([affordance]);

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
                Assert.Same(recentSpeech.Delivery.Mode, entry.DeliveryMode);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.ValidAffordance, entry.Kind);
                Assert.Equal("type=examine-object;target=herb-patch", entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.UntrustedPlayerDialogue, entry.Kind);
                Assert.Same(request.DialogueEvent.Utterance.Text, entry.Text);
                Assert.Same(request.DialogueEvent.Delivery.Mode, entry.DeliveryMode);
            });
    }

    [Fact]
    public void AcceptsEmptySuppliedContext()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        CharacterModelGenerationRequest result = Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            [],
            [],
            new CharacterIntentionAffordanceSnapshot([]));

        Assert.Equal(2, result.Context.Entries.Count);
    }

    [Fact]
    public void SharesTheExactAffordanceSnapshotWithResponseValidation()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterIntentionAffordanceSnapshot affordances = new(
            [
                new CharacterIntentionAffordance(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch")),
            ]);
        CharacterModelGenerationRequest generationRequest = Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            [],
            [],
            affordances);
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("herb-patch")),
            [],
            []);

        CharacterTurnResponseValidationResult validation =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                response,
                new WorldObjectSnapshot(
                    [
                        CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
                            "herb-patch",
                            "Hidden display name",
                            "Hidden description."),
                    ]),
                affordances);

        Assert.True(validation.IsValid);
        CharacterModelContextEntry contextAffordance = Assert.Single(
            generationRequest.Context.Entries.Where(
                entry => entry.Kind == CharacterModelContextEntryKind.ValidAffordance));
        Assert.Equal("type=examine-object;target=herb-patch", contextAffordance.Text);
        Assert.DoesNotContain("Hidden", contextAffordance.Text, StringComparison.Ordinal);
        Assert.Same(affordances.Entries[0].Type, response.ImmediateIntention.Type);
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
        CharacterIntentionAffordanceSnapshot affordances = new([]);
        Character wrongCharacter = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]), "someone-else");
        CharacterPerceptionSnapshot wrongPerception = new(new CharacterId("someone-else"), []);

        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            null!, character, perception, worldObjects, knowledge, [], [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, null!, perception, worldObjects, knowledge, [], [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, null!, worldObjects, knowledge, [], [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, null!, knowledge, [], [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, null!, [], [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, null!, [], affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, [], null!, affordances));
        Assert.Throws<ArgumentNullException>(() => CharacterModelContextAssembler.Assemble(
            request, character, perception, worldObjects, knowledge, [], [], null!));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request, wrongCharacter, perception, worldObjects, knowledge, [], [], affordances));
        Assert.Throws<ArgumentException>(() => CharacterModelContextAssembler.Assemble(
            request, character, wrongPerception, worldObjects, knowledge, [], [], affordances));
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
            new CharacterIntentionAffordanceSnapshot([])));
        Assert.Throws<ArgumentException>(() => Assemble(
            request,
            character,
            CreateEmptyPerception(character),
            [null!],
            [],
            new CharacterIntentionAffordanceSnapshot([])));
    }

    [Fact]
    public void FitsExcessiveContextWithoutChangingInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        CharacterModelContextEntry memory = new(CharacterModelContextEntryKind.Memory, "memory");
        CharacterModelContextEntry[] memories = Enumerable
            .Repeat(memory, CharacterModelContextSnapshot.MaximumEntryCount)
            .ToArray();
        CharacterIntentionAffordance affordance = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("herb-patch"));
        CharacterIntentionAffordanceSnapshot affordances = new([affordance]);

        CharacterModelGenerationRequest result = Assemble(
            request, character, CreateEmptyPerception(character), memories, [], affordances);
        Assert.Equal(128, result.Context.Entries.Count);
        Assert.Equal(request.DialogueEvent.Utterance.Text, result.Context.Entries[^1].Text);
        Assert.All(memories, retained => Assert.Same(memory, retained));
        Assert.Same(affordance, affordances.Entries[0]);
    }

    [Fact]
    public void LongOptionalContextRetainsLatestOutcomeGoalsAndExactRequiredContext()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([new CharacterGoal("help-safely", "Help safely.")]));
        CharacterModelContextEntry[] memories = Enumerable.Range(0, 128)
            .Select(index => new CharacterModelContextEntry(CharacterModelContextEntryKind.Memory,
                $"memory-{index}:" + new string('x', 1000))).ToArray();
        CharacterModelContextEntry outcome = new(CharacterModelContextEntryKind.ActionOutcome,
            "Completed examination: exact authoritative result.");
        CharacterIntentionAffordanceSnapshot affordances = new(
            [new CharacterIntentionAffordance(CharacterIntentionType.ExamineObject, new ObjectId("herb-patch"))]);

        CharacterModelGenerationRequest Generate() => CharacterModelContextAssembler.Assemble(
            request, character, CreateEmptyPerception(character), new WorldObjectSnapshot([]),
            CreateKnowledge(character), memories, [], affordances, [outcome]);
        CharacterModelGenerationRequest result = Generate();

        Assert.Contains(outcome, result.Context.Entries);
        Assert.Contains(result.Context.Entries, entry => entry.Kind == CharacterModelContextEntryKind.Goal);
        Assert.Contains(memories[^1], result.Context.Entries);
        Assert.DoesNotContain(memories[0], result.Context.Entries);
        Assert.Equal(result.Context.Entries.Select(entry => entry.Text),
            Generate().Context.Entries.Select(entry => entry.Text));
        Assert.True(result.Context.Entries.Sum(entry => entry.Text.Length) <= 32000);
        Assert.True(result.Context.Entries.Count <= 128);
        Assert.Equal(request.DialogueEvent.Utterance.Text, result.Context.Entries[^1].Text);
        Assert.Same(request.DialogueEvent.Delivery.Mode, result.Context.Entries[^1].DeliveryMode);
        Assert.Equal("type=examine-object;target=herb-patch", result.Context.Entries[^2].Text);
    }

    [Fact]
    public void FitsUnboundedPerceptionVolumeAtMaximumDescriptionLength()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(new CharacterGoalSet([]));
        WorldObject[] objects = Enumerable.Range(0, 128).Select(index =>
            CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
                $"object-{index}", "Object", new string('x', WorldObjectDescription.MaximumLength))).ToArray();
        CharacterPerceptionSnapshot perception = new(character.Identity.Id, objects.Select(item =>
            new WorldObjectVisibility(item.Identity.Id, VisibilityState.Identified)));

        CharacterModelGenerationRequest result = CharacterModelContextAssembler.Assemble(
            request, character, perception, new WorldObjectSnapshot(objects), CreateKnowledge(character),
            [], [], new CharacterIntentionAffordanceSnapshot([]));

        Assert.Equal(7, result.Context.Entries.Count(entry =>
            entry.Kind == CharacterModelContextEntryKind.FilteredPerception));
        Assert.Same(objects[0].Description.Text, result.Context.Entries[0].Text);
        Assert.True(result.Context.Entries.Sum(entry => entry.Text.Length) <= 32000);
        Assert.Equal(request.DialogueEvent.Utterance.Text, result.Context.Entries[^1].Text);
    }

    [Fact]
    public void ReportsMandatoryBudgetExhaustionWithoutPartialRequest()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(new CharacterGoalSet([]));
        CharacterIntentionAffordanceSnapshot affordances = new(Enumerable.Range(0, 128).Select(index =>
            new CharacterIntentionAffordance(CharacterIntentionType.ExamineObject, new ObjectId($"object-{index}"))));

        CharacterModelContextAssemblyStatus status = CharacterModelContextAssembler.TryAssemble(
            request, character, CreateEmptyPerception(character), new WorldObjectSnapshot([]),
            CreateKnowledge(character), [], [], affordances, out CharacterModelGenerationRequest? result);

        Assert.Equal(CharacterModelContextAssemblyStatus.RequiredContextBudgetExceeded, status);
        Assert.Null(result);
        Assert.Equal(128, affordances.Entries.Count);
        Assert.Throws<InvalidOperationException>(() => Assemble(
            request, character, CreateEmptyPerception(character), [], [], affordances));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MandatoryOverflowDoesNotMaskInvalidOptionalInputs(int invalidInput)
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(new CharacterGoalSet([]));
        CharacterIntentionAffordanceSnapshot affordances = new(Enumerable.Range(0, 128).Select(index =>
            new CharacterIntentionAffordance(CharacterIntentionType.ExamineObject, new ObjectId($"object-{index}"))));
        CharacterPerceptionSnapshot wrongOwner = new(new CharacterId("someone-else"), []);
        CharacterPerceptionSnapshot missingObject = new(character.Identity.Id,
            [new WorldObjectVisibility(new ObjectId("missing-object"), VisibilityState.Identified)]);

        CharacterPerceptionSnapshot perception = invalidInput switch
        {
            0 => wrongOwner,
            1 => missingObject,
            _ => CreateEmptyPerception(character),
        };
        KnowledgeSnapshot knowledge = invalidInput == 2 ? new KnowledgeSnapshot([]) : CreateKnowledge(character);
        DialogueEvent[] recent = invalidInput == 3 ? [null!] : [];
        Assert.Throws<ArgumentException>(() => Try(perception, knowledge, recent));

        void Try(CharacterPerceptionSnapshot perception, KnowledgeSnapshot knowledge, IEnumerable<DialogueEvent> recent) =>
            CharacterModelContextAssembler.TryAssemble(request, character, perception, new WorldObjectSnapshot([]),
                knowledge, [], recent, affordances, out _);
    }

    private static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        Character character,
        CharacterPerceptionSnapshot perception,
        IEnumerable<CharacterModelContextEntry> memories,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        CharacterIntentionAffordanceSnapshot validAffordances) =>
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

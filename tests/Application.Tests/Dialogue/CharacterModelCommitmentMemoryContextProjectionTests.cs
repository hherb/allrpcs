using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelCommitmentMemoryContextProjectionTests
{
    [Fact]
    public void ProjectsOnlyRespondingCharacterCommitmentsInStableCanonicalOrder()
    {
        CharacterId healerId = new("healer");
        CharacterCommitmentMemory promise = Commitment(
            healerId,
            CharacterCommitmentKind.Promise,
            "seeker",
            "turn-one",
            "  Inspect the moonwort before collection.  ",
            "inspect-moonwort");
        CharacterCommitmentMemory task = Commitment(
            healerId,
            CharacterCommitmentKind.AssignedTask,
            "hermit",
            "turn-two",
            "Return with a clean mortar.",
            "return-with-mortar");
        CharacterMemoryAggregate healer = Aggregate(healerId, [promise, task]);
        CharacterMemoryAggregate hermit = Aggregate(
            new CharacterId("hermit"),
            [Commitment(
                new CharacterId("hermit"),
                CharacterCommitmentKind.Other,
                "seeker",
                "turn-three",
                "Keep watch by the stream.",
                "watch-stream")]);
        CharacterMemorySnapshot memory = new([hermit, healer]);

        CharacterModelContextEntry[] entries = CharacterModelCommitmentMemoryContextProjection
            .Project(new CharacterId("healer"), memory)
            .ToArray();

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Memory, entry.Kind);
                Assert.Equal(
                    "kind=promise;counterparty=seeker;source-turn=turn-one;"
                        + "description=  Inspect the moonwort before collection.  ",
                    entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.Memory, entry.Kind);
                Assert.Equal(
                    "kind=assigned-task;counterparty=hermit;source-turn=turn-two;"
                        + "description=Return with a clean mortar.",
                    entry.Text);
            });
        Assert.Same(promise, healer.Commitments.Commitments[0]);
        Assert.Same(task, healer.Commitments.Commitments[1]);
        Assert.DoesNotContain(entries, entry => entry.Text.Contains("watch", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, entry => entry.Text.Contains(
            promise.CommitmentId.Value,
            StringComparison.Ordinal));
    }

    [Fact]
    public void AcceptsExactEmptyCommitmentsAndFailsClosedForMissingCharacter()
    {
        CharacterId healerId = new("healer");
        CharacterMemorySnapshot memory = new([Aggregate(healerId, [])]);

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelCommitmentMemoryContextProjection.Project(healerId, memory);

        Assert.Empty(entries);
        Assert.Empty(CharacterModelCommitmentMemoryContextProjection.Project(
            new CharacterId("hermit"),
            memory));
        Assert.Empty(CharacterModelCommitmentMemoryContextProjection.Project(
            healerId,
            new CharacterMemorySnapshot([])));
    }

    [Fact]
    public void RejectsMemoryThatWouldExceedExistingContextBoundsWithoutChangingIt()
    {
        CharacterId healerId = new("healer");
        string description = new('x', CharacterCommitmentMemory.MaximumDescriptionLength);
        CharacterCommitmentMemory[] commitments = Enumerable.Range(0, 33)
            .Select(index => Commitment(
                healerId,
                CharacterCommitmentKind.Promise,
                "seeker",
                $"turn-{NumberWord(index)}",
                description,
                $"commitment-{NumberWord(index)}"))
            .ToArray();
        CharacterMemorySnapshot memory = new([Aggregate(healerId, commitments)]);

        Assert.Throws<ArgumentException>(() =>
            CharacterModelCommitmentMemoryContextProjection.Project(healerId, memory));
        Assert.All(
            commitments,
            commitment => Assert.Contains(
                memory.Characters.Single().Commitments.Commitments,
                retained => ReferenceEquals(retained, commitment)));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureProjectionMethod()
    {
        CharacterId healerId = new("healer");
        CharacterMemorySnapshot memory = new([Aggregate(healerId, [])]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelCommitmentMemoryContextProjection.Project(null!, memory));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelCommitmentMemoryContextProjection.Project(healerId, null!));

        System.Reflection.MethodInfo method = Assert.Single(
            typeof(CharacterModelCommitmentMemoryContextProjection).GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(CharacterModelCommitmentMemoryContextProjection.Project), method.Name);
        Assert.Equal(
            [typeof(CharacterId), typeof(CharacterMemorySnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static CharacterMemoryAggregate Aggregate(
        CharacterId characterId,
        IEnumerable<CharacterCommitmentMemory> commitments) => new(
        characterId,
        new CharacterRecentConversationWindow(new CharacterId(characterId.Value), []),
        new CharacterConversationSummarySnapshot(new CharacterId(characterId.Value), []),
        new CharacterEpisodicMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterEpistemicMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterBeliefAssessmentSnapshot(new CharacterId(characterId.Value), []),
        new CharacterRelationshipMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterCommitmentMemorySnapshot(new CharacterId(characterId.Value), commitments),
        new CharacterMemoryIndex(new CharacterId(characterId.Value), []));

    private static CharacterCommitmentMemory Commitment(
        CharacterId characterId,
        CharacterCommitmentKind kind,
        string counterparty,
        string sourceTurn,
        string description,
        string commitmentId) => new(
        new CharacterCommitmentId(commitmentId),
        new CharacterId(characterId.Value),
        kind,
        new CharacterId(counterparty),
        new CharacterTurnId(sourceTurn),
        description);

    private static string NumberWord(int value) => value switch
    {
        0 => "zero",
        1 => "one",
        2 => "two",
        3 => "three",
        4 => "four",
        5 => "five",
        6 => "six",
        7 => "seven",
        8 => "eight",
        9 => "nine",
        10 => "ten",
        11 => "eleven",
        12 => "twelve",
        13 => "thirteen",
        14 => "fourteen",
        15 => "fifteen",
        16 => "sixteen",
        17 => "seventeen",
        18 => "eighteen",
        19 => "nineteen",
        20 => "twenty",
        21 => "twenty-one",
        22 => "twenty-two",
        23 => "twenty-three",
        24 => "twenty-four",
        25 => "twenty-five",
        26 => "twenty-six",
        27 => "twenty-seven",
        28 => "twenty-eight",
        29 => "twenty-nine",
        30 => "thirty",
        31 => "thirty-one",
        _ => "thirty-two",
    };
}

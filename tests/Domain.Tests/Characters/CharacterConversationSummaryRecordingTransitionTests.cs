using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Tests.Dialogue;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterConversationSummaryRecordingTransitionTests
{
    [Fact]
    public void ReachingCapacitySucceedsThenRejectsValidSummaryWithoutMaskingInvalidInput()
    {
        CharacterId character = new("healer");
        CharacterConversationSummarySnapshot source = new(character, Enumerable.Range(0, 127)
            .Select(index => Summary(character, $"summary-{index}", ["turn-one"])));
        CharacterRecentConversationWindow window = Window("turn-one");
        CharacterTurnTranscriptSnapshot transcript = Transcript();
        CharacterConversationSummaryRecordingResult success = CharacterConversationSummaryRecordingTransition.Record(
            Summary(character, "last", ["turn-one"]), source, window, transcript);
        Assert.Equal(CharacterConversationSummaryRecordingOutcome.Recorded, success.Outcome);
        Assert.Equal(128, success.Summaries.Summaries.Count);
        Assert.Equal(127, source.Summaries.Count);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            CharacterConversationSummaryRecordingResult full = CharacterConversationSummaryRecordingTransition.Record(
                Summary(character, "extra", ["turn-one"]), success.Summaries, window, transcript);
            Assert.Equal(CharacterConversationSummaryRecordingOutcome.CapacityRejected, full.Outcome);
            Assert.Same(success.Summaries, full.Summaries);
            Assert.Same(window, full.RecentConversation);
            Assert.Same(transcript, full.Transcript);
        }
        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(character, "last", ["turn-one"]), success.Summaries, window, transcript));
        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(character, "missing", ["absent"]), success.Summaries, window, transcript));
    }

    [Fact]
    public void AppendsExactSummaryAndRetainsConversationSourcesUnchanged()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummary existing = Summary(characterId, "existing", ["turn-one"]);
        CharacterConversationSummary appended = Summary(
            new CharacterId("healer"),
            "appended",
            ["turn-one", "turn-three"]);
        CharacterConversationSummarySnapshot summaries = new(characterId, [existing]);
        CharacterRecentConversationWindow recent = Window("turn-one", "turn-two", "turn-three");
        CharacterTurnTranscriptSnapshot transcript = Transcript();

        CharacterConversationSummaryRecordingResult result =
            CharacterConversationSummaryRecordingTransition.Record(
                appended,
                summaries,
                recent,
                transcript);

        Assert.Equal(CharacterConversationSummaryRecordingOutcome.Recorded, result.Outcome);
        Assert.Collection(
            result.Summaries.Summaries,
            summary => Assert.Same(existing, summary),
            summary => Assert.Same(appended, summary));
        Assert.Same(characterId, result.Summaries.CharacterId);
        Assert.Same(recent, result.RecentConversation);
        Assert.Same(transcript, result.Transcript);
        Assert.Single(summaries.Summaries);
        Assert.Collection(
            recent.Turns,
            turn => Assert.Equal("turn-one", turn.TurnId.Value),
            turn => Assert.Equal("turn-two", turn.TurnId.Value),
            turn => Assert.Equal("turn-three", turn.TurnId.Value));
        Assert.Single(transcript.Entries);
    }

    [Fact]
    public void RejectsAbsentOrReorderedSourceTurns()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummarySnapshot summaries = new(characterId, []);
        CharacterRecentConversationWindow recent = Window("turn-one", "turn-two", "turn-three");
        CharacterTurnTranscriptSnapshot transcript = Transcript();

        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(characterId, "absent", ["turn-one", "turn-four"]),
            summaries,
            recent,
            transcript));
        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(characterId, "reordered", ["turn-three", "turn-one"]),
            summaries,
            recent,
            transcript));
    }

    [Fact]
    public void RejectsMismatchedCharactersDuplicateAndFullSnapshot()
    {
        CharacterId characterId = new("healer");
        CharacterRecentConversationWindow recent = Window("turn-one");
        CharacterTurnTranscriptSnapshot transcript = Transcript();
        CharacterConversationSummary existing = Summary(characterId, "summary", ["turn-one"]);
        CharacterConversationSummarySnapshot summaries = new(characterId, [existing]);
        CharacterConversationSummarySnapshot full = new(
            characterId,
            Enumerable.Range(0, CharacterConversationSummarySnapshot.MaximumSummaryCount)
                .Select(index => Summary(characterId, $"summary-{index}", ["turn-one"])));

        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(new CharacterId("hermit"), "other", ["turn-one"]),
            summaries,
            recent,
            transcript));
        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            existing,
            summaries,
            recent,
            transcript));
        CharacterConversationSummaryRecordingResult rejected =
            CharacterConversationSummaryRecordingTransition.Record(
                Summary(characterId, "extra", ["turn-one"]), full, recent, transcript);
        Assert.Equal(CharacterConversationSummaryRecordingOutcome.CapacityRejected, rejected.Outcome);
        Assert.Same(full, rejected.Summaries);
        Assert.Same(recent, rejected.RecentConversation);
        Assert.Same(transcript, rejected.Transcript);
        Assert.Throws<ArgumentException>(() => CharacterConversationSummaryRecordingTransition.Record(
            Summary(characterId, "other-window", ["turn-one"]),
            summaries,
            new CharacterRecentConversationWindow(new CharacterId("hermit"), []),
            transcript));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummary summary = Summary(characterId, "summary", ["turn-one"]);
        CharacterConversationSummarySnapshot summaries = new(characterId, []);
        CharacterRecentConversationWindow recent = Window("turn-one");
        CharacterTurnTranscriptSnapshot transcript = Transcript();

        Assert.Throws<ArgumentNullException>(() => CharacterConversationSummaryRecordingTransition.Record(
            null!, summaries, recent, transcript));
        Assert.Throws<ArgumentNullException>(() => CharacterConversationSummaryRecordingTransition.Record(
            summary, null!, recent, transcript));
        Assert.Throws<ArgumentNullException>(() => CharacterConversationSummaryRecordingTransition.Record(
            summary, summaries, null!, transcript));
        Assert.Throws<ArgumentNullException>(() => CharacterConversationSummaryRecordingTransition.Record(
            summary, summaries, recent, null!));
    }

    [Fact]
    public void ExposesOnlyOnePureOperationAndImmutableResult()
    {
        System.Reflection.MethodInfo method = Assert.Single(
            typeof(CharacterConversationSummaryRecordingTransition).GetMethods(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        Type resultType = typeof(CharacterConversationSummaryRecordingResult);
        Type[] expectedParameters =
        [
            typeof(CharacterConversationSummary),
            typeof(CharacterConversationSummarySnapshot),
            typeof(CharacterRecentConversationWindow),
            typeof(CharacterTurnTranscriptSnapshot),
        ];
        string[] expectedProperties = ["Outcome", "RecentConversation", "Summaries", "Transcript"];

        Assert.Equal(nameof(CharacterConversationSummaryRecordingTransition.Record), method.Name);
        Assert.Equal(resultType, method.ReturnType);
        Assert.Equal(
            expectedParameters,
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            expectedProperties,
            resultType.GetProperties().Select(property => property.Name).Order().ToArray());
        Assert.All(resultType.GetProperties(), property => Assert.False(property.CanWrite));
    }

    private static CharacterConversationSummary Summary(
        CharacterId characterId,
        string summaryId,
        IEnumerable<string> turnIds) =>
        CharacterConversationSummaryTests.Summary(
            turnIds.Select(turnId => new CharacterTurnId(turnId)),
            characterId: characterId,
            summaryId: summaryId);

    private static CharacterRecentConversationWindow Window(params string[] turnIds) =>
        new(
            new CharacterId("healer"),
            turnIds.Select(turnId => CharacterRecentConversationWindowTests.Turn(
                turnId,
                "What happened?",
                "We discussed it.")));

    private static CharacterTurnTranscriptSnapshot Transcript() =>
        new(
            [new CharacterTurnTranscriptEntry(
                new DialogueDelivery(
                    new DialogueUtterance(new CharacterId("healer"), "We discussed it."),
                    DialogueDeliveryMode.OrdinarySpeech))]);
}

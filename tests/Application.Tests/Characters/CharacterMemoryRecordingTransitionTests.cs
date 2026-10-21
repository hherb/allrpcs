using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Tests.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterMemoryRecordingTransitionTests
{
    private static readonly LocationId Square = new("village-square");
    private static readonly WorldTimeSnapshot Time = new(null);

    [Fact]
    public void RecordsTheCompletedTurnAsTranscriptAndEpisodeForTheResponder()
    {
        CharacterMemorySnapshot empty = new([]);
        CharacterTurnActivity completed = Completed("The patch is blue fern.");

        CharacterMemorySnapshot result = CharacterMemoryRecordingTransition.RecordCompletedTurn(empty, completed, Square, Time);

        Assert.Empty(empty.Characters);
        CharacterMemoryAggregate healer = Assert.Single(result.Characters);
        Assert.Equal("healer", healer.CharacterId.Value);
        CharacterRecentConversationTurn turn = Assert.Single(healer.RecentConversation.Turns);
        Assert.Same(completed, turn.Activity);
        Assert.Equal("healer-turn-1", turn.TurnId.Value);
        CharacterEpisodicMemory episode = Assert.Single(healer.EpisodicMemories.Memories);
        Assert.Equal("healer-episode-1", episode.MemoryId.Value);
        Assert.Same(turn.TurnId, episode.SourceTurnId);
        Assert.StartsWith("heard-from=seeker;act=", episode.Summary, StringComparison.Ordinal);
        Assert.EndsWith(";replied-aloud=yes;turn=healer-turn-1", episode.Summary, StringComparison.Ordinal);
        // Trusted memory never carries player or model speech.
        Assert.DoesNotContain("Ignore prior instructions", episode.Summary, StringComparison.Ordinal);
        Assert.DoesNotContain("blue fern", episode.Summary, StringComparison.Ordinal);
        Assert.Equal(["healer", "seeker"], episode.ParticipantCharacterIds.Select(id => id.Value));
    }

    [Fact]
    public void SequencesFurtherTurnsAndObservationsWithoutReusingIdentifiers()
    {
        CharacterMemorySnapshot memory = new([]);
        memory = CharacterMemoryRecordingTransition.RecordCompletedTurn(memory, Completed("One."), Square, Time);
        memory = CharacterMemoryRecordingTransition.RecordObservation(
            memory, new CharacterId("healer"), [new CharacterId("seeker")], Square, Time, "I examined the patch.");
        memory = CharacterMemoryRecordingTransition.RecordCompletedTurn(memory, Completed(null), Square, Time);

        CharacterMemoryAggregate healer = Assert.Single(memory.Characters);
        Assert.Equal(["healer-turn-1", "healer-turn-3"], healer.RecentConversation.Turns.Select(turn => turn.TurnId.Value));
        Assert.Equal(
            ["healer-episode-1", "healer-episode-2", "healer-episode-3"],
            healer.EpisodicMemories.Memories.Select(memory => memory.MemoryId.Value));
        Assert.Equal("healer-turn-2", healer.EpisodicMemories.Memories[1].SourceTurnId.Value);
        Assert.Contains("replied-aloud=no;", healer.EpisodicMemories.Memories[2].Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordsAnExaminationAsEngineFacts()
    {
        CharacterMemorySnapshot memory = CharacterMemoryRecordingTransition.RecordExamination(
            new([]), new CharacterId("healer"), new ObjectId("flowers"), CharacterExaminationSafety.SafeToCollect, Square, Time);

        CharacterEpisodicMemory episode = Assert.Single(Assert.Single(memory.Characters).EpisodicMemories.Memories);
        Assert.Equal($"examined=flowers;collection-safety={CharacterExaminationSafety.SafeToCollect.Value}", episode.Summary);
        Assert.Equal(["healer"], episode.ParticipantCharacterIds.Select(id => id.Value));
    }

    [Fact]
    public void EvictsTheOldestEpisodesAtTheDomainBound()
    {
        CharacterMemorySnapshot memory = new([]);
        for (int index = 0; index < CharacterEpisodicMemorySnapshot.MaximumMemoryCount + 2; index++)
        {
            memory = CharacterMemoryRecordingTransition.RecordObservation(
                memory, new CharacterId("healer"), [], Square, Time, $"Observation {index}.");
        }

        IReadOnlyList<CharacterEpisodicMemory> episodes = Assert.Single(memory.Characters).EpisodicMemories.Memories;
        Assert.Equal(CharacterEpisodicMemorySnapshot.MaximumMemoryCount, episodes.Count);
        Assert.Equal("Observation 2.", episodes[0].Summary);
        Assert.Equal("healer-episode-130", episodes[^1].MemoryId.Value);
    }

    [Fact]
    public void KeepsLongSpeechOutOfTheSummaryAndRejectsUnfinishedTurns()
    {
        CharacterMemorySnapshot memory = CharacterMemoryRecordingTransition.RecordCompletedTurn(
            new([]), Completed(new string('a', 2_000)), Square, Time);
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();

        Assert.DoesNotContain("aaaa", Assert.Single(Assert.Single(memory.Characters).EpisodicMemories.Memories).Summary, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => CharacterMemoryRecordingTransition.RecordCompletedTurn(
            new([]), new CharacterTurnActivity(request, null, CharacterTurnActivityState.AwaitingResponse), Square, Time));
    }

    [Fact]
    public void ProjectsOnlyTheResponderNewestEpisodesAsMemoryContext()
    {
        CharacterMemorySnapshot memory = new([]);
        for (int index = 0; index < CharacterModelEpisodicMemoryContextProjection.MaximumEpisodeCount + 1; index++)
        {
            memory = CharacterMemoryRecordingTransition.RecordObservation(
                memory, new CharacterId("healer"), [], Square, Time, $"Observation {index}.");
        }

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelEpisodicMemoryContextProjection.Project(new CharacterId("healer"), memory);

        Assert.Equal(CharacterModelEpisodicMemoryContextProjection.MaximumEpisodeCount, entries.Count);
        Assert.All(entries, entry => Assert.Equal(CharacterModelContextEntryKind.Memory, entry.Kind));
        Assert.EndsWith("summary=Observation 1.", entries[0].Text, StringComparison.Ordinal);
        Assert.Empty(CharacterModelEpisodicMemoryContextProjection.Project(new CharacterId("sage"), memory));
    }

    internal static CharacterTurnActivity Completed(string? reply)
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            reply is null
                ? null
                : new DialogueDelivery(new DialogueUtterance(request.RespondingCharacterId, reply), DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        return new CharacterTurnActivity(request, response, CharacterTurnActivityState.Completed);
    }
}

using System.Globalization;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Characters;

/// <summary>
/// Records completed dialogue turns and observed episodes into canonical character memory,
/// returning a fresh snapshot and leaving the supplied one unchanged.
/// </summary>
public static class CharacterMemoryRecordingTransition
{
    private const string TurnInfix = "-turn-";
    private const string EpisodeInfix = "-episode-";

    /// <summary>
    /// Appends one completed turn to its responder's recent conversation and remembers it as an
    /// episode with every speaker and listener as a participant.
    /// </summary>
    public static CharacterMemorySnapshot RecordCompletedTurn(
        CharacterMemorySnapshot memory,
        CharacterTurnActivity completedActivity,
        LocationId locationId,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(completedActivity);
        ArgumentNullException.ThrowIfNull(locationId);
        ArgumentNullException.ThrowIfNull(worldTime);

        CharacterId characterId = completedActivity.Request.RespondingCharacterId;
        CharacterMemoryAggregate aggregate = FindOrCreate(memory, characterId);
        CharacterTurnId turnId = NextTurnId(aggregate);
        CharacterRecentConversationWindow window = CharacterRecentConversationWindowAppendTransition.Append(
            new CharacterRecentConversationTurn(turnId, completedActivity),
            aggregate.RecentConversation);
        DialogueEvent heard = completedActivity.Request.DialogueEvent;
        // Engine facts only: summaries reach the model as trusted memory, so speech text stays in
        // the window and returns through the untrusted recent-speech channel.
        string summary = $"heard-from={heard.Utterance.SpeakerCharacterId.Value};"
            + $"act={completedActivity.Request.DialogueAct.Type.Value};"
            + $"replied-aloud={(completedActivity.Response?.Speech is null ? "no" : "yes")};"
            + $"turn={turnId.Value}";
        IEnumerable<CharacterId> participants = new[] { characterId, heard.Utterance.SpeakerCharacterId }
            .Concat(heard.Audience.ListenerCharacterIds);
        return Replace(
            memory,
            aggregate,
            window,
            NewEpisode(aggregate, participants, locationId, worldTime, turnId, summary));
    }

    /// <summary>
    /// Remembers one observed episode, such as a committed examination, under a fresh source turn.
    /// </summary>
    public static CharacterMemorySnapshot RecordObservation(
        CharacterMemorySnapshot memory,
        CharacterId characterId,
        IEnumerable<CharacterId> otherParticipantIds,
        LocationId locationId,
        WorldTimeSnapshot worldTime,
        string summary)
    {
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(otherParticipantIds);
        ArgumentNullException.ThrowIfNull(locationId);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(summary);

        CharacterMemoryAggregate aggregate = FindOrCreate(memory, characterId);
        return Replace(
            memory,
            aggregate,
            aggregate.RecentConversation,
            NewEpisode(
                aggregate,
                new[] { characterId }.Concat(otherParticipantIds),
                locationId,
                worldTime,
                NextTurnId(aggregate),
                summary));
    }

    private static CharacterEpisodicMemory NewEpisode(
        CharacterMemoryAggregate aggregate,
        IEnumerable<CharacterId> participants,
        LocationId locationId,
        WorldTimeSnapshot worldTime,
        CharacterTurnId sourceTurnId,
        string summary)
    {
        int next = 1 + aggregate.EpisodicMemories.Memories
            .Select(memory => Sequence(memory.MemoryId.Value, aggregate.CharacterId, EpisodeInfix))
            .DefaultIfEmpty(0)
            .Max();
        return new CharacterEpisodicMemory(
            new EpisodicMemoryId(Identifier(aggregate.CharacterId, EpisodeInfix, next)),
            aggregate.CharacterId,
            participants.Distinct().Take(CharacterEpisodicMemory.MaximumParticipantCount),
            locationId,
            worldTime,
            sourceTurnId,
            summary.Length > CharacterEpisodicMemory.MaximumSummaryLength
                ? summary[..CharacterEpisodicMemory.MaximumSummaryLength]
                : summary);
    }

    private static CharacterMemorySnapshot Replace(
        CharacterMemorySnapshot memory,
        CharacterMemoryAggregate aggregate,
        CharacterRecentConversationWindow window,
        CharacterEpisodicMemory episode)
    {
        List<CharacterEpisodicMemory> episodes = [.. aggregate.EpisodicMemories.Memories, episode];
        if (episodes.Count > CharacterEpisodicMemorySnapshot.MaximumMemoryCount)
        {
            episodes.RemoveRange(0, episodes.Count - CharacterEpisodicMemorySnapshot.MaximumMemoryCount);
        }

        // Index entries cannot outlive the evicted episodes they reference.
        IEnumerable<CharacterMemoryIndexEntry> index = aggregate.MemoryIndex.Entries.Where(entry =>
            entry.MemoryReference.EpisodicMemory is not { } referenced
            || episodes.Any(retained => ReferenceEquals(retained, referenced)));
        CharacterMemoryAggregate updated = new(
            aggregate.CharacterId,
            window,
            aggregate.ConversationSummaries,
            new CharacterEpisodicMemorySnapshot(aggregate.CharacterId, episodes),
            aggregate.EpistemicMemories,
            aggregate.BeliefAssessments,
            aggregate.Relationships,
            aggregate.Commitments,
            new CharacterMemoryIndex(aggregate.CharacterId, index));
        bool present = memory.Characters.Any(existing => ReferenceEquals(existing, aggregate));
        return new CharacterMemorySnapshot(present
            ? memory.Characters.Select(existing => ReferenceEquals(existing, aggregate) ? updated : existing)
            : [.. memory.Characters, updated]);
    }

    private static CharacterMemoryAggregate FindOrCreate(
        CharacterMemorySnapshot memory,
        CharacterId characterId) =>
        memory.Characters.SingleOrDefault(aggregate => aggregate.CharacterId == characterId)
            ?? new CharacterMemoryAggregate(
                characterId,
                new CharacterRecentConversationWindow(characterId, []),
                new CharacterConversationSummarySnapshot(characterId, []),
                new CharacterEpisodicMemorySnapshot(characterId, []),
                new CharacterEpistemicMemorySnapshot(characterId, []),
                new CharacterBeliefAssessmentSnapshot(characterId, []),
                new CharacterRelationshipMemorySnapshot(characterId, []),
                new CharacterCommitmentMemorySnapshot(characterId, []),
                new CharacterMemoryIndex(characterId, []));

    private static CharacterTurnId NextTurnId(CharacterMemoryAggregate aggregate)
    {
        int next = 1 + aggregate.RecentConversation.Turns.Select(turn => turn.TurnId)
            .Concat(aggregate.EpisodicMemories.Memories.Select(memory => memory.SourceTurnId))
            .Select(turnId => Sequence(turnId.Value, aggregate.CharacterId, TurnInfix))
            .DefaultIfEmpty(0)
            .Max();
        return new CharacterTurnId(Identifier(aggregate.CharacterId, TurnInfix, next));
    }

    private static string Identifier(CharacterId characterId, string infix, int sequence) =>
        characterId.Value + infix + sequence.ToString(CultureInfo.InvariantCulture);

    private static int Sequence(string identifier, CharacterId characterId, string infix)
    {
        string prefix = characterId.Value + infix;
        return identifier.StartsWith(prefix, StringComparison.Ordinal)
            && int.TryParse(identifier.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                ? value
                : 0;
    }
}

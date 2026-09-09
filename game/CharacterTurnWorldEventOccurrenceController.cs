using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates atomic authoritative character-action occurrences in the live scene.</summary>
public partial class CharacterTurnWorldEventOccurrenceController : Node
{
    private CharacterTurnTranscriptController transcriptController = null!;
    private WorldEventController worldEventController = null!;

    /// <summary>Gets the exact most recently accepted character-action occurrence.</summary>
    public CharacterTurnWorldEventOccurrence? LastOccurrence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>Occurs one exact eligible speaking response's immediate physical intention.</summary>
    public bool TryOccurImmediateIntention(
        CharacterTurnActivity activity,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(worldEvent);

        CharacterIntention? action = activity.Response?.ImmediateIntention;
        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (action is null
            || action.Type == CharacterIntentionType.Wait
            || worldEvents is null
            || eventLog is null
            || worldTime is null
            || transcript is null)
        {
            return false;
        }

        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                activity.Request.RespondingCharacterId,
                action));
        if (!CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript))
        {
            return false;
        }

        CharacterTurnWorldEventOccurrenceResult result =
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);
        worldEventController.InstallCharacterTurnOccurrence(result.WorldEventResult);
        transcriptController.InstallCharacterTurnOccurrence(result.Transcript);
        LastOccurrence = result.Occurrence;
        return true;
    }

    /// <summary>Occurs exact-target collection after the exact authoritative safe result.</summary>
    public bool TryOccurPostExaminationCollection(
        CharacterExaminationResult result,
        CharacterIntention collection,
        WorldEvent worldEvent)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(worldEvent);

        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (collection.Type == CharacterIntentionType.Wait
            || worldEvents is null
            || eventLog is null
            || worldTime is null
            || transcript is null)
        {
            return false;
        }

        CharacterTurnWorldEventOccurrence occurrence = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                result.Occurrence.TranscriptEntry.CharacterId,
                collection));
        if (!CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript))
        {
            return false;
        }

        CharacterTurnWorldEventOccurrenceResult occurrenceResult =
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);
        worldEventController.InstallCharacterTurnOccurrence(
            occurrenceResult.WorldEventResult);
        transcriptController.InstallCharacterTurnOccurrence(occurrenceResult.Transcript);
        LastOccurrence = occurrenceResult.Occurrence;
        return true;
    }
}

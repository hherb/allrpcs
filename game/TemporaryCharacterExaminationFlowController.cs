using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Advances the legacy exact herb-patch examination and safe collection fixture for checks that
/// install turns directly. It is off unless a check turns on its processing.
/// </summary>
public partial class TemporaryCharacterExaminationFlowController : Node
{
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private CharacterTurnWorldEventOccurrenceController occurrenceController = null!;
    private TemporaryCharacterExaminationResultSource resultSource = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private ObjectId safeTargetObjectId = null!;
    private CharacterTurnResponse? observedResponse;

    /// <summary>Gets the exact authoritative safe examination result selected by this flow.</summary>
    public CharacterExaminationResult? LastExaminationResult { get; private set; }

    /// <summary>Gets the exact collection occurrence accepted after that result.</summary>
    public CharacterTurnWorldEventOccurrence? LastCollectionOccurrence { get; private set; }

    // BEGIN herb collection presentation ownership
    /// <summary>Notifies observers only after the unchanged collection transaction is installed.</summary>
    public event Action<HerbCollectionCommit>? CollectionCommitted;
    /// <summary>The latest producer-owned token; it is never replayed to late subscribers.</summary>
    internal HerbCollectionCommit? LastCollectionCommit { get; private set; }
    internal bool OwnsCollection(HerbCollectionCommit? token) => token is not null
        && ReferenceEquals(token, LastCollectionCommit) && ReferenceEquals(token.Owner, this)
        && ReferenceEquals(token.Result, LastExaminationResult)
        && ReferenceEquals(token.Occurrence, LastCollectionOccurrence)
        && ReferenceEquals(token.Occurrence, occurrenceController.LastOccurrence)
        && ReferenceEquals(token.EventLog, GetNode<WorldEventController>("../WorldEventController").EventLog)
        && ReferenceEquals(token.WorldTime, GetNode<WorldEventController>("../WorldEventController").WorldTime)
        && ReferenceEquals(token.Transcript, transcriptController.Transcript)
        && token.Transcript.Entries.Count >= 2
        && ReferenceEquals(token.Transcript.Entries[^1], token.Occurrence.TranscriptEntry)
        && ReferenceEquals(token.Transcript.Entries[^2], token.Result.Occurrence.TranscriptEntry)
        && token.EventLog.Entries.Count > 0
        && ReferenceEquals(token.EventLog.Entries[^1], token.Occurrence.WorldEvent.EventId)
        && ReferenceEquals(token.WorldTime.CurrentEventId, token.Occurrence.WorldEvent.EventId);

    private void NotifyCollection(CharacterExaminationResult result)
    {
        var world = GetNode<WorldEventController>("../WorldEventController");
        if (LastCollectionOccurrence is null || world.EventLog is null || world.WorldTime is null
            || transcriptController.Transcript is null) { return; }
        var token = new HerbCollectionCommit(this, result, LastCollectionOccurrence,
            world.EventLog, world.WorldTime, transcriptController.Transcript);
        LastCollectionCommit = token;
        foreach (Action<HerbCollectionCommit> observer in CollectionCommitted?.GetInvocationList() ?? [])
        {
            try { observer(token); }
            catch (Exception error) { GD.PrintErr("Herb collection presentation observer failed: " + error.Message); }
        }
    }
    // END herb collection presentation ownership

    /// <inheritdoc />
    public override void _Ready()
    {
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        occurrenceController = GetNode<CharacterTurnWorldEventOccurrenceController>(
            "../CharacterTurnWorldEventOccurrenceController");
        resultSource = GetNode<TemporaryCharacterExaminationResultSource>(
            "../TemporaryCharacterExaminationResultSource");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        safeTargetObjectId = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        // A check fixture only: play collects through HerbExaminationActionController. Checks that
        // install turns directly turn this on.
        SetProcess(false);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (LastExaminationResult is null)
        {
            ProcessExamination();
            return;
        }

        if (LastCollectionOccurrence is null)
        {
            ProcessCollection(LastExaminationResult);
        }
    }

    private void ProcessExamination()
    {
        CharacterTurnWorldEventOccurrence? observed = occurrenceController.LastOccurrence;
        if (observed is not null
            && ReferenceEquals(observed.WorldEvent, worldEventSource.HerbPatchExaminedWorldEvent))
        {
            LastExaminationResult = resultSource.CreateResult(observed, safeTargetObjectId);
            return;
        }

        CharacterTurnActivity? activity = turnController.Activity;
        CharacterTurnResponse? response = activity?.Response;
        CharacterIntention? intention = response?.ImmediateIntention;
        if (activity is null
            || response is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            || intention?.Type != CharacterIntentionType.ExamineObject
            || intention.TargetObjectId != safeTargetObjectId)
        {
            return;
        }

        if (!ReferenceEquals(response, observedResponse))
        {
            CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
            if (response.Speech is { } speech
                && transcript is { Entries.Count: > 0 }
                && ReferenceEquals(transcript.Entries[^1].SpokenStatement, speech))
            {
                observedResponse = response;
            }

            return;
        }

        _ = occurrenceController.TryOccurImmediateIntention(
            activity,
            worldEventSource.HerbPatchExaminedWorldEvent);
    }

    private void ProcessCollection(CharacterExaminationResult result)
    {
        CharacterIntention collection =
            DeterministicPostExaminationIntentionSelector.Select(result);
        if (occurrenceController.TryOccurPostExaminationCollection(
                result,
                collection,
                worldEventSource.HerbPatchCollectedWorldEvent))
        {
            LastCollectionOccurrence = occurrenceController.LastOccurrence;
            NotifyCollection(result);
        }
    }
}

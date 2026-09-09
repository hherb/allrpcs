using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Advances the temporary exact herb-patch examination and safe collection flow.</summary>
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
            || !ReferenceEquals(intention.TargetObjectId, safeTargetObjectId))
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
        }
    }
}

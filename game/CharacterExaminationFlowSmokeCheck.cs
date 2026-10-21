using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the complete deterministic herb-patch examination flow when enabled.</summary>
public partial class CharacterExaminationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_EXAMINATION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string GeneratedMode = "generated";
    private const string PlayerText = "Please examine the flowers and collect them only if safe.";

    private readonly ScriptedExaminationModel generatedModel = new();

    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryCharacterExaminationFlowController flowController = null!;
    private CharacterTurnActivityController turnController = null!;
    private TemporaryCharacterTurnResponseSource responseSource = null!;
    private WorldEventController worldEventController = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private LineEdit input = null!;
    private object? initialWorldObjects;
    private object? initialPerception;
    private int stage;
    private bool useGeneratedResponse;

    /// <inheritdoc />
    public override void _Ready()
    {
        string mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool enabled = mode is EnabledEnvironmentValue or GeneratedMode;
        SetProcess(enabled);
        if (!enabled)
        {
            return;
        }

        useGeneratedResponse = mode == GeneratedMode;
        if (useGeneratedResponse
            && !GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource")
                .TrySupplyModel(generatedModel))
        {
            Fail("Generated examination smoke could not supply its exact scripted model.");
            return;
        }

        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        flowController = GetNode<TemporaryCharacterExaminationFlowController>(
            "../TemporaryCharacterExaminationFlowController");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        responseSource = GetNode<TemporaryCharacterTurnResponseSource>(
            "../TemporaryCharacterTurnResponseSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldEventSource = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            // Direct installs bypass the conversation coordinator, so they drive the legacy
            // fixture; generated turns take the normal path, which never runs it.
            flowController.SetProcess(!useGeneratedResponse);
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            input.Text = PlayerText;
            GetNode<Healer>("../Healer").Visible = true; // Explicit present listener in this disposable fixture.
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (useGeneratedResponse)
            {
                if (generatedModel.ReceivedRequest is null)
                {
                    return;
                }

                stage = 2;
                return;
            }

            CharacterTurnActivity? activity = turnController.Activity;
            if (activity is null
                || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
            {
                Fail("Focused examination scaffolding did not observe its awaiting turn.");
                return;
            }

            CharacterTurnResponse response = responseSource.CreateResponse(
                activity.Request,
                GetNode<TemporaryWorldObjectSource>(
                    "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id);
            if (!turnController.TryReceiveResponse(response))
            {
                Fail(
                    "Focused examination scaffolding could not install its deterministic "
                    + "response.");
                return;
            }

            stage = 2;
            return;
        }

        if (useGeneratedResponse)
        {
            VerifyGeneratedDispatch();
            return;
        }

        if (flowController.LastCollectionOccurrence is null)
        {
            return;
        }

        SetProcess(false);
        CharacterExaminationResult? result = flowController.LastExaminationResult;
        CharacterTurnWorldEventOccurrence collection = flowController.LastCollectionOccurrence;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        if (result is null
            || !ReferenceEquals(result.Safety, CharacterExaminationSafety.SafeToCollect)
            || !ReferenceEquals(result.Occurrence.WorldEvent, worldEventSource.HerbPatchExaminedWorldEvent)
            || !ReferenceEquals(collection.WorldEvent, worldEventSource.HerbPatchCollectedWorldEvent)
            || !ReferenceEquals(result.TargetObjectId, collection.TranscriptEntry.PhysicalAction?.TargetObjectId)
            || collection.TranscriptEntry.PhysicalAction?.Type != CharacterIntentionType.CollectObject
            || transcript is not { Entries.Count: 4 }
            || transcript.Entries[0].SpokenStatement?.Text != PlayerText
            || transcript.Entries[1].SpokenStatement?.Text
                != DeterministicExaminationResponseSelector.ExaminationSpeech
            || !ReferenceEquals(transcript.Entries[2], result.Occurrence.TranscriptEntry)
            || !ReferenceEquals(transcript.Entries[3], collection.TranscriptEntry)
            || eventLog is not { Entries.Count: 3 }
            || !ReferenceEquals(eventLog.Entries[1], result.Occurrence.WorldEvent.EventId)
            || !ReferenceEquals(eventLog.Entries[2], collection.WorldEvent.EventId)
            || !ReferenceEquals(worldEventController.WorldTime?.CurrentEventId, collection.WorldEvent.EventId)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception))
        {
            Fail("Live examination flow failed exact ordered safe collection containment.");
            return;
        }

        GD.Print(
            "Character examination flow smoke passed: exact player and healer speech, "
            + "examination, authoritative safe result, and collection occurred in order "
            + "while world objects and perception remained unchanged.");
        GetTree().Quit();
    }

    // The walk, examination and collection of the normal path are proven by
    // IngredientRemedyFlowCheck; this proves the model boundary up to its dispatch.
    private void VerifyGeneratedDispatch()
    {
        HerbExaminationActionController action =
            GetNode<HerbExaminationActionController>("../HerbExaminationActionController");
        // Offer sets the status; it stays even if the walk then fails in the same frame.
        if (action.Status.Length == 0)
        {
            return;
        }

        SetProcess(false);
        if (!ProvesGeneratedResponseBoundary()
            || flowController.LastExaminationResult is not null
            || action.LastResult is not null)
        {
            Fail("Generated examination did not enter only the normal examination path.");
            return;
        }

        GD.Print(
            "Generated character examination flow smoke passed: one validated conditional model "
            + "response used a distinct value-equal known target and entered only the normal "
            + "examination path.");
        GetTree().Quit();
    }

    private bool ProvesGeneratedResponseBoundary()
    {
        CharacterModelGenerationRequest? request = generatedModel.ReceivedRequest;
        CharacterTurnResponse? response = generatedModel.Response;
        ObjectId authoritativeTarget = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id;
        CharacterModelStreamTermination? termination = GetNode<CharacterModelGenerationFlowController>(
            "../CharacterModelGenerationFlowController").Result?.Updates[^1]
            as CharacterModelStreamTermination;
        CharacterModelContextEntry[] affordances = request?.Context.Entries
            .Where(entry => entry.Kind == CharacterModelContextEntryKind.ValidAffordance)
            .ToArray() ?? [];
        ObjectId? proposedTarget = response?.ImmediateIntention.TargetObjectId;

        return generatedModel.CallCount == 1
            && request is not null
            && response is not null
            && termination is
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Failure: null,
            }
            && ReferenceEquals(termination.Response, response)
            && ReferenceEquals(response.Request, request.TurnRequest)
            && ReferenceEquals(
                response.ImmediateIntention.Type,
                CharacterIntentionType.ExamineObject)
            && proposedTarget == authoritativeTarget
            && !ReferenceEquals(proposedTarget, authoritativeTarget)
            && affordances.Length == 1
            && affordances[0].Text == "type=examine-object;target=herb-patch"
            && request.Context.Entries.All(
                entry => !entry.Text.Contains("stone", StringComparison.OrdinalIgnoreCase)
                    && !entry.Text.Contains("secret", StringComparison.OrdinalIgnoreCase));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class ScriptedExaminationModel : ICharacterModel
    {
        public int CallCount { get; private set; }

        public CharacterModelGenerationRequest? ReceivedRequest { get; private set; }

        public CharacterTurnResponse? Response { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            ReceivedRequest = request;
            Response = new CharacterTurnResponse(
                request.TurnRequest,
                CharacterTurnDisposition.ConditionallyAccept,
                new DialogueDelivery(
                    new DialogueUtterance(
                        request.TurnRequest.RespondingCharacterId,
                        DeterministicExaminationResponseSelector.ExaminationSpeech),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch")),
                [],
                []);

            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(
                DeterministicExaminationResponseSelector.ExaminationSpeech);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                Response,
                null);
        }
    }
}

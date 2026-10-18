using System.Reflection;
using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Deterministic Main ownership failures, with no configured provider or microphone.</summary>
public partial class DialogueOwnershipMainSmokeCheck : Node
{
    private DialogueInputController input = null!;
    private LineEdit edit = null!;
    private Label status = null!;
    private Main main = null!;
    private CharacterTurnParticipationRoundTaskController round = null!;
    private CharacterTurnActivityController turn = null!;
    private DialogueEventController events = null!;
    private CharacterTurnTranscriptController transcript = null!;
    private string mode = "";

    public override async void _Ready()
    {
        mode = OS.GetEnvironment("ALLRPCS_DIALOGUE_OWNERSHIP_CHECK");
        if (mode.Length == 0) { return; }
        await Frames(5);
        try
        {
            main = GetParent<Main>();
            input = GetNode<DialogueInputController>("../DialogueInputController");
            edit = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
            status = GetNode<Label>("../DialogueOverlay/Panel/Status");
            round = GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController");
            turn = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
            _ = GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource").TryClearBindings();
            events = GetNode<DialogueEventController>("../DialogueEventController");
            transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
            var healer = GetNode<CharacterBody2D>("../Healer");
            var seeker = GetNode<CharacterBody2D>("../Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false);
            healer.Visible = true; // This disposable dialogue fixture supplies a present listener.
            seeker.Position = healer.Position + new Vector2(12, 0);
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = true;
            if (mode == "pending")
            {
                Submit("first owned submission");
                DialogueDelivery first = input.LastSubmittedDelivery!;
                Submit("second editable draft");
                Require(ReferenceEquals(input.LastSubmittedDelivery, first), "Same-frame submission overwrote accepted delivery");
                Require(edit.Text == "second editable draft" && status.Text.Contains("BUSY"), "Busy rejection erased draft or hid status");
                await Frames(5);
                Require(transcript.Transcript!.Entries.Count(entry => ReferenceEquals(entry.Delivery, first)) == 1,
                    "Accepted delivery did not occur exactly once");
            }
            else if (mode == "once")
            {
                Submit("side effect fails once");
                main.SetProcess(false);
                // An uninitialized transcript makes the append fail after the delivery occurred.
                PropertyInfo retainedTranscript = typeof(CharacterTurnTranscriptController).GetProperty(nameof(transcript.Transcript))!;
                object retained = retainedTranscript.GetValue(transcript)!;
                retainedTranscript.SetValue(transcript, null);
                try { main.ConversationTurns.ProcessDialogueSubmission(); } catch (InvalidOperationException) { }
                DialogueEvent first = events.LastOccurredEvent!;
                retainedTranscript.SetValue(transcript, retained);
                main.ConversationTurns.ProcessDialogueSubmission();
                Require(ReferenceEquals(events.LastOccurredEvent, first), "Failed side effect retried occurrence");
                Require(!transcript.Transcript!.Entries.Any(entry => ReferenceEquals(entry.Delivery, first.Delivery)), "Failed delivery was replayed");
            }
            else if (mode == "hidden")
            {
                healer.Visible = false;
                Submit("absent listener must not hear");
                await Frames(2);
                var healerId = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
                Require(!events.LastOccurredEvent!.Audience.ListenerCharacterIds.Contains(healerId), "Absent healer heard speech");
                Require(events.History.RecentFor(healerId, events.LastOccurredEvent).Count == 0, "Absent healer retained history");
            }
            else if (mode == "hiddenreply")
            {
                main.SetProcess(false);
                var recorder = GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder");
                var roster = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants;
                healer.Visible = false;
                DialogueEvent heard = new(new DialogueDelivery(new DialogueUtterance(input.SpeakerCharacterId!, "earlier heard request"), DialogueDeliveryMode.OrdinarySpeech),
                    new DialogueAudienceSnapshot(input.SpeakerCharacterId!, roster.Select(character => character.Identity.Id)));
                DialogueDelivery Record(Character character, string text)
                {
                    CharacterTurnRequest request = new(character.Identity.Id, heard, new DialogueAct(heard.Utterance, DialogueActType.Request));
                    Require(turn.TryStart(request), "Fixture turn did not start");
                    DialogueDelivery delivery = new(new DialogueUtterance(character.Identity.Id, text), DialogueDeliveryMode.OrdinarySpeech);
                    Require(turn.TryReceiveResponse(new CharacterTurnResponse(request, CharacterTurnDisposition.Accept, delivery,
                        new CharacterIntention(CharacterIntentionType.Wait, null), [], [])), "Fixture response did not install");
                    recorder._Process(0);
                    return delivery;
                }
                DialogueDelivery absent = Record(roster[0], "absent healer reply");
                Require(!transcript.Transcript!.Entries.Any(entry => ReferenceEquals(entry.Delivery, absent)), "Absent healer recorded speech");
                Require(CharacterTurnStartPolicy.CanStart(turn.Activity), "Skipped hidden speech retained busy turn");
                healer.Visible = true;
                recorder._Process(0);
                Require(!transcript.Transcript!.Entries.Any(entry => ReferenceEquals(entry.Delivery, absent)), "Stale hidden reply replayed on return");
                healer.Visible = false;
                DialogueDelivery sage = Record(roster[1], "sage speaks without absent healer");
                Require(!events.LastOccurredEvent!.Audience.ListenerCharacterIds.Contains(roster[0].Identity.Id), "Absent healer heard sage reply");
                Require(ReferenceEquals(events.LastOccurredEvent.Delivery, sage), "Sage reply was not recorded");
                Require(turn.TryComplete(), "Recorded sage fixture did not complete");
                healer.Visible = true;
                Require(GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource")
                    .TrySupplyModel(new ValidModel()), "Fresh visible participation fixture unavailable");
                SuccessModel visible = new();
                Require(GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource").TrySupplyModel(visible), "Fresh visible model unavailable");
                Submit("fresh visible submission after absence");
                DialogueDelivery accepted = input.LastSubmittedDelivery!;
                Require(accepted.Utterance.Text == "fresh visible submission after absence" && edit.Text.Length == 0,
                    "Hidden turn blocked next visible submission");
                main.SetProcess(true);
                await Wait(() => visible.Delivery is not null && transcript.Transcript!.Entries.Any(entry => ReferenceEquals(entry.Delivery, visible.Delivery)));
                Require(ReferenceEquals(visible.Request?.TurnRequest.DialogueEvent.Delivery, accepted), "Next visible reply consumed wrong submission");
                Require(transcript.Transcript!.Entries.Count(entry => ReferenceEquals(entry.Delivery, visible.Delivery)) == 1, "Next visible healer reply not recorded once");
            }
            else if (mode is "unavailable" or "stopped" or "timeout" or "held" or "invalid")
            {
                var source = GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource");
                TaskCompletionSource<CharacterTurnParticipationDecision> held = new();
                FailureModel failureModel = new(mode, held);
                if (mode == "invalid")
                {
                    // The non-addressed sage must hear; the healer's startup walk left it at the 192 px edge.
                    Vector2 sage = GetNode<Marker2D>("../SageHearingPosition").Position;
                    seeker.Position = sage - new Vector2(96, 0);
                    healer.Position = seeker.Position - new Vector2(12, 0);
                }
                if (mode != "unavailable") { Require(source.TrySupplyModel(failureModel), "Fixture unavailable"); }
                Submit("failure is visible and retryable");
                DialogueDelivery first = input.LastSubmittedDelivery!;
                if (mode == "held")
                {
                    await Frames(2);
                    Submit("held round draft");
                    Require(ReferenceEquals(input.LastSubmittedDelivery, first) && edit.Text == "held round draft"
                        && status.Text.Contains("BUSY"), "Held response accepted another delivery");
                    held.SetException(new InvalidOperationException("synthetic unavailable"));
                }
                await Wait(() => round.Status == CharacterTurnParticipationRoundTaskStatus.Idle);
                Require(status.Text.Contains("retry", StringComparison.OrdinalIgnoreCase), "Provider failure had no visible retry status");
                if (mode == "invalid") { Require(failureModel.RejectedNonAddressed, "Invalid fixture did not exercise non-addressed Respond rejection"); }
                Require(edit.Editable, "Failure disabled editing");
                var map = GetNode<SeekerMapOpenCloseInputController>("../SeekerMapOpenCloseInputController");
                typeof(SeekerMapOpenCloseInputController).GetMethod("TryOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(map, null);
                GetNode<Button>("../CommandOverlay/SaveLoadPanel/Save").EmitSignal(Button.SignalName.Pressed);
                Require(GetNode<Label>("../CommandOverlay/SaveLoadPanel/Status").Text == SaveLoadInputController.SavedStatusText, "Save after failure failed");
                File.Delete(SaveDocumentStorageController.CurrentFilePath());
                Submit("retry after provider failure");
                Require(!ReferenceEquals(first, input.LastSubmittedDelivery), "Failure retained busy ownership");
            }
            else if (mode.StartsWith("generation-", StringComparison.Ordinal))
            {
                Require(GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource")
                    .TrySupplyModel(new ValidModel()), "Participation fixture unavailable");
                TaskCompletionSource<bool> release = new();
                var model = new GenerationFailure(mode, release);
                Require(GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource").TrySupplyModel(model), "Generation fixture unavailable");
                Submit("owned generation failure");
                DialogueDelivery first = input.LastSubmittedDelivery!;
                await Wait(() => model.Started);
                Require(ReferenceEquals(model.Request!.TurnRequest.DialogueEvent.Delivery, first), "Generation consumed another delivery");
                if (mode == "generation-held")
                {
                    Submit("held generation draft");
                    Require(ReferenceEquals(input.LastSubmittedDelivery, first) && edit.Text == "held generation draft"
                        && status.Text.Contains("BUSY"), "Held generation accepted another submission");
                }
                release.SetResult(true);
                await Wait(() => CharacterTurnStartPolicy.CanStart(turn.Activity));
                Require(status.Text.Contains("retry", StringComparison.OrdinalIgnoreCase), "Generation failure missing visible retry status");
                Submit("retry generation");
                Require(!ReferenceEquals(first, input.LastSubmittedDelivery), "Generation failure prevented retry");
            }
            else if (mode == "budget")
            {
                var source = GetNode<TemporaryCharacterModelGenerationInputSource>("../TemporaryCharacterModelGenerationInputSource");
                var participants = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants;
                var excessive = new CharacterIntentionAffordanceSnapshot(Enumerable.Range(0, 128).Select(index =>
                    new CharacterIntentionAffordance(CharacterIntentionType.ExamineObject, new ObjectId($"object-{index}"))));
                var field = typeof(TemporaryCharacterModelGenerationInputSource).GetField("affordances", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var original = (IReadOnlyDictionary<CharacterId, CharacterIntentionAffordanceSnapshot>)field.GetValue(source)!;
                field.SetValue(source, original.ToDictionary(pair => pair.Key, pair => excessive));
                var model = GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource");
                Require(model.TrySupplyModel(new ValidModel()), "Fixture unavailable");
                main.SetProcess(false);
                Submit("budget exhaustion");
                main.ConversationTurns.ProcessDialogueSubmission();
                await Wait(() => turn.Activity?.State == CharacterTurnActivityState.AwaitingResponse);
                main.ConversationTurns.ProcessCharacterTurnResponse();
                Require(status.Text.Contains("context", StringComparison.OrdinalIgnoreCase) && status.Text.Contains("retry", StringComparison.OrdinalIgnoreCase), "Budget failure missing visible status");
                Require(CharacterTurnStartPolicy.CanStart(turn.Activity), "Budget failure retained busy turn");
                field.SetValue(source, original);
                CharacterTurnRequest retryRequest = turn.Activity!.Request;
                Require(turn.TryStart(retryRequest) && source.CreateInput(turn.Activity!) is not null, "Context retry did not recover");
            }
            GD.Print("PASS: dialogue ownership Main: " + mode);
            GetTree().Quit();
        }
        catch (Exception exception) { GD.PushError(mode + ": " + exception); GetTree().Quit(1); }
    }
    private void Submit(string text) { edit.Text = text; edit.EmitSignal(LineEdit.SignalName.TextSubmitted, text); }
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task Wait(Func<bool> predicate) { await Frames(3); for (int i = 0; !predicate(); i++) { Require(i < 120, "Fixture deadline"); await Frames(1); } }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private sealed class FailureModel(string mode, TaskCompletionSource<CharacterTurnParticipationDecision> held) : ICharacterTurnParticipationModel
    {
        internal bool RejectedNonAddressed { get; private set; }
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request, CancellationToken token)
        {
            if (mode == "held") { return new(held.Task.WaitAsync(token)); }
            if (mode == "invalid")
            {
                CharacterTurnParticipationDecision decision = new(request.ScheduledCharacter.Identity.Id, CharacterTurnParticipationKind.Respond, null);
                RejectedNonAddressed |= !request.IsDirectlyAddressed && !CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision);
                return ValueTask.FromResult(decision);
            }
            return ValueTask.FromException<CharacterTurnParticipationDecision>(mode == "timeout"
                ? new TimeoutException("synthetic timeout") : new HttpRequestException("synthetic stopped provider"));
        }
    }
    private sealed class GenerationFailure(string mode, TaskCompletionSource<bool> release) : ICharacterModel
    {
        internal bool Started { get; private set; }
        internal CharacterModelGenerationRequest? Request { get; private set; }
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken token)
        {
            Request = request; Started = true;
            await release.Task.WaitAsync(token);
            yield return mode == "generation-timeout"
                ? new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.TimedOut, null, null)
                : new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Failed, null,
                    new CharacterModelFailure(CharacterModelFailureCode.Unavailable));
        }
    }
    private sealed class SuccessModel : ICharacterModel
    {
        internal CharacterModelGenerationRequest? Request { get; private set; }
        internal DialogueDelivery? Delivery { get; private set; }
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            Request = request;
            Delivery = new(new DialogueUtterance(request.TurnRequest.RespondingCharacterId, "next visible healer reply"), DialogueDeliveryMode.OrdinarySpeech);
            yield return new CharacterModelTextDelta(Delivery.Utterance.Text);
            yield return new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Completed,
                new CharacterTurnResponse(request.TurnRequest, CharacterTurnDisposition.Accept, Delivery,
                    new CharacterIntention(CharacterIntentionType.Wait, null), [], []), null);
        }
    }
    private sealed class ValidModel : ICharacterTurnParticipationModel
    {
        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request, CancellationToken token) => ValueTask.FromResult(
            new CharacterTurnParticipationDecision(request.ScheduledCharacter.Identity.Id,
                request.IsDirectlyAddressed ? CharacterTurnParticipationKind.Respond : CharacterTurnParticipationKind.RemainSilent,
                request.IsDirectlyAddressed ? null : request.ScheduledCharacter.Goals.Entries[0]));
    }
}

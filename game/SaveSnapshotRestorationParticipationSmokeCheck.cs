using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

public partial class SaveSnapshotRestorationControllerSmokeCheck
{
    private async Task CheckHeldParticipationRestoreAsync()
    {
        try
        {
            TemporaryCharacterTurnParticipationModelSource source =
                GetNode<TemporaryCharacterTurnParticipationModelSource>("../TemporaryCharacterTurnParticipationModelSource");
            CharacterTurnParticipationRoundTaskController round =
                GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController");
            CharacterTurnTranscriptController transcript =
                GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
            HeldRestoreParticipation model = new();
            if (!source.TrySupplyModel(model)) { throw new InvalidOperationException("Supply held model"); }
            SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
            _ = map.TryInitialize(discoveryController.SearcherCharacterId);
            _ = SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routeProgress, map,
                GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource"));
            _ = GetNode<RepresentativeJournalCompositionCoordinator>(
                "../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize();
            seeker.GlobalPosition = GetNode<Healer>("../Healer").GlobalPosition + new Vector2(12, 0);
            var snapshot = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController").Capture()!;
            LineEdit input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
            input.Text = "Please answer after I load.";
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, input.Text);
            for (int frame = 0; frame < 60 && model.Request is null; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (model.Request is null || round.Status != CharacterTurnParticipationRoundTaskStatus.Running)
            { throw new InvalidOperationException("Held participation request starts"); }
            object? priorTranscript = transcript.Transcript;
            object? priorBudget = GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController").Budget;
            if (!CheckPreparationFailurePreservesState(snapshot)
                || round.Status != CharacterTurnParticipationRoundTaskStatus.Running
                || !ReferenceEquals(priorTranscript, transcript.Transcript)
                || !ReferenceEquals(priorBudget, GetNode<AutonomousConversationBudgetController>(
                    "../AutonomousConversationBudgetController").Budget))
            { throw new InvalidOperationException("Failed final preparation changed active session"); }
            if (!controller.TryRestore(snapshot)) { throw new InvalidOperationException("Restore held participation"); }
            model.Release.TrySetResult();
            for (int frame = 0; frame < 12; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (round.Status != CharacterTurnParticipationRoundTaskStatus.Idle || round.Result is not null
                || transcript.Transcript is not { Entries.Count: 0 }
                || GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity is not null
                || GetNode<CharacterModelGenerationFlowController>("../CharacterModelGenerationFlowController").HasStarted
                || GetNode<AutonomousConversationBudgetController>("../AutonomousConversationBudgetController").Budget is not null)
            { throw new InvalidOperationException($"Late participation completion revived the restored session: {round.Status}, transcript={transcript.Transcript?.Entries.Count}, activity={GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController").Activity?.State}"); }
            HeldRestoreParticipation next = new();
            if (!source.TrySupplyModel(next)) { throw new InvalidOperationException("Supply next model"); }
            input.Text = "A fresh request after loading.";
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, input.Text);
            for (int frame = 0; frame < 60 && next.Request is null; frame++)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (next.Request is null || transcript.Transcript is not { Entries.Count: 1 })
            { throw new InvalidOperationException("Next submission after restore failed"); }
            if (!controller.TryRestore(snapshot)) { throw new InvalidOperationException("Final cleanup restore"); }
            next.Release.TrySetResult();
            GD.Print("Save restoration controller smoke passed: held participation retired, late completion fenced, next submission accepted.");
            GetTree().Quit();
        }
        catch (Exception exception) { Fail("Restore participation regression: " + exception.Message); }
    }

    private sealed class HeldRestoreParticipation : ICharacterTurnParticipationModel
    {
        internal TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal CharacterTurnParticipationConsiderationRequest? Request { get; private set; }

        public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request, CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            Request = request;
            await Release.Task;
            return new CharacterTurnParticipationDecision(request.ScheduledCharacter.Identity.Id,
                CharacterTurnParticipationKind.Respond, null);
        }
    }
}

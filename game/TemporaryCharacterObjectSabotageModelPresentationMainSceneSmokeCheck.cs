using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicitly started model-selected concealment presentation in Main.</summary>
public partial class TemporaryCharacterObjectSabotageModelPresentationMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_MODEL_PRESENTATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private CharacterObjectSabotageConcealmentPresentationController presentation = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator coordinator = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource modelSource = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;
    private TemporaryCharacterObjectSabotageConsiderationFlowController flow = null!;
    private Node2D body = null!;
    private WorldObjectConcealmentSnapshot initialConcealment = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private WorldObjectConcealmentSnapshot selectedConcealment = null!;
    private DiscoveryEvidenceSnapshot selectedEvidence = null!;
    private WorldObjectSnapshot worldObjects = null!;
    private Vector2 initialPosition;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        presentation = GetNode<CharacterObjectSabotageConcealmentPresentationController>(
            "../CharacterObjectSabotageConcealmentPresentationController");
        coordinator = GetNode<CharacterObjectSabotageConcealmentPresentationCoordinator>(
            "../CharacterObjectSabotageConcealmentPresentationCoordinator");
        modelSource = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        flow = GetNode<TemporaryCharacterObjectSabotageConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageConsiderationFlowController");
        body = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        initialConcealment = source.Concealment;
        initialEvidence = source.Evidence;
        worldObjects = source.WorldObjects;
        initialPosition = body.Position;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene model concealment presentation exceeded its smoke duration.");
            return;
        }

        if (stage == 0)
        {
            StartSelection();
            return;
        }

        if (stage == 1)
        {
            ObserveSelectionAndStartAbstention();
            return;
        }

        ObserveAbstention();
    }

    private void StartSelection()
    {
        bool inert = task.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && task.Result is null
            && body.Visible
            && ReferenceEquals(state.Concealment, initialConcealment)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && ReferenceEquals(presentation.LastPresentedConcealment, initialConcealment);
        SelectingModel model = new();
        if (!inert
            || !modelSource.TrySupplyModel(model)
            || !flow.TryStart()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            Fail("Main scene did not remain inert before the explicit selected decision.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        selectedConcealment = state.Concealment!;
        selectedEvidence = state.Evidence!;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { SelectedCandidate: not null } result
            || body.Visible
            || !ReferenceEquals(presentation.LastPresentedConcealment, selectedConcealment)
            || !ReferenceEquals(result.Request.Concealment, initialConcealment)
            || !ReferenceEquals(result.Request.Evidence, initialEvidence)
            || !ReferenceEquals(result.SelectedCandidate, source.Candidate)
            || !ReferenceEquals(
                selectedConcealment.ConcealedObjectIds.Single(),
                source.Candidate.Evidence.ObjectId)
            || !ReferenceEquals(selectedEvidence.Entries.Single(), source.Candidate.Evidence)
            || coordinator.TryPresentCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || task.Result is not null
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Concealment, initialConcealment)
            || !ReferenceEquals(source.Evidence, initialEvidence)
            || body.Position != initialPosition)
        {
            Fail("The exact selected result did not hide only the main-scene water cask once.");
            return;
        }

        if (!modelSource.TrySupplyModel(new AbstainingModel())
            || !flow.TryStart()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            Fail("A later explicit main-scene decision did not start after acknowledgement.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstention()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || coordinator.TryPresentCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || task.Result is not null
            || body.Visible
            || !ReferenceEquals(state.Concealment, selectedConcealment)
            || !ReferenceEquals(state.Evidence, selectedEvidence)
            || !ReferenceEquals(presentation.LastPresentedConcealment, selectedConcealment)
            || !ReferenceEquals(source.WorldObjects, worldObjects)
            || !ReferenceEquals(source.Concealment, initialConcealment)
            || !ReferenceEquals(source.Evidence, initialEvidence)
            || body.Position != initialPosition)
        {
            Fail("The later acknowledged abstention changed main-scene source or presentation state.");
            return;
        }

        GD.Print(
            "Main-scene model concealment presentation smoke passed: one exact selected result was "
            + "acknowledged, a later abstention completed and was acknowledged, and source, "
            + "world-object, presentation, and position state remained exact.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class SelectingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(
                new CharacterObjectSabotageConcealmentDecision(
                    request.Character.Identity.Id,
                    request.Candidates.Single().Evidence.EvidenceId));
        }
    }

    private sealed class AbstainingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(
                new CharacterObjectSabotageConcealmentDecision(request.Character.Identity.Id, null));
        }
    }
}

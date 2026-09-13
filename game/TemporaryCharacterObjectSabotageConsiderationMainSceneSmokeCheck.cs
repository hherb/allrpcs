using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks inert main-scene concealment consideration wiring and one explicit abstention.</summary>
public partial class TemporaryCharacterObjectSabotageConsiderationMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource modelSource = null!;
    private TemporaryCharacterObjectSabotageConsiderationFlowController flow = null!;
    private AbstainingModel? model;
    private WorldObjectConcealmentSnapshot initialConcealment = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
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
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        flow = GetNode<TemporaryCharacterObjectSabotageConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageConsiderationFlowController");
        initialConcealment = source.Concealment;
        initialEvidence = source.Evidence;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene concealment consideration did not finish within its smoke duration.");
            return;
        }

        if (stage == 0)
        {
            StartExplicitAbstention();
            return;
        }

        ObserveExplicitAbstention();
    }

    private void StartExplicitAbstention()
    {
        bool remainedInert = ReferenceEquals(state.Concealment, initialConcealment)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && initialConcealment.ConcealedObjectIds.Count == 0
            && initialEvidence.Entries.Count == 0
            && task.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && task.Result is null;
        model = new AbstainingModel();
        if (!remainedInert
            || !modelSource.TrySupplyModel(model)
            || !flow.TryStart()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            Fail("Main-scene concealment consideration was not inert before explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveExplicitAbstention()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        CharacterObjectSabotageConcealmentConsiderationRequest? request = model!.Request;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null } result
            || request is null
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Concealment, initialConcealment)
            || !ReferenceEquals(request.Evidence, initialEvidence)
            || !ReferenceEquals(request.Candidates.Single(), source.Candidate)
            || !ReferenceEquals(state.Concealment, initialConcealment)
            || !ReferenceEquals(state.Evidence, initialEvidence)
            || source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("Main-scene concealment consideration failed exact abstention resolution.");
            return;
        }

        GD.Print(
            "Main-scene concealment consideration smoke passed: startup remained inert, exact "
            + "authored state reached one explicitly supplied model, and accepted abstention "
            + "retained exact scene and source snapshots without automatic sabotage.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class AbstainingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public CharacterObjectSabotageConcealmentConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return ValueTask.FromResult(
                new CharacterObjectSabotageConcealmentDecision(
                    request.Character.Identity.Id,
                    null));
        }
    }
}

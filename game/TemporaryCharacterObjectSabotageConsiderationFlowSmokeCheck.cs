using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit authored concealment consideration through the scene task.</summary>
public partial class TemporaryCharacterObjectSabotageConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private TemporaryCharacterObjectSabotageConcealmentModelSource modelSource = null!;
    private CharacterObjectSabotageConcealmentTaskController task = null!;
    private TemporaryCharacterObjectSabotageConsiderationFlowController flow = null!;
    private RecordingModel? selectionModel;
    private RecordingModel? overlapModel;
    private WorldObjectConcealmentSnapshot initialConcealment = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private WorldObjectConcealmentSnapshot? appliedConcealment;
    private DiscoveryEvidenceSnapshot? appliedEvidence;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        state = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageConcealmentModelSource>(
            "../TemporaryCharacterObjectSabotageConcealmentModelSource");
        task = GetNode<CharacterObjectSabotageConcealmentTaskController>(
            "../CharacterObjectSabotageConcealmentTaskController");
        flow = GetNode<TemporaryCharacterObjectSabotageConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageConsiderationFlowController");
        initialConcealment = source.Concealment;
        initialEvidence = source.Evidence;
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Temporary concealment consideration did not finish within its smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartExactSelection();
                break;
            case 1:
                ObserveSelectionAndStartOfflineFailure();
                break;
            case 2:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Temporary concealment consideration entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactSelection()
    {
        selectionModel = new RecordingModel(request => new(
            request.Character.Identity.Id,
            request.Candidates.Single().Evidence.EvidenceId));
        overlapModel = new RecordingModel(request => new(request.Character.Identity.Id, null));

        bool unchangedBeforeRequest = ReferenceEquals(state.Concealment, initialConcealment)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && task.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && task.Result is null;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running;
        bool overlapRetained = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!unchangedBeforeRequest
            || !started
            || !overlapRetained
            || !HasClosedSurface()
            || !ThrowsArgumentNull(() => modelSource.TrySupplyModel(null!)))
        {
            Fail("Temporary concealment consideration failed explicit start or overlap handling.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        appliedConcealment = state.Concealment;
        appliedEvidence = state.Evidence;
        CharacterObjectSabotageConcealmentConsiderationRequest? request = selectionModel!.Request;
        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { } result
            || request is null
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Concealment, initialConcealment)
            || !ReferenceEquals(request.Evidence, initialEvidence)
            || !ReferenceEquals(request.Candidates.Single(), source.Candidate)
            || appliedConcealment is null
            || appliedEvidence is null
            || !ReferenceEquals(
                appliedConcealment.ConcealedObjectIds.Single(),
                source.Candidate.Occurrence.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), source.Candidate.Evidence)
            || !ReferenceEquals(modelSource.TakeModel(), overlapModel)
            || overlapModel!.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart())
        {
            Fail("Temporary concealment consideration failed exact request or resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Concealment, appliedConcealment)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || source.Concealment.ConcealedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("Temporary concealment consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Temporary concealment consideration flow smoke passed: no automatic effect, exact "
            + "request composition, one-shot decision, atomic main-thread resolution, overlap "
            + "preservation, stable offline failure, closed surface, and unchanged source state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageConsiderationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterObjectSabotageConcealmentConsiderationRequest,
            CharacterObjectSabotageConcealmentDecision> decide)
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageConcealmentConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}

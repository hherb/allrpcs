using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit authored move consideration through the scene task.</summary>
public partial class TemporaryCharacterObjectSabotageMoveConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageMoveSource source = null!;
    private CharacterObjectSabotageMoveController state = null!;
    private TemporaryCharacterObjectSabotageMoveModelSource modelSource = null!;
    private CharacterObjectSabotageMoveTaskController task = null!;
    private TemporaryCharacterObjectSabotageMoveConsiderationFlowController flow = null!;
    private CharacterObjectSabotageMovePresentationController presentation = null!;
    private CharacterObjectSabotageMovePresentationCoordinator coordinator = null!;
    private Node2D body = null!;
    private RecordingModel? selectionModel;
    private RecordingModel? overlapModel;
    private WorldObjectPlacementSnapshot initialPlacements = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private WorldObjectPlacementSnapshot? appliedPlacements;
    private DiscoveryEvidenceSnapshot? appliedEvidence;
    private Vector2 initialPosition;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
            "../TemporaryCharacterObjectSabotageMoveModelSource");
        task = GetNode<CharacterObjectSabotageMoveTaskController>(
            "../CharacterObjectSabotageMoveTaskController");
        flow = GetNode<TemporaryCharacterObjectSabotageMoveConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageMoveConsiderationFlowController");
        presentation = GetNode<CharacterObjectSabotageMovePresentationController>(
            "../CharacterObjectSabotageMovePresentationController");
        coordinator = GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
            "../CharacterObjectSabotageMovePresentationCoordinator");
        body = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        initialPlacements = source.Placements;
        initialEvidence = source.Evidence;
        initialPosition = body.Position;
        coordinator.SetProcess(false);
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Temporary move consideration did not finish within its smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartExactSelection();
                break;
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Temporary move consideration entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactSelection()
    {
        selectionModel = new RecordingModel(request => new(
            request.Character.Identity.Id,
            request.Candidates.Single().Evidence.Evidence.EvidenceId));
        overlapModel = new RecordingModel(request => new(request.Character.Identity.Id, null));

        bool unchangedBeforeRequest = ReferenceEquals(state.Placements, initialPlacements)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
            && task.Result is null
            && ReferenceEquals(presentation.LastPresentedPlacements, initialPlacements)
            && !coordinator.TryPresentCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Running;
        bool overlapRetained = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!unchangedBeforeRequest
            || !started
            || !overlapRetained
            || !HasClosedSurface())
        {
            Fail("Temporary move consideration failed explicit start or overlap handling.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        appliedPlacements = state.Placements;
        appliedEvidence = state.Evidence;
        CharacterObjectSabotageMoveConsiderationRequest? request = selectionModel!.Request;
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { } result
            || request is null
            || appliedPlacements is null
            || appliedEvidence is null)
        {
            Fail("Temporary move consideration did not produce a complete selected result.");
            return;
        }

        if (coordinator.TryAcknowledgeCompletedResult()
            || !coordinator.TryPresentCompletedResult()
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Placements, initialPlacements)
            || !ReferenceEquals(request.Evidence, initialEvidence)
            || !ReferenceEquals(request.Candidates.Single(), source.Candidate)
            || !ReferenceEquals(
                appliedPlacements.Entries.Single().WorldObject,
                source.Candidate.Destination.Occurrence.Target)
            || !ReferenceEquals(appliedPlacements.Entries.Single().Location, source.Destination)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), source.Candidate.Evidence.Evidence)
            || !ReferenceEquals(presentation.LastPresentedPlacements, appliedPlacements)
            || body.Position == initialPosition
            || coordinator.TryPresentCompletedResult()
            || overlapModel!.CallCount != 0
            || !presentation.TryPresent(appliedPlacements, initialPlacements)
            || body.Position != initialPosition
            || coordinator.TryAcknowledgeCompletedResult()
            || !presentation.TryPresent(initialPlacements, appliedPlacements)
            || body.Position == initialPosition
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || !flow.TryStart())
        {
            Fail("Temporary move consideration failed exact request or resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel!.CallCount != 1
            || coordinator.TryPresentCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || !ReferenceEquals(state.Placements, appliedPlacements)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(presentation.LastPresentedPlacements, appliedPlacements)
            || body.Position == initialPosition
            || !flow.TryStart())
        {
            Fail("Temporary move abstention changed exact presentation or scene state.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Placements, appliedPlacements)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(presentation.LastPresentedPlacements, appliedPlacements)
            || body.Position == initialPosition
            || !ReferenceEquals(source.Placements.Entries.Single().Location, source.SourceLocation)
            || source.Evidence.Entries.Count != 0)
        {
            Fail("Temporary move consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Temporary move consideration flow smoke passed: no automatic effect, exact request "
            + "composition, one-shot decision, atomic main-thread resolution, overlap preservation, "
            + "one-time selected presentation, inert abstention, stable offline failure, closed "
            + "surface, exact explicit acknowledgement, and unchanged source state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageMoveConsiderationFlowController);
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterObjectSabotageMoveConsiderationRequest, CharacterObjectSabotageMoveDecision> decide)
        : ICharacterObjectSabotageMoveDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageMoveConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}

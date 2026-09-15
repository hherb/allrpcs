using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored contamination consideration in the main scene.</summary>
public partial class TemporaryCharacterObjectSabotageContaminationConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_CONTAMINATION_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageContaminationSource source = null!;
    private CharacterObjectSabotageContaminationController state = null!;
    private TemporaryCharacterObjectSabotageContaminationModelSource modelSource = null!;
    private CharacterObjectSabotageContaminationTaskController task = null!;
    private TemporaryCharacterObjectSabotageContaminationConsiderationFlowController flow = null!;
    private CharacterObjectSabotageContaminationPresentationController presentation = null!;
    private CharacterObjectSabotageContaminationPresentationCoordinator coordinator = null!;
    private CanvasItem contaminationMarker = null!;
    private RecordingModel? selectionModel;
    private RecordingModel? overlapModel;
    private WorldObjectContaminationSnapshot initialContamination = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private WorldObjectContaminationSnapshot? appliedContamination;
    private DiscoveryEvidenceSnapshot? appliedEvidence;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        if (!isEnabled)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
            "../TemporaryCharacterObjectSabotageContaminationSource");
        state = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageContaminationModelSource>(
            "../TemporaryCharacterObjectSabotageContaminationModelSource");
        task = GetNode<CharacterObjectSabotageContaminationTaskController>(
            "../CharacterObjectSabotageContaminationTaskController");
        flow = GetNode<TemporaryCharacterObjectSabotageContaminationConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageContaminationConsiderationFlowController");
        presentation = GetNode<CharacterObjectSabotageContaminationPresentationController>(
            "../CharacterObjectSabotageContaminationPresentationController");
        coordinator = GetNode<CharacterObjectSabotageContaminationPresentationCoordinator>(
            "../CharacterObjectSabotageContaminationPresentationCoordinator");
        contaminationMarker = GetNode<CanvasItem>(
            "../InteractionCandidates/WaterCask/ContaminationMarker");
        initialContamination = source.Contamination;
        initialEvidence = source.Evidence;
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
            Fail("Temporary contamination consideration did not finish within its smoke duration.");
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
                Fail("Temporary contamination consideration entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactSelection()
    {
        selectionModel = new RecordingModel(request => new(
            request.Character.Identity.Id,
            request.Candidates.Single().Evidence.EvidenceId));
        overlapModel = new RecordingModel(request => new(request.Character.Identity.Id, null));

        bool unchangedBeforeRequest = ReferenceEquals(state.Contamination, initialContamination)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && source.WorldObjects.Entries.Count == 3
            && ReferenceEquals(source.Candidate.Occurrence.Target, source.WorldObjects.Entries[^1])
            && source.Candidate.Occurrence.Target.Identity.Id.Value == "village-water-cask"
            && CharacterObjectSabotageContaminationPolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                initialContamination,
                initialEvidence,
                source.Candidate)
            && task.Status == CharacterObjectSabotageContaminationTaskStatus.Idle
            && task.Result is null
            && ReferenceEquals(presentation.LastPresentedContamination, initialContamination)
            && !contaminationMarker.Visible
            && !coordinator.TryPresentCompletedResult()
            && !coordinator.TryAcknowledgeCompletedResult();
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterObjectSabotageContaminationTaskStatus.Running;
        bool overlapRetained = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!unchangedBeforeRequest
            || !started
            || !overlapRetained
            || !HasClosedSourceSurface()
            || !HasClosedFlowSurface()
            || !HasClosedCoordinatorSurface())
        {
            Fail("Temporary contamination consideration failed explicit start or overlap handling.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        appliedContamination = state.Contamination;
        appliedEvidence = state.Evidence;
        CharacterObjectSabotageContaminationConsiderationRequest? request = selectionModel!.Request;
        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || task.Result is not { } result
            || request is null
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Contamination, initialContamination)
            || !ReferenceEquals(request.Evidence, initialEvidence)
            || !ReferenceEquals(request.Candidates.Single(), source.Candidate)
            || appliedContamination is null
            || appliedEvidence is null
            || !ReferenceEquals(
                appliedContamination.ContaminatedObjectIds.Single(),
                source.Candidate.Occurrence.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), source.Candidate.Evidence)
            || overlapModel!.CallCount != 0
            || coordinator.TryAcknowledgeCompletedResult()
            || !coordinator.TryPresentCompletedResult()
            || !ReferenceEquals(presentation.LastPresentedContamination, appliedContamination)
            || !contaminationMarker.Visible
            || coordinator.TryPresentCompletedResult()
            || !presentation.TryPresent(appliedContamination, initialContamination)
            || contaminationMarker.Visible
            || coordinator.TryAcknowledgeCompletedResult()
            || !presentation.TryPresent(initialContamination, appliedContamination)
            || !contaminationMarker.Visible
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart())
        {
            Fail("Temporary contamination consideration failed exact request or resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel!.CallCount != 1
            || coordinator.TryPresentCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(state.Contamination, appliedContamination)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(presentation.LastPresentedContamination, appliedContamination)
            || !contaminationMarker.Visible
            || !flow.TryStart())
        {
            Fail("Temporary contamination abstention changed exact scene state.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Contamination, appliedContamination)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(presentation.LastPresentedContamination, appliedContamination)
            || !contaminationMarker.Visible
            || coordinator.TryPresentCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || source.Contamination.ContaminatedObjectIds.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("Temporary contamination consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Temporary contamination consideration main-scene smoke passed: inert startup, exact "
            + "request composition, one-shot selection and abstention, atomic main-thread resolution, "
            + "overlap preservation, stable offline failure, one-time presentation, exact "
            + "acknowledgement, closed surface, and unchanged source.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageContaminationSource);
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        return properties.SequenceEqual(
                ["Candidate", "Character", "Contamination", "Evidence", "PhysicalAccess", "WorldObjects"])
            && methods.SequenceEqual(["_Ready"]);
    }

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageContaminationConsiderationFlowController);
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

    private static bool HasClosedCoordinatorSurface()
    {
        Type type = typeof(CharacterObjectSabotageContaminationPresentationCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(
                ["TryAcknowledgeCompletedResult", "TryPresentCompletedResult", "_Process", "_Ready"])
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
        Func<CharacterObjectSabotageContaminationConsiderationRequest,
            CharacterObjectSabotageContaminationDecision> decide)
        : ICharacterObjectSabotageContaminationDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageContaminationConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}

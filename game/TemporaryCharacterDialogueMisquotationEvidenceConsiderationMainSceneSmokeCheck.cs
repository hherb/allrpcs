using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored misquotation-evidence consideration in the main scene.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_DIALOGUE_MISQUOTATION_EVIDENCE_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController flow = null!;
    private CharacterDialogueMisquotationEvidenceExposureController exposure = null!;
    private CharacterDialogueMisquotationEvidenceExposureCoordinator coordinator = null!;
    private CharacterDialogueMisquotationEvidenceViewController view = null!;
    private CharacterDialogueMisquotationEvidencePresentationSelectionController presentation =
        null!;
    private CharacterDialogueMisquotationEvidencePresentationController presentationProjection =
        null!;
    private TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController
        presentationFlow = null!;
    private CharacterDialogueMisquotationEvidenceExposureSnapshot? selectedExposure;
    private RecordingModel? selectionModel;
    private RecordingModel? overlapModel;
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

        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        flow = GetNode<TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController>(
            "../TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController");
        exposure = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        coordinator = GetNode<CharacterDialogueMisquotationEvidenceExposureCoordinator>(
            "../CharacterDialogueMisquotationEvidenceExposureCoordinator");
        view = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        presentation =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        presentationProjection =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        presentationFlow =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController");
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene misquotation-evidence consideration exceeded its smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
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
                Fail("Main-scene misquotation-evidence consideration entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        selectionModel = new RecordingModel(request => new(
            request.Actor.Identity.Id,
            request.Candidates.Single()));
        overlapModel = new RecordingModel(request => new(request.Actor.Identity.Id, null));
        CharacterDialogueMisquotationEvidenceViewSnapshot? initialView = view.View;

        bool inertStartup = task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && task.Result is null
            && exposure.Exposure is { Entries.Count: 0 }
            && exposure.Exposure is { } initialExposure
            && ReferenceEquals(view.SourceExposure, initialExposure)
            && initialView is { Entries.Count: 0 }
            && ReferenceEquals(initialView.WitnessCharacterId, source.WitnessCharacterId)
            && PresentationSelectionIsInert()
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0
            && SourceIsUnchanged();
        bool abstained = initialView is not null
            && presentationFlow.TryChoose(initialView, null)
            && presentation.Selection is { SelectedEvidence: null } abstention
            && ReferenceEquals(abstention.WitnessView, initialView)
            && presentationProjection.Presentation is null;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!inertStartup
            || !abstained
            || !started
            || !overlapPreserved
            || !HasClosedSurfaces())
        {
            Fail("Main-scene misquotation-evidence consideration failed explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        CharacterDialogueMisquotationEvidenceConsiderationRequest? request = selectionModel!.Request;
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { } result
            || request is null
            || !RequestRetainsExactSource(request)
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedEvidence, source.Candidate)
            || selectionModel.CallCount != 1
            || overlapModel!.CallCount != 0
            || !SourceIsUnchanged()
            || exposure.Exposure is not { Entries.Count: 1 } currentExposure
            || !ReferenceEquals(currentExposure.Entries.Single(), source.Candidate)
            || !ViewRetainsExactSource(currentExposure)
            || !PresentationSelectionIsInert()
            || coordinator.TryExposeCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart())
        {
            Fail("Main-scene misquotation-evidence consideration failed exact selection.");
            return;
        }

        selectedExposure = currentExposure;
        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        CharacterDialogueMisquotationEvidenceConsiderationRequest? request = overlapModel!.Request;
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { SelectedEvidence: null } result
            || request is null
            || !RequestRetainsExactSource(request)
            || !ReferenceEquals(result.Request, request)
            || overlapModel.CallCount != 1
            || !SourceIsUnchanged()
            || !ReferenceEquals(exposure.Exposure, selectedExposure)
            || !ViewRetainsExactSource(selectedExposure!)
            || !PresentationSelectionIsInert()
            || coordinator.TryExposeCompletedResult()
            || !coordinator.TryAcknowledgeCompletedResult()
            || !flow.TryStart())
        {
            Fail("Main-scene misquotation-evidence abstention changed exact source state.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || task.Result is not null
            || selectionModel!.CallCount != 1
            || overlapModel!.CallCount != 1
            || !ReferenceEquals(exposure.Exposure, selectedExposure)
            || !ViewRetainsExactSource(selectedExposure!)
            || !PresentationSelectionIsInert()
            || coordinator.TryExposeCompletedResult()
            || coordinator.TryAcknowledgeCompletedResult()
            || !SourceIsUnchanged())
        {
            Fail("Main-scene misquotation-evidence offline failure changed source state.");
            return;
        }

        if (view.View is not { } currentView
            || !presentationFlow.TryChoose(currentView, source.Candidate)
            || presentation.Selection is not { } selection
            || presentationProjection.Presentation is not { } projected
            || !ReferenceEquals(selection.WitnessView, currentView)
            || !ReferenceEquals(selection.SelectedEvidence, source.Candidate)
            || !ReferenceEquals(projected.Selection, selection)
            || !ReferenceEquals(projected.WitnessCharacterId, source.WitnessCharacterId)
            || !ReferenceEquals(projected.SourceEvent, source.SourceEvent)
            || !ReferenceEquals(projected.DeliveredEvent, source.DeliveredEvent)
            || presentationFlow.TryChoose(currentView, source.Candidate)
            || !SourceIsUnchanged())
        {
            Fail("Main-scene misquotation-evidence presentation choice was not exact and one-shot.");
            return;
        }

        GD.Print(
            "Misquotation-evidence main-scene smoke passed: inert startup, exact source-owned "
            + "request, one-shot selection and abstention, overlap preservation, explicit reset, "
            + "exact exposure, witness view and acknowledgement, stable offline failure, closed "
            + "flows, explicit presentation abstention and selection, and unchanged dialogue "
            + "evidence.");
        GetTree().Quit();
    }

    private bool RequestRetainsExactSource(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request) =>
        ReferenceEquals(request.Actor, source.Actor)
        && ReferenceEquals(request.Candidates.Single(), source.Candidate);

    private bool SourceIsUnchanged() =>
        ReferenceEquals(source.Candidate.Occurrence, source.Occurrence)
        && ReferenceEquals(source.Occurrence.Proposal.SourceEvent, source.SourceEvent)
        && ReferenceEquals(source.Occurrence.DeliveredEvent, source.DeliveredEvent)
        && ReferenceEquals(source.Candidate.WitnessCharacterId, source.WitnessCharacterId)
        && CharacterDialogueMisquotationEvidencePolicy.CanObserve(source.Actor, source.Candidate);

    private bool ViewRetainsExactSource(
        CharacterDialogueMisquotationEvidenceExposureSnapshot expectedExposure) =>
        ReferenceEquals(view.SourceExposure, expectedExposure)
        && view.View is { Entries.Count: 1 } currentView
        && ReferenceEquals(currentView.WitnessCharacterId, source.WitnessCharacterId)
        && ReferenceEquals(currentView.Entries.Single(), source.Candidate)
        && ReferenceEquals(
            currentView.Entries.Single().Occurrence.DeliveredEvent,
            source.DeliveredEvent);

    private bool PresentationSelectionIsInert() =>
        presentation.Selection is null && presentationProjection.Presentation is null;

    private static bool HasClosedSurfaces()
    {
        Type flowType = typeof(
            TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController);
        string[] flowMethods = flowType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Type exposureFlowType = typeof(
            TemporaryCharacterDialogueMisquotationEvidenceExposureFlowController);
        string[] exposureFlowMethods = exposureFlowType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();
        Type presentationFlowType = typeof(
            TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController);
        string[] presentationFlowMethods = presentationFlowType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return flowMethods.SequenceEqual(["TryStart", "_Ready"])
            && flowType.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0
            && exposureFlowMethods.SequenceEqual(["_Ready"])
            && exposureFlowType.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0
            && presentationFlowMethods.SequenceEqual(["TryChoose", "_Ready"])
            && presentationFlowType.GetProperties(
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
        Func<CharacterDialogueMisquotationEvidenceConsiderationRequest,
            CharacterDialogueMisquotationEvidenceDecision> decide)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}

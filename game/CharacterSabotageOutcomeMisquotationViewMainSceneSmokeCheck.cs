using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact eligible misquotation outcome through its witness-view route.</summary>
public partial class CharacterSabotageOutcomeMisquotationViewMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_OUTCOME_MISQUOTATION_VIEW_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;
    private CharacterDialogueMisquotationEvidenceExposureController exposureState = null!;
    private CharacterDialogueMisquotationEvidenceViewController viewState = null!;
    private CharacterDialogueMisquotationEvidencePresentationSelectionController selections = null!;
    private CharacterDialogueMisquotationEvidencePresentationController presentations = null!;
    private CharacterSabotageActionMisquotationRequestController misquotationAuthor = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource actionModels = null!;
    private CharacterSabotageActionTaskController actionTask = null!;
    private CharacterSabotageActionRequestConsiderationFlowController actionFlow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator specificRequests = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource misquotationModels = null!;
    private CharacterDialogueMisquotationEvidenceTaskController misquotationTask = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator specificStart = null!;
    private CharacterSabotageActionSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator specificOutcome = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterSabotageOutcomeRecoveryCoordinator recovery = null!;
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private RecordingActionModel actionModel = null!;
    private RecordingMisquotationModel misquotationModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        exposureState = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        viewState = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        selections = GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
            "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        presentations = GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
            "../CharacterDialogueMisquotationEvidencePresentationController");
        misquotationAuthor = GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController");
        authors = GetAuthors();
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        actionModels = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        actionTask = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        actionFlow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        specificRequests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        misquotationModels = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        misquotationTask = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        specificStart = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        specificResult = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        specificOutcome = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeRecoveryCoordinator>(
            "../CharacterSabotageOutcomeRecoveryCoordinator");
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        actionModel = new();
        misquotationModel = new();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene misquotation view routing exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartActionSelection();
        }
        else if (stage == 1)
        {
            StartMisquotationSelection();
        }
        else if (stage == 2)
        {
            RouteMisquotationView();
        }
        else
        {
            Fail("Main-scene misquotation view routing entered an unknown stage.");
        }
    }

    private void StartActionSelection()
    {
        bool inert = authors.All(author => author.Request is null)
            && recovery.AuthorRequest is null
            && recovery.RoutedMisquotationExposure is null
            && recovery.RoutedMisquotationView is null
            && recovery.MisquotationSelection is null
            && recovery.MisquotationPresentation is null
            && !recovery.TryRouteMisquotation(null, null, null)
            && StateIsInitial();
        bool authored = misquotationAuthor.TryCreate(
            source.Actor,
            source.SourceEvent,
            source.DeliveredEvent,
            source.Occurrence,
            source.WitnessCharacterId,
            source.Candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(source.Actor, outputs);
        bool started = composer.Request is { } request
            && actionModels.TrySupplyModel(actionModel)
            && actionFlow.TryStart(request);

        if (!inert
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[4], misquotationAuthor.Request)
            || !composed
            || !started
            || actionTask.Status != CharacterSabotageActionTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene misquotation author did not start one exact inert generic selection.");
            return;
        }

        stage = 1;
    }

    private void StartMisquotationSelection()
    {
        if (actionTask.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (actionTask.Status != CharacterSabotageActionTaskStatus.Completed
            || actionTask.Result is not { } result
            || actionModel.CallCount != 1
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Misquote)
            || !selection.TryObserveCompletedResult()
            || !binding.TryBindObservedSelection()
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> selected
            || !ReferenceEquals(selected.Request, misquotationAuthor.Request)
            || !ReferenceEquals(selected.Candidate, source.Candidate)
            || !binding.TryAcknowledgeBoundResult()
            || !specificRequests.TryCompose(
                binding.SelectedAuthorRequest,
                binding.SelectedCandidateBinding)
            || specificRequests.MisquotationRequest is not { } misquotationRequest
            || !ReferenceEquals(misquotationRequest.Actor, source.Actor)
            || !ReferenceEquals(misquotationRequest.Candidates.Single(), source.Candidate)
            || !misquotationModels.TrySupplyModel(misquotationModel)
            || !specificStart.TryStart()
            || misquotationTask.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Running
            || !StateIsInitial())
        {
            Fail("Main-scene generic misquotation selection did not start its exact typed request.");
            return;
        }

        stage = 2;
    }

    private void RouteMisquotationView()
    {
        if (misquotationTask.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (misquotationTask.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || misquotationTask.Result is not { SelectedEvidence: { } selected }
            || !ReferenceEquals(selected, source.Candidate)
            || misquotationModel.CallCount != 1
            || !specificResult.TryObserveCompletedResult()
            || !specificResult.TryAcknowledgeCompletedResult()
            || !specificOutcome.TryBindAcknowledgedOutcome()
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, null)
            || eligibility.EligibilityCount != 1
            || eligibility.MisquotationExposure is not { } exposure
            || eligibility.MisquotationView is not { } view
            || !ReferenceEquals(exposureState.Exposure, exposure)
            || !ReferenceEquals(viewState.View, view))
        {
            Fail("Main-scene exact misquotation outcome did not reach its eligible witness view.");
            return;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthor = new(
            source.Actor,
            [CharacterSabotageActionKind.Misquote]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot rebuiltExposure = new(
            exposure.Entries);
        CharacterDialogueMisquotationEvidenceViewSnapshot rebuiltView = new(
            view.WitnessCharacterId,
            view.Entries);

        if (recovery.TryRouteMisquotation(rebuiltAuthor, exposure, view)
            || recovery.TryRouteMisquotation(
                eligibility.AuthorRequest,
                rebuiltExposure,
                view)
            || recovery.TryRouteMisquotation(
                eligibility.AuthorRequest,
                exposure,
                rebuiltView)
            || !recovery.TryRouteMisquotation(
                eligibility.AuthorRequest,
                exposure,
                view)
            || !ReferenceEquals(recovery.RoutedMisquotationExposure, exposure)
            || !ReferenceEquals(recovery.RoutedMisquotationView, view)
            || recovery.MisquotationSelection is not { } routedSelection
            || !ReferenceEquals(routedSelection.WitnessView, view)
            || !ReferenceEquals(routedSelection.SelectedEvidence, source.Candidate)
            || !ReferenceEquals(selections.Selection, routedSelection)
            || recovery.MisquotationPresentation is not { } presentation
            || !ReferenceEquals(presentation.Selection, routedSelection)
            || !ReferenceEquals(presentations.Presentation, presentation)
            || !ReferenceEquals(presentation.SourceEvent, source.SourceEvent)
            || !ReferenceEquals(presentation.DeliveredEvent, source.DeliveredEvent)
            || actionModel.CallCount != 1
            || misquotationModel.CallCount != 1
            || !terminalRoute.TryRetain(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Misquote)
            || !ReferenceEquals(terminalRoute.MisquotationPresentation, presentation)
            || terminalRoute.TerminalOutputCount != 1
            || terminalRoute.TryRetain(
                binding.SelectedAuthorRequest,
                CharacterSabotageActionKind.Misquote)
            || recovery.TryRouteMisquotation(
                eligibility.AuthorRequest,
                exposure,
                view))
        {
            Fail("Main-scene exact eligible misquotation did not route once without another model call.");
            return;
        }

        GD.Print(
            "Main-scene sabotage outcome misquotation-view smoke passed: ordinary startup stayed "
            + "inert; one exact live misquotation selection retained its author, evidence, "
            + "acknowledgement, exposure, and witness-view tokens; and the existing truth-neutral "
            + "presentation route projected it once without consuming evidence or another model, "
            + "then the exact terminal route was retained without scheduling another action.");
        GetTree().Quit();
    }

    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        misquotationAuthor,
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool StateIsInitial() =>
        exposureState.Exposure is { Entries.Count: 0 } initialExposure
        && ReferenceEquals(viewState.SourceExposure, initialExposure)
        && viewState.View is { Entries.Count: 0 }
        && selections.Selection is null
        && presentations.Presentation is null;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingActionModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    request.Actor.Identity.Id,
                    CharacterSabotageActionKind.Misquote));
        }
    }

    private sealed class RecordingMisquotationModel
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public int CallCount { get; private set; }

        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return ValueTask.FromResult(
                new CharacterDialogueMisquotationEvidenceDecision(
                    request.Actor.Identity.Id,
                    request.Candidates.Single()));
        }
    }
}

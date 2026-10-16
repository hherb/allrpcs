using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Movement;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Hosts the keyboard-movement greybox scene.
/// </summary>
public partial class Main : Node
{
    private const string InitialFocusedSearchResultText = "Focused search: NO ATTEMPT";
    private const int AutonomousConversationMaximumTurnCount = 2;
    private const int SabotageTurnMeaningfulEventInterval = 2;
    private const float DialogueHearingRangePixels = 192.0f;
    private const float WhisperHearingRangePixels = 48.0f;
    private static readonly TimeSpan CharacterModelGenerationDeadline = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan IndependentTaskStepInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ParticipationRoundDeadline = TimeSpan.FromSeconds(30);
    private static readonly StringName CharacterTurnCancelAction = new("character_turn_cancel");
    private static readonly StringName FocusedSearchCancelAction = new("focused_search_cancel");
    private static readonly StringName FocusedSearchCompleteAction = new("focused_search_complete");
    private static readonly StringName FocusedSearchResolveAction = new("focused_search_resolve");
    private static readonly StringName FocusedSearchStartAction = new("focused_search_start");
    private static readonly StringName HealerToggleAction = new("healer_toggle_follow_wait");
    private static readonly StringName HealerMoveAction = new("healer_move");
    private static readonly StringName LayeredInscriptionDiscoverAction =
        new("layered_inscription_discover");

    internal bool ConditionalCollectionScaffolding { get; set; }
    private HerbExaminationActionController examinationAction = null!;
    private Healer healer = null!;
    private SeekerWalkAnimationPresenter seekerAnimationPresenter = null!;
    private AutonomousConversationBudgetController autonomousConversationBudgetController = null!;
    private OrdinaryPlayerDialogueParticipationCoordinator participationCoordinator = null!;
    private CharacterTurnParticipationRoundTaskController participationRoundController = null!;
    private TemporaryCharacterTurnParticipationModelSource participationModelSource = null!;
    private CharacterModelGenerationFlowController modelFlowController = null!;
    private CharacterTurnActivityController characterTurnController = null!;
    private CharacterTurnTranscriptController characterTurnTranscriptController = null!;
    private DialogueEventController dialogueEventController = null!;
    private DialogueInputController dialogueInputController = null!;
    private TemporaryCharacterModelGenerationInputSource characterModelInputSource = null!;
    private TemporaryHealerCharacterSource healerCharacterSource = null!;
    private TemporaryOrdinaryCharacterParticipantSource ordinaryCharacterParticipantSource = null!;
    private TemporaryCharacterIndependentTaskSource independentTaskSource = null!;
    private CharacterIndependentTaskDepartureController independentTaskDepartureController = null!;
    private TemporaryCharacterIndependentTaskOccurrenceSource independentTaskOccurrenceSource = null!;
    private CharacterIndependentTaskBodyPresentationController independentTaskBodyPresentationController = null!;
    private CharacterIndependentTaskRoundTripCoordinator independentTaskRoundTripCoordinator = null!;
    private CharacterIndependentTaskStepClockController independentTaskStepClockController = null!;
    private TemporaryCharacterIndependentTaskTravelSource independentTaskTravelSource = null!;
    private CharacterIndependentTaskTravelController independentTaskTravelController = null!;
    private CharacterIndependentTaskReturnTravelController independentTaskReturnTravelController = null!;
    private TemporaryCharacterObjectSabotageSource objectSabotageSource = null!;
    private TemporaryCharacterObjectSabotagePresentationFlowController
        objectSabotagePresentationFlow = null!;
    private TemporaryCharacterObjectSabotageMoveSource objectSabotageMoveSource = null!;
    private TemporaryCharacterObjectSabotageMovePresentationFlowController
        objectSabotageMovePresentationFlow = null!;
    private TemporaryCharacterObjectSabotageSubstitutionSource
        objectSabotageSubstitutionSource = null!;
    private TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController
        objectSabotageSubstitutionPresentationFlow = null!;
    private TemporaryCharacterObjectSabotageContaminationSource
        objectSabotageContaminationSource = null!;
    private TemporaryCharacterObjectSabotageContaminationPresentationFlowController
        objectSabotageContaminationPresentationFlow = null!;
    private TemporaryCharacterModelSource characterModelSource = null!;
    private Marker2D healerCommandDestination = null!;
    private Marker2D sageHearingPosition = null!;
    private Label healerModeStatus = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private Label focusedSearchResultStatus = null!;
    private InteractionFocusController interactionFocusController = null!;
    private OpeningRegionLayeredInscriptionDiscoveryCoordinator
        layeredInscriptionDiscoveryCoordinator = null!;
    private OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator
        layeredInscriptionEnvironmentalFlowCoordinator = null!;
    private OpeningRegionStreamResiduePassiveNoticingCoordinator
        streamResiduePassiveNoticingCoordinator = null!;
    private OpeningRegionWagonClueFocusedSearchRoutingCoordinator
        wagonClueFocusedSearchRoutingCoordinator = null!;
    private TemporaryResolvedPerceptionIndicator[] resolvedPerceptionIndicators = [];
    private GreyboxInteractionTarget[] resolvedPerceptionTargets = [];
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private TemporaryFocusedSearchOutcomeSource focusedSearchOutcomeSource = null!;
    private TemporaryCharacterTurnSource characterTurnSource = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private FirstPatientPreparationController firstPatientPreparationController = null!;
    private TemporaryObservationReinterpretationCandidateSource
        observationReinterpretationCandidateSource = null!;
    private TemporaryPassiveNoticingCandidateSource passiveNoticingCandidateSource = null!;
    private TemporarySeekerCharacterSource seekerCharacterSource = null!;
    private TemporarySeekerPerceptionSource seekerPerceptionSource = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private CharacterModelStreamSnapshot? lastProcessedModelResult;
    private CharacterTurnActivity? lastProcessedCompletedCharacterTurn;
    private DialogueDelivery? lastProcessedDialogueDelivery;
    private HealerCommandMode displayedMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        healer = GetNode<Healer>("Healer");
        seekerAnimationPresenter = GetNode<SeekerWalkAnimationPresenter>(
            "Seeker/WalkAnimation");
        autonomousConversationBudgetController = GetNode<AutonomousConversationBudgetController>(
            "AutonomousConversationBudgetController");
        participationCoordinator = GetNode<OrdinaryPlayerDialogueParticipationCoordinator>(
            "OrdinaryPlayerDialogueParticipationCoordinator");
        participationRoundController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "CharacterTurnParticipationRoundTaskController");
        participationModelSource = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "TemporaryCharacterTurnParticipationModelSource");
        characterTurnController = GetNode<CharacterTurnActivityController>(
            "CharacterTurnActivityController");
        characterTurnTranscriptController = GetNode<CharacterTurnTranscriptController>(
            "CharacterTurnTranscriptController");
        dialogueEventController = GetNode<DialogueEventController>("DialogueEventController");
        dialogueInputController = GetNode<DialogueInputController>("DialogueInputController");
        characterModelInputSource = GetNode<TemporaryCharacterModelGenerationInputSource>(
            "TemporaryCharacterModelGenerationInputSource");
        healerCharacterSource = GetNode<TemporaryHealerCharacterSource>(
            "TemporaryHealerCharacterSource");
        ordinaryCharacterParticipantSource = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "TemporaryOrdinaryCharacterParticipantSource");
        independentTaskSource = GetNode<TemporaryCharacterIndependentTaskSource>(
            "TemporaryCharacterIndependentTaskSource");
        independentTaskDepartureController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "CharacterIndependentTaskDepartureController");
        independentTaskOccurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "TemporaryCharacterIndependentTaskOccurrenceSource");
        independentTaskBodyPresentationController =
            GetNode<CharacterIndependentTaskBodyPresentationController>(
                "CharacterIndependentTaskBodyPresentationController");
        independentTaskRoundTripCoordinator =
            GetNode<CharacterIndependentTaskRoundTripCoordinator>(
                "CharacterIndependentTaskRoundTripCoordinator");
        independentTaskStepClockController =
            GetNode<CharacterIndependentTaskStepClockController>(
                "CharacterIndependentTaskStepClockController");
        independentTaskTravelSource =
            GetNode<TemporaryCharacterIndependentTaskTravelSource>(
                "TemporaryCharacterIndependentTaskTravelSource");
        independentTaskTravelController =
            GetNode<CharacterIndependentTaskTravelController>(
                "CharacterIndependentTaskTravelController");
        independentTaskReturnTravelController =
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "CharacterIndependentTaskReturnTravelController");
        objectSabotageSource = GetNode<TemporaryCharacterObjectSabotageSource>(
            "TemporaryCharacterObjectSabotageSource");
        objectSabotagePresentationFlow =
            GetNode<TemporaryCharacterObjectSabotagePresentationFlowController>(
                "TemporaryCharacterObjectSabotagePresentationFlowController");
        objectSabotageMoveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "TemporaryCharacterObjectSabotageMoveSource");
        objectSabotageMovePresentationFlow =
            GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>(
                "TemporaryCharacterObjectSabotageMovePresentationFlowController");
        objectSabotageSubstitutionSource =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "TemporaryCharacterObjectSabotageSubstitutionSource");
        objectSabotageSubstitutionPresentationFlow =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController>(
                "TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController");
        objectSabotageContaminationSource =
            GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
                "TemporaryCharacterObjectSabotageContaminationSource");
        objectSabotageContaminationPresentationFlow =
            GetNode<TemporaryCharacterObjectSabotageContaminationPresentationFlowController>(
                "TemporaryCharacterObjectSabotageContaminationPresentationFlowController");
        characterModelSource = GetNode<TemporaryCharacterModelSource>(
            "TemporaryCharacterModelSource");
        healerCommandDestination = GetNode<Marker2D>("HealerCommandDestination");
        sageHearingPosition = GetNode<Marker2D>("SageHearingPosition");
        healerModeStatus = GetNode<Label>("CommandOverlay/Panel/ModeStatus");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "Seeker/FocusedSearchActivityController");
        focusedSearchResultStatus = GetNode<Label>("CommandOverlay/Panel/FocusedSearchResult");
        interactionFocusController = GetNode<InteractionFocusController>(
            "InteractionFocusController");
        layeredInscriptionDiscoveryCoordinator =
            GetNode<OpeningRegionLayeredInscriptionDiscoveryCoordinator>(
                "OpeningRegionLayeredInscriptionDiscoveryCoordinator");
        layeredInscriptionEnvironmentalFlowCoordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator>(
                "OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator");
        streamResiduePassiveNoticingCoordinator =
            GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
                "OpeningRegionStreamResiduePassiveNoticingCoordinator");
        wagonClueFocusedSearchRoutingCoordinator =
            GetNode<OpeningRegionWagonClueFocusedSearchRoutingCoordinator>(
                "OpeningRegionWagonClueFocusedSearchRoutingCoordinator");
        resolvedPerceptionIndicators =
        [
            GetNode<TemporaryResolvedPerceptionIndicator>(
                "InteractionCandidates/HerbPatch/TemporaryResolvedPerceptionIndicator"),
            GetNode<TemporaryResolvedPerceptionIndicator>(
                "InteractionCandidates/StoneMarker/TemporaryResolvedPerceptionIndicator"),
        ];
        resolvedPerceptionTargets =
        [
            GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch"),
            GetNode<GreyboxInteractionTarget>("InteractionCandidates/StoneMarker"),
        ];
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "TemporaryFocusedSearchContextSource");
        focusedSearchOutcomeSource = GetNode<TemporaryFocusedSearchOutcomeSource>(
            "TemporaryFocusedSearchOutcomeSource");
        characterTurnSource = GetNode<TemporaryCharacterTurnSource>(
            "TemporaryCharacterTurnSource");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "TemporaryFirstPatientTreatmentContextSource");
        firstPatientPreparationController = GetNode<FirstPatientPreparationController>(
            "FirstPatientPreparationController");
        observationReinterpretationCandidateSource =
            GetNode<TemporaryObservationReinterpretationCandidateSource>(
                "TemporaryObservationReinterpretationCandidateSource");
        passiveNoticingCandidateSource = GetNode<TemporaryPassiveNoticingCandidateSource>(
            "TemporaryPassiveNoticingCandidateSource");
        seekerCharacterSource = GetNode<TemporarySeekerCharacterSource>(
            "TemporarySeekerCharacterSource");
        seekerPerceptionSource = GetNode<TemporarySeekerPerceptionSource>(
            "TemporarySeekerPerceptionSource");
        worldEventSource = GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>("TemporaryWorldObjectSource");
        worldEventController = GetNode<WorldEventController>("WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("WorldObjectController");
        CharacterModelStreamController modelStreamController =
            GetNode<CharacterModelStreamController>("CharacterModelStreamController");
        if (!modelStreamController.TryInitialize(new CharacterModelStreamSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-model stream controller could not initialize empty.");
        }

        modelFlowController = GetNode<CharacterModelGenerationFlowController>(
            "CharacterModelGenerationFlowController");
        if (!modelFlowController.TryInitialize(
                modelStreamController,
                GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                    "DialogueOverlay/Panel/ModelStatus"),
                GetNode<TemporaryCharacterModelStreamTextLabel>(
                    "DialogueOverlay/Panel/ModelStreamText")))
        {
            throw new InvalidOperationException(
                "The character-model generation flow could not initialize.");
        }

        if (!dialogueInputController.TryInitialize(
                seekerCharacterSource.Character.Identity.Id))
        {
            throw new InvalidOperationException(
                "The dialogue-input controller could not initialize the scene Seeker.");
        }

        if (!characterTurnTranscriptController.TryInitialize(
                new CharacterTurnTranscriptSnapshot([])))
        {
            throw new InvalidOperationException(
                "The character-turn transcript controller could not initialize empty.");
        }

        InitializeFirstPatientState();
        if (!firstPatientPreparationController.TryInitialize(
                firstPatientContextSource.Context.Scenario.Remedy.Recipe,
                firstPatientContextSource.Context.Scenario.Ingredients))
        {
            throw new InvalidOperationException(
                "The first-patient preparation controller could not initialize scene ingredients.");
        }

        InitializeWorldEvents();
        examinationAction = new() { Name = "HerbExaminationActionController" };
        AddChild(examinationAction);
        MoveChild(examinationAction, GetChildCount() - 2);
        RecordInitialWorldEvent();
        InitializeSabotageTurnOpportunity();
        if (!worldObjectController.TryInitializeWorldObjects(worldObjectSource.WorldObjects))
        {
            throw new InvalidOperationException(
                "The world-object controller could not initialize the scene snapshot.");
        }

        InitializeAndStartIndependentTaskDeparture();
        InitializeIndependentTaskTravel();
        InitializeIndependentTaskStepClock();
        InitializeIndependentTaskBodyPresentation();

        if (!focusedSearchController.TryInitializePerception(seekerPerceptionSource.Perception))
        {
            throw new InvalidOperationException(
                "The focused-search controller could not initialize Seeker perception.");
        }

        ProduceInitialPassiveNoticingCandidate();
        ResolveInitialPassiveNoticingCandidate();
        ResolveOpeningRegionStreamResiduePassiveNoticing();
        ProduceInitialObservationReinterpretationCandidate();
        ResolveInitialObservationReinterpretationCandidate();

        RefreshResolvedPerceptionPresentation();

        focusedSearchResultStatus.Text = InitialFocusedSearchResultText;
        UpdateModeStatus(force: true);
    }

    private void InitializeFirstPatientState()
    {
        FirstPatientStateController controller =
            GetNode<FirstPatientStateController>("FirstPatientStateController");
        if (!controller.TryInitialize(
                firstPatientContextSource.Context.Scenario.DiseaseTruth,
                new PreparedIngredientBatchSnapshot([]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([])))
        {
            throw new InvalidOperationException(
                "The first-patient controller could not initialize the authored scene state.");
        }
    }

    private void InitializeAndStartIndependentTaskDeparture()
    {
        CharacterIndependentTaskActivitySnapshot activities = independentTaskSource.Activities;
        CharacterPartySnapshot party = independentTaskSource.Party;
        if (!independentTaskDepartureController.TryInitialize(activities, party)
            || !independentTaskDepartureController.TryStartAndSeparate(
                independentTaskSource.DepartingCharacterId,
                activities,
                party))
        {
            throw new InvalidOperationException(
                "The authored independent-task departure could not start atomically.");
        }
    }

    private void InitializeIndependentTaskTravel()
    {
        CharacterIndependentTaskActivity activeActivity =
            independentTaskDepartureController.ActiveActivity
            ?? throw new InvalidOperationException(
                "The authored independent task did not retain its active departure state.");
        NumericsVector2 origin = new(healer.GlobalPosition.X, healer.GlobalPosition.Y);
        CharacterIndependentTaskTravelPlan plan =
            independentTaskTravelSource.CreatePlan(activeActivity, origin);
        if (!independentTaskTravelController.TryInitialize(plan))
        {
            throw new InvalidOperationException(
                "The authored independent-task travel plan could not initialize.");
        }
    }

    private void InitializeIndependentTaskStepClock()
    {
        if (!independentTaskStepClockController.TryInitialize(
                new CharacterIndependentTaskStepCadence(IndependentTaskStepInterval)))
        {
            throw new InvalidOperationException(
                "The authored independent-task step clock could not initialize.");
        }
    }

    private void InitializeIndependentTaskBodyPresentation()
    {
        CharacterPartySnapshot party = independentTaskDepartureController.Party
            ?? throw new InvalidOperationException(
                "The independent-task party state was not initialized for body presentation.");
        if (!independentTaskBodyPresentationController.TryInitialize(
                independentTaskSource.DepartingCharacterId,
                healer,
                party))
        {
            throw new InvalidOperationException(
                "The authored independent-task body presentation could not initialize.");
        }
    }

    private void InitializeWorldEvents()
    {
        if (!worldEventController.TryInitialize(
                worldEventSource.WorldEvents,
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)))
        {
            throw new InvalidOperationException(
                "The world-event controller could not initialize the scene state.");
        }
    }

    private void InitializeSabotageTurnOpportunity()
    {
        TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source =
            GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
                "TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        CharacterSabotageTurnOpportunityController controller =
            GetNode<CharacterSabotageTurnOpportunityController>(
                "CharacterSabotageTurnOpportunityController");
        if (!controller.TryInitialize(
                source.Actor,
                new CharacterSabotageTurnOpportunityCadence(
                    SabotageTurnMeaningfulEventInterval)))
        {
            throw new InvalidOperationException(
                "The sabotage-turn opportunity controller could not initialize inertly.");
        }
    }

    private void RecordInitialWorldEvent()
    {
        WorldEventId sourceEventId = worldEventSource.InitialWorldEvent.EventId;
        WorldEvent worldEvent = worldEventController.FindWorldEvent(sourceEventId)
            ?? throw new InvalidOperationException(
                "The initial greybox world event is absent from authoritative world events.");

        if (!worldEventController.TryOccur(worldEvent.EventId))
        {
            throw new InvalidOperationException(
                "The initial greybox world event could not occur.");
        }
    }

    private void ProduceInitialPassiveNoticingCandidate()
    {
        PassiveNoticingCandidate candidate = passiveNoticingCandidateSource.Candidate;
        Character character = seekerCharacterSource.Character;
        WorldObject worldObject = worldObjectController.FindWorldObject(candidate.TargetObjectId)
            ?? throw new InvalidOperationException(
                "The passive-noticing candidate target is absent from live world objects.");

        if (!focusedSearchController.TryProducePassiveNoticingCandidate(
                candidate,
                character,
                worldObject))
        {
            throw new InvalidOperationException(
                "The passive-noticing candidate could not be produced for the Seeker.");
        }
    }

    private void ResolveInitialPassiveNoticingCandidate()
    {
        if (!focusedSearchController.TryResolveNextPassiveNoticingCandidate())
        {
            throw new InvalidOperationException(
                "The initial passive-noticing candidate could not be resolved for the Seeker.");
        }
    }

    private void ResolveOpeningRegionStreamResiduePassiveNoticing()
    {
        if (!streamResiduePassiveNoticingCoordinator.TryResolve(
                streamResiduePassiveNoticingCoordinator.Perception,
                streamResiduePassiveNoticingCoordinator.Candidates,
                streamResiduePassiveNoticingCoordinator.Candidate))
        {
            throw new InvalidOperationException(
                "The opening-region stream residue could not be passively noticed.");
        }
    }

    private void ProduceInitialObservationReinterpretationCandidate()
    {
        ObservationReinterpretationCandidate candidate =
            observationReinterpretationCandidateSource.Candidate;
        Character character = seekerCharacterSource.Character;
        WorldObject worldObject = worldObjectController.FindWorldObject(candidate.TargetObjectId)
            ?? throw new InvalidOperationException(
                "The observation-reinterpretation candidate target is absent from live world objects.");

        if (!focusedSearchController.TryProduceObservationReinterpretationCandidate(
                candidate,
                character,
                worldObject))
        {
            throw new InvalidOperationException(
                "The observation-reinterpretation candidate could not be produced for the Seeker.");
        }
    }

    private void ResolveInitialObservationReinterpretationCandidate()
    {
        if (!focusedSearchController.TryResolveNextObservationReinterpretationCandidate(
                seekerCharacterSource.Character))
        {
            throw new InvalidOperationException(
                "The initial observation-reinterpretation candidate could not be resolved for the Seeker.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        ProcessIndependentTaskRoundTrip(delta);
        ProcessCharacterTurnResponse();
        ProcessCompletedCharacterTurnParticipation();
        ProcessDialogueSubmission();
        UpdateModeStatus(force: false);
    }

    private void ProcessIndependentTaskRoundTrip(double delta)
    {
        CharacterIndependentTaskStepCadence cadence =
            independentTaskStepClockController.Cadence
            ?? throw new InvalidOperationException(
                "The independent-task step cadence was not initialized.");
        CharacterIndependentTaskStepClock clock =
            independentTaskStepClockController.Clock
            ?? throw new InvalidOperationException(
                "The independent-task step clock was not initialized.");
        CharacterIndependentTaskTravelPlan outboundPlan =
            independentTaskTravelController.Plan
            ?? throw new InvalidOperationException(
                "The independent-task outbound plan was not initialized.");
        CharacterIndependentTaskTravelProgress outboundProgress =
            independentTaskTravelController.Progress
            ?? throw new InvalidOperationException(
                "The independent-task outbound progress was not initialized.");
        CharacterIndependentTaskReturnTravelPlan? returnPlan =
            independentTaskReturnTravelController.Plan;
        CharacterIndependentTaskReturnTravelProgress? returnProgress =
            independentTaskReturnTravelController.Progress;
        if ((returnPlan is null) != (returnProgress is null))
        {
            throw new InvalidOperationException(
                "The independent-task return plan and progress must be jointly present.");
        }

        if (returnProgress?.IsComplete == true)
        {
            return;
        }

        CharacterIndependentTaskWorldEventOccurrence? candidate = null;
        System.Numerics.Vector2 reachedWaypoint;
        if (returnPlan is null)
        {
            CharacterIndependentTaskActivity activeActivity =
                independentTaskDepartureController.ActiveActivity
                ?? throw new InvalidOperationException(
                    "Outbound independent-task travel requires its exact active activity.");
            if (outboundProgress.IsComplete)
            {
                throw new InvalidOperationException(
                    "Completed outbound travel did not initialize exact return travel.");
            }

            candidate = independentTaskOccurrenceSource.CreateOccurrence(activeActivity);
            reachedWaypoint = outboundPlan.Waypoints[outboundProgress.CompletedWaypointCount];
        }
        else
        {
            reachedWaypoint = returnPlan.Waypoints[returnProgress!.CompletedWaypointCount];
        }

        CharacterIndependentTaskActivitySnapshot activities =
            independentTaskDepartureController.Activities
            ?? throw new InvalidOperationException(
                "Independent-task activities were not initialized.");
        CharacterPartySnapshot party = independentTaskDepartureController.Party
            ?? throw new InvalidOperationException(
                "Independent-task party state was not initialized.");
        WorldEventLogSnapshot eventLog = worldEventController.EventLog
            ?? throw new InvalidOperationException(
                "World-event log state was not initialized.");
        WorldTimeSnapshot worldTime = worldEventController.WorldTime
            ?? throw new InvalidOperationException(
                "World-time state was not initialized.");
        CharacterTurnTranscriptSnapshot transcript =
            characterTurnTranscriptController.Transcript
            ?? throw new InvalidOperationException(
                "Character-turn transcript state was not initialized.");
        TimeSpan elapsed = BoundedIndependentTaskElapsed(delta, cadence);
        if (!independentTaskRoundTripCoordinator.TryAdvanceElapsed(
                cadence,
                clock,
                elapsed,
                outboundPlan,
                outboundProgress,
                returnPlan,
                returnProgress,
                reachedWaypoint,
                candidate,
                activities,
                party,
                eventLog,
                worldTime,
                transcript))
        {
            throw new InvalidOperationException(
                "The exact live independent-task elapsed step could not advance.");
        }

        CharacterPartySnapshot currentParty = independentTaskDepartureController.Party
            ?? throw new InvalidOperationException(
                "Independent-task party state disappeared after elapsed processing.");
        if (!ReferenceEquals(currentParty, party)
            && !independentTaskBodyPresentationController.TryPresent(party, currentParty))
        {
            throw new InvalidOperationException(
                "The exact independent-task party change could not update body presentation.");
        }
    }

    private static TimeSpan BoundedIndependentTaskElapsed(
        double delta,
        CharacterIndependentTaskStepCadence cadence)
    {
        if (!double.IsFinite(delta) || delta < 0.0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delta),
                delta,
                "Independent-task frame time must be finite and non-negative.");
        }

        return TimeSpan.FromSeconds(
            Math.Min(delta, cadence.Interval.TotalSeconds));
    }

    private void ProcessCharacterTurnResponse()
    {
        if (modelFlowController.HasStarted)
        {
            ProcessCharacterModelTurnResponse();
            if (modelFlowController.IsGenerating)
            {
                return;
            }
        }

        CharacterTurnActivity? activity = characterTurnController.Activity;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        if (modelFlowController.Result is not null && !modelFlowController.TryReset())
        {
            throw new InvalidOperationException(
                "The completed character-model flow could not reset for the next exact turn.");
        }

        Character character = CharacterTurnParticipantLookup.Find(
            ordinaryCharacterParticipantSource.Participants,
            activity)
            ?? throw new InvalidOperationException(
                "The awaiting ordinary character turn had no exact LLM participant.");
        CharacterControllerAssignment assignment = character.ControllerAssignment;
        TemporaryCharacterModelGenerationInput? input = characterModelInputSource.CreateInput(
            activity);
        if (!ReferenceEquals(assignment.CharacterId, activity.Request.RespondingCharacterId)
            || !ReferenceEquals(assignment.ControllerType, ControllerType.Llm)
            || input is null
            || !ReferenceEquals(input.GenerationRequest.TurnRequest, activity.Request)
            || !modelFlowController.TryStart(
                characterModelSource.TakeModel(),
                input.GenerationRequest,
                input.WorldObjects,
                input.Affordances,
                CharacterModelGenerationDeadline))
        {
            throw new InvalidOperationException(
                "The ordinary LLM healer turn could not start its bounded generation.");
        }
    }

    private void ProcessCharacterModelTurnResponse()
    {
        CharacterModelStreamSnapshot? result = modelFlowController.Result;
        if (result is null || ReferenceEquals(result, lastProcessedModelResult))
        {
            return;
        }

        lastProcessedModelResult = result;
        if (result.Updates[^1] is not CharacterModelStreamTermination termination)
        {
            throw new InvalidOperationException("The completed model flow was not terminal.");
        }

        CharacterTurnActivity? activity = characterTurnController.Activity;
        if (termination is
            {
                Status: CharacterModelStreamTerminationStatus.Completed,
                Response: not null,
            })
        {
            if (activity is not null
                && CharacterTurnResponseReceptionPolicy.CanReceive(activity, termination.Response))
            {
                if (!characterTurnController.TryReceiveResponse(termination.Response))
                {
                    throw new InvalidOperationException(
                        "The validated model response could not enter its exact awaiting turn.");
                }
                var scaffold = GetNode<TemporaryCharacterExaminationFlowController>(
                    "TemporaryCharacterExaminationFlowController");
                if (!ConditionalCollectionScaffolding)
                {
                    scaffold.SetProcess(false);
                    examinationAction.Offer(characterTurnController.Activity!);
                }
            }

            return;
        }

        if (activity is not null
            && CharacterTurnCancellationPolicy.CanCancel(activity)
            && !characterTurnController.TryCancel())
        {
            throw new InvalidOperationException(
                "The unsuccessful model response could not cancel its exact pending turn.");
        }
    }

    private void ProcessCompletedCharacterTurnParticipation()
    {
        CharacterTurnActivity? activity = characterTurnController.Activity;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            || ReferenceEquals(activity, lastProcessedCompletedCharacterTurn))
        {
            return;
        }

        lastProcessedCompletedCharacterTurn = activity;
        DialogueUtterance? speech = activity.Response?.Speech;
        DialogueEvent? occurredEvent = dialogueEventController.LastOccurredEvent;
        if (speech is null || !ReferenceEquals(occurredEvent?.Utterance, speech))
        {
            return;
        }

        Character[] scheduledCharacters = GetHeardOrdinaryParticipants(occurredEvent);
        if (scheduledCharacters.Length == 0)
        {
            return;
        }

        _ = participationCoordinator.TryStart(
            occurredEvent,
            scheduledCharacters,
            [],
            participationModelSource,
            participationRoundController,
            autonomousConversationBudgetController,
            characterTurnController,
            ParticipationRoundDeadline);
    }

    private void ProcessDialogueSubmission()
    {
        DialogueDelivery? delivery = dialogueInputController.LastSubmittedDelivery;
        if (delivery is null || ReferenceEquals(delivery, lastProcessedDialogueDelivery))
        {
            return;
        }

        DialogueUtterance utterance = delivery.Utterance;
        IReadOnlyList<Character> ordinaryParticipants =
            ordinaryCharacterParticipantSource.Participants;
        DialogueHearingCandidate[] candidates =
        [
            new(utterance.SpeakerCharacterId, ToNumerics(GetNode<Node2D>("Seeker").Position)),
            new(ordinaryParticipants[0].Identity.Id, ToNumerics(healer.Position)),
            new(ordinaryParticipants[1].Identity.Id, ToNumerics(sageHearingPosition.Position)),
        ];
        if (!dialogueEventController.TryOccur(
                delivery,
                candidates[0].Position,
                DialogueHearingRangePixels,
                WhisperHearingRangePixels,
                candidates))
        {
            throw new InvalidOperationException(
                "The accepted player dialogue delivery could not occur.");
        }

        DialogueEvent occurredEvent = dialogueEventController.LastOccurredEvent
            ?? throw new InvalidOperationException("The occurred player dialogue event was absent.");
        if (!ReferenceEquals(occurredEvent.Delivery, delivery)
            || !characterTurnTranscriptController.TryAppendSpokenStatement(
                occurredEvent.Delivery))
        {
            throw new InvalidOperationException(
                "The occurred player dialogue delivery could not append to the transcript.");
        }

        StartParticipationRoundIfEligible();

        lastProcessedDialogueDelivery = delivery;
    }

    private void StartParticipationRoundIfEligible()
    {
        DialogueEvent occurredEvent = dialogueEventController.LastOccurredEvent
            ?? throw new InvalidOperationException("The occurred player dialogue event was absent.");
        Character[] heardParticipants = GetHeardOrdinaryParticipants(occurredEvent);
        Character healerCharacter = ordinaryCharacterParticipantSource.Participants[0];
        CharacterId[] directAddresses = heardParticipants.Any(
            character => ReferenceEquals(character, healerCharacter))
            ? [new CharacterId(healerCharacter.Identity.Id.Value)]
            : [];
        _ = participationCoordinator.TryStartNewChain(
            occurredEvent,
            heardParticipants,
            directAddresses,
            participationModelSource,
            participationRoundController,
            autonomousConversationBudgetController,
            characterTurnController,
            new AutonomousConversationBudget(
                0,
                AutonomousConversationMaximumTurnCount,
                false),
            ParticipationRoundDeadline);
    }

    private Character[] GetHeardOrdinaryParticipants(DialogueEvent occurredEvent)
    {
        HashSet<CharacterId> heardCharacterIds =
            occurredEvent.Audience.ListenerCharacterIds.ToHashSet();
        return ordinaryCharacterParticipantSource.Participants
            .Where(character => heardCharacterIds.Contains(character.Identity.Id))
            .ToArray();
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.WorldAllowed(this)) { return; }
        if (ControlInputMatcher.Pressed(@event, CharacterTurnCancelAction))
        {
            examinationAction.Cancel();
            _ = modelFlowController.TryCancel();
            _ = participationRoundController.TryCancel();
            _ = characterTurnController.TryCancel();
            _ = autonomousConversationBudgetController.TryInterrupt();
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchCancelAction))
        {
            FocusedSearchRequest? activeRequest = GetActiveFocusedSearchRequest();
            bool changed = activeRequest is not null
                && focusedSearchController.TryCancelSearch(activeRequest);
            PresentFocusedSearchResult("CANCEL", changed);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchCompleteAction))
        {
            FocusedSearchRequest? activeRequest = GetActiveFocusedSearchRequest();
            FocusedSearchOutcome? outcome = activeRequest is null
                ? null
                : focusedSearchOutcomeSource.CreateOutcome(
                    activeRequest,
                    worldObjectController.FindWorldObject(activeRequest.TargetObjectId));
            bool changed = outcome is not null
                && focusedSearchController.TryCompleteSearchWithOutcome(outcome);
            PresentFocusedSearchResult("COMPLETE", changed);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchResolveAction))
        {
            FocusedSearchOutcome? outcome = GetFirstRetainedFocusedSearchOutcome();
            bool changed = outcome is not null
                && worldObjectController.FindWorldObject(outcome.Request.TargetObjectId)
                    is { } worldObject
                && TryResolveFocusedSearchOutcome(outcome, worldObject);
            if (changed)
            {
                RefreshResolvedPerceptionPresentation();
                TryRecoverObjectSabotagesForResolvedOutcome(outcome!);
                seekerAnimationPresenter.PresentSuccessfulInteraction();
            }

            PresentFocusedSearchResult("RESOLVE", changed);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchStartAction))
        {
            bool changed = interactionFocusController.FocusedTargetObjectId
                    is { } focusedTargetObjectId
                && worldObjectController.FindWorldObject(focusedTargetObjectId)
                    is { } focusedWorldObject
                && interactionFocusController.TryStartFocusedSearch(
                    focusedWorldObject,
                    focusedSearchContextSource.Conditions,
                    focusedSearchContextSource.Capabilities,
                    focusedSearchContextSource.Inventory);
            PresentFocusedSearchResult("START", changed);
        }
        else if (ControlInputMatcher.Pressed(@event, LayeredInscriptionDiscoverAction))
        {
            if (layeredInscriptionDiscoveryCoordinator.TryDiscover(
                    layeredInscriptionDiscoveryCoordinator.SceneSource.KnowledgeableSeeker))
            {
                OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator variation =
                    layeredInscriptionEnvironmentalFlowCoordinator.VariationCoordinator;
                OpeningRegionEnvironmentalStateSource environment =
                    layeredInscriptionEnvironmentalFlowCoordinator.EnvironmentalSource;
                _ = layeredInscriptionEnvironmentalFlowCoordinator.TryComplete(
                    layeredInscriptionEnvironmentalFlowCoordinator.ShrineSource.Presentation,
                    environment.BaselineRegions,
                    environment.CurrentRegions,
                    variation.RevealedLightingState,
                    variation.RevealedSoundState);
            }
        }
        else if (ControlInputMatcher.Pressed(@event, HealerToggleAction))
        {
            if (healer.CommandMode == HealerCommandMode.Follow)
            {
                healer.CommandWait();
            }
            else
            {
                healer.CommandFollow();
            }
        }
        else if (ControlInputMatcher.Pressed(@event, HealerMoveAction))
        {
            _ = healer.TryCommandMoveToDestination(healerCommandDestination.GlobalPosition);
        }
        else
        {
            return;
        }

        UpdateModeStatus(force: true);
        GetViewport().SetInputAsHandled();
    }

    private FocusedSearchRequest? GetActiveFocusedSearchRequest()
    {
        return focusedSearchController.Activities.Entries
            .FirstOrDefault(
                activity => activity.Request.SearcherCharacterId
                    == focusedSearchController.SearcherCharacterId)
            ?.Request;
    }

    private FocusedSearchOutcome? GetFirstRetainedFocusedSearchOutcome()
    {
        return focusedSearchController.Outcomes.Entries.FirstOrDefault(
            outcome => outcome.Request.SearcherCharacterId
                == focusedSearchController.SearcherCharacterId);
    }

    private bool TryResolveFocusedSearchOutcome(
        FocusedSearchOutcome outcome,
        WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(worldObject);

        WorldObject herbPatch = worldObjectSource.WorldObjects.Entries[0];
        if (!ReferenceEquals(worldObject, herbPatch)
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified))
        {
            bool resolved = focusedSearchController.TryResolveOutcome(outcome, worldObject);
            if (resolved && ReferenceEquals(
                    worldObject,
                    wagonClueFocusedSearchRoutingCoordinator.WagonClue))
            {
                _ = wagonClueFocusedSearchRoutingCoordinator.TryRouteResolvedOutcome(
                    outcome,
                    worldObject,
                    focusedSearchController.Perception!);
            }

            return resolved;
        }

        WorldEvent advancingEvent = worldEventSource.HerbPatchIdentifiedWorldEvent;
        if (focusedSearchController.Perception is not { } perception
            || worldEventController.WorldEvents is not { } worldEvents
            || worldEventController.EventLog is not { } eventLog
            || worldEventController.WorldTime is not { } worldTime
            || !WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                perception,
                focusedSearchController.Outcomes)
            || !WorldEventOccurrencePolicy.CanOccur(
                advancingEvent.EventId,
                worldEvents,
                eventLog,
                worldTime))
        {
            return false;
        }

        if (!focusedSearchController.TryResolveOutcome(outcome, worldObject)
            || !worldEventController.TryOccur(advancingEvent.EventId))
        {
            throw new InvalidOperationException(
                "Prevalidated focused-search world-event resolution failed to commit.");
        }

        return true;
    }

    private void PresentFocusedSearchResult(string action, bool changed)
    {
        focusedSearchResultStatus.Text =
            $"Focused search {action}: {(changed ? "CHANGED" : "UNCHANGED")}";
    }

    private void RefreshResolvedPerceptionPresentation()
    {
        for (int index = 0; index < resolvedPerceptionIndicators.Length; index++)
        {
            resolvedPerceptionIndicators[index].Present(
                worldObjectController.FindWorldObject(
                    resolvedPerceptionTargets[index].ObjectId),
                focusedSearchController.Perception);
        }
    }

    private void TryRecoverObjectSabotagesForResolvedOutcome(FocusedSearchOutcome outcome)
    {
        if (focusedSearchController.Perception is not { } perception)
        {
            return;
        }

        if (outcome.Request.TargetObjectId == objectSabotageSource.Candidate.Evidence.ObjectId)
        {
            _ = objectSabotagePresentationFlow.TryRecover(perception);
        }

        if (outcome.Request.TargetObjectId
            == objectSabotageMoveSource.Candidate.Evidence.Evidence.ObjectId)
        {
            _ = objectSabotageMovePresentationFlow.TryRecover(perception);
        }

        if (outcome.Request.TargetObjectId
            == objectSabotageSubstitutionSource.Candidate.Evidence.Evidence.ObjectId)
        {
            _ = objectSabotageSubstitutionPresentationFlow.TryRecover(perception);
        }

        if (outcome.Request.TargetObjectId
            == objectSabotageContaminationSource.Candidate.Evidence.ObjectId)
        {
            _ = objectSabotageContaminationPresentationFlow.TryRecover(perception);
        }
    }

    private void UpdateModeStatus(bool force)
    {
        if (!force && displayedMode == healer.CommandMode)
        {
            return;
        }

        displayedMode = healer.CommandMode;
        string mode = examinationAction.Running && displayedMode == HealerCommandMode.MoveToDestination
            ? "EXAMINE HERB PATCH" : FormatMode(displayedMode);
        healerModeStatus.Text = $"Current healer mode: {mode}";
    }

    private static string FormatMode(HealerCommandMode mode)
    {
        return mode switch
        {
            HealerCommandMode.Follow => "FOLLOW",
            HealerCommandMode.Wait => "WAIT",
            HealerCommandMode.MoveToDestination => "MOVE TO PINK RING",
            _ => "UNKNOWN",
        };
    }

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
}

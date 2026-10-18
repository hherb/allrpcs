using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Input;
using Allrpcs.Application.Movement;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
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
    private const int SabotageTurnMeaningfulEventInterval = 2;
    private static readonly TimeSpan IndependentTaskStepInterval = TimeSpan.FromSeconds(3);
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

    /// <summary>Gets the per-frame conversation-turn state machine Main drives.</summary>
    internal ConversationTurnCoordinator<TemporaryCharacterModelGenerationInput> ConversationTurns
    {
        get;
        private set;
    } = null!;

    private HerbExaminationActionController examinationAction = null!;
    private Healer healer = null!;
    private AutonomousConversationBudgetController autonomousConversationBudgetController = null!;
    private OrdinaryPlayerDialogueParticipationCoordinator participationCoordinator = null!;
    private CharacterTurnParticipationRoundTaskController participationRoundController = null!;
    private CharacterModelGenerationFlowController modelFlowController = null!;
    private CharacterTurnActivityController characterTurnController = null!;
    private CharacterTurnTranscriptController characterTurnTranscriptController = null!;
    private DialogueInputController dialogueInputController = null!;
    private TemporaryCharacterModelGenerationInputSource characterModelInputSource = null!;
    private TemporaryHealerCharacterSource healerCharacterSource = null!;
    private TemporaryCharacterIndependentTaskSource independentTaskSource = null!;
    private CharacterIndependentTaskDepartureController independentTaskDepartureController = null!;
    private TemporaryCharacterIndependentTaskOccurrenceSource independentTaskOccurrenceSource = null!;
    private CharacterIndependentTaskBodyPresentationController independentTaskBodyPresentationController = null!;
    private CharacterIndependentTaskRoundTripCoordinator independentTaskRoundTripCoordinator = null!;
    private CharacterIndependentTaskStepClockController independentTaskStepClockController = null!;
    private TemporaryCharacterIndependentTaskTravelSource independentTaskTravelSource = null!;
    private CharacterIndependentTaskTravelController independentTaskTravelController = null!;
    private CharacterIndependentTaskReturnTravelController independentTaskReturnTravelController = null!;
    private Marker2D healerCommandDestination = null!;
    private Label healerModeStatus = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private OpeningRegionLayeredInscriptionDiscoveryCoordinator
        layeredInscriptionDiscoveryCoordinator = null!;
    private OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator
        layeredInscriptionEnvironmentalFlowCoordinator = null!;
    private OpeningRegionStreamResiduePassiveNoticingCoordinator
        streamResiduePassiveNoticingCoordinator = null!;
    private TemporaryResolvedPerceptionIndicator[] resolvedPerceptionIndicators = [];
    private GreyboxInteractionTarget[] resolvedPerceptionTargets = [];
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
    private FocusedSearchInputCoordinator focusedSearchInput = null!;
    private HealerCommandMode displayedMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessPriority = MainProcessOrder.MainPriority;
        healer = GetNode<Healer>("Healer");
        autonomousConversationBudgetController = GetNode<AutonomousConversationBudgetController>(
            "AutonomousConversationBudgetController");
        participationCoordinator = GetNode<OrdinaryPlayerDialogueParticipationCoordinator>(
            "OrdinaryPlayerDialogueParticipationCoordinator");
        participationRoundController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "CharacterTurnParticipationRoundTaskController");
        characterTurnController = GetNode<CharacterTurnActivityController>(
            "CharacterTurnActivityController");
        characterTurnTranscriptController = GetNode<CharacterTurnTranscriptController>(
            "CharacterTurnTranscriptController");
        dialogueInputController = GetNode<DialogueInputController>("DialogueInputController");
        participationCoordinator.StatusChanged += dialogueInputController.SetStatus;
        dialogueInputController.CanAccept = () => ConversationTurns.CanAcceptSubmission();
        characterModelInputSource = GetNode<TemporaryCharacterModelGenerationInputSource>(
            "TemporaryCharacterModelGenerationInputSource");
        healerCharacterSource = GetNode<TemporaryHealerCharacterSource>(
            "TemporaryHealerCharacterSource");
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
        healerCommandDestination = GetNode<Marker2D>("HealerCommandDestination");
        healerModeStatus = GetNode<Label>("CommandOverlay/Panel/ModeStatus");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "Seeker/FocusedSearchActivityController");
        layeredInscriptionDiscoveryCoordinator =
            GetNode<OpeningRegionLayeredInscriptionDiscoveryCoordinator>(
                "OpeningRegionLayeredInscriptionDiscoveryCoordinator");
        layeredInscriptionEnvironmentalFlowCoordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator>(
                "OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator");
        streamResiduePassiveNoticingCoordinator =
            GetNode<OpeningRegionStreamResiduePassiveNoticingCoordinator>(
                "OpeningRegionStreamResiduePassiveNoticingCoordinator");
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
        // Part of MainProcessOrder: immediately before WholeChannelPresentation.
        MoveChild(examinationAction, GetNode("WholeChannelPresentation").GetIndex());
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

        InitializeCoordinators();
        GetNode<Label>("CommandOverlay/Panel/FocusedSearchResult").Text =
            FocusedSearchInputCoordinator.InitialResultText;
        UpdateModeStatus(force: true);
    }

    private void InitializeCoordinators()
    {
        ConversationTurns = new(new ConversationTurnNodePorts(
            modelFlowController,
            characterModelInputSource,
            GetNode<TemporaryCharacterModelSource>("TemporaryCharacterModelSource"),
            characterTurnController,
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "TemporaryOrdinaryCharacterParticipantSource"),
            GetNode<Node2D>("Seeker"),
            healer,
            GetNode<Marker2D>("SageHearingPosition"),
            dialogueInputController,
            GetNode<DialogueEventController>("DialogueEventController"),
            characterTurnTranscriptController,
            participationCoordinator,
            participationRoundController,
            GetNode<TemporaryCharacterTurnParticipationModelSource>(
                "TemporaryCharacterTurnParticipationModelSource"),
            autonomousConversationBudgetController,
            examinationAction,
            GetNode<TemporaryCharacterExaminationFlowController>(
                "TemporaryCharacterExaminationFlowController"),
            () => ConditionalCollectionScaffolding));
        focusedSearchInput = new(new FocusedSearchInputNodePorts(
            focusedSearchController,
            GetNode<InteractionFocusController>("InteractionFocusController"),
            GetNode<TemporaryFocusedSearchContextSource>("TemporaryFocusedSearchContextSource"),
            GetNode<TemporaryFocusedSearchOutcomeSource>("TemporaryFocusedSearchOutcomeSource"),
            worldObjectController,
            worldObjectSource,
            worldEventController,
            worldEventSource,
            GetNode<OpeningRegionWagonClueFocusedSearchRoutingCoordinator>(
                "OpeningRegionWagonClueFocusedSearchRoutingCoordinator"),
            GetNode<TemporaryCharacterObjectSabotageSource>("TemporaryCharacterObjectSabotageSource"),
            GetNode<TemporaryCharacterObjectSabotagePresentationFlowController>(
                "TemporaryCharacterObjectSabotagePresentationFlowController"),
            GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "TemporaryCharacterObjectSabotageMoveSource"),
            GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>(
                "TemporaryCharacterObjectSabotageMovePresentationFlowController"),
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "TemporaryCharacterObjectSabotageSubstitutionSource"),
            GetNode<TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController>(
                "TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController"),
            GetNode<TemporaryCharacterObjectSabotageContaminationSource>(
                "TemporaryCharacterObjectSabotageContaminationSource"),
            GetNode<TemporaryCharacterObjectSabotageContaminationPresentationFlowController>(
                "TemporaryCharacterObjectSabotageContaminationPresentationFlowController"),
            GetNode<SeekerWalkAnimationPresenter>("Seeker/WalkAnimation"),
            GetNode<Label>("CommandOverlay/Panel/FocusedSearchResult"),
            RefreshResolvedPerceptionPresentation));
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
        ConversationTurns.Process();
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

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ControlInputContext.WorldAllowed(this)) { return; }
        if (ControlInputMatcher.Pressed(@event, CharacterTurnCancelAction))
        {
            ConversationTurns.Cancel();
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchCancelAction))
        {
            focusedSearchInput.Handle(FocusedSearchInputAction.Cancel);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchCompleteAction))
        {
            focusedSearchInput.Handle(FocusedSearchInputAction.Complete);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchResolveAction))
        {
            focusedSearchInput.Handle(FocusedSearchInputAction.Resolve);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchStartAction))
        {
            focusedSearchInput.Handle(FocusedSearchInputAction.Start);
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
}


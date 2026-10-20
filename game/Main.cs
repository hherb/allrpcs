using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Input;
using Allrpcs.Application.Movement;
using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Hosts the keyboard-movement greybox scene.
/// </summary>
public partial class Main : Node
{
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
    internal ConversationTurnCoordinator<TemporaryCharacterModelGenerationInput> ConversationTurns =>
        conversationTurns
        ?? throw new InvalidOperationException("Main's conversation coordinator is used before _Ready completed.");

    private FocusedSearchInputCoordinator FocusedSearchInput => focusedSearchInput
        ?? throw new InvalidOperationException("Main's focused-search input is used before _Ready completed.");

    private CharacterIndependentTaskTickCoordinator IndependentTaskTick => independentTaskTick
        ?? throw new InvalidOperationException("Main's independent-task tick is used before _Ready completed.");

    private HerbExaminationActionController examinationAction = null!;
    private Healer healer = null!;
    private Marker2D healerCommandDestination = null!;
    private Label healerModeStatus = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private OpeningRegionLayeredInscriptionDiscoveryCoordinator
        layeredInscriptionDiscoveryCoordinator = null!;
    private OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator
        layeredInscriptionEnvironmentalFlowCoordinator = null!;
    private TemporaryResolvedPerceptionIndicator[] resolvedPerceptionIndicators = [];
    private GreyboxInteractionTarget[] resolvedPerceptionTargets = [];
    private WorldObjectController worldObjectController = null!;
    private ConversationTurnCoordinator<TemporaryCharacterModelGenerationInput>? conversationTurns;
    private FocusedSearchInputCoordinator? focusedSearchInput;
    private CharacterIndependentTaskTickCoordinator? independentTaskTick;
    private HealerCommandMode displayedMode;

    /// <inheritdoc />
    public override void _EnterTree() => SmokeCheckHost.Attach(this);

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessPriority = MainProcessOrder.MainPriority;
        healer = GetNode<Healer>("Healer");
        DialogueInputController dialogueInput = GetNode<DialogueInputController>("DialogueInputController");
        GetNode<OrdinaryPlayerDialogueParticipationCoordinator>(
            "OrdinaryPlayerDialogueParticipationCoordinator").StatusChanged += dialogueInput.SetStatus;
        dialogueInput.CanAccept = () => ConversationTurns.CanAcceptSubmission();
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
        worldObjectController = GetNode<WorldObjectController>("WorldObjectController");

        // The examination controller joins between the two halves only to keep the established
        // startup order; its _Ready reads no state either half installs.
        InitializeBeforeExamination();
        examinationAction = new() { Name = "HerbExaminationActionController" };
        AddChild(examinationAction);
        // Part of MainProcessOrder: appended after WholeChannelPresentation, so moving to its index
        // lands immediately before it.
        MoveChild(examinationAction, GetNode("WholeChannelPresentation").GetIndex());
        InitializeAfterExamination();

        RefreshResolvedPerceptionPresentation();

        InitializeCoordinators();
        GetNode<Label>("CommandOverlay/Panel/FocusedSearchResult").Text =
            FocusedSearchInputCoordinator.InitialResultText;
        UpdateModeStatus(force: true);

        // A new game starts with Seeker customization (slice step 1). Embedding proof scenes and
        // requested checks keep the ordinary start state they assert against.
        if (GetTree().CurrentScene == this && !SmokeCheckHost.SuppressesNewGameStart())
        {
            _ = GetNode<SeekerCustomizationMenuController>("SeekerCustomizationOverlay/Panel").TryOpen();
        }
    }

    private void InitializeCoordinators()
    {
        WorldEventController worldEventController = GetNode<WorldEventController>("WorldEventController");
        CharacterTurnTranscriptController transcript =
            GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
        conversationTurns = new(new ConversationTurnNodePorts(
            GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController"),
            GetNode<TemporaryCharacterModelGenerationInputSource>(
                "TemporaryCharacterModelGenerationInputSource"),
            GetNode<TemporaryCharacterModelSource>("TemporaryCharacterModelSource"),
            GetNode<CharacterTurnActivityController>("CharacterTurnActivityController"),
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "TemporaryOrdinaryCharacterParticipantSource"),
            GetNode<Node2D>("Seeker"),
            healer,
            GetNode<Marker2D>("SageHearingPosition"),
            GetNode<DialogueInputController>("DialogueInputController"),
            GetNode<DialogueEventController>("DialogueEventController"),
            transcript,
            GetNode<OrdinaryPlayerDialogueParticipationCoordinator>(
                "OrdinaryPlayerDialogueParticipationCoordinator"),
            GetNode<CharacterTurnParticipationRoundTaskController>(
                "CharacterTurnParticipationRoundTaskController"),
            GetNode<TemporaryCharacterTurnParticipationModelSource>(
                "TemporaryCharacterTurnParticipationModelSource"),
            GetNode<AutonomousConversationBudgetController>("AutonomousConversationBudgetController"),
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
            GetNode<TemporaryWorldObjectSource>("TemporaryWorldObjectSource"),
            worldEventController,
            GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource"),
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
        independentTaskTick = new(new CharacterIndependentTaskTickNodePorts(
            GetNode<CharacterIndependentTaskStepClockController>(
                "CharacterIndependentTaskStepClockController"),
            GetNode<CharacterIndependentTaskTravelController>("CharacterIndependentTaskTravelController"),
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "CharacterIndependentTaskReturnTravelController"),
            GetNode<CharacterIndependentTaskDepartureController>(
                "CharacterIndependentTaskDepartureController"),
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "TemporaryCharacterIndependentTaskOccurrenceSource"),
            GetNode<CharacterIndependentTaskRoundTripCoordinator>(
                "CharacterIndependentTaskRoundTripCoordinator"),
            GetNode<CharacterIndependentTaskBodyPresentationController>(
                "CharacterIndependentTaskBodyPresentationController"),
            worldEventController,
            transcript));
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        IndependentTaskTick.Process(delta);
        ConversationTurns.Process();
        UpdateModeStatus(force: false);
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
            FocusedSearchInput.Handle(FocusedSearchInputAction.Cancel);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchCompleteAction))
        {
            FocusedSearchInput.Handle(FocusedSearchInputAction.Complete);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchResolveAction))
        {
            FocusedSearchInput.Handle(FocusedSearchInputAction.Resolve);
        }
        else if (ControlInputMatcher.Pressed(@event, FocusedSearchStartAction))
        {
            FocusedSearchInput.Handle(FocusedSearchInputAction.Start);
        }
        else if (ControlInputMatcher.Pressed(@event, LayeredInscriptionDiscoverAction))
        {
            if (layeredInscriptionDiscoveryCoordinator.TryDiscover(
                    layeredInscriptionDiscoveryCoordinator.SceneSource.KnowledgeableSeeker))
            {
                _ = layeredInscriptionEnvironmentalFlowCoordinator.TryCompleteRevealed();
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
        healerModeStatus.Text = HealerModeStatusText.Format(displayedMode, examinationAction.Running);
    }
}

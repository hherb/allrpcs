using Allrpcs.Application.Movement;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Hosts the keyboard-movement greybox scene.
/// </summary>
public partial class Main : Node
{
    private const string InitialFocusedSearchResultText = "Focused search: NO ATTEMPT";
    private static readonly StringName FocusedSearchCancelAction = new("focused_search_cancel");
    private static readonly StringName FocusedSearchCompleteAction = new("focused_search_complete");
    private static readonly StringName FocusedSearchResolveAction = new("focused_search_resolve");
    private static readonly StringName FocusedSearchStartAction = new("focused_search_start");
    private static readonly StringName HealerToggleAction = new("healer_toggle_follow_wait");
    private static readonly StringName HealerMoveAction = new("healer_move");

    private Healer healer = null!;
    private Marker2D healerCommandDestination = null!;
    private Label healerModeStatus = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private Label focusedSearchResultStatus = null!;
    private InteractionFocusController interactionFocusController = null!;
    private TemporaryResolvedPerceptionIndicator[] resolvedPerceptionIndicators = [];
    private GreyboxInteractionTarget[] resolvedPerceptionTargets = [];
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private TemporaryFocusedSearchOutcomeSource focusedSearchOutcomeSource = null!;
    private TemporaryObservationReinterpretationCandidateSource
        observationReinterpretationCandidateSource = null!;
    private TemporaryPassiveNoticingCandidateSource passiveNoticingCandidateSource = null!;
    private TemporarySeekerCharacterSource seekerCharacterSource = null!;
    private TemporarySeekerPerceptionSource seekerPerceptionSource = null!;
    private TemporaryWorldEventSource worldEventSource = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private WorldEventLogController worldEventLogController = null!;
    private WorldObjectController worldObjectController = null!;
    private HealerCommandMode displayedMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        healer = GetNode<Healer>("Healer");
        healerCommandDestination = GetNode<Marker2D>("HealerCommandDestination");
        healerModeStatus = GetNode<Label>("CommandOverlay/Panel/ModeStatus");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "Seeker/FocusedSearchActivityController");
        focusedSearchResultStatus = GetNode<Label>("CommandOverlay/Panel/FocusedSearchResult");
        interactionFocusController = GetNode<InteractionFocusController>(
            "InteractionFocusController");
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
        worldEventLogController = GetNode<WorldEventLogController>("WorldEventLogController");
        worldObjectController = GetNode<WorldObjectController>("WorldObjectController");
        RecordInitialWorldEvent();
        if (!worldObjectController.TryInitializeWorldObjects(worldObjectSource.WorldObjects))
        {
            throw new InvalidOperationException(
                "The world-object controller could not initialize the scene snapshot.");
        }

        if (!focusedSearchController.TryInitializePerception(seekerPerceptionSource.Perception))
        {
            throw new InvalidOperationException(
                "The focused-search controller could not initialize Seeker perception.");
        }

        ProduceInitialPassiveNoticingCandidate();
        ResolveInitialPassiveNoticingCandidate();
        ProduceInitialObservationReinterpretationCandidate();
        ResolveInitialObservationReinterpretationCandidate();

        RefreshResolvedPerceptionPresentation();

        focusedSearchResultStatus.Text = InitialFocusedSearchResultText;
        UpdateModeStatus(force: true);
    }

    private void RecordInitialWorldEvent()
    {
        if (!worldEventLogController.TryAppend(worldEventSource.EventId))
        {
            throw new InvalidOperationException(
                "The initial greybox world event could not be recorded.");
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
        _ = delta;
        UpdateModeStatus(force: false);
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(FocusedSearchCancelAction))
        {
            FocusedSearchRequest? activeRequest = GetActiveFocusedSearchRequest();
            bool changed = activeRequest is not null
                && focusedSearchController.TryCancelSearch(activeRequest);
            PresentFocusedSearchResult("CANCEL", changed);
        }
        else if (@event.IsActionPressed(FocusedSearchCompleteAction))
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
        else if (@event.IsActionPressed(FocusedSearchResolveAction))
        {
            FocusedSearchOutcome? outcome = GetFirstRetainedFocusedSearchOutcome();
            bool changed = outcome is not null
                && worldObjectController.FindWorldObject(outcome.Request.TargetObjectId)
                    is { } worldObject
                && focusedSearchController.TryResolveOutcome(outcome, worldObject);
            if (changed)
            {
                RefreshResolvedPerceptionPresentation();
            }

            PresentFocusedSearchResult("RESOLVE", changed);
        }
        else if (@event.IsActionPressed(FocusedSearchStartAction))
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
        else if (@event.IsActionPressed(HealerToggleAction))
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
        else if (@event.IsActionPressed(HealerMoveAction))
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

    private void UpdateModeStatus(bool force)
    {
        if (!force && displayedMode == healer.CommandMode)
        {
            return;
        }

        displayedMode = healer.CommandMode;
        healerModeStatus.Text = $"Current healer mode: {FormatMode(displayedMode)}";
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

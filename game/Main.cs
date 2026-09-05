using Allrpcs.Application.Movement;
using Allrpcs.Domain.Discovery;
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
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private TemporaryFocusedSearchOutcomeSource focusedSearchOutcomeSource = null!;
    private TemporarySeekerPerceptionSource seekerPerceptionSource = null!;
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
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "TemporaryFocusedSearchContextSource");
        focusedSearchOutcomeSource = GetNode<TemporaryFocusedSearchOutcomeSource>(
            "TemporaryFocusedSearchOutcomeSource");
        seekerPerceptionSource = GetNode<TemporarySeekerPerceptionSource>(
            "TemporarySeekerPerceptionSource");
        if (!focusedSearchController.TryInitializePerception(seekerPerceptionSource.Perception))
        {
            throw new InvalidOperationException(
                "The focused-search controller could not initialize Seeker perception.");
        }

        focusedSearchResultStatus.Text = InitialFocusedSearchResultText;
        UpdateModeStatus(force: true);
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
            bool changed = activeRequest is not null
                && focusedSearchController.TryCompleteSearchWithOutcome(
                    focusedSearchOutcomeSource.CreateOutcome(activeRequest));
            PresentFocusedSearchResult("COMPLETE", changed);
        }
        else if (@event.IsActionPressed(FocusedSearchResolveAction))
        {
            FocusedSearchOutcome? outcome = GetFirstRetainedFocusedSearchOutcome();
            bool changed = outcome is not null
                && focusedSearchController.TryResolveOutcome(outcome);
            PresentFocusedSearchResult("RESOLVE", changed);
        }
        else if (@event.IsActionPressed(FocusedSearchStartAction))
        {
            bool changed = interactionFocusController.TryStartFocusedSearch(
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

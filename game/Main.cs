using Allrpcs.Application.Movement;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Hosts the keyboard-movement greybox scene.
/// </summary>
public partial class Main : Node
{
    private static readonly StringName FocusedSearchStartAction = new("focused_search_start");
    private static readonly StringName HealerToggleAction = new("healer_toggle_follow_wait");
    private static readonly StringName HealerMoveAction = new("healer_move");

    private Healer healer = null!;
    private Marker2D healerCommandDestination = null!;
    private Label healerModeStatus = null!;
    private InteractionFocusController interactionFocusController = null!;
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private HealerCommandMode displayedMode;

    /// <inheritdoc />
    public override void _Ready()
    {
        healer = GetNode<Healer>("Healer");
        healerCommandDestination = GetNode<Marker2D>("HealerCommandDestination");
        healerModeStatus = GetNode<Label>("CommandOverlay/Panel/ModeStatus");
        interactionFocusController = GetNode<InteractionFocusController>(
            "InteractionFocusController");
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "TemporaryFocusedSearchContextSource");
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
        if (@event.IsActionPressed(FocusedSearchStartAction))
        {
            _ = interactionFocusController.TryStartFocusedSearch(
                focusedSearchContextSource.Conditions,
                focusedSearchContextSource.Capabilities,
                focusedSearchContextSource.Inventory);
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

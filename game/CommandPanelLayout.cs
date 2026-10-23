using Allrpcs.Application.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Stacks the command panel's lines that currently say something and fits the panel to them, so
/// empty or idle status lines do not cover the world (Phase 4 playtest).
/// </summary>
public partial class CommandPanelLayout : ColorRect
{
    private const float Margin = 7;
    private const float Spacing = 3;
    private Label[] lines = [];

    /// <inheritdoc />
    public override void _Ready() => lines = [.. GetChildren().OfType<Label>()];

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float y = Margin;
        foreach (Label line in lines)
        {
            line.Visible = IsShown(line.Text);
            if (!line.Visible) { continue; }
            line.Position = new(line.Position.X, y);
            y += line.Size.Y + Spacing;
        }
        Size = new(Size.X, y + Margin - Spacing);
    }

    /// <summary>Hides empty lines and the idle focus/search statuses until they report something.</summary>
    internal static bool IsShown(string text) => text.Length > 0
        && text != InteractionFocusController.NoFocusText && text != FocusedSearchInputCoordinator.InitialResultText;
}

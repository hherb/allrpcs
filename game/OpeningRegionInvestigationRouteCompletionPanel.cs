using Allrpcs.Application.Investigations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders and explicitly acknowledges one exact safe route-completion presentation.</summary>
public partial class OpeningRegionInvestigationRouteCompletionPanel : CanvasLayer
{
    private Button acknowledge = null!;
    private Label content = null!;
    private OpeningRegionInvestigationRouteCompletionPresentationCoordinator coordinator = null!;
    private Label title = null!;
    private OpeningRegionInvestigationRouteCompletionPresentationSnapshot? presentation;

    /// <summary>Gets the exact safe presentation rendered while its owner retains it.</summary>
    public OpeningRegionInvestigationRouteCompletionPresentationSnapshot? Presentation =>
        presentation is not null
        && ReferenceEquals(coordinator.Presentation, presentation)
            ? presentation
            : null;

    /// <inheritdoc />
    public override void _Ready()
    {
        coordinator = GetNode<OpeningRegionInvestigationRouteCompletionPresentationCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionPresentationCoordinator");
        title = GetNode<Label>("Panel/Title");
        content = GetNode<Label>("Panel/Content");
        acknowledge = GetNode<Button>("Panel/Acknowledge");
        acknowledge.Pressed += OnAcknowledgePressed;
        ClearPresentation();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (presentation is not null && Presentation is null)
        {
            ClearPresentation();
        }

        if (presentation is null && coordinator.Presentation is { } current)
        {
            _ = TryPresent(current);
        }
    }

    /// <summary>Renders only the presentation owner's exact current safe snapshot.</summary>
    public bool TryPresent(
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot expectedPresentation)
    {
        ArgumentNullException.ThrowIfNull(expectedPresentation);

        if (!ReferenceEquals(coordinator.Presentation, expectedPresentation)
            || ReferenceEquals(Presentation, expectedPresentation))
        {
            return false;
        }

        presentation = expectedPresentation;
        title.Text = expectedPresentation.Title;
        content.Text = expectedPresentation.Content;
        Show();
        acknowledge.GrabFocus();
        return true;
    }

    private void OnAcknowledgePressed()
    {
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot? current = Presentation;
        if (current is null || !coordinator.TryAcknowledge(current))
        {
            return;
        }

        ClearPresentation();
    }

    private void ClearPresentation()
    {
        acknowledge?.ReleaseFocus();
        presentation = null;
        title.Text = string.Empty;
        content.Text = string.Empty;
        Hide();
    }
}

using Allrpcs.Application.Map;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders one exact current visibility-safe read-only map presentation.</summary>
public partial class SeekerMapPanel : CanvasLayer
{
    private Label content = null!;
    private SeekerMapPresentationController presentations = null!;
    private Label title = null!;

    /// <summary>Gets the exact safe map presentation currently rendered by the panel.</summary>
    public SeekerMapPresentationSnapshot? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        presentations = GetNode<SeekerMapPresentationController>(
            "../SeekerMapPresentationController");
        title = GetNode<Label>("Panel/Title");
        content = GetNode<Label>("Panel/Scroll/Content");
        Presentation = null;
        title.Text = string.Empty;
        content.Text = string.Empty;
        Hide();
    }

    /// <summary>Renders only the presentation controller's exact current safe snapshot.</summary>
    public bool TryPresent(SeekerMapPresentationSnapshot expectedPresentation)
    {
        ArgumentNullException.ThrowIfNull(expectedPresentation);

        if (!ReferenceEquals(presentations.Presentation, expectedPresentation))
        {
            return false;
        }

        if (ReferenceEquals(Presentation, expectedPresentation))
        {
            if (Visible)
            {
                return false;
            }

            Show();
            return true;
        }

        SeekerMapPresentationText text =
            SeekerMapPresentationTextProjection.Project(expectedPresentation);
        Presentation = expectedPresentation;
        title.Text = text.Title;
        content.Text = text.Content;
        Show();
        return true;
    }

    /// <summary>Hides one visible map presentation without discarding its safe text.</summary>
    public bool TryClose()
    {
        if (!Visible)
        {
            return false;
        }

        Hide();
        return true;
    }
}

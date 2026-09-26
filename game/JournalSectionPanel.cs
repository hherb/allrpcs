using Allrpcs.Application.Journal;
using Godot;

namespace Allrpcs.Game;

/// <summary>Renders one exact current content-safe journal-section presentation.</summary>
public partial class JournalSectionPanel : CanvasLayer
{
    private Label content = null!;
    private JournalSectionPresentationController presentations = null!;
    private Label sectionLabel = null!;

    /// <summary>Gets the exact journal-section presentation currently rendered by the panel.</summary>
    public JournalSectionPresentationSnapshot? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        presentations = GetNode<JournalSectionPresentationController>(
            "../JournalSectionPresentationController");
        sectionLabel = GetNode<Label>("Panel/SectionLabel");
        content = GetNode<Label>("Panel/Scroll/Content");
        Presentation = null;
        sectionLabel.Text = string.Empty;
        content.Text = string.Empty;
        Hide();
    }

    /// <summary>Renders only the presentation controller's exact current non-null snapshot.</summary>
    public bool TryPresent(JournalSectionPresentationSnapshot expectedPresentation)
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

        JournalSectionPresentationText text =
            JournalSectionPresentationTextProjection.Project(expectedPresentation);
        Presentation = expectedPresentation;
        sectionLabel.Text = text.SectionLabel;
        content.Text = text.Content;
        Show();
        return true;
    }

    /// <summary>Hides one currently visible journal presentation without discarding its text.</summary>
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

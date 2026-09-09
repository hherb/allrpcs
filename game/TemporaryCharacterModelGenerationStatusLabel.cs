using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Presents a content-free character-model generation status until runtime composition replaces
/// this temporary seam.
/// </summary>
public partial class TemporaryCharacterModelGenerationStatusLabel : Label
{
    /// <summary>Gets the exact currently presented canonical status, if any.</summary>
    public CharacterModelGenerationStatusPresentation? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ClearPresentation();
    }

    /// <summary>Projects and visibly presents one stream prefix without retaining it.</summary>
    public void Present(CharacterModelStreamSnapshot snapshot)
    {
        CharacterModelGenerationStatusPresentation presentation =
            CharacterModelGenerationStatusPresentation.Project(snapshot);
        Presentation = presentation;
        Text = presentation.Text;
        Show();
    }

    /// <summary>Observes and presents the exact initialized snapshot without retaining its owner.</summary>
    public bool Observe(CharacterModelStreamController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        CharacterModelStreamSnapshot? snapshot = controller.Snapshot;
        if (snapshot is null)
        {
            return false;
        }

        Present(snapshot);
        return true;
    }

    /// <summary>Clears and hides the temporary status without changing prior inputs.</summary>
    public void ClearPresentation()
    {
        Presentation = null;
        Text = string.Empty;
        Hide();
    }
}

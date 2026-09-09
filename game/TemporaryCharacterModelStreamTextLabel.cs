using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents only the bounded text-delta projection of one character-model stream.</summary>
public partial class TemporaryCharacterModelStreamTextLabel : Label
{
    /// <summary>Gets the exact currently presented text-only projection, if any.</summary>
    public CharacterModelStreamTextPresentation? Presentation { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ClearPresentation();
    }

    /// <summary>Projects one stream without retaining it or interpreting terminal content.</summary>
    public void Present(CharacterModelStreamSnapshot snapshot)
    {
        CharacterModelStreamTextPresentation presentation =
            CharacterModelStreamTextPresentation.Project(snapshot);
        Presentation = presentation;
        Text = presentation.Text;
        if (presentation.HasText)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    /// <summary>Observes the exact initialized snapshot without retaining its owner.</summary>
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

    /// <summary>Clears and hides streamed text without changing prior inputs.</summary>
    public void ClearPresentation()
    {
        Presentation = null;
        Text = string.Empty;
        Hide();
    }
}

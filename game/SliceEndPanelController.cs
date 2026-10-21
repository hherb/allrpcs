using Allrpcs.Application.Diseases;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// The slice's explicit end screen. A completed slice may keep exploring; any ending can return to
/// the title, which reloads Main and so offers Continue or New game again. The panel is a
/// full-screen backdrop that swallows clicks, so no overlay button (Save above all, which would
/// overwrite the save with a lost game) can be used while an ending is shown.
/// </summary>
public partial class SliceEndPanelController : Control
{
    private Label title = null!;
    private Label body = null!;
    private Button keepExploring = null!;
    private Button returnToTitle = null!;
    private Seeker seeker = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        title = GetNode<Label>("Box/Title");
        body = GetNode<Label>("Box/Body");
        keepExploring = GetNode<Button>("Box/KeepExploring");
        returnToTitle = GetNode<Button>("Box/ReturnToTitle");
        seeker = GetNode<Seeker>("../../Seeker");
        keepExploring.Pressed += Close;
        returnToTitle.Pressed += ReturnToTitle;
        Hide();
    }

    /// <summary>Gets the ending shown now, or null while the panel is closed.</summary>
    internal FirstPatientSliceOutcome? Shown { get; private set; }

    /// <summary>Shows one ending and holds the Seeker while it is open.</summary>
    internal void Present(FirstPatientSliceOutcome outcome)
    {
        (string titleText, string bodyText) = FirstPatientStakesCopy.Ending(outcome);
        title.Text = titleText;
        body.Text = bodyText;
        keepExploring.Visible = outcome == FirstPatientSliceOutcome.Completed;
        if (Shown is null) { seeker.SuspendForModal(this); }
        Shown = outcome;
        Show();
        (keepExploring.Visible ? keepExploring : returnToTitle).GrabFocus();
    }

    /// <summary>Closes the panel and releases the Seeker; closing a closed panel does nothing.</summary>
    internal void Close()
    {
        if (Shown is null) { return; }
        Shown = null;
        Hide();
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is not null && IsAncestorOf(focus)) { focus.ReleaseFocus(); }
        seeker.ResumeAfterModal(this);
    }

    private void ReturnToTitle()
    {
        Close();
        GetTree().ReloadCurrentScene();
    }
}

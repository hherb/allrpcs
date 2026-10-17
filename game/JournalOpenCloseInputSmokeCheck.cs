using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit journal open and close input with modal movement ownership.</summary>
public partial class JournalOpenCloseInputSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_OPEN_CLOSE_INPUT_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ToggleAction = "journal_toggle";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        JournalOpenCloseInputController input =
            GetNode<JournalOpenCloseInputController>("../JournalOpenCloseInputController");
        JournalController journalController = GetNode<JournalController>("../JournalController");
        JournalSectionSelectionController selection =
            GetNode<JournalSectionSelectionController>("../JournalSectionSelectionController");
        JournalSectionPresentationController presentation =
            GetNode<JournalSectionPresentationController>("../JournalSectionPresentationController");
        JournalSectionPanel panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        Button close = panel.GetNode<Button>("Panel/Close");
        Button next = panel.GetNode<Button>("Panel/Next");
        Button previous = panel.GetNode<Button>("Panel/Previous");
        Label sectionLabel = panel.GetNode<Label>("Panel/SectionLabel");
        Label content = panel.GetNode<Label>("Panel/Scroll/Content");
        Seeker seeker = GetNode<Seeker>("../Seeker");
        FocusedSearchActivityController focusedSearch =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        WorldObjectController worldObjects = GetNode<WorldObjectController>(
            "../WorldObjectController");
        DialogueEventController dialogue = GetNode<DialogueEventController>(
            "../DialogueEventController");
        FirstPatientStateController firstPatient = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        TemporaryFirstPatientTreatmentContextSource treatment =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource");

        CharacterPerceptionSnapshot perception = focusedSearch.Perception
            ?? throw new InvalidOperationException("Missing initialized Seeker perception.");
        WorldObjectSnapshot objects = worldObjects.WorldObjects
            ?? throw new InvalidOperationException("Missing initialized world objects.");
        DialogueEvent? lastDialogue = dialogue.LastOccurredEvent;
        ExperimentJournalSnapshot experiments = firstPatient.Experiments
            ?? throw new InvalidOperationException("Missing initialized experiment journal.");
        FirstPatientTreatmentContext context = treatment.Context;

        input._UnhandledInput(Action("journal_unrelated", pressed: true));
        input._UnhandledInput(Action(ToggleAction, pressed: false));
        if (journalController.Journal is not null
            || selection.Selection is not null
            || presentation.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || close.HasFocus()
            || !seeker.IsPhysicsProcessing()
            || !HasOnlyPhysicalKey(ToggleAction, Key.J))
        {
            Fail("Journal input changed state during hidden startup or unrelated input.");
            return;
        }

        seeker.Velocity = new Vector2(24.0f, -12.0f);
        input._UnhandledInput(Action(ToggleAction, pressed: true));
        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Journal input did not compose the journal.");
        JournalSectionPresentationSnapshot shown = presentation.Presentation
            ?? throw new InvalidOperationException("Journal input did not project a section.");
        JournalSectionPresentationText expected =
            JournalSectionPresentationTextProjection.Project(shown);
        if (!ReferenceEquals(selection.Selection, journal.Catalogue.Sections[0])
            || !ReferenceEquals(shown.Section, journal.Catalogue.Sections[0])
            || !ReferenceEquals(panel.Presentation, shown)
            || !panel.Visible
            || sectionLabel.Text != expected.SectionLabel
            || content.Text != expected.Content
            || seeker.IsPhysicsProcessing()
            || seeker.Velocity != Vector2.Zero
            || !close.HasFocus()
            || !previous.Disabled
            || next.Disabled)
        {
            Fail("Journal input did not open the exact first safe section modally.");
            return;
        }

        for (int index = 1; index < journal.Catalogue.Sections.Count; index++)
        {
            next.EmitSignal(BaseButton.SignalName.Pressed);
            shown = presentation.Presentation
                ?? throw new InvalidOperationException("Journal navigation lost presentation.");
            expected = JournalSectionPresentationTextProjection.Project(shown);
            if (!ReferenceEquals(selection.Selection, journal.Catalogue.Sections[index])
                || !ReferenceEquals(shown.Section, journal.Catalogue.Sections[index])
                || !ReferenceEquals(panel.Presentation, shown)
                || sectionLabel.Text != expected.SectionLabel
                || content.Text != expected.Content
                || previous.Disabled
                || next.Disabled != (index == journal.Catalogue.Sections.Count - 1))
            {
                Fail("Journal next control did not traverse exact sections in canonical order.");
                return;
            }
        }

        JournalSectionPresentationSnapshot last = shown;
        next.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(presentation.Presentation, last)
            || !ReferenceEquals(panel.Presentation, last))
        {
            Fail("Journal next control changed state past the final section.");
            return;
        }

        for (int index = journal.Catalogue.Sections.Count - 2; index >= 0; index--)
        {
            previous.EmitSignal(BaseButton.SignalName.Pressed);
            shown = presentation.Presentation
                ?? throw new InvalidOperationException("Journal navigation lost presentation.");
            expected = JournalSectionPresentationTextProjection.Project(shown);
            if (!ReferenceEquals(selection.Selection, journal.Catalogue.Sections[index])
                || !ReferenceEquals(shown.Section, journal.Catalogue.Sections[index])
                || !ReferenceEquals(panel.Presentation, shown)
                || sectionLabel.Text != expected.SectionLabel
                || content.Text != expected.Content
                || previous.Disabled != (index == 0)
                || next.Disabled)
            {
                Fail("Journal previous control did not traverse exact sections in canonical order.");
                return;
            }
        }

        JournalSectionPresentationSnapshot firstAgain = shown;
        previous.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(presentation.Presentation, firstAgain)
            || !ReferenceEquals(panel.Presentation, firstAgain))
        {
            Fail("Journal previous control changed state before the first section.");
            return;
        }

        next.GrabFocus();
        input._UnhandledInput(Action(ToggleAction, pressed: true));
        if (panel.Visible
            || seeker.IsPhysicsProcessing() is false
            || GetViewport().GuiGetFocusOwner() is not null
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(presentation.Presentation, shown)
            || !ReferenceEquals(panel.Presentation, shown)
            || sectionLabel.Text != expected.SectionLabel
            || content.Text != expected.Content)
        {
            Fail("Journal toggle did not close while preserving exact rendered state.");
            return;
        }

        input._UnhandledInput(Action(ToggleAction, pressed: true));
        if (!panel.Visible
            || seeker.IsPhysicsProcessing()
            || !close.HasFocus()
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(presentation.Presentation, shown)
            || !ReferenceEquals(panel.Presentation, shown))
        {
            Fail("Journal input did not reopen the retained exact presentation.");
            return;
        }

        close.EmitSignal(BaseButton.SignalName.Pressed);
        if (panel.Visible
            || !seeker.IsPhysicsProcessing()
            || GetViewport().GuiGetFocusOwner() is not null)
        {
            Fail("Journal close control did not release focus and resume movement.");
            return;
        }

        seeker.SetPhysicsProcess(false);
        input._UnhandledInput(Action(ToggleAction, pressed: true));
        input._UnhandledInput(Action(ToggleAction, pressed: true));
        bool restoredSuspendedMovement = !seeker.IsPhysicsProcessing();
        seeker.SetPhysicsProcess(true);

        if (!restoredSuspendedMovement
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || !HasClosedSurface())
        {
            Fail("Journal input changed source, gameplay, or prior movement ownership.");
            return;
        }

        GD.Print(
            "Journal open-close input smoke passed: inert startup, explicit first-section "
            + "composition, exact safe presentation, focus capture and release, movement stop and "
            + "restoration, canonical closed-end section traversal, toggle and close-control paths, "
            + "retained reopen state, original suspension preservation, closed surface, and source "
            + "isolation were correct.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(JournalOpenCloseInputController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["ResetAfterRestore", "_Ready", "_UnhandledInput"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = action,
        Pressed = pressed,
    };

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

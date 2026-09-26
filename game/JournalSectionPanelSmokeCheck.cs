using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit exact journal-section panel rendering when enabled.</summary>
public partial class JournalSectionPanelSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_SECTION_PANEL_CHECK";
    private const string EnabledEnvironmentValue = "1";

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

        RepresentativeJournalCompositionCoordinator composition =
            GetNode<RepresentativeJournalCompositionCoordinator>(
                "../RepresentativeJournalCompositionCoordinator");
        JournalController journalController = GetNode<JournalController>("../JournalController");
        JournalSectionSelectionController selection =
            GetNode<JournalSectionSelectionController>("../JournalSectionSelectionController");
        JournalSectionPresentationController presentation =
            GetNode<JournalSectionPresentationController>("../JournalSectionPresentationController");
        JournalSectionPanel panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        Label sectionLabel = panel.GetNode<Label>("Panel/SectionLabel");
        Label content = panel.GetNode<Label>("Panel/Scroll/Content");
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
        Label focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        CanvasItem commandPanel = GetNode<CanvasItem>("../CommandOverlay/Panel");

        CharacterPerceptionSnapshot perception = focusedSearch.Perception
            ?? throw new InvalidOperationException("Missing initialized Seeker perception.");
        WorldObjectSnapshot objects = worldObjects.WorldObjects
            ?? throw new InvalidOperationException("Missing initialized world objects.");
        DialogueEvent? lastDialogue = dialogue.LastOccurredEvent;
        ExperimentJournalSnapshot experiments = firstPatient.Experiments
            ?? throw new InvalidOperationException("Missing initialized experiment journal.");
        FirstPatientTreatmentContext context = treatment.Context;
        string focusText = focusStatus.Text;
        bool commandPanelVisible = commandPanel.Visible;

        if (panel.Visible
            || panel.Presentation is not null
            || sectionLabel.Text.Length != 0
            || content.Text.Length != 0
            || !ThrowsArgumentNull(() => panel.TryPresent(null!))
            || !composition.TryComposeAndInitialize())
        {
            Fail("Journal section panel did not begin hidden, empty, and inert.");
            return;
        }

        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Representative journal was not installed.");
        JournalSectionPresentationSnapshot? priorPresentation = null;
        string priorLabel = string.Empty;
        string priorContent = string.Empty;
        for (int index = 0; index < journal.Catalogue.Sections.Count; index++)
        {
            JournalSectionDefinition section = journal.Catalogue.Sections[index];
            bool projected = index == 0
                ? selection.TrySelect(journal, section) && presentation.TryProject()
                : selection.TrySelect(journal, section) && presentation.TryRefresh();
            JournalSectionPresentationSnapshot? current = presentation.Presentation;
            if (!projected || current is null)
            {
                Fail("Journal section panel could not arrange an exact section presentation.");
                return;
            }

            if (priorPresentation is not null
                && (panel.TryPresent(priorPresentation)
                    || !ReferenceEquals(panel.Presentation, priorPresentation)
                    || sectionLabel.Text != priorLabel
                    || content.Text != priorContent))
            {
                Fail("Journal section panel changed rendered state for a stale presentation.");
                return;
            }

            JournalSectionPresentationSnapshot rebuilt =
                JournalSectionPresentationProjection.Project(journal, section);
            JournalSectionDefinition foreignSection =
                journal.Catalogue.Sections[(index + 1) % journal.Catalogue.Sections.Count];
            JournalSectionPresentationSnapshot foreign =
                JournalSectionPresentationProjection.Project(journal, foreignSection);
            JournalSectionPresentationSnapshot? beforeRejectedPresentation = panel.Presentation;
            string beforeRejectedLabel = sectionLabel.Text;
            string beforeRejectedContent = content.Text;
            if (panel.TryPresent(rebuilt)
                || panel.TryPresent(foreign)
                || !ReferenceEquals(panel.Presentation, beforeRejectedPresentation)
                || sectionLabel.Text != beforeRejectedLabel
                || content.Text != beforeRejectedContent)
            {
                Fail("Journal section panel accepted rebuilt or foreign presentation state.");
                return;
            }

            JournalSectionPresentationText expected =
                JournalSectionPresentationTextProjection.Project(current);
            if (!panel.TryPresent(current)
                || !panel.Visible
                || !ReferenceEquals(panel.Presentation, current)
                || sectionLabel.Text != expected.SectionLabel
                || content.Text != expected.Content
                || panel.TryPresent(current)
                || !ReferenceEquals(panel.Presentation, current))
            {
                Fail("Journal section panel did not render one exact stable safe section.");
                return;
            }

            priorPresentation = current;
            priorLabel = sectionLabel.Text;
            priorContent = content.Text;
        }

        if (!ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || focusStatus.Text != focusText
            || commandPanel.Visible != commandPanelVisible
            || !HasClosedSurface())
        {
            Fail("Journal section panel changed journal, source, gameplay, or unrelated UI state.");
            return;
        }

        GD.Print(
            "Journal section panel smoke passed: hidden inert startup, all five exact safe text "
            + "arms, neutral empty content, explicit refreshed replacement, rebuilt, foreign, "
            + "stale, repeated, and null rejection, closed surface, and journal, source, gameplay, "
            + "and unrelated UI isolation were correct.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(JournalSectionPanel);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);
        return methods.SequenceEqual(["TryClose", "TryPresent", "_Ready"])
            && properties.Length == 1
            && properties[0].Name == nameof(JournalSectionPanel.Presentation)
            && properties[0].PropertyType == typeof(JournalSectionPresentationSnapshot)
            && properties[0].SetMethod?.IsPrivate == true;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

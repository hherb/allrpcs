using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit content-safe selected-section projection when enabled.</summary>
public partial class JournalSectionPresentationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_SECTION_PRESENTATION_CHECK";
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

        CharacterPerceptionSnapshot perception = focusedSearch.Perception
            ?? throw new InvalidOperationException("Missing initialized Seeker perception.");
        WorldObjectSnapshot objects = worldObjects.WorldObjects
            ?? throw new InvalidOperationException("Missing initialized world objects.");
        DialogueEvent? lastDialogue = dialogue.LastOccurredEvent;
        ExperimentJournalSnapshot experiments = firstPatient.Experiments
            ?? throw new InvalidOperationException("Missing initialized experiment journal.");
        FirstPatientTreatmentContext context = treatment.Context;
        string focusText = focusStatus.Text;

        if (journalController.Journal is not null
            || selection.Selection is not null
            || presentation.Presentation is not null
            || presentation.TryProject()
            || !composition.TryComposeAndInitialize()
            || presentation.TryProject())
        {
            Fail("Journal section presentation did not remain inert before exact selection.");
            return;
        }

        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Representative journal was not installed.");
        JournalSectionDefinition observations = journal.Catalogue.Sections[0];
        JournalSectionDefinition testimony = journal.Catalogue.Sections[1];

        if (!selection.TrySelect(journal, observations)
            || !presentation.TryProject())
        {
            Fail("Journal section presentation rejected the exact selected section.");
            return;
        }

        JournalSectionPresentationSnapshot projected = presentation.Presentation
            ?? throw new InvalidOperationException("Selected journal section was not projected.");
        if (!ReferenceEquals(projected.Section, observations)
            || !ReferenceEquals(projected.Observations, journal.Observations)
            || projected.Testimony is not null
            || projected.Hypotheses is not null
            || projected.Experiments is not null
            || projected.OpenQuestions is not null
            || !selection.TrySelect(journal, testimony)
            || presentation.TryProject()
            || !ReferenceEquals(presentation.Presentation, projected)
            || !ReferenceEquals(projected.Section, observations)
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || !string.Equals(focusStatus.Text, focusText, StringComparison.Ordinal)
            || !HasClosedSurface())
        {
            Fail("Journal section presentation exposed unsafe content or changed surrounding state.");
            return;
        }

        GD.Print(
            "Journal section presentation smoke passed: inert startup, exact selected-section "
            + "projection, one safe content arm, one-shot ownership, deferred refresh, closed "
            + "public surface, and journal, source, and UI isolation were correct.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(JournalSectionPresentationController);
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        System.Reflection.PropertyInfo? presentation = properties.SingleOrDefault(
            property => property.Name == nameof(JournalSectionPresentationController.Presentation));
        return properties.Length == 2
            && presentation?.PropertyType == typeof(JournalSectionPresentationSnapshot)
            && presentation.CanRead
            && presentation.SetMethod?.IsPrivate == true
            && properties.Any(property =>
                property.Name == nameof(JournalSectionPresentationController.IsCurrent)
                && property.PropertyType == typeof(bool)
                && property.SetMethod is null)
            && methods.Length == 3
            && methods.Any(method => method.Name == nameof(Node._Ready))
            && methods.Any(method =>
                method.Name == nameof(JournalSectionPresentationController.TryProject)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 0)
            && methods.Any(method =>
                method.Name == nameof(JournalSectionPresentationController.TryRefresh)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 0);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

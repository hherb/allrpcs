using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene-owned journal-section selection when explicitly enabled.</summary>
public partial class JournalSectionSelectionSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_SECTION_SELECTION_CHECK";
    private const string EnabledEnvironmentValue = "1";

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
        JournalSnapshot foreignJournal = ComposeCompleteEmptyJournal();

        if (journalController.Journal is not null
            || selection.Selection is not null
            || selection.TrySelect(foreignJournal, foreignJournal.Catalogue.Sections[0])
            || selection.Selection is not null
            || !composition.TryComposeAndInitialize())
        {
            Fail("Journal section selection did not remain inert before journal composition.");
            return;
        }

        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Representative journal was not installed.");
        JournalSectionDefinition observations = journal.Catalogue.Sections[0];
        JournalSectionDefinition testimony = journal.Catalogue.Sections[1];
        JournalSectionDefinition rebuiltObservations = new(
            observations.Key,
            observations.DisplayLabel);
        JournalSectionDefinition foreignObservations = foreignJournal.Catalogue.Sections[0];

        if (!selection.TrySelect(journal, observations)
            || !ReferenceEquals(selection.Selection, observations)
            || selection.TrySelect(journal, observations)
            || !ReferenceEquals(selection.Selection, observations)
            || !selection.TrySelect(journal, testimony)
            || !ReferenceEquals(selection.Selection, testimony)
            || selection.TrySelect(foreignJournal, foreignObservations)
            || selection.TrySelect(journal, rebuiltObservations)
            || selection.TrySelect(journal, foreignObservations)
            || !ThrowsArgumentNull(() => selection.TrySelect(null!, testimony))
            || !ThrowsArgumentNull(() => selection.TrySelect(journal, null!))
            || !ReferenceEquals(selection.Selection, testimony)
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || !string.Equals(focusStatus.Text, focusText, StringComparison.Ordinal)
            || !HasClosedSurface())
        {
            Fail("Journal section selection violated exact ownership or state isolation.");
            return;
        }

        GD.Print(
            "Journal section selection smoke passed: inert startup, exact canonical selection, "
            + "explicit replacement, uninitialized, stale, rebuilt, foreign, repeated, and null "
            + "rejection, closed public surface, and journal, source, and UI isolation were correct.");
        GetTree().Quit();
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    private static JournalSnapshot ComposeCompleteEmptyJournal()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(journal, []));
        journal = JournalTestimonySnapshotProjection.Compose(journal, testimony);
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot([])));
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        journal = JournalExperimentSnapshotProjection.Compose(journal, experiments);
        JournalOpenQuestionSectionSnapshot openQuestions =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(
                    journal,
                    new OpenInvestigationQuestionSnapshot([])));
        return JournalOpenQuestionSnapshotProjection.Compose(journal, openQuestions);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(JournalSectionSelectionController);
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
        return properties.Length == 1
            && properties[0].Name == nameof(JournalSectionSelectionController.Selection)
            && properties[0].PropertyType == typeof(JournalSectionDefinition)
            && properties[0].CanRead
            && properties[0].SetMethod?.IsPrivate == true
            && methods.Length == 2
            && methods.Any(method => method.Name == nameof(Node._Ready))
            && methods.Any(method =>
                method.Name == nameof(JournalSectionSelectionController.TrySelect)
                && method.ReturnType == typeof(bool)
                && method.GetParameters() is
                [
                { ParameterType: var journalType },
                { ParameterType: var sectionType },
                ]
                && journalType == typeof(JournalSnapshot)
                && sectionType == typeof(JournalSectionDefinition));
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

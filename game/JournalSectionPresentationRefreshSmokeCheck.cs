using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit selected-section presentation replacement when enabled.</summary>
public partial class JournalSectionPresentationRefreshSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_SECTION_PRESENTATION_REFRESH_CHECK";
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

        if (presentation.TryRefresh()
            || !composition.TryComposeAndInitialize())
        {
            Fail("Journal section presentation refresh did not remain inert before projection.");
            return;
        }

        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Representative journal was not installed.");
        JournalSectionDefinition observations = journal.Catalogue.Sections[0];
        JournalSectionDefinition testimony = journal.Catalogue.Sections[1];
        if (!selection.TrySelect(journal, observations)
            || presentation.TryRefresh()
            || !presentation.TryProject())
        {
            Fail("Journal section presentation refresh bypassed initial projection ownership.");
            return;
        }

        JournalSectionPresentationSnapshot observationPresentation = presentation.Presentation
            ?? throw new InvalidOperationException("Observations presentation was not projected.");
        if (presentation.TryRefresh()
            || !ReferenceEquals(presentation.Presentation, observationPresentation)
            || !selection.TrySelect(journal, testimony)
            || !presentation.TryRefresh())
        {
            Fail("Journal section presentation did not refresh only after exact replacement.");
            return;
        }

        JournalSectionPresentationSnapshot testimonyPresentation = presentation.Presentation
            ?? throw new InvalidOperationException("Testimony presentation was not projected.");
        if (ReferenceEquals(testimonyPresentation, observationPresentation)
            || !ReferenceEquals(testimonyPresentation.Section, testimony)
            || !ReferenceEquals(testimonyPresentation.Testimony, journal.Testimony)
            || testimonyPresentation.Observations is not null
            || testimonyPresentation.Hypotheses is not null
            || testimonyPresentation.Experiments is not null
            || testimonyPresentation.OpenQuestions is not null
            || presentation.TryRefresh()
            || !ReferenceEquals(presentation.Presentation, testimonyPresentation)
            || !selection.TrySelect(journal, observations)
            || !presentation.TryRefresh())
        {
            Fail("Journal section presentation refresh produced unsafe or repeated state.");
            return;
        }

        JournalSectionPresentationSnapshot replacedAgain = presentation.Presentation
            ?? throw new InvalidOperationException("Replacement observations were not projected.");
        if (!ReferenceEquals(replacedAgain.Section, observations)
            || !ReferenceEquals(replacedAgain.Observations, journal.Observations)
            || !ReferenceEquals(observationPresentation.Section, observations)
            || !ReferenceEquals(observationPresentation.Observations, journal.Observations)
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || !string.Equals(focusStatus.Text, focusText, StringComparison.Ordinal))
        {
            Fail("Journal section presentation refresh changed prior values or surrounding state.");
            return;
        }

        GD.Print(
            "Journal section presentation refresh smoke passed: explicit exact replacement, "
            + "unprojected, unchanged, and repeated rejection, immutable prior values, and journal, "
            + "source, and UI isolation were correct.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}

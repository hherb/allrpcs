using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes the scene's existing exact safe sources into one representative journal.</summary>
public partial class RepresentativeJournalCompositionCoordinator : Node
{
    private DialogueEventController dialogue = null!;
    private FirstPatientStateController firstPatient = null!;
    private FocusedSearchActivityController focusedSearch = null!;
    private JournalController journal = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator patientTestimony = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
    private TemporaryFirstPatientTreatmentContextSource treatmentContext = null!;
    private WorldObjectController worldObjects = null!;
    private IReadOnlyList<FirstPatientHypothesisTest>? hypothesisTests;

    /// <summary>Gets the authored hypotheses the player can propose and test.</summary>
    public IReadOnlyList<FirstPatientHypothesisTest> HypothesisTests =>
        hypothesisTests ??= RepresentativeFirstPatientHypothesisSource.Create(treatmentContext.Context.Scenario);

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
        firstPatient = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        focusedSearch = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        journal = GetNode<JournalController>("../JournalController");
        patientTestimony =
            GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
                "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        sageConsultation = GetNode<OpeningRegionSageConsultationInputCoordinator>(
            "../OpeningRegionSageConsultationInputCoordinator");
        treatmentContext = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        worldObjects = GetNode<WorldObjectController>("../WorldObjectController");
    }

    /// <summary>Composes and installs one journal from the scene's current exact source snapshots.</summary>
    public bool TryComposeAndInitialize()
    {
        CharacterPerceptionSnapshot? perception = focusedSearch.Perception;
        WorldObjectSnapshot? objects = worldObjects.WorldObjects;
        ExperimentJournalSnapshot? experiments = firstPatient.Experiments;
        IngredientExperimentJournalSnapshot? doses = firstPatient.AdministrationExperiments;
        FirstPatientTreatmentContext? context = treatmentContext.Context;
        if (perception is null
            || objects is null
            || experiments is null
            || doses is null
            || context is null)
        {
            return false;
        }

        try
        {
            JournalSnapshot snapshot = Compose(
                perception,
                objects,
                GetTestimonyEvents(),
                experiments);
            return journal.TryInitialize(JournalInvestigationTransition.Refresh(
                snapshot, HypothesisTests, context.Scenario, doses, experiments));
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException)
        {
            GD.PrintErr("Journal composition failed: " + error.Message);
            return false;
        }
    }

    /// <summary>Brings the journal's hypotheses, experiments and open questions up to date.</summary>
    public bool TryRefresh()
    {
        JournalSnapshot? current = journal.Journal;
        ExperimentJournalSnapshot? experiments = firstPatient.Experiments;
        IngredientExperimentJournalSnapshot? doses = firstPatient.AdministrationExperiments;
        if (current is null || experiments is null || doses is null)
        {
            return false;
        }

        try
        {
            JournalSnapshot refreshed = JournalInvestigationTransition.Refresh(
                current, HypothesisTests, treatmentContext.Context.Scenario, doses, experiments);
            return ReferenceEquals(refreshed, current) || journal.TryReplace(current, refreshed);
        }
        catch (ArgumentException error)
        {
            // Outcomes and open questions stay stale, so say why.
            GD.PrintErr("Journal refresh failed: " + error.Message);
            return false;
        }
    }

    // The player proposes hypotheses; open questions come from the refresh that follows.
    private static JournalSnapshot Compose(
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects,
        IReadOnlyList<DialogueEvent> dialogueEvents,
        ExperimentJournalSnapshot experiments)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterPerceptionSnapshot visiblePerception = new(
            perception.CharacterId,
            perception.Entries.Where(entry =>
                !ReferenceEquals(entry.VisibilityState, VisibilityState.Unnoticed)));
        JournalObservationSectionBinding observationBinding =
            JournalObservationSectionProjection.Bind(
                catalogue,
                perception.CharacterId,
                visiblePerception);
        JournalObservationWorldBinding observationWorld = JournalObservationWorldProjection.Bind(
            observationBinding,
            worldObjects);
        JournalSnapshot snapshot = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(observationWorld));
        JournalTestimonySectionSnapshot testimonySection =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(snapshot, dialogueEvents));
        snapshot = JournalTestimonySnapshotProjection.Compose(snapshot, testimonySection);

        JournalHypothesisSectionSnapshot hypothesisSection =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(snapshot, new InvestigationHypothesisSnapshot([])));
        snapshot = JournalHypothesisSnapshotProjection.Compose(snapshot, hypothesisSection);

        JournalExperimentSectionSnapshot experimentSection =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(snapshot, experiments));
        snapshot = JournalExperimentSnapshotProjection.Compose(snapshot, experimentSection);

        JournalOpenQuestionSectionSnapshot questionSection =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(snapshot, new OpenInvestigationQuestionSnapshot([])));
        return JournalOpenQuestionSnapshotProjection.Compose(snapshot, questionSection);
    }

    private DialogueEvent[] GetTestimonyEvents()
    {
        List<DialogueEvent> events = [];
        AddDistinct(events, patientTestimony.Testimony);
        AddDistinct(events, sageConsultation.Consultation);
        AddDistinct(events, dialogue.LastOccurredEvent);
        return [.. events];
    }

    private static void AddDistinct(List<DialogueEvent> events, DialogueEvent? candidate)
    {
        if (candidate is not null
            && !events.Any(existing => ReferenceEquals(existing, candidate)))
        {
            events.Add(candidate);
        }
    }
}

using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
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
    private TemporaryFirstPatientTreatmentContextSource treatmentContext = null!;
    private WorldObjectController worldObjects = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
        firstPatient = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        focusedSearch = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        journal = GetNode<JournalController>("../JournalController");
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
        FirstPatientTreatmentContext? context = treatmentContext.Context;
        if (perception is null
            || objects is null
            || experiments is null
            || context is null)
        {
            return false;
        }

        try
        {
            JournalSnapshot snapshot = Compose(
                perception,
                objects,
                dialogue.LastOccurredEvent,
                context,
                experiments);
            return journal.TryInitialize(snapshot);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static JournalSnapshot Compose(
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects,
        DialogueEvent? lastDialogueEvent,
        FirstPatientTreatmentContext treatmentContext,
        ExperimentJournalSnapshot experiments)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observationBinding =
            JournalObservationSectionProjection.Bind(
                catalogue,
                perception.CharacterId,
                perception);
        JournalObservationWorldBinding observationWorld = JournalObservationWorldProjection.Bind(
            observationBinding,
            worldObjects);
        JournalSnapshot snapshot = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(observationWorld));
        IEnumerable<DialogueEvent> testimony = lastDialogueEvent is null
            ? []
            : [lastDialogueEvent];
        JournalTestimonySectionSnapshot testimonySection =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(snapshot, testimony));
        snapshot = JournalTestimonySnapshotProjection.Compose(snapshot, testimonySection);

        IReadOnlyList<InvestigationConclusionEvidencePlan> plans =
            treatmentContext.Scenario.Investigation.Plans.Plans;
        InvestigationHypothesisSnapshot hypotheses = new(
            plans.Select(plan => plan.Conclusion.Hypothesis));
        JournalHypothesisSectionSnapshot hypothesisSection =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(snapshot, hypotheses));
        snapshot = JournalHypothesisSnapshotProjection.Compose(snapshot, hypothesisSection);

        JournalExperimentSectionSnapshot experimentSection =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(snapshot, experiments));
        snapshot = JournalExperimentSnapshotProjection.Compose(snapshot, experimentSection);

        OpenInvestigationQuestionSnapshot openQuestions = new(
            plans
                .Where(plan => !treatmentContext.Conclusions.Conclusions.Any(
                    conclusion => ReferenceEquals(conclusion, plan.Conclusion)))
                .Select(plan => plan.Conclusion.Question));
        JournalOpenQuestionSectionSnapshot questionSection =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(snapshot, openQuestions));
        return JournalOpenQuestionSnapshotProjection.Compose(snapshot, questionSection);
    }
}

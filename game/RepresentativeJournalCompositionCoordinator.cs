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
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator patientTestimony = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
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
                GetTestimonyEvents(),
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
        IReadOnlyList<DialogueEvent> dialogueEvents,
        FirstPatientTreatmentContext treatmentContext,
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

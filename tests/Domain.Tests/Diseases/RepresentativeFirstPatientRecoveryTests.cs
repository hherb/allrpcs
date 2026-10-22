using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Tests.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class RepresentativeFirstPatientRecoveryTests
{
    [Fact]
    public void TheRemedyDoseStabilizesWithoutAnEarlierConfirmedConclusion()
    {
        // The dose is the experiment that confirms the conclusion, so nothing is confirmed first.
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientScenario scenario = context.Scenario;
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "stabilized-by-first-dose",
            WorldEventTimeEffect.NoAdvance);
        IngredientAdministrationOutcome administration =
            StabilizingRemedyAdministrationPolicyTests.Administration(
                scenario,
                scenario.Remedy.Recipe,
                worldEvent);

        FirstPatientStabilizationResult result = FirstPatientStabilizationTransition.Apply(
            new ExperimentId("first-dose"),
            context,
            administration,
            new CharacterConditionSet([]),
            "Administer one prepared stabilizing remedy dose.",
            "The patient's breathing becomes steadier.",
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]),
            new PreparedIngredientBatchSnapshot(
                [new PreparedIngredientBatch(administration.Dosage.Preparation)]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));

        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(result.DiseaseTruth.Infections).CareState);
        Assert.Single(result.AdministrationExperiments.Records);
        Assert.Single(result.Experiments.Records);
        Assert.Empty(result.PreparedBatches.Batches);
    }

    [Fact]
    public void LosingAnySingleAuthoredSourceStillLeavesACompleteRoute()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        InvestigationConclusionEvidencePlan plan = Assert.Single(scenario.Investigation.Plans.Plans);

        foreach (InvestigationEvidenceSource lostSource in scenario.Investigation.Sources.Sources)
        {
            CharacterEvidenceViewSnapshot remainingEvidence = new(
                new CharacterId("seeker"),
                scenario.Investigation.Sources.Sources
                    .Where(source => !ReferenceEquals(source, lostSource))
                    .Select(source => source.EvidenceId));

            Assert.True(InvestigationConclusionConfirmationPolicy.CanConfirm(plan, remainingEvidence));
        }
    }

    [Fact]
    public void AReasonableButUnauthoredTheoryCannotAuthorizeTheRemedy()
    {
        FirstPatientTreatmentContext authored = RepresentativeFirstPatientTreatmentContextSource.Create();
        InvestigationConclusion expected = authored.EvidenceRequirement.Conclusion;
        InvestigationHypothesis wrongHypothesis = new(
            new InvestigationHypothesisId("warming-moonwort-stabilizes"),
            expected.Question,
            "Heating moonwort without cooling may preserve a stabilizing preparation.");
        InvestigationConclusion wrongConclusion = new(
            new InvestigationConclusionId("warming-moonwort-stabilizes"),
            expected.Question,
            wrongHypothesis,
            "Heating without cooling preserves the remedy.");
        FirstPatientTreatmentContext wrongTheory = new(
            authored.Scenario,
            new StabilizingRemedyEvidenceRequirement(authored.Scenario.Remedy, wrongConclusion));

        Assert.False(FirstPatientTreatmentContextPolicy.IsConsistent(wrongTheory));
    }

    [Fact]
    public void AFailedExperimentIsRecordedAndTheUnstabilizedPatientCanDeteriorate()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientScenario scenario = context.Scenario;
        IngredientPreparationRecipe ineffectiveRecipe = IngredientPreparationActivityTests.Recipe();
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "ineffective-dose-observed",
            WorldEventTimeEffect.Advance);
        IngredientAdministrationOutcome administration =
            StabilizingRemedyAdministrationPolicyTests.Administration(
                scenario,
                ineffectiveRecipe,
                worldEvent);
        CharacterConditionSet observedConditions = new(
            [new CharacterCondition("breathing", "Breathing remains shallow and irregular.")]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([worldEvent.EventId]);

        IngredientAdministrationExperimentExecutionResult failedExperiment =
            IngredientAdministrationExperimentExecutionTransition.Execute(
                new ExperimentId("ineffective-uncooked-dose"),
                administration,
                observedConditions,
                "Administer one unheated preparation.",
                "Breathing remains shallow and irregular.",
                worldEvents,
                eventLog,
                new PreparedIngredientBatchSnapshot(
                    [new PreparedIngredientBatch(administration.Dosage.Preparation)]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([]));
        Assert.Throws<InvalidOperationException>(() => FirstPatientStabilizationTransition.Apply(
            new ExperimentId("must-not-stabilize"),
            context,
            administration,
            observedConditions,
            "Administer one unheated preparation.",
            "Breathing remains shallow and irregular.",
            worldEvents,
            eventLog,
            new PreparedIngredientBatchSnapshot(
                [new PreparedIngredientBatch(administration.Dosage.Preparation)]),
            failedExperiment.AdministrationExperiments,
            failedExperiment.Experiments));

        DiseaseTruthSnapshot deteriorated = DiseaseProgressionTransition.Advance(
            Assert.Single(scenario.DiseaseTruth.Infections),
            scenario.DiseaseTruth,
            worldEvent,
            worldEvents,
            eventLog,
            new WorldTimeSnapshot(worldEvent.EventId));

        FirstPatientScenario deterioratedScenario = new(
            scenario.PatientId,
            deteriorated,
            scenario.Ingredients,
            scenario.Remedy,
            scenario.Investigation);
        FirstPatientTreatmentContext recoveryContext = new(
            deterioratedScenario,
            context.EvidenceRequirement);
        WorldEvent recoveryEvent = DiseaseProgressionPolicyTests.Event(
            "correct-dose-after-failure",
            WorldEventTimeEffect.NoAdvance);
        IngredientAdministrationOutcome recoveryAdministration =
            StabilizingRemedyAdministrationPolicyTests.Administration(
                deterioratedScenario,
                scenario.Remedy.Recipe,
                recoveryEvent);
        FirstPatientStabilizationResult recovered = FirstPatientStabilizationTransition.Apply(
            new ExperimentId("correct-dose-after-failure"),
            recoveryContext,
            recoveryAdministration,
            new CharacterConditionSet(
                [new CharacterCondition("breathing", "Breathing becomes steadier.")]),
            "Administer one correctly cooled preparation.",
            "Breathing becomes steadier.",
            new WorldEventSnapshot([recoveryEvent]),
            new WorldEventLogSnapshot([recoveryEvent.EventId]),
            new PreparedIngredientBatchSnapshot(
                [new PreparedIngredientBatch(recoveryAdministration.Dosage.Preparation)]),
            failedExperiment.AdministrationExperiments,
            failedExperiment.Experiments);

        Assert.Same(
            observedConditions,
            Assert.Single(failedExperiment.AdministrationExperiments.Records).ObservedConditions);
        Assert.Same(DiseaseStage.Wasting, Assert.Single(deteriorated.Infections).Stage);
        Assert.Same(DiseaseCareState.Unassisted, Assert.Single(deteriorated.Infections).CareState);
        Assert.Same(DiseaseStage.DeepSleep, Assert.Single(scenario.DiseaseTruth.Infections).Stage);
        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(recovered.DiseaseTruth.Infections).CareState);
        Assert.Equal(2, recovered.AdministrationExperiments.Records.Count);
        Assert.Equal(2, recovered.Experiments.Records.Count);
        Assert.Empty(recovered.PreparedBatches.Batches);
    }
}

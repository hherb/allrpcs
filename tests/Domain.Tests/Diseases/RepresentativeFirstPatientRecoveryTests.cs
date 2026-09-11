using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Tests.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class RepresentativeFirstPatientRecoveryTests
{
    [Fact]
    public void EitherDistinctEvidenceRouteCanAuthorizeEarlyStabilization()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        InvestigationConclusionEvidencePlan plan = Assert.Single(scenario.Investigation.Plans.Plans);

        Assert.Equal(2, plan.Routes.Count);
        for (int routeIndex = 0; routeIndex < plan.Routes.Count; routeIndex++)
        {
            InvestigationConclusionEvidenceRoute route = plan.Routes[routeIndex];
            CharacterEvidenceViewSnapshot visibleEvidence = new(
                new CharacterId("seeker"),
                route.RequiredEvidenceIds);
            InvestigationConclusionConfirmationResult confirmation =
                InvestigationConclusionConfirmationTransition.Confirm(
                    plan,
                    visibleEvidence,
                    new OpenInvestigationQuestionSnapshot([plan.Conclusion.Question]),
                    new InvestigationConclusionSnapshot([]));
            FirstPatientTreatmentContext context = new(
                scenario,
                new StabilizingRemedyEvidenceRequirement(scenario.Remedy, plan.Conclusion),
                confirmation.Conclusions);
            WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
                $"stabilized-by-route-{routeIndex}",
                WorldEventTimeEffect.NoAdvance);
            IngredientAdministrationOutcome administration =
                StabilizingRemedyAdministrationPolicyTests.Administration(
                    scenario,
                    scenario.Remedy.Recipe,
                    worldEvent);

            FirstPatientStabilizationResult result = FirstPatientStabilizationTransition.Apply(
                new ExperimentId($"route-{routeIndex}-dose"),
                context,
                administration,
                new CharacterConditionSet([]),
                new WorldEventSnapshot([worldEvent]),
                new WorldEventLogSnapshot([worldEvent.EventId]),
                new IngredientExperimentJournalSnapshot([]));

            Assert.Same(DiseaseCareState.Stabilized, Assert.Single(result.DiseaseTruth.Infections).CareState);
            Assert.Single(result.ExperimentJournal.Records);
        }
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
            new StabilizingRemedyEvidenceRequirement(authored.Scenario.Remedy, wrongConclusion),
            new InvestigationConclusionSnapshot([wrongConclusion]));

        Assert.False(FirstPatientTreatmentContextPolicy.IsAuthorized(wrongTheory));
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

        IngredientExperimentJournalSnapshot journal =
            IngredientAdministrationExperimentRecordingTransition.Record(
                new ExperimentId("ineffective-uncooked-dose"),
                administration,
                observedConditions,
                worldEvents,
                eventLog,
                new IngredientExperimentJournalSnapshot([]));
        Assert.Throws<InvalidOperationException>(() => FirstPatientStabilizationTransition.Apply(
            new ExperimentId("must-not-stabilize"),
            context,
            administration,
            observedConditions,
            worldEvents,
            eventLog,
            journal));

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
            context.EvidenceRequirement,
            context.Conclusions);
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
            new WorldEventSnapshot([recoveryEvent]),
            new WorldEventLogSnapshot([recoveryEvent.EventId]),
            journal);

        Assert.Same(observedConditions, Assert.Single(journal.Records).ObservedConditions);
        Assert.Same(DiseaseStage.Wasting, Assert.Single(deteriorated.Infections).Stage);
        Assert.Same(DiseaseCareState.Unassisted, Assert.Single(deteriorated.Infections).CareState);
        Assert.Same(DiseaseStage.DeepSleep, Assert.Single(scenario.DiseaseTruth.Infections).Stage);
        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(recovered.DiseaseTruth.Infections).CareState);
        Assert.Equal(2, recovered.ExperimentJournal.Records.Count);
    }
}

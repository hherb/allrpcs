using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class FirstPatientRecoveryRestorationProjectionTests
{
    [Theory]
    [InlineData("not-heard", false, false)]
    [InlineData("patient-testimony-heard", true, false)]
    [InlineData("sage-consulted", true, true)]
    public void RebuildsOnlyTheExactCompletedRecoveryTokens(
        string stageValue,
        bool expectsTestimony,
        bool expectsConsultation)
    {
        TestState state = CreateState();

        FirstPatientRecoveryRestoration result =
            FirstPatientRecoveryRestorationProjection.Restore(
                FirstPatientRecoveryStage.Parse(stageValue),
                state.Context,
                state.StabilizedTruth,
                state.AdministrationExperiments,
                state.WorldEvent,
                state.WorldEventLog,
                state.Sage,
                state.SeekerId);

        Assert.Equal(expectsTestimony, result.PatientTestimony is not null);
        Assert.Equal(expectsConsultation, result.SageConsultation is not null);
        if (result.PatientTestimony is not null)
        {
            Assert.Same(
                state.Context.Scenario.PatientId,
                result.PatientTestimony.Utterance.SpeakerCharacterId);
            Assert.Same(
                state.SeekerId,
                Assert.Single(result.PatientTestimony.Audience.ListenerCharacterIds));
        }

        if (result.SageConsultation is not null)
        {
            Assert.Same(state.Sage.Identity.Id, result.SageConsultation.Utterance.SpeakerCharacterId);
            Assert.Same(
                state.SeekerId,
                Assert.Single(result.SageConsultation.Audience.ListenerCharacterIds));
        }
    }

    [Fact]
    public void NotHeardNeedsNoStabilizationButCompletedStagesRejectMissingProvenance()
    {
        TestState state = CreateState();

        FirstPatientRecoveryRestoration inert =
            FirstPatientRecoveryRestorationProjection.Restore(
                FirstPatientRecoveryStage.NotHeard,
                state.Context,
                state.Context.Scenario.DiseaseTruth,
                new IngredientExperimentJournalSnapshot([]),
                null,
                new WorldEventLogSnapshot([]),
                state.Sage,
                state.SeekerId);

        Assert.Null(inert.PatientTestimony);
        Assert.Null(inert.SageConsultation);
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientRecoveryRestorationProjection.Restore(
                FirstPatientRecoveryStage.PatientTestimonyHeard,
                state.Context,
                state.Context.Scenario.DiseaseTruth,
                state.AdministrationExperiments,
                state.WorldEvent,
                state.WorldEventLog,
                state.Sage,
                state.SeekerId));
        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientRecoveryRestorationProjection.Restore(
                FirstPatientRecoveryStage.SageConsulted,
                state.Context,
                state.StabilizedTruth,
                state.AdministrationExperiments,
                null,
                state.WorldEventLog,
                state.Sage,
                state.SeekerId));
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientRecoveryRestorationProjection.Restore(
                FirstPatientRecoveryStage.PatientTestimonyHeard,
                state.Context,
                state.StabilizedTruth,
                state.AdministrationExperiments,
                state.WorldEvent,
                new WorldEventLogSnapshot([]),
                state.Sage,
                state.SeekerId));
    }

    [Fact]
    public void RestoresSageConsultationAfterAWrongDoseFollowedByTheRemedy()
    {
        TestState state = CreateState();
        FirstPatientTreatmentContext context = state.Context;
        IngredientPreparationRecipe recipe = context.Scenario.AlternativeRecipes[0];
        IngredientPreparationActivity activity =
            IngredientPreparationStartTransition.Start(recipe, context.Scenario.Ingredients).Activity;
        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            activity = IngredientPreparationActivityProgressionTransition.Advance(activity, step);
        }

        IngredientPreparationOutcome lookAlike = IngredientPreparationCompletionTransition.Complete(activity);
        WorldEvent firstDose = new(
            new WorldEventId("healer-administered-first-patient-look-alike"),
            WorldEventTimeEffect.Advance);
        IngredientExperimentJournalSnapshot both = new(
            [
                new IngredientAdministrationExperimentRecord(
                    new ExperimentId("first-patient-look-alike-administration"),
                    new IngredientAdministrationOutcome(
                        new CharacterId("healer"),
                        context.Scenario.PatientId,
                        new IngredientDosage(lookAlike, lookAlike.Quantity),
                        firstDose),
                    new CharacterConditionSet([])),
                .. state.AdministrationExperiments.Records,
            ]);

        FirstPatientRecoveryRestoration result = FirstPatientRecoveryRestorationProjection.Restore(
            FirstPatientRecoveryStage.SageConsulted,
            context,
            state.StabilizedTruth,
            both,
            both.Records[^1].Administration.WorldEvent,
            new WorldEventLogSnapshot([firstDose.EventId, state.WorldEvent.EventId]),
            state.Sage,
            state.SeekerId);

        Assert.NotNull(result.PatientTestimony);
        Assert.NotNull(result.SageConsultation);
    }

    private static TestState CreateState()
    {
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientPreparationStartResult start = IngredientPreparationStartTransition.Start(
            recipe,
            context.Scenario.Ingredients);
        IngredientPreparationActivity activity = start.Activity;
        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            activity = IngredientPreparationActivityProgressionTransition.Advance(activity, step);
        }

        IngredientPreparationOutcome preparation =
            IngredientPreparationCompletionTransition.Complete(activity);
        WorldEvent worldEvent = new(
            new WorldEventId("healer-administered-first-patient-remedy"),
            WorldEventTimeEffect.Advance);
        IngredientAdministrationOutcome administration = new(
            new CharacterId("healer"),
            context.Scenario.PatientId,
            new IngredientDosage(preparation, preparation.Quantity),
            worldEvent);
        IngredientExperimentJournalSnapshot administrationExperiments = new(
            [
                new IngredientAdministrationExperimentRecord(
                    new ExperimentId("first-patient-remedy-administration"),
                    administration,
                    new CharacterConditionSet([])),
            ]);
        CharacterDiseaseState initial = Assert.Single(context.Scenario.DiseaseTruth.Infections);
        DiseaseTruthSnapshot stabilizedTruth = new(
            context.Scenario.DiseaseTruth.Definitions,
            [
                new CharacterDiseaseState(
                    initial.CharacterId,
                    initial.Disease,
                    initial.Stage,
                    initial.LastProgressionEventId,
                    DiseaseCareState.Stabilized),
            ]);
        return new TestState(
            context,
            stabilizedTruth,
            administrationExperiments,
            worldEvent,
            new WorldEventLogSnapshot([worldEvent.EventId]),
            RepresentativeSageCharacterSource.Create(),
            new CharacterId("seeker"));
    }

    private sealed record TestState(
        FirstPatientTreatmentContext Context,
        DiseaseTruthSnapshot StabilizedTruth,
        IngredientExperimentJournalSnapshot AdministrationExperiments,
        WorldEvent WorldEvent,
        WorldEventLogSnapshot WorldEventLog,
        Character Sage,
        CharacterId SeekerId);
}

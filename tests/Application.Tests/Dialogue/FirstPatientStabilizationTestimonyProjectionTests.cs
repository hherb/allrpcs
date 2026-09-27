using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class FirstPatientStabilizationTestimonyProjectionTests
{
    [Fact]
    public void ProjectsFixedOrdinaryPatientTestimonyFromExactStabilization()
    {
        TestState state = CreateState();

        DialogueDelivery delivery = FirstPatientStabilizationTestimonyProjection.Project(
            state.Context,
            state.StabilizedTruth,
            state.AdministrationExperiments,
            state.WorldEvent);

        Assert.Same(DialogueDeliveryMode.OrdinarySpeech, delivery.Mode);
        Assert.Same(state.Context.Scenario.PatientId, delivery.Utterance.SpeakerCharacterId);
        Assert.Equal(
            FirstPatientStabilizationTestimonyProjection.TestimonyText,
            delivery.Utterance.Text);
    }

    [Fact]
    public void RejectsMissingStaleOrRebuiltStabilizationProvenance()
    {
        TestState state = CreateState();
        WorldEvent rebuiltEvent = new(state.WorldEvent.EventId, state.WorldEvent.TimeEffect);

        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientStabilizationTestimonyProjection.Project(
                state.Context,
                state.Context.Scenario.DiseaseTruth,
                state.AdministrationExperiments,
                state.WorldEvent));
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientStabilizationTestimonyProjection.Project(
                state.Context,
                state.StabilizedTruth,
                new IngredientExperimentJournalSnapshot([]),
                state.WorldEvent));
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientStabilizationTestimonyProjection.Project(
                state.Context,
                state.StabilizedTruth,
                state.AdministrationExperiments,
                rebuiltEvent));
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientStabilizationTestimonyProjection.Project(
                RepresentativeFirstPatientTreatmentContextSource.Create(),
                state.StabilizedTruth,
                state.AdministrationExperiments,
                state.WorldEvent));
    }

    [Fact]
    public void ExposesOnlyOnePureProjectionMethodAndTheFixedText()
    {
        Type type = typeof(FirstPatientStabilizationTestimonyProjection);
        System.Reflection.MethodInfo method = Assert.Single(type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.FieldInfo field = Assert.Single(type.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(FirstPatientStabilizationTestimonyProjection.Project), method.Name);
        Assert.Equal(typeof(DialogueDelivery), method.ReturnType);
        Assert.True(field.IsLiteral);
        Assert.Equal("TestimonyText", field.Name);
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
            worldEvent);
    }

    private sealed record TestState(
        FirstPatientTreatmentContext Context,
        DiseaseTruthSnapshot StabilizedTruth,
        IngredientExperimentJournalSnapshot AdministrationExperiments,
        WorldEvent WorldEvent);
}

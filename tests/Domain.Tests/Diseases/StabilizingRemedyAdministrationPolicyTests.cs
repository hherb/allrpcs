using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class StabilizingRemedyAdministrationPolicyTests
{
    [Fact]
    public void AcceptsTheExactCurrentRemedyAdministration()
    {
        AdministrationInput input = Input();

        Assert.True(StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context,
            input.Administration,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsReplacementPatientAndRecipeIdentities()
    {
        AdministrationInput input = Input();
        IngredientAdministrationOutcome replacementPatient = new(
            new CharacterId("healer"),
            new CharacterId(input.Scenario.PatientId.Value),
            input.Administration.Dosage,
            input.Administration.WorldEvent);
        IngredientPreparationRecipe recipe = input.Scenario.Remedy.Recipe;
        IngredientPreparationRecipe replacementRecipe = new(
            new PreparationRecipeId(recipe.RecipeId.Value),
            recipe.Plan,
            recipe.Inputs,
            recipe.ResultDefinition,
            recipe.ResultQuantity);
        IngredientAdministrationOutcome replacementRecipeAdministration = Administration(
            input.Scenario,
            replacementRecipe,
            input.Administration.WorldEvent);

        Assert.False(StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context,
            replacementPatient,
            input.WorldEvents,
            input.EventLog));
        Assert.False(StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context,
            replacementRecipeAdministration,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsStructurallyInvalidScenarioAndNoncurrentEvent()
    {
        AdministrationInput input = Input();
        FirstPatientScenario invalidScenario = new(
            input.Scenario.PatientId,
            new DiseaseTruthSnapshot(input.Scenario.DiseaseTruth.Definitions, []),
            input.Scenario.Ingredients,
            input.Scenario.Remedy,
            input.Scenario.Investigation);
        FirstPatientTreatmentContext invalidContext = new(
            invalidScenario,
            input.Context.EvidenceRequirement,
            input.Context.Conclusions);

        Assert.False(StabilizingRemedyAdministrationPolicy.CanStabilize(
            invalidContext,
            input.Administration,
            input.WorldEvents,
            input.EventLog));
        Assert.False(StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context,
            input.Administration,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePurePolicySurface()
    {
        AdministrationInput input = Input();

        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationPolicy.CanStabilize(
            null!, input.Administration, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context, input.Administration, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => StabilizingRemedyAdministrationPolicy.CanStabilize(
            input.Context, input.Administration, input.WorldEvents, null!));

        Type policyType = typeof(StabilizingRemedyAdministrationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanStabilize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(FirstPatientTreatmentContext),
                typeof(IngredientAdministrationOutcome),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }

    internal static AdministrationInput Input()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientScenario scenario = context.Scenario;
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "first-patient-stabilized",
            WorldEventTimeEffect.NoAdvance);
        return new AdministrationInput(
            context,
            Administration(scenario, scenario.Remedy.Recipe, worldEvent),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]));
    }

    internal static IngredientAdministrationOutcome Administration(
        FirstPatientScenario scenario,
        IngredientPreparationRecipe recipe,
        WorldEvent worldEvent)
    {
        IngredientPreparationActivity activity = IngredientPreparationOutcomeTests.Complete(recipe);
        IngredientPreparationOutcome preparation = new(
            activity,
            recipe.ResultDefinition,
            recipe.ResultQuantity);
        IngredientDosage dosage = new(preparation, recipe.ResultQuantity);
        return new IngredientAdministrationOutcome(
            new CharacterId("healer"),
            scenario.PatientId,
            dosage,
            worldEvent);
    }

    internal sealed record AdministrationInput(
        FirstPatientTreatmentContext Context,
        IngredientAdministrationOutcome Administration,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog)
    {
        internal FirstPatientScenario Scenario => Context.Scenario;
    }
}

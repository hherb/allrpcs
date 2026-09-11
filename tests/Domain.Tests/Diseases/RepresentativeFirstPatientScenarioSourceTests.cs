using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class RepresentativeFirstPatientScenarioSourceTests
{
    [Fact]
    public void CreatesThePreStabilizationGreySleepScenario()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();

        CharacterDiseaseState infection = Assert.Single(scenario.DiseaseTruth.Infections);
        Assert.Equal("first-sleeping-patient", scenario.PatientId.Value);
        Assert.Same(scenario.PatientId, infection.CharacterId);
        Assert.Equal("grey-sleep", scenario.Remedy.Disease.DiseaseId.Value);
        Assert.Same(scenario.Remedy.Disease, infection.Disease);
        Assert.Same(DiseaseStage.DeepSleep, infection.Stage);
        Assert.Same(DiseaseCareState.Unassisted, infection.CareState);
        Assert.Equal("cooling-moonwort-remedy", scenario.Remedy.Recipe.RecipeId.Value);
        Assert.Equal(
            ["moonwort-leaf", "clean-water"],
            scenario.Remedy.Recipe.Inputs.Select(input => input.Definition.IngredientId.Value));
        Assert.True(FirstPatientScenarioStructuralPolicy.IsAcceptable(scenario));
    }

    [Fact]
    public void RetainsRecipeInputDefinitionsAndUnitsInAvailableIngredientState()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();

        foreach (IngredientPreparationInputRequirement requirement in scenario.Remedy.Recipe.Inputs)
        {
            IngredientState state = Assert.Single(scenario.Ingredients.States.Where(
                candidate => ReferenceEquals(candidate.Definition, requirement.Definition)));

            Assert.Same(requirement.Quantity.Unit, state.Quantity.Unit);
            Assert.True(state.Quantity.Amount >= requirement.Quantity.Amount);
        }
    }

    [Fact]
    public void CreatesFreshIndependentAggregatesAndExposesOnlyOneFactoryMethod()
    {
        FirstPatientScenario first = RepresentativeFirstPatientScenarioSource.Create();
        FirstPatientScenario second = RepresentativeFirstPatientScenarioSource.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.PatientId, second.PatientId);
        Assert.NotSame(first.DiseaseTruth, second.DiseaseTruth);
        Assert.NotSame(first.Ingredients, second.Ingredients);
        Assert.NotSame(first.Remedy, second.Remedy);
        Assert.NotSame(first.Investigation, second.Investigation);

        Type sourceType = typeof(RepresentativeFirstPatientScenarioSource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(FirstPatientScenario), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
    }

    internal static ScenarioInput Input()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        return new ScenarioInput(
            scenario.PatientId,
            scenario.DiseaseTruth,
            scenario.Ingredients,
            scenario.Remedy,
            scenario.Investigation);
    }

    internal sealed record ScenarioInput(
        CharacterId PatientId,
        DiseaseTruthSnapshot DiseaseTruth,
        IngredientStateSnapshot Ingredients,
        StabilizingRemedyDefinition Remedy,
        CriticalInvestigationConclusionScenario Investigation)
    {
        internal FirstPatientScenario With(
            CharacterId? patientId = null,
            DiseaseTruthSnapshot? diseaseTruth = null,
            IngredientStateSnapshot? ingredients = null,
            StabilizingRemedyDefinition? remedy = null,
            CriticalInvestigationConclusionScenario? investigation = null) =>
            new(
                patientId ?? PatientId,
                diseaseTruth ?? DiseaseTruth,
                ingredients ?? Ingredients,
                remedy ?? Remedy,
                investigation ?? Investigation);
    }
}

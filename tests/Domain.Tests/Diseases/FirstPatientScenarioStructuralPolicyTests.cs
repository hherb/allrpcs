using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientScenarioStructuralPolicyTests
{
    [Fact]
    public void AcceptsTheRepresentativeFirstPatientScenarioWithoutChangingIt()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        CharacterDiseaseState infection = Assert.Single(scenario.DiseaseTruth.Infections);
        IngredientState[] ingredients = scenario.Ingredients.States.ToArray();

        bool acceptable = FirstPatientScenarioStructuralPolicy.IsAcceptable(scenario);

        Assert.True(acceptable);
        Assert.Same(scenario.PatientId, infection.CharacterId);
        Assert.Same(scenario.Remedy.Disease, infection.Disease);
        Assert.Equal(ingredients, scenario.Ingredients.States);
    }

    [Fact]
    public void RejectsValueEqualReplacementPatientAndDiseaseDefinitions()
    {
        RepresentativeFirstPatientScenarioSourceTests.ScenarioInput input =
            RepresentativeFirstPatientScenarioSourceTests.Input();
        FirstPatientScenario replacementPatient = input.With(
            patientId: new CharacterId(input.PatientId.Value));
        StabilizingRemedyDefinition replacementRemedy = new(
            new DiseaseDefinition(new DiseaseId(input.Remedy.Disease.DiseaseId.Value)),
            input.Remedy.Recipe);

        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(replacementPatient));
        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(input.With(remedy: replacementRemedy)));
    }

    [Fact]
    public void RejectsValueEqualReplacementIngredientsAndWrongUnits()
    {
        RepresentativeFirstPatientScenarioSourceTests.ScenarioInput input =
            RepresentativeFirstPatientScenarioSourceTests.Input();
        IngredientState[] replacements = input.Ingredients.States
            .Select(state => new IngredientState(
                new IngredientDefinition(new IngredientId(state.Definition.IngredientId.Value)),
                state.Quantity,
                state.Freshness,
                state.PhysicalCondition,
                state.Contamination))
            .ToArray();
        IngredientState first = input.Ingredients.States[0];
        IngredientState wrongUnit = new(
            first.Definition,
            new IngredientQuantity(first.Quantity.Amount, IngredientQuantityUnit.Millilitre),
            first.Freshness,
            first.PhysicalCondition,
            first.Contamination);
        IngredientStateSnapshot wrongUnits = new(
            [wrongUnit, .. input.Ingredients.States.Skip(1)]);

        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(
            input.With(ingredients: new IngredientStateSnapshot(replacements))));
        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(input.With(ingredients: wrongUnits)));
    }

    [Fact]
    public void RejectsMissingOrMismatchedPatientStateAndUnsafeInvestigation()
    {
        RepresentativeFirstPatientScenarioSourceTests.ScenarioInput input =
            RepresentativeFirstPatientScenarioSourceTests.Input();
        DiseaseTruthSnapshot missingPatient = new(input.DiseaseTruth.Definitions, []);
        CharacterDiseaseState mismatchedInfection = new(
            new CharacterId("different-patient"),
            input.Remedy.Disease,
            DiseaseStage.DeepSleep,
            null,
            DiseaseCareState.Unassisted);
        DiseaseTruthSnapshot mismatchedPatient = new(
            input.DiseaseTruth.Definitions,
            [mismatchedInfection]);
        CriticalInvestigationConclusionScenario unsafeInvestigation = new(
            new CriticalInvestigationConclusionPlanSnapshot([]),
            input.Investigation.Sources);

        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(
            input.With(diseaseTruth: missingPatient)));
        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(
            input.With(diseaseTruth: mismatchedPatient)));
        Assert.False(FirstPatientScenarioStructuralPolicy.IsAcceptable(
            input.With(investigation: unsafeInvestigation)));
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePurePolicySurface()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientScenarioStructuralPolicy.IsAcceptable(null!));

        Type policyType = typeof(FirstPatientScenarioStructuralPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsAcceptable", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(FirstPatientScenario)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }
}

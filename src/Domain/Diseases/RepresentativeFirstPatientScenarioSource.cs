using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Authors the pure representative first-patient stabilization prerequisites.</summary>
public static class RepresentativeFirstPatientScenarioSource
{
    /// <summary>Creates fresh exact grey-sleep, remedy, ingredient, and investigation state.</summary>
    public static FirstPatientScenario Create()
    {
        DiseaseDefinition greySleep = new(new DiseaseId("grey-sleep"));
        CharacterId patientId = new("first-sleeping-patient");
        DiseaseTruthSnapshot diseaseTruth = new(
            new DiseaseDefinitionSnapshot([greySleep]),
            [
                new CharacterDiseaseState(
                    patientId,
                    greySleep,
                    DiseaseStage.DeepSleep,
                    null,
                    DiseaseCareState.Unassisted),
            ]);
        IngredientDefinition moonwortLeaf = new(new IngredientId("moonwort-leaf"));
        IngredientDefinition cleanWater = new(new IngredientId("clean-water"));
        IngredientPreparationRecipe recipe = new(
            new PreparationRecipeId("cooling-moonwort-remedy"),
            new IngredientPreparationPlan(
                [
                    Step(IngredientPreparationKind.Grinding),
                    Step(IngredientPreparationKind.Heating),
                    Step(IngredientPreparationKind.Steeping),
                    Step(IngredientPreparationKind.Cooling),
                ]),
            [
                Requirement(moonwortLeaf, 250, IngredientQuantityUnit.Milligram),
                Requirement(cleanWater, 100, IngredientQuantityUnit.Millilitre),
            ],
            new IngredientDefinition(new IngredientId("cooled-moonwort-preparation")),
            new IngredientQuantity(100, IngredientQuantityUnit.Millilitre));
        IngredientStateSnapshot ingredients = new(
            [
                State(moonwortLeaf, 500, IngredientQuantityUnit.Milligram),
                State(cleanWater, 250, IngredientQuantityUnit.Millilitre),
            ]);

        return new FirstPatientScenario(
            patientId,
            diseaseTruth,
            ingredients,
            new StabilizingRemedyDefinition(greySleep, recipe),
            RepresentativeFirstPatientInvestigationSource.Create());
    }

    private static IngredientPreparationStep Step(IngredientPreparationKind kind) => new(kind);

    private static IngredientPreparationInputRequirement Requirement(
        IngredientDefinition definition,
        int amount,
        IngredientQuantityUnit unit) =>
        new(definition, new IngredientQuantity(amount, unit));

    private static IngredientState State(
        IngredientDefinition definition,
        int amount,
        IngredientQuantityUnit unit) =>
        new(
            definition,
            new IngredientQuantity(amount, unit),
            IngredientFreshness.Fresh,
            IngredientPhysicalCondition.Intact,
            IngredientContamination.Uncontaminated);
}

using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class RemedyPreparationLifecycleTests
{
    [Fact]
    public void TheRemedyNeedsTheGatheredHerbWhileTheLookAlikeIsMadeFromStationStock()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        IngredientState herb = scenario.GatheredIngredients.States[0];
        RemedyPreparationLifecycle lifecycle = new(scenario);

        Assert.Same(scenario.StationIngredients, lifecycle.Ingredients);
        Assert.False(lifecycle.TryStart(scenario.Remedy.Recipe));
        Assert.False(lifecycle.TryStart(
            scenario.Remedy.Recipe,
            RepresentativeFirstPatientScenarioSource.Create().GatheredIngredients.States[0]));
        Assert.True(lifecycle.TryStart(scenario.Remedy.Recipe, herb));
        Assert.Same(scenario.Remedy.Recipe, lifecycle.Activity!.Recipe);
        Assert.False(lifecycle.TryStart(scenario.AlternativeRecipes[0]));

        RemedyPreparationLifecycle lookAlike = new(scenario);
        Assert.True(lookAlike.TryStart(scenario.AlternativeRecipes[0]));
        Assert.False(new RemedyPreparationLifecycle(scenario).TryStart(
            RepresentativeFirstPatientScenarioSource.Create().AlternativeRecipes[0]));
    }

    [Fact]
    public void RestartsOnlyAfterAWrongRemedyWasGiven()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        RemedyPreparationLifecycle lookAlike = Completed(scenario, scenario.AlternativeRecipes[0], null);
        RemedyPreparationLifecycle remedy = Completed(
            scenario, scenario.Remedy.Recipe, scenario.GatheredIngredients.States[0]);

        Assert.False(lookAlike.TryRestartAfterFailedDose(new IngredientExperimentJournalSnapshot([])));
        Assert.False(remedy.TryRestartAfterFailedDose(Doses(scenario, remedy.Outcome!)));
        Assert.True(lookAlike.TryRestartAfterFailedDose(Doses(scenario, lookAlike.Outcome!)));
        Assert.Null(lookAlike.Activity);
        Assert.Null(lookAlike.Outcome);
        Assert.Same(scenario.StationIngredients, lookAlike.Ingredients);
        Assert.True(lookAlike.TryStart(scenario.Remedy.Recipe, scenario.GatheredIngredients.States[0]));
    }

    [Fact]
    public void RestoresTheNewestRetainedPreparationOrIdleAndRejectsForeignRecipes()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        IngredientPreparationOutcome wrong =
            Completed(scenario, scenario.AlternativeRecipes[0], null).Outcome!;
        IngredientPreparationOutcome right = Completed(
            scenario, scenario.Remedy.Recipe, scenario.GatheredIngredients.States[0]).Outcome!;
        RemedyPreparationLifecycle lifecycle = new(scenario);

        lifecycle.PrepareRestoration(State(scenario, [right], Doses(scenario, wrong)))();
        Assert.Same(right, lifecycle.Outcome);
        Assert.Same(right.Activity, lifecycle.Activity);

        lifecycle.PrepareRestoration(State(scenario, [], Doses(scenario, wrong)))();
        Assert.Same(wrong, lifecycle.Outcome);

        lifecycle.PrepareRestoration(State(scenario, [], new IngredientExperimentJournalSnapshot([])))();
        Assert.Null(lifecycle.Outcome);
        Assert.Same(scenario.StationIngredients, lifecycle.Ingredients);

        FirstPatientScenario other = RepresentativeFirstPatientScenarioSource.Create();
        IngredientPreparationOutcome foreign = Completed(other, other.AlternativeRecipes[0], null).Outcome!;
        Assert.Throws<InvalidOperationException>(() => lifecycle.PrepareRestoration(
            State(scenario, [foreign], new IngredientExperimentJournalSnapshot([]))));
    }

    [Theory]
    [InlineData(true, true, RemedyStationAction.PrepareRemedy)]
    [InlineData(true, false, RemedyStationAction.PrepareRemedy)]
    [InlineData(false, true, RemedyStationAction.ReceiveFromHealer)]
    [InlineData(false, false, RemedyStationAction.PrepareLookAlike)]
    public void TheStationPrefersTheSeekersHerbThenTheHealersThenTheLookAlike(
        bool seekerCarries,
        bool healerCarries,
        RemedyStationAction expected) =>
        Assert.Equal(expected, RemedyStationPolicy.Choose(seekerCarries, healerCarries));

    private static RemedyPreparationLifecycle Completed(
        FirstPatientScenario scenario,
        IngredientPreparationRecipe recipe,
        IngredientState? herb)
    {
        RemedyPreparationLifecycle lifecycle = new(scenario);
        Assert.True(lifecycle.TryStart(recipe, herb));
        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            Assert.True(lifecycle.TryAdvance(step));
        }

        Assert.True(lifecycle.TryComplete());
        return lifecycle;
    }

    private static IngredientExperimentJournalSnapshot Doses(
        FirstPatientScenario scenario,
        IngredientPreparationOutcome preparation) =>
        new(
            [
                new IngredientAdministrationExperimentRecord(
                    new ExperimentId("dose"),
                    new IngredientAdministrationOutcome(
                        new CharacterId("healer"),
                        scenario.PatientId,
                        new IngredientDosage(preparation, preparation.Quantity),
                        new WorldEvent(new WorldEventId("dose"), WorldEventTimeEffect.Advance)),
                    new CharacterConditionSet([new CharacterCondition("breathing", "Unchanged.")])),
            ]);

    private static FirstPatientSaveState State(
        FirstPatientScenario scenario,
        IReadOnlyList<IngredientPreparationOutcome> batches,
        IngredientExperimentJournalSnapshot doses) =>
        new(
            scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot(batches.Select(batch => new PreparedIngredientBatch(batch))),
            doses,
            new ExperimentJournalSnapshot([]));
}

using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class FirstPatientHypothesisTestPolicyTests
{
    [Fact]
    public void AuthorsTheRemedyAndLookAlikeHypothesesForTheOneOpenQuestion()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();

        IReadOnlyList<FirstPatientHypothesisTest> tests = RepresentativeFirstPatientHypothesisSource.Create(scenario);

        Assert.Equal(2, tests.Count);
        Assert.Same(scenario.Investigation.Plans.Plans[0].Conclusion.Hypothesis, tests[0].Hypothesis);
        Assert.Same(scenario.Remedy.Recipe, tests[0].Recipe);
        Assert.Equal("station-leaves-stabilize", tests[1].Hypothesis.HypothesisId.Value);
        Assert.Same(tests[0].Hypothesis.Question, tests[1].Hypothesis.Question);
        Assert.Same(scenario.AlternativeRecipes[0], tests[1].Recipe);
    }

    [Fact]
    public void IsUntestedUntilItsRecipeIsDosedThenConfirmedOnlyForTheRemedy()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        IReadOnlyList<FirstPatientHypothesisTest> tests = RepresentativeFirstPatientHypothesisSource.Create(scenario);
        IngredientExperimentJournalSnapshot none = new([]);
        IngredientExperimentJournalSnapshot lookAlike = Doses(scenario, scenario.AlternativeRecipes[0]);
        IngredientExperimentJournalSnapshot both = Doses(scenario, scenario.AlternativeRecipes[0], scenario.Remedy.Recipe);

        Assert.Equal(InvestigationHypothesisTestOutcome.Untested, FirstPatientHypothesisTestPolicy.Evaluate(tests[0], scenario, none));
        Assert.Equal(InvestigationHypothesisTestOutcome.Untested, FirstPatientHypothesisTestPolicy.Evaluate(tests[0], scenario, lookAlike));
        Assert.Equal(InvestigationHypothesisTestOutcome.Refuted, FirstPatientHypothesisTestPolicy.Evaluate(tests[1], scenario, lookAlike));
        Assert.Equal(InvestigationHypothesisTestOutcome.Confirmed, FirstPatientHypothesisTestPolicy.Evaluate(tests[0], scenario, both));
        Assert.Equal(InvestigationHypothesisTestOutcome.Refuted, FirstPatientHypothesisTestPolicy.Evaluate(tests[1], scenario, both));
    }

    internal static IngredientExperimentJournalSnapshot Doses(
        FirstPatientScenario scenario,
        params IngredientPreparationRecipe[] recipes) => new(recipes.Select((recipe, index) =>
        {
            IngredientPreparationActivity activity =
                IngredientPreparationStartTransition.Start(recipe, scenario.Ingredients).Activity;
            foreach (IngredientPreparationStep step in recipe.Plan.Steps)
            {
                activity = IngredientPreparationActivityProgressionTransition.Advance(activity, step);
            }

            IngredientPreparationOutcome prepared = IngredientPreparationCompletionTransition.Complete(activity);
            return new IngredientAdministrationExperimentRecord(
                new ExperimentId($"dose-{index}"),
                new IngredientAdministrationOutcome(
                    new CharacterId("healer"),
                    scenario.PatientId,
                    new IngredientDosage(prepared, prepared.Quantity),
                    new WorldEvent(new WorldEventId($"dose-event-{index}"), WorldEventTimeEffect.Advance)),
                new CharacterConditionSet([]));
        }));
}

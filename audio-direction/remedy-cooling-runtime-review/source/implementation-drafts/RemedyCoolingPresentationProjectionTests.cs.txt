using System.Numerics;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Tests.Audio;

public sealed class RemedyCoolingPresentationProjectionTests
{
    private static IngredientPreparationRecipe Recipe() => RepresentativeFirstPatientScenarioSource.Create().Remedy.Recipe;
    [Fact]
    public void OnlyExactImmutableFourthStepTransitionIsRecognized()
    {
        var recipe = Recipe(); var before = new IngredientPreparationActivity(recipe, new(recipe.Plan, 3));
        var step = recipe.Plan.Steps[3]; var after = IngredientPreparationActivityProgressionTransition.Advance(before, step);
        Assert.True(RemedyCoolingPresentationProjection.IsCommit(recipe, step, before, after));
        Assert.Equal(3, before.Progress.NextStepIndex); Assert.Equal(4, after.Progress.NextStepIndex);
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, new(step.Kind), before, after));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(Recipe(), step, before, after));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, step, after, after));
    }
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    [InlineData(2, 4)]
    [InlineData(2, 3)]
    [InlineData(4, 4)]
    public void OtherProgressCannotBecomeCooling(int start, int end)
    {
        var recipe = Recipe();
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, recipe.Plan.Steps[3],
            new(recipe, new(recipe.Plan, start)), new(recipe, new(recipe.Plan, end))));
    }
    [Fact]
    public void NullAndForeignActivityCannotProducePresentation()
    {
        var recipe = Recipe(); var step = recipe.Plan.Steps[3]; var before = new IngredientPreparationActivity(recipe, new(recipe.Plan, 3));
        var after = new IngredientPreparationActivity(recipe, new(recipe.Plan, 4));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(null, step, before, after));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, null, before, after));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, step, null, after));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, step, before, null));
        var other = Recipe();
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, step, before, new(other, new(other.Plan, 4))));
        Assert.False(RemedyCoolingPresentationProjection.IsCommit(recipe, recipe.Plan.Steps[0], before, after));
    }
    [Theory]
    [InlineData(0, true)]
    [InlineData(47, true)]
    [InlineData(48, true)]
    [InlineData(48.01, false)]
    [InlineData(49, false)]
    [InlineData(float.NaN, false)]
    [InlineData(float.PositiveInfinity, false)]
    public void InclusiveActualRangeIsRequired(float distance, bool expected) =>
        Assert.Equal(expected, RemedyCoolingPresentationProjection.Local(new(2300 + distance, 1300), new(2300 + distance, 1300), new(2300, 1300)));
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(1.001, false)]
    [InlineData(float.NaN, false)]
    public void SuppliedPositionCannotForgeTheActualBody(float difference, bool expected) =>
        Assert.Equal(expected, RemedyCoolingPresentationProjection.Local(new(2300, 1300), new(2300 + difference, 1300), new(2300, 1300)));
    [Theory]
    [InlineData(0, false)]
    [InlineData(-0.1, false)]
    [InlineData(0.1, true)]
    [InlineData(0.101, false)]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    [InlineData(0.0166666666667, true)]
    public void InvalidClockCannotRetainPresentation(double delta, bool expected) =>
        Assert.Equal(expected, RemedyCoolingPresentationProjection.ValidDelta(delta));
}

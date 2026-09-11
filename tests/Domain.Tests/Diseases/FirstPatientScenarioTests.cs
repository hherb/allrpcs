using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientScenarioTests
{
    [Fact]
    public void RetainsEveryExactScenarioInput()
    {
        RepresentativeFirstPatientScenarioSourceTests.ScenarioInput input =
            RepresentativeFirstPatientScenarioSourceTests.Input();

        FirstPatientScenario scenario = new(
            input.PatientId,
            input.DiseaseTruth,
            input.Ingredients,
            input.Remedy,
            input.Investigation);

        Assert.Same(input.PatientId, scenario.PatientId);
        Assert.Same(input.DiseaseTruth, scenario.DiseaseTruth);
        Assert.Same(input.Ingredients, scenario.Ingredients);
        Assert.Same(input.Remedy, scenario.Remedy);
        Assert.Same(input.Investigation, scenario.Investigation);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableScenarioSurface()
    {
        RepresentativeFirstPatientScenarioSourceTests.ScenarioInput input =
            RepresentativeFirstPatientScenarioSourceTests.Input();

        Assert.Throws<ArgumentNullException>(() => new FirstPatientScenario(
            null!, input.DiseaseTruth, input.Ingredients, input.Remedy, input.Investigation));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientScenario(
            input.PatientId, null!, input.Ingredients, input.Remedy, input.Investigation));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientScenario(
            input.PatientId, input.DiseaseTruth, null!, input.Remedy, input.Investigation));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientScenario(
            input.PatientId, input.DiseaseTruth, input.Ingredients, null!, input.Investigation));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientScenario(
            input.PatientId, input.DiseaseTruth, input.Ingredients, input.Remedy, null!));

        Type scenarioType = typeof(FirstPatientScenario);
        System.Reflection.PropertyInfo[] properties = scenarioType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(scenarioType.GetConstructors());

        Assert.Equal(
            ["DiseaseTruth", "Ingredients", "Investigation", "PatientId", "Remedy"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(CharacterId),
                typeof(DiseaseTruthSnapshot),
                typeof(IngredientStateSnapshot),
                typeof(StabilizingRemedyDefinition),
                typeof(CriticalInvestigationConclusionScenario),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(scenarioType.GetEvents());
        Assert.Empty(scenarioType.GetMethods().Where(
            method => method.DeclaringType == scenarioType && !method.IsSpecialName));
        Assert.True(scenarioType.IsSealed);
    }
}

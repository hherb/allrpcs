using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientTreatmentContextTests
{
    [Fact]
    public void RetainsEveryExactTreatmentInput()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        FirstPatientTreatmentContext retained = new(
            context.Scenario,
            context.EvidenceRequirement);

        Assert.Same(context.Scenario, retained.Scenario);
        Assert.Same(context.EvidenceRequirement, retained.EvidenceRequirement);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableContextSurface()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<ArgumentNullException>(() => new FirstPatientTreatmentContext(
            null!, context.EvidenceRequirement));
        Assert.Throws<ArgumentNullException>(() => new FirstPatientTreatmentContext(
            context.Scenario, null!));

        Type contextType = typeof(FirstPatientTreatmentContext);
        System.Reflection.PropertyInfo[] properties = contextType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(contextType.GetConstructors());

        Assert.Equal(
            ["EvidenceRequirement", "Scenario"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [
                typeof(FirstPatientScenario),
                typeof(StabilizingRemedyEvidenceRequirement),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(contextType.GetEvents());
        Assert.Equal(
            ["WithDiseaseTruth"],
            contextType.GetMethods()
                .Where(method => method.DeclaringType == contextType && !method.IsSpecialName)
                .Select(method => method.Name));
        Assert.True(contextType.IsSealed);
    }

    [Fact]
    public void WithTheSameDiseaseTruthReturnsTheSameContext()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Same(context, context.WithDiseaseTruth(context.Scenario.DiseaseTruth));
        Assert.Throws<ArgumentNullException>(() => context.WithDiseaseTruth(null!));
    }

    [Fact]
    public void WithLiveDiseaseTruthKeepsEveryOtherAuthoredReference()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        FirstPatientScenario authored = context.Scenario;
        CharacterDiseaseState infection = Assert.Single(authored.DiseaseTruth.Infections);
        DiseaseTruthSnapshot progressed = new(
            authored.DiseaseTruth.Definitions,
            [new CharacterDiseaseState(
                infection.CharacterId,
                infection.Disease,
                DiseaseStage.Wasting,
                null,
                infection.CareState)]);

        FirstPatientTreatmentContext live = context.WithDiseaseTruth(progressed);

        Assert.NotSame(context, live);
        Assert.Same(progressed, live.Scenario.DiseaseTruth);
        Assert.Same(authored.PatientId, live.Scenario.PatientId);
        Assert.Same(authored.Ingredients, live.Scenario.Ingredients);
        Assert.Same(authored.Remedy, live.Scenario.Remedy);
        Assert.Same(authored.Investigation, live.Scenario.Investigation);
        Assert.Same(authored.GatheredIngredients, live.Scenario.GatheredIngredients);
        Assert.Equal(authored.AlternativeRecipes, live.Scenario.AlternativeRecipes);
        Assert.Same(context.EvidenceRequirement, live.EvidenceRequirement);
        Assert.True(FirstPatientTreatmentContextPolicy.IsConsistent(live));
    }
}

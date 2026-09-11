using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class SuccessfulDiseaseCureOutcomeTests
{
    private static readonly string[] ExpectedPropertyNames = ["Infection", "WorldEvent"];

    [Fact]
    public void RetainsExactInfectionAndAuthoredWorldEvent()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            disease,
            DiseaseStage.DeepSleep);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-cured",
            WorldEventTimeEffect.NoAdvance);

        SuccessfulDiseaseCureOutcome outcome = new(infection, worldEvent);

        Assert.Same(infection, outcome.Infection);
        Assert.Same(worldEvent, outcome.WorldEvent);
    }

    [Fact]
    public void RejectsNullValuesAndExposesOnlyImmutableProvenance()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            disease,
            DiseaseStage.DeepSleep);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-cured",
            WorldEventTimeEffect.NoAdvance);

        Assert.Throws<ArgumentNullException>(() => new SuccessfulDiseaseCureOutcome(null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() => new SuccessfulDiseaseCureOutcome(infection, null!));

        Type outcomeType = typeof(SuccessfulDiseaseCureOutcome);
        Assert.Equal(
            ExpectedPropertyNames,
            outcomeType.GetProperties().Select(property => property.Name).Order());
        Assert.All(outcomeType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(outcomeType.GetMethods().Where(method =>
            method.DeclaringType == outcomeType
            && !method.IsSpecialName
            && method.IsPublic));
        Assert.Empty(outcomeType.GetEvents());
        Assert.True(outcomeType.IsSealed);
    }
}

using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseExposureTransitionTests
{
    [Fact]
    public void AppendsExactExposedInfectionWithoutChangingInputs()
    {
        DiseaseExposurePolicyTests.ExposureInput input = DiseaseExposurePolicyTests.Input();

        DiseaseTruthSnapshot result = DiseaseExposureTransition.Apply(
            input.Outcome,
            input.Truth,
            input.Protections,
            input.WorldEvents,
            input.EventLog);

        CharacterDiseaseState infection = Assert.Single(result.Infections);
        Assert.Same(input.Truth.Definitions, result.Definitions);
        Assert.Same(input.Outcome.CharacterId, infection.CharacterId);
        Assert.Same(input.Outcome.Disease, infection.Disease);
        Assert.Same(DiseaseStage.Exposed, infection.Stage);
        Assert.Null(infection.LastProgressionEventId);
        Assert.Empty(input.Truth.Infections);
        Assert.Empty(input.Protections.ActiveProtections);
    }

    [Fact]
    public void RetainsExistingInfectionsInStableOrderAndRejectsProtectedExposure()
    {
        DiseaseExposurePolicyTests.ExposureInput input = DiseaseExposurePolicyTests.Input();
        CharacterDiseaseState existing = new(
            new("healer"),
            input.Outcome.Disease,
            DiseaseStage.Drowsing,
            null,
            DiseaseCareState.Unassisted);
        DiseaseTruthSnapshot truth = new(input.Truth.Definitions, [existing]);

        DiseaseTruthSnapshot result = DiseaseExposureTransition.Apply(
            input.Outcome,
            truth,
            input.Protections,
            input.WorldEvents,
            input.EventLog);

        Assert.Same(existing, result.Infections[0]);
        Assert.Same(input.Outcome.CharacterId, result.Infections[1].CharacterId);

        DiseaseProtectionDefinition protection = Assert.Single(
            input.Protections.Definitions.Definitions);
        DiseaseProtectionSnapshot active = new(
            input.Protections.Definitions,
            [new CharacterDiseaseProtection(input.Outcome.CharacterId, protection)]);
        Assert.Throws<InvalidOperationException>(() => DiseaseExposureTransition.Apply(
            input.Outcome,
            input.Truth,
            active,
            input.WorldEvents,
            input.EventLog));
    }
}

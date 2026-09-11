using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProtectionApplicationTransitionTests
{
    [Fact]
    public void AppendsProtectionWithExactCharacterAndDefinition()
    {
        DiseaseProtectionApplicationPolicyTests.ProtectionInput input =
            DiseaseProtectionApplicationPolicyTests.Input();

        DiseaseProtectionSnapshot result = DiseaseProtectionApplicationTransition.Apply(
            input.Outcome,
            input.Protections,
            input.WorldEvents,
            input.EventLog);

        CharacterDiseaseProtection protection = Assert.Single(result.ActiveProtections);
        Assert.Same(input.Protections.Definitions, result.Definitions);
        Assert.Same(input.Outcome.EvidenceView.CharacterId, protection.CharacterId);
        Assert.Same(input.Outcome.Definition, protection.Definition);
        Assert.Empty(input.Protections.ActiveProtections);
    }

    [Fact]
    public void RejectsIneligibleApplicationWithoutChangingProtections()
    {
        DiseaseProtectionApplicationPolicyTests.ProtectionInput input =
            DiseaseProtectionApplicationPolicyTests.Input();

        Assert.Throws<InvalidOperationException>(() =>
            DiseaseProtectionApplicationTransition.Apply(
                input.Outcome,
                input.Protections,
                input.WorldEvents,
                new([])));
        Assert.Empty(input.Protections.ActiveProtections);
    }
}

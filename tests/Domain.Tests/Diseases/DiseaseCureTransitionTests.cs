using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCureTransitionTests
{
    [Fact]
    public void RemovesOnlyTheExactCuredInfectionInStableOrder()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState before = new(
            new CharacterId("first-patient"),
            disease,
            DiseaseStage.EarlyIllness,
            null);
        CharacterDiseaseState cured = new(
            new CharacterId("patient"),
            disease,
            DiseaseStage.DeepSleep,
            null);
        CharacterDiseaseState after = new(
            new CharacterId("last-patient"),
            disease,
            DiseaseStage.Drowsing,
            null);
        DiseaseDefinitionSnapshot definitions = new([disease]);
        DiseaseTruthSnapshot truth = new(definitions, [before, cured, after]);
        DiseaseCureEligibilityPolicyTests.CureInput input =
            DiseaseCureEligibilityPolicyTests.Input();
        SuccessfulDiseaseCureOutcome outcome = new(cured, input.Outcome.WorldEvent);

        DiseaseTruthSnapshot result = DiseaseCureTransition.Apply(
            outcome,
            truth,
            input.WorldEvents,
            input.EventLog);

        Assert.NotSame(truth, result);
        Assert.Same(definitions, result.Definitions);
        Assert.Collection(
            result.Infections,
            infection => Assert.Same(before, infection),
            infection => Assert.Same(after, infection));
        Assert.Collection(
            truth.Infections,
            infection => Assert.Same(before, infection),
            infection => Assert.Same(cured, infection),
            infection => Assert.Same(after, infection));
    }

    [Fact]
    public void RejectsIneligibleCureWithoutChangingTruth()
    {
        DiseaseCureEligibilityPolicyTests.CureInput input =
            DiseaseCureEligibilityPolicyTests.Input();

        Assert.Throws<InvalidOperationException>(() => DiseaseCureTransition.Apply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            new([])));
        Assert.Same(input.Outcome.Infection, Assert.Single(input.Truth.Infections));
    }

    [Fact]
    public void ExposesOnlyOnePureTransition()
    {
        Type transitionType = typeof(DiseaseCureTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.Equal(typeof(DiseaseTruthSnapshot), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}

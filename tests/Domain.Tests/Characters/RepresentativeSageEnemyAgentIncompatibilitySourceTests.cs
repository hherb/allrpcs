using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class RepresentativeSageEnemyAgentIncompatibilitySourceTests
{
    [Fact]
    public void CreatesOneExplicitSageAndEnemyAgentBeliefGoalConflict()
    {
        CharacterBeliefGoalIncompatibility fixture =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();

        Assert.Equal("village-sage", fixture.FirstCharacter.Identity.Id.Value);
        Assert.Equal("timely-recovery-outranks-political-leverage", fixture.FirstBelief.Key);
        Assert.Equal("advance-evidence-led-recovery", fixture.FirstGoal.Key);
        Assert.Equal("enemy-agent", fixture.SecondCharacter.Identity.Id.Value);
        Assert.Equal("limited-delay-serves-the-crown", fixture.SecondBelief.Key);
        Assert.Equal("delay-rival-recovery", fixture.SecondGoal.Key);
        Assert.Contains("incompatible", fixture.Rationale, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "southern crown",
            fixture.SecondCharacter.PublicBackground.Text,
            StringComparison.Ordinal);
        Assert.Contains(
            "southern crown",
            fixture.SecondCharacter.PrivateBackground.Text,
            StringComparison.Ordinal);
        Assert.Equal(
            ["compare-presented-evidence", "challenge-unsupported-assumptions"],
            fixture.FirstCharacter.Capabilities.Entries.Select(capability => capability.Key));
        Assert.Equal(
            ["maintain-cover", "assess-local-influence"],
            fixture.SecondCharacter.Capabilities.Entries.Select(capability => capability.Key));
        Assert.Same(
            fixture.FirstBelief,
            fixture.FirstCharacter.Beliefs.Entries.Single(belief => belief.Key == fixture.FirstBelief.Key));
        Assert.Same(
            fixture.FirstGoal,
            fixture.FirstCharacter.Goals.Entries.Single(goal => goal.Key == fixture.FirstGoal.Key));
        Assert.Same(
            fixture.SecondBelief,
            fixture.SecondCharacter.Beliefs.Entries.Single(belief => belief.Key == fixture.SecondBelief.Key));
        Assert.Same(
            fixture.SecondGoal,
            fixture.SecondCharacter.Goals.Entries.Single(goal => goal.Key == fixture.SecondGoal.Key));
    }

    [Fact]
    public void CreatesFreshIndependentCompositionsAndExposesOnlyOneFactoryMethod()
    {
        CharacterBeliefGoalIncompatibility first =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibility second =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.FirstCharacter, second.FirstCharacter);
        Assert.NotSame(first.FirstBelief, second.FirstBelief);
        Assert.NotSame(first.FirstGoal, second.FirstGoal);
        Assert.NotSame(first.SecondCharacter, second.SecondCharacter);
        Assert.NotSame(first.SecondBelief, second.SecondBelief);
        Assert.NotSame(first.SecondGoal, second.SecondGoal);

        Type sourceType = typeof(RepresentativeSageEnemyAgentIncompatibilitySource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterBeliefGoalIncompatibility), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetConstructors());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
        Assert.True(sourceType.IsAbstract && sourceType.IsSealed);
    }
}

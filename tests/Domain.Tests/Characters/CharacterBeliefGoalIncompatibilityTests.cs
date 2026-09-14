using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityTests
{
    [Fact]
    public void RetainsExactCharactersBeliefsGoalsAndRationaleWithoutMutation()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterBelief sageBelief = sage.Beliefs.Entries[0];
        CharacterGoal sageGoal = sage.Goals.Entries[0];
        CharacterBelief agentBelief = agent.Beliefs.Entries[0];
        CharacterGoal agentGoal = agent.Goals.Entries[0];
        CharacterBelief[] sageBeliefs = [.. sage.Beliefs.Entries];
        CharacterGoal[] sageGoals = [.. sage.Goals.Entries];
        CharacterBelief[] agentBeliefs = [.. agent.Beliefs.Entries];
        CharacterGoal[] agentGoals = [.. agent.Goals.Entries];

        CharacterBeliefGoalIncompatibility declaration = new(
            sage,
            sageBelief,
            sageGoal,
            agent,
            agentBelief,
            agentGoal,
            "These authored positions cannot all be satisfied at the same time.");

        Assert.Same(sage, declaration.FirstCharacter);
        Assert.Same(sageBelief, declaration.FirstBelief);
        Assert.Same(sageGoal, declaration.FirstGoal);
        Assert.Same(agent, declaration.SecondCharacter);
        Assert.Same(agentBelief, declaration.SecondBelief);
        Assert.Same(agentGoal, declaration.SecondGoal);
        Assert.Equal(
            "These authored positions cannot all be satisfied at the same time.",
            declaration.Rationale);
        Assert.Equal(sageBeliefs, sage.Beliefs.Entries);
        Assert.Equal(sageGoals, sage.Goals.Entries);
        Assert.Equal(agentBeliefs, agent.Beliefs.Entries);
        Assert.Equal(agentGoals, agent.Goals.Entries);
    }

    [Fact]
    public void RejectsNullComponents()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterBelief sageBelief = sage.Beliefs.Entries[0];
        CharacterGoal sageGoal = sage.Goals.Entries[0];
        CharacterBelief agentBelief = agent.Beliefs.Entries[0];
        CharacterGoal agentGoal = agent.Goals.Entries[0];

        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            null!, sageBelief, sageGoal, agent, agentBelief, agentGoal, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, null!, sageGoal, agent, agentBelief, agentGoal, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, sageBelief, null!, agent, agentBelief, agentGoal, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, sageBelief, sageGoal, null!, agentBelief, agentGoal, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, sageBelief, sageGoal, agent, null!, agentGoal, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, sageBelief, sageGoal, agent, agentBelief, null!, "rationale"));
        Assert.Throws<ArgumentNullException>(() => new CharacterBeliefGoalIncompatibility(
            sage, sageBelief, sageGoal, agent, agentBelief, agentGoal, null!));
    }

    [Fact]
    public void RejectsSameCharacterByValueEvenAcrossFreshCompositions()
    {
        Character first = RepresentativeSageCharacterSource.Create();
        Character second = RepresentativeSageCharacterSource.Create();

        Assert.Throws<ArgumentException>(() => new CharacterBeliefGoalIncompatibility(
            first,
            first.Beliefs.Entries[0],
            first.Goals.Entries[0],
            second,
            second.Beliefs.Entries[0],
            second.Goals.Entries[0],
            "rationale"));
    }

    [Fact]
    public void RejectsBeliefsAndGoalsNotRetainedByTheirExactOwningCharacter()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();

        Assert.Throws<ArgumentException>(() => new CharacterBeliefGoalIncompatibility(
            sage,
            new CharacterBelief(sage.Beliefs.Entries[0].Key, sage.Beliefs.Entries[0].Statement),
            sage.Goals.Entries[0],
            agent,
            agent.Beliefs.Entries[0],
            agent.Goals.Entries[0],
            "rationale"));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefGoalIncompatibility(
            sage,
            sage.Beliefs.Entries[0],
            new CharacterGoal(sage.Goals.Entries[0].Key, sage.Goals.Entries[0].Description),
            agent,
            agent.Beliefs.Entries[0],
            agent.Goals.Entries[0],
            "rationale"));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefGoalIncompatibility(
            sage,
            sage.Beliefs.Entries[0],
            sage.Goals.Entries[0],
            agent,
            new CharacterBelief(agent.Beliefs.Entries[0].Key, agent.Beliefs.Entries[0].Statement),
            agent.Goals.Entries[0],
            "rationale"));
        Assert.Throws<ArgumentException>(() => new CharacterBeliefGoalIncompatibility(
            sage,
            sage.Beliefs.Entries[0],
            sage.Goals.Entries[0],
            agent,
            agent.Beliefs.Entries[0],
            new CharacterGoal(agent.Goals.Entries[0].Key, agent.Goals.Entries[0].Description),
            "rationale"));
    }

    [Fact]
    public void RequiresANonBlankBoundedRationale()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();

        CharacterBeliefGoalIncompatibility maximum = Create(
            sage,
            agent,
            new string('r', CharacterBeliefGoalIncompatibility.MaximumRationaleLength));

        Assert.Equal(CharacterBeliefGoalIncompatibility.MaximumRationaleLength, maximum.Rationale.Length);
        Assert.Throws<ArgumentException>(() => Create(sage, agent, string.Empty));
        Assert.Throws<ArgumentException>(() => Create(sage, agent, " \t\r\n"));
        Assert.Throws<ArgumentException>(() => Create(
            sage,
            agent,
            new string('r', CharacterBeliefGoalIncompatibility.MaximumRationaleLength + 1)));
    }

    private static CharacterBeliefGoalIncompatibility Create(
        Character first,
        Character second,
        string rationale) => new(
            first,
            first.Beliefs.Entries[0],
            first.Goals.Entries[0],
            second,
            second.Beliefs.Entries[0],
            second.Goals.Entries[0],
            rationale);
}

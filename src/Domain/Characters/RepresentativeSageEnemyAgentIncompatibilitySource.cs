namespace Allrpcs.Domain.Characters;

/// <summary>Authors one explicit incompatible-belief and incompatible-goal character fixture.</summary>
public static class RepresentativeSageEnemyAgentIncompatibilitySource
{
    /// <summary>Creates a fresh declaration from the representative sage and enemy-agent roles.</summary>
    public static CharacterBeliefGoalIncompatibility Create()
    {
        Character authoredSage = RepresentativeSageCharacterSource.Create();
        CharacterBelief sageBelief = new(
            "timely-recovery-outranks-political-leverage",
            "Political leverage cannot justify delaying evidence-led recovery from an outbreak.");
        CharacterGoal sageGoal = new(
            "advance-evidence-led-recovery",
            "Advance evidence-led recovery without delay for political concessions.");
        Character sage = new(
            authoredSage.Identity,
            authoredSage.PublicBackground,
            authoredSage.PrivateBackground,
            authoredSage.Capabilities,
            authoredSage.Limitations,
            authoredSage.ControllerAssignment,
            authoredSage.Conditions,
            authoredSage.Inventory,
            authoredSage.Knowledge,
            new CharacterBeliefSet([.. authoredSage.Beliefs.Entries, sageBelief]),
            authoredSage.Relationships,
            new CharacterGoalSet([.. authoredSage.Goals.Entries, sageGoal]));

        Character authoredAgent = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterBelief agentBelief = new(
            "limited-delay-serves-the-crown",
            "Delaying rival recovery is justified to gain concessions while mass civilian harm "
            + "remains avoidable.");
        Character agent = new(
            authoredAgent.Identity,
            authoredAgent.PublicBackground,
            authoredAgent.PrivateBackground,
            authoredAgent.Capabilities,
            authoredAgent.Limitations,
            authoredAgent.ControllerAssignment,
            authoredAgent.Conditions,
            authoredAgent.Inventory,
            authoredAgent.Knowledge,
            new CharacterBeliefSet([.. authoredAgent.Beliefs.Entries, agentBelief]),
            authoredAgent.Relationships,
            authoredAgent.Goals);
        CharacterGoal agentGoal = agent.Goals.Entries.Single(
            goal => goal.Key == "delay-rival-recovery");

        return new CharacterBeliefGoalIncompatibility(
            sage,
            sageBelief,
            sageGoal,
            agent,
            agentBelief,
            agentGoal,
            "The sage's timely recovery belief and goal are incompatible with the agent's belief "
            + "that limited delay is justified and goal to delay rival recovery.");
    }
}

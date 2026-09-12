using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class RepresentativeEnemyAgentCharacterSourceTests
{
    [Fact]
    public void CreatesAnExactlyAlignedLlmAgentWithPrivateNationalObjectiveAndBounds()
    {
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();

        Assert.Equal("enemy-agent", agent.Identity.Id.Value);
        Assert.Equal("VALE", agent.Identity.DisplayName);
        Assert.Same(agent.Identity.Id, agent.ControllerAssignment.CharacterId);
        Assert.Same(ControllerType.Llm, agent.ControllerAssignment.ControllerType);
        Assert.DoesNotContain("agent", agent.PublicBackground.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("southern crown", agent.PrivateBackground.Text, StringComparison.Ordinal);
        Assert.Equal("agent-national-orders", Assert.Single(agent.Knowledge.KnowledgeReferences).Value);
        Assert.Equal(
            ["maintain-cover", "assess-local-influence"],
            agent.Capabilities.Entries.Select(capability => capability.Key));
        Assert.Equal(
            [
                "must-use-authoritative-actions",
                "requires-physical-and-informational-access",
                "cannot-remove-all-recovery-routes",
            ],
            agent.Limitations.Entries.Select(limitation => limitation.Key));
        Assert.Equal(
            ["delay-rival-recovery", "avoid-mass-civilian-harm"],
            agent.Goals.Entries.Select(goal => goal.Key));
        Assert.Equal(
            "civilian-harm-can-outweigh-orders",
            Assert.Single(agent.Beliefs.Entries).Key);
        Assert.Empty(agent.Conditions.Entries);
        Assert.Empty(agent.Inventory.ObjectReferences);
        Assert.Empty(agent.Relationships.Entries);
    }

    [Fact]
    public void KeepsTheNationalObjectivePrivateAndAuthorsAConflictingAllegianceMotive()
    {
        Character agent = RepresentativeEnemyAgentCharacterSource.Create();

        Assert.DoesNotContain("southern crown", agent.PublicBackground.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("delay", agent.PublicBackground.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "prolong the outbreak",
            agent.PrivateBackground.Text,
            StringComparison.Ordinal);
        Assert.Contains(
            agent.Goals.Entries,
            goal => goal.Key == "delay-rival-recovery");
        Assert.Contains(
            agent.Goals.Entries,
            goal => goal.Key == "avoid-mass-civilian-harm");
        Assert.Contains(
            "change allegiance",
            agent.Beliefs.Entries[0].Statement,
            StringComparison.Ordinal);
    }

    [Fact]
    public void CreatesFreshIndependentCompositionsAndExposesOnlyOneFactoryMethod()
    {
        Character first = RepresentativeEnemyAgentCharacterSource.Create();
        Character second = RepresentativeEnemyAgentCharacterSource.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.Identity, second.Identity);
        Assert.NotSame(first.Identity.Id, second.Identity.Id);
        Assert.NotSame(first.PublicBackground, second.PublicBackground);
        Assert.NotSame(first.PrivateBackground, second.PrivateBackground);
        Assert.NotSame(first.Capabilities, second.Capabilities);
        Assert.NotSame(first.Limitations, second.Limitations);
        Assert.NotSame(first.ControllerAssignment, second.ControllerAssignment);
        Assert.NotSame(first.Conditions, second.Conditions);
        Assert.NotSame(first.Inventory, second.Inventory);
        Assert.NotSame(first.Knowledge, second.Knowledge);
        Assert.NotSame(first.Beliefs, second.Beliefs);
        Assert.NotSame(first.Relationships, second.Relationships);
        Assert.NotSame(first.Goals, second.Goals);

        Type sourceType = typeof(RepresentativeEnemyAgentCharacterSource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Character), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetConstructors());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
        Assert.True(sourceType.IsAbstract && sourceType.IsSealed);
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class RepresentativeHermitCharacterSourceTests
{
    [Fact]
    public void CreatesExactlyTwoLlmHermitsInStableOrderWithExactIdentityAlignment()
    {
        IReadOnlyList<Character> hermits = RepresentativeHermitCharacterSource.Create();

        Assert.Collection(
            hermits,
            first => AssertHermit(first, "woodland-hermit", "ELIAN"),
            second => AssertHermit(second, "riverside-hermit", "SERA"));
    }

    [Fact]
    public void KeepsEachHermitsDistinctPrivateKnowledgeBeliefAndMotiveOutOfTheOtherComposition()
    {
        IReadOnlyList<Character> hermits = RepresentativeHermitCharacterSource.Create();
        Character woodlandHermit = hermits[0];
        Character riversideHermit = hermits[1];

        Assert.NotEmpty(woodlandHermit.PrivateBackground.Text);
        Assert.NotEmpty(riversideHermit.PrivateBackground.Text);
        Assert.NotEqual(
            woodlandHermit.PrivateBackground.Text,
            riversideHermit.PrivateBackground.Text);

        AssertPrivateState(
            woodlandHermit,
            "woodland-hermit-old-growth-remedies",
            "protect-old-growth-remedies",
            "outsiders-overharvest-remedies");
        AssertPrivateState(
            riversideHermit,
            "riverside-hermit-laundry-history",
            "trace-sickness-through-water",
            "laundry-runoff-spreads-sickness");

        Assert.DoesNotContain(
            woodlandHermit.Knowledge.KnowledgeReferences[0],
            riversideHermit.Knowledge.KnowledgeReferences);
        Assert.DoesNotContain(
            woodlandHermit.Goals.Entries[0],
            riversideHermit.Goals.Entries);
        Assert.DoesNotContain(
            woodlandHermit.Beliefs.Entries[0],
            riversideHermit.Beliefs.Entries);
        Assert.DoesNotContain(
            riversideHermit.Knowledge.KnowledgeReferences[0],
            woodlandHermit.Knowledge.KnowledgeReferences);
        Assert.DoesNotContain(
            riversideHermit.Goals.Entries[0],
            woodlandHermit.Goals.Entries);
        Assert.DoesNotContain(
            riversideHermit.Beliefs.Entries[0],
            woodlandHermit.Beliefs.Entries);
    }

    [Fact]
    public void CreatesFreshReadOnlyCompositionsAndExposesOnlyOneFactoryMethod()
    {
        IReadOnlyList<Character> first = RepresentativeHermitCharacterSource.Create();
        IReadOnlyList<Character> second = RepresentativeHermitCharacterSource.Create();

        Assert.NotSame(first, second);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Character>)first).Add(first[0]));

        for (int index = 0; index < first.Count; index++)
        {
            Assert.NotSame(first[index], second[index]);
            Assert.NotSame(first[index].Identity, second[index].Identity);
            Assert.NotSame(first[index].Identity.Id, second[index].Identity.Id);
            Assert.NotSame(first[index].PublicBackground, second[index].PublicBackground);
            Assert.NotSame(first[index].PrivateBackground, second[index].PrivateBackground);
            Assert.NotSame(first[index].Capabilities, second[index].Capabilities);
            Assert.NotSame(first[index].Limitations, second[index].Limitations);
            Assert.NotSame(first[index].ControllerAssignment, second[index].ControllerAssignment);
            Assert.NotSame(first[index].Conditions, second[index].Conditions);
            Assert.NotSame(first[index].Inventory, second[index].Inventory);
            Assert.NotSame(first[index].Knowledge, second[index].Knowledge);
            Assert.NotSame(first[index].Beliefs, second[index].Beliefs);
            Assert.NotSame(first[index].Relationships, second[index].Relationships);
            Assert.NotSame(first[index].Goals, second[index].Goals);
        }

        Type sourceType = typeof(RepresentativeHermitCharacterSource);
        System.Reflection.MethodInfo method = Assert.Single(sourceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sourceType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IReadOnlyList<Character>), method.ReturnType);
        Assert.Empty(method.GetParameters());
        Assert.Empty(sourceType.GetConstructors());
        Assert.Empty(sourceType.GetProperties());
        Assert.Empty(sourceType.GetEvents());
        Assert.True(sourceType.IsAbstract && sourceType.IsSealed);
    }

    private static void AssertHermit(Character character, string id, string displayName)
    {
        Assert.Equal(id, character.Identity.Id.Value);
        Assert.Equal(displayName, character.Identity.DisplayName);
        Assert.Same(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Same(ControllerType.Llm, character.ControllerAssignment.ControllerType);
    }

    private static void AssertPrivateState(
        Character character,
        string knowledgeId,
        string goalKey,
        string beliefKey)
    {
        Assert.Equal(knowledgeId, Assert.Single(character.Knowledge.KnowledgeReferences).Value);
        Assert.Equal(goalKey, Assert.Single(character.Goals.Entries).Key);
        Assert.Equal(beliefKey, Assert.Single(character.Beliefs.Entries).Key);
    }
}

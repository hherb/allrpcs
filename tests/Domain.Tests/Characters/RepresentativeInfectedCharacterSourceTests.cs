using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class RepresentativeInfectedCharacterSourceTests
{
    [Fact]
    public void CreatesExactlyTwoDormantInfectedRolesInStableOrderWithExactAlignment()
    {
        IReadOnlyList<Character> infected = RepresentativeInfectedCharacterSource.Create();

        Assert.Collection(
            infected,
            forester => AssertInfected(
                forester,
                "sleeping-forester",
                "ROWAN",
                "forester-hidden-trails",
                "return-to-woodland-patrol"),
            scribe => AssertInfected(
                scribe,
                "sleeping-scribe",
                "MIRA",
                "scribe-village-records",
                "preserve-outbreak-records"));
    }

    [Fact]
    public void KeepsEachRolesLatentAuthoredStateDistinctWithoutAddingWakeBehavior()
    {
        IReadOnlyList<Character> infected = RepresentativeInfectedCharacterSource.Create();
        Character forester = infected[0];
        Character scribe = infected[1];

        Assert.NotEmpty(forester.PublicBackground.Text);
        Assert.NotEmpty(scribe.PublicBackground.Text);
        Assert.Equal("afflicted-deep-sleep", Assert.Single(forester.Conditions.Entries).Key);
        Assert.Equal("afflicted-deep-sleep", Assert.Single(scribe.Conditions.Entries).Key);
        Assert.Equal("track-woodland-paths", Assert.Single(forester.Capabilities.Entries).Key);
        Assert.Equal("interpret-village-records", Assert.Single(scribe.Capabilities.Entries).Key);
        Assert.Equal("unresponsive-while-dormant", Assert.Single(forester.Limitations.Entries).Key);
        Assert.Equal("unresponsive-while-dormant", Assert.Single(scribe.Limitations.Entries).Key);
        Assert.DoesNotContain(
            forester.Knowledge.KnowledgeReferences[0],
            scribe.Knowledge.KnowledgeReferences);
        Assert.DoesNotContain(forester.Goals.Entries[0], scribe.Goals.Entries);
        Assert.DoesNotContain(
            scribe.Knowledge.KnowledgeReferences[0],
            forester.Knowledge.KnowledgeReferences);
        Assert.DoesNotContain(scribe.Goals.Entries[0], forester.Goals.Entries);
        Assert.Empty(forester.Inventory.ObjectReferences);
        Assert.Empty(scribe.Inventory.ObjectReferences);
        Assert.Empty(forester.Relationships.Entries);
        Assert.Empty(scribe.Relationships.Entries);
    }

    [Fact]
    public void CreatesFreshReadOnlyCompositionsAndExposesOnlyOneFactoryMethod()
    {
        IReadOnlyList<Character> first = RepresentativeInfectedCharacterSource.Create();
        IReadOnlyList<Character> second = RepresentativeInfectedCharacterSource.Create();

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

        Type sourceType = typeof(RepresentativeInfectedCharacterSource);
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

    private static void AssertInfected(
        Character character,
        string id,
        string displayName,
        string knowledgeId,
        string goalKey)
    {
        Assert.Equal(id, character.Identity.Id.Value);
        Assert.Equal(displayName, character.Identity.DisplayName);
        Assert.Same(character.Identity.Id, character.ControllerAssignment.CharacterId);
        Assert.Same(ControllerType.Dormant, character.ControllerAssignment.ControllerType);
        Assert.Equal(knowledgeId, Assert.Single(character.Knowledge.KnowledgeReferences).Value);
        Assert.Equal(goalKey, Assert.Single(character.Goals.Entries).Key);
    }
}

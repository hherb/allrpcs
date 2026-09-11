using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class RepresentativeSageCharacterSourceTests
{
    [Fact]
    public void CreatesAnExactlyAlignedLlmSageWithBoundedConsultantState()
    {
        Character sage = RepresentativeSageCharacterSource.Create();

        Assert.Equal("village-sage", sage.Identity.Id.Value);
        Assert.Equal("ORIN", sage.Identity.DisplayName);
        Assert.Same(sage.Identity.Id, sage.ControllerAssignment.CharacterId);
        Assert.Same(ControllerType.Llm, sage.ControllerAssignment.ControllerType);
        Assert.NotEmpty(sage.PublicBackground.Text);
        Assert.NotEmpty(sage.PrivateBackground.Text);
        Assert.Equal(
            ["compare-presented-evidence", "challenge-unsupported-assumptions"],
            sage.Capabilities.Entries.Select(capability => capability.Key));
        Assert.Equal(
            ["requires-presented-evidence", "cannot-author-authoritative-truth"],
            sage.Limitations.Entries.Select(limitation => limitation.Key));
        Assert.Equal(
            "sage-shrine-oral-history",
            Assert.Single(sage.Knowledge.KnowledgeReferences).Value);
        Assert.Equal(
            "contradictions-reveal-assumptions",
            Assert.Single(sage.Beliefs.Entries).Key);
        Assert.Equal(
            "strengthen-investigation-reasoning",
            Assert.Single(sage.Goals.Entries).Key);
        Assert.Empty(sage.Conditions.Entries);
        Assert.Empty(sage.Inventory.ObjectReferences);
        Assert.Empty(sage.Relationships.Entries);
    }

    [Fact]
    public void CreatesFreshIndependentCompositionsAndExposesOnlyOneFactoryMethod()
    {
        Character first = RepresentativeSageCharacterSource.Create();
        Character second = RepresentativeSageCharacterSource.Create();

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

        Type sourceType = typeof(RepresentativeSageCharacterSource);
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

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterKnowledgeTests
{
    [Fact]
    public void EmptyKnowledgeIsExplicitlyValid()
    {
        CharacterKnowledge knowledge = new(Array.Empty<KnowledgeId>());

        Assert.Empty(knowledge.KnowledgeReferences);
    }

    [Fact]
    public void PreservesStableReferenceOrderAndTypedIdentifiers()
    {
        KnowledgeId first = new("healing-herb-appearance");
        KnowledgeId second = new("stream-water-location");

        CharacterKnowledge knowledge = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, knowledge.KnowledgeReferences);
        Assert.All(knowledge.KnowledgeReferences, reference => Assert.IsType<KnowledgeId>(reference));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        KnowledgeId original = new("healing-herb-appearance");
        KnowledgeId replacement = new("stream-water-location");
        KnowledgeId[] source = [original];

        CharacterKnowledge knowledge = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, knowledge.KnowledgeReferences);
    }

    [Fact]
    public void ExposesNoMutableCollection()
    {
        CharacterKnowledge knowledge = new(new[] { new KnowledgeId("healing-herb-appearance") });
        IList<KnowledgeId> exposedReferences = Assert.IsAssignableFrom<IList<KnowledgeId>>(
            knowledge.KnowledgeReferences);

        Assert.True(exposedReferences.IsReadOnly);
        Assert.Throws<NotSupportedException>(
            () => exposedReferences.Add(new KnowledgeId("stream-water-location")));
    }

    [Fact]
    public void RejectsDuplicateIdentifiers()
    {
        KnowledgeId first = new("healing-herb-appearance");
        KnowledgeId duplicate = new("healing-herb-appearance");

        Assert.Throws<ArgumentException>(() => new CharacterKnowledge(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterKnowledge(null!));
        Assert.Throws<ArgumentException>(() => new CharacterKnowledge(new KnowledgeId[] { null! }));
    }

    [Fact]
    public void ExposesOnlyOrderedKnowledgeReferences()
    {
        string[] publicProperties = typeof(CharacterKnowledge)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] publicMethods = typeof(CharacterKnowledge)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(CharacterKnowledge) && !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("KnowledgeReferences", property));
        Assert.Empty(publicMethods);
        Assert.Equal(
            typeof(IReadOnlyList<KnowledgeId>),
            typeof(CharacterKnowledge).GetProperty("KnowledgeReferences")!.PropertyType);
    }
}

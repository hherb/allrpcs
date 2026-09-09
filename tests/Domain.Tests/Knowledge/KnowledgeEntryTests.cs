using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;

namespace Allrpcs.Domain.Tests.Knowledge;

public sealed class KnowledgeEntryTests
{
    [Fact]
    public void RetainsExactIdentifierAndAuthoredText()
    {
        KnowledgeId id = new("moonwort-use");
        string text = "  Moonwort must be cooled before use.\r\n";

        KnowledgeEntry entry = new(id, text);

        Assert.Same(id, entry.Id);
        Assert.Same(text, entry.Text);
    }

    [Fact]
    public void AcceptsMaximumLengthAndRejectsInvalidInputs()
    {
        KnowledgeId id = new("moonwort-use");
        string maximum = new('k', KnowledgeEntry.MaximumTextLength);

        Assert.Same(maximum, new KnowledgeEntry(id, maximum).Text);
        Assert.Throws<ArgumentNullException>(() => new KnowledgeEntry(null!, "Knowledge."));
        Assert.Throws<ArgumentNullException>(() => new KnowledgeEntry(id, null!));
        Assert.Throws<ArgumentException>(() => new KnowledgeEntry(id, string.Empty));
        Assert.Throws<ArgumentException>(() => new KnowledgeEntry(id, " \t\r\n"));
        Assert.Throws<ArgumentException>(() => new KnowledgeEntry(id, maximum + "x"));
    }

    [Fact]
    public void ExposesOnlyImmutableAuthoredKnowledgeState()
    {
        (string Name, Type Type, bool CanWrite)[] properties = typeof(KnowledgeEntry)
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            properties,
            property =>
            {
                Assert.Equal("Id", property.Name);
                Assert.Equal(typeof(KnowledgeId), property.Type);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("Text", property.Name);
                Assert.Equal(typeof(string), property.Type);
                Assert.False(property.CanWrite);
            });
    }
}

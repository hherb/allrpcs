using Allrpcs.Application.Journal;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSectionCatalogueTests
{
    [Fact]
    public void RetainsTheCompleteCanonicalCatalogueInStableOrder()
    {
        JournalSectionDefinition[] sections = CanonicalSections();

        JournalSectionCatalogue catalogue = new(sections);

        Assert.Equal(
            ["observations", "testimony", "hypotheses", "experiments", "open-questions"],
            catalogue.Sections.Select(section => section.Key));
        Assert.Equal(sections, catalogue.Sections);
        for (int index = 0; index < sections.Length; index++)
        {
            Assert.Same(sections[index], catalogue.Sections[index]);
        }
    }

    [Fact]
    public void CopiesTheCallerCollectionAndExposesItReadOnly()
    {
        JournalSectionDefinition[] sections = CanonicalSections();
        JournalSectionCatalogue catalogue = new(sections);

        sections[0] = new JournalSectionDefinition("unknown", "Unknown");

        Assert.Equal("observations", catalogue.Sections[0].Key);
        Assert.IsAssignableFrom<IReadOnlyList<JournalSectionDefinition>>(catalogue.Sections);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)catalogue.Sections).Clear());
    }

    [Fact]
    public void RejectsNullEmptyIncompleteAndNullEntryCollections()
    {
        JournalSectionDefinition[] sections = CanonicalSections();

        Assert.Throws<ArgumentNullException>(() => new JournalSectionCatalogue(null!));
        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue([]));
        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue(sections[..^1]));
        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue(
            [sections[0], sections[1], null!, sections[3], sections[4]]));
    }

    [Fact]
    public void RejectsDuplicateReorderedAndUnknownCanonicalDefinitions()
    {
        JournalSectionDefinition[] sections = CanonicalSections();

        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue(
            [sections[0], sections[1], sections[1], sections[3], sections[4]]));
        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue(
            [sections[1], sections[0], sections[2], sections[3], sections[4]]));
        Assert.Throws<ArgumentException>(() => new JournalSectionCatalogue(
            [sections[0], sections[1], sections[2], sections[3], new("unknown", "Unknown")]));
    }

    private static JournalSectionDefinition[] CanonicalSections() =>
    [
        new("observations", "Observations"),
        new("testimony", "Testimony"),
        new("hypotheses", "Hypotheses"),
        new("experiments", "Experiments"),
        new("open-questions", "Open Questions"),
    ];
}

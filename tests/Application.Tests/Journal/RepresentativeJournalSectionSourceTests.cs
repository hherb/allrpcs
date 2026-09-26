using Allrpcs.Application.Journal;

namespace Allrpcs.Application.Tests.Journal;

public sealed class RepresentativeJournalSectionSourceTests
{
    [Fact]
    public void AuthorsTheFiveContentFreeSectionsInStableOrder()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();

        Assert.Equal(
            [
                ("observations", "Observations"),
                ("testimony", "Testimony"),
                ("hypotheses", "Hypotheses"),
                ("experiments", "Experiments"),
                ("open-questions", "Open Questions"),
            ],
            catalogue.Sections.Select(section => (section.Key, section.DisplayLabel)));
    }

    [Fact]
    public void ReturnsIndependentCataloguesWithFreshExactDefinitions()
    {
        JournalSectionCatalogue first = RepresentativeJournalSectionSource.Create();
        JournalSectionCatalogue second = RepresentativeJournalSectionSource.Create();

        Assert.NotSame(first, second);
        Assert.Equal(first.Sections.Count, second.Sections.Count);
        for (int index = 0; index < first.Sections.Count; index++)
        {
            Assert.NotSame(first.Sections[index], second.Sections[index]);
        }
    }
}

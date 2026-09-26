using Allrpcs.Application.Journal;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSectionDefinitionTests
{
    [Fact]
    public void RetainsExactBoundedAuthoredValues()
    {
        JournalSectionDefinition section = new("open-questions", " Open Questions ");

        Assert.Equal("open-questions", section.Key);
        Assert.Equal(" Open Questions ", section.DisplayLabel);
    }

    [Theory]
    [InlineData(null, "Observations")]
    [InlineData("observations", null)]
    public void RejectsNullValues(string? key, string? displayLabel)
    {
        Assert.Throws<ArgumentNullException>(() => new JournalSectionDefinition(key!, displayLabel!));
    }

    [Theory]
    [InlineData("", "Observations")]
    [InlineData(" ", "Observations")]
    [InlineData("observations", "")]
    [InlineData("observations", "\t")]
    public void RejectsEmptyOrWhitespaceOnlyValues(string key, string displayLabel)
    {
        Assert.Throws<ArgumentException>(() => new JournalSectionDefinition(key, displayLabel));
    }

    [Fact]
    public void RejectsOverlongValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new JournalSectionDefinition(
            new string('k', JournalSectionDefinition.MaximumKeyLength + 1),
            "Observations"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new JournalSectionDefinition(
            "observations",
            new string('l', JournalSectionDefinition.MaximumDisplayLabelLength + 1)));
    }
}

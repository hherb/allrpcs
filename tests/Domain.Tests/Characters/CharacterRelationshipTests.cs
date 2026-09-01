using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipTests
{
    [Fact]
    public void PreservesRelatedCharacterIdentifierAndAuthoredSummaryExactly()
    {
        CharacterId relatedCharacterId = new("travelling-healer");
        const string Summary = "  Trusted after the woodland journey.\r\n";

        CharacterRelationship relationship = new(relatedCharacterId, Summary);

        Assert.Same(relatedCharacterId, relationship.RelatedCharacterId);
        Assert.Equal(Summary, relationship.Summary);
    }

    [Fact]
    public void EqualityUsesRelatedCharacterIdentifierAndAuthoredSummary()
    {
        CharacterRelationship first = new(
            new CharacterId("travelling-healer"),
            "Trusted travelling companion.");
        CharacterRelationship same = new(
            new CharacterId("travelling-healer"),
            "Trusted travelling companion.");
        CharacterRelationship differentCharacter = new(
            new CharacterId("village-sage"),
            "Trusted travelling companion.");
        CharacterRelationship differentSummary = new(
            new CharacterId("travelling-healer"),
            "Respected travelling companion.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentCharacter);
        Assert.NotEqual(first, differentSummary);
    }

    [Fact]
    public void PreservesSummaryAtMaximumLength()
    {
        string summary = new('r', CharacterRelationship.MaximumSummaryLength);

        CharacterRelationship relationship = new(new CharacterId("travelling-healer"), summary);

        Assert.Equal(summary, relationship.Summary);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlySummaries(string summary)
    {
        Assert.Throws<ArgumentException>(
            () => new CharacterRelationship(new CharacterId("travelling-healer"), summary));
    }

    [Fact]
    public void RejectsOversizedAndNullValues()
    {
        string oversizedSummary = new('r', CharacterRelationship.MaximumSummaryLength + 1);

        Assert.Throws<ArgumentException>(
            () => new CharacterRelationship(new CharacterId("travelling-healer"), oversizedSummary));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterRelationship(null!, "Trusted travelling companion."));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterRelationship(new CharacterId("travelling-healer"), null!));
    }

    [Fact]
    public void ExposesOnlyRelatedCharacterAndAuthoredSummary()
    {
        string[] publicProperties = typeof(CharacterRelationship)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("RelatedCharacterId", property),
            property => Assert.Equal("Summary", property));
    }
}

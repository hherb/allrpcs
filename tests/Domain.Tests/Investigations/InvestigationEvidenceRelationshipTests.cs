using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationEvidenceRelationshipTests
{
    [Fact]
    public void ExposesOnlyTheClosedCanonicalVocabulary()
    {
        Assert.Collection(
            InvestigationEvidenceRelationship.All,
            value => Assert.Same(InvestigationEvidenceRelationship.Supports, value),
            value => Assert.Same(InvestigationEvidenceRelationship.Contradicts, value));
        Assert.Equal(
            ["supports", "contradicts"],
            InvestigationEvidenceRelationship.All.Select(value => value.Value));
    }

    [Fact]
    public void ParsesEachExactValueToTheCanonicalSingleton()
    {
        foreach (InvestigationEvidenceRelationship value in InvestigationEvidenceRelationship.All)
        {
            Assert.Same(value, InvestigationEvidenceRelationship.Parse(value.Value));
            Assert.Equal(value.Value, value.ToString());
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Supports")]
    [InlineData("supports ")]
    [InlineData("unrelated")]
    public void RejectsUnknownValues(string value)
    {
        Assert.Throws<ArgumentException>(() => InvestigationEvidenceRelationship.Parse(value));
    }

    [Fact]
    public void RejectsNullAndExposesNoPublicConstructionOrMutation()
    {
        Assert.Throws<ArgumentNullException>(() => InvestigationEvidenceRelationship.Parse(null!));
        Assert.Empty(typeof(InvestigationEvidenceRelationship).GetConstructors());
        Assert.All(
            typeof(InvestigationEvidenceRelationship).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.Empty(typeof(InvestigationEvidenceRelationship).GetEvents());
        Assert.True(typeof(InvestigationEvidenceRelationship).IsSealed);
    }
}

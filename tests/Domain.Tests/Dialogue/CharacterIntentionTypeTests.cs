using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterIntentionTypeTests
{
    private static readonly string[] ExpectedValues =
        ["wait", "examine-object", "collect-object"];

    private static readonly string[] ExpectedPropertyNames =
        ["All", "CollectObject", "ExamineObject", "Value", "Wait"];

    [Fact]
    public void DefinesTheInitialCanonicalIntentionSetInStableOrder()
    {
        Assert.Collection(
            CharacterIntentionType.All,
            type => Assert.Same(CharacterIntentionType.Wait, type),
            type => Assert.Same(CharacterIntentionType.ExamineObject, type),
            type => Assert.Same(CharacterIntentionType.CollectObject, type));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(ExpectedValues, CharacterIntentionType.All.Select(type => type.Value));

        foreach (CharacterIntentionType type in CharacterIntentionType.All)
        {
            Assert.Equal(type.Value, type.ToString());
            Assert.Same(type, CharacterIntentionType.Parse(type.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Wait")]
    [InlineData("examine_object")]
    [InlineData("collect--object")]
    [InlineData("collect-object ")]
    [InlineData("move-object")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterIntentionType.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterIntentionType.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedIntentionTypeSurface()
    {
        string[] publicProperties = typeof(CharacterIntentionType)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterIntentionType).GetConstructors());
    }
}

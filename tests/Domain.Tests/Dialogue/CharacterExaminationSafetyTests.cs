using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterExaminationSafetyTests
{
    private static readonly string[] ExpectedValues =
        ["safe-to-collect", "unsafe-to-collect"];

    private static readonly string[] ExpectedPropertyNames =
        ["All", "SafeToCollect", "UnsafeToCollect", "Value"];

    [Fact]
    public void DefinesTheCompleteCanonicalSafetySetInStableOrder()
    {
        Assert.Collection(
            CharacterExaminationSafety.All,
            safety => Assert.Same(CharacterExaminationSafety.SafeToCollect, safety),
            safety => Assert.Same(CharacterExaminationSafety.UnsafeToCollect, safety));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(ExpectedValues, CharacterExaminationSafety.All.Select(value => value.Value));

        foreach (CharacterExaminationSafety safety in CharacterExaminationSafety.All)
        {
            Assert.Equal(safety.Value, safety.ToString());
            Assert.Same(safety, CharacterExaminationSafety.Parse(safety.ToString()));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Safe-to-collect")]
    [InlineData("safe_to_collect")]
    [InlineData("unsafe--to-collect")]
    [InlineData("safe-to-collect ")]
    [InlineData("unknown")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => CharacterExaminationSafety.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => CharacterExaminationSafety.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedSafetySurface()
    {
        string[] publicProperties = typeof(CharacterExaminationSafety)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterExaminationSafety).GetConstructors());
    }
}

using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueDeliveryModeTests
{
    private static readonly string[] ExpectedValues = ["ordinary-speech", "whispering"];

    private static readonly string[] ExpectedPropertyNames = ["All", "OrdinarySpeech", "Value", "Whispering"];

    [Fact]
    public void DefinesTheCompleteCanonicalDeliveryModeSetInStableOrder()
    {
        Assert.Collection(
            DialogueDeliveryMode.All,
            mode => Assert.Same(DialogueDeliveryMode.OrdinarySpeech, mode),
            mode => Assert.Same(DialogueDeliveryMode.Whispering, mode));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        Assert.Equal(ExpectedValues, DialogueDeliveryMode.All.Select(mode => mode.Value));

        foreach (DialogueDeliveryMode mode in DialogueDeliveryMode.All)
        {
            Assert.Equal(mode.Value, mode.ToString());
            Assert.Same(mode, DialogueDeliveryMode.Parse(mode.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("ordinary_speech")]
    [InlineData("Ordinary-speech")]
    [InlineData("whisper")]
    [InlineData("whispering ")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => DialogueDeliveryMode.Parse(value));
    }

    [Fact]
    public void RejectsNullText()
    {
        Assert.Throws<ArgumentNullException>(() => DialogueDeliveryMode.Parse(null!));
    }

    [Fact]
    public void ExposesOnlyTheClosedDeliveryModeSurface()
    {
        string[] publicProperties = typeof(DialogueDeliveryMode)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(DialogueDeliveryMode).GetConstructors());
    }
}

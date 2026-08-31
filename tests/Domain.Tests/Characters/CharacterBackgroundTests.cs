using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBackgroundTests
{
    [Fact]
    public void PreservesPublicAndPrivateAuthoredTextExactly()
    {
        const string PublicText = "  Known as a village healer.\r\nTrusted by most neighbours.  ";
        const string PrivateText = "  Secretly doubts the accepted remedy.\n";

        PublicCharacterBackground publicBackground = new(PublicText);
        PrivateCharacterBackground privateBackground = new(PrivateText);

        Assert.Equal(PublicText, publicBackground.Text);
        Assert.Equal(PrivateText, privateBackground.Text);
    }

    [Fact]
    public void EmptyPublicAndPrivateBackgroundsAreExplicitlyValid()
    {
        PublicCharacterBackground publicBackground = new(string.Empty);
        PrivateCharacterBackground privateBackground = new(string.Empty);

        Assert.Equal(string.Empty, publicBackground.Text);
        Assert.Equal(string.Empty, privateBackground.Text);
    }

    [Fact]
    public void PublicAndPrivateBackgroundsHaveDistinctValueTypes()
    {
        object publicBackground = new PublicCharacterBackground("Authored text");
        object privateBackground = new PrivateCharacterBackground("Authored text");

        Assert.NotEqual(publicBackground, privateBackground);
    }

    [Fact]
    public void EqualityUsesAuthoredTextWithinEachBoundary()
    {
        Assert.Equal(
            new PublicCharacterBackground("Known history"),
            new PublicCharacterBackground("Known history"));
        Assert.NotEqual(
            new PublicCharacterBackground("Known history"),
            new PublicCharacterBackground("Different history"));
        Assert.Equal(
            new PrivateCharacterBackground("Private motive"),
            new PrivateCharacterBackground("Private motive"));
        Assert.NotEqual(
            new PrivateCharacterBackground("Private motive"),
            new PrivateCharacterBackground("Different motive"));
    }

    [Fact]
    public void AcceptsTheMaximumBackgroundLength()
    {
        string text = new('b', PublicCharacterBackground.MaximumLength);

        Assert.Equal(text, new PublicCharacterBackground(text).Text);
        Assert.Equal(text, new PrivateCharacterBackground(text).Text);
    }

    [Fact]
    public void RejectsOversizedBackgrounds()
    {
        string text = new('b', PublicCharacterBackground.MaximumLength + 1);

        Assert.Throws<ArgumentException>(() => new PublicCharacterBackground(text));
        Assert.Throws<ArgumentException>(() => new PrivateCharacterBackground(text));
    }

    [Fact]
    public void RejectsNullBackgrounds()
    {
        Assert.Throws<ArgumentNullException>(() => new PublicCharacterBackground(null!));
        Assert.Throws<ArgumentNullException>(() => new PrivateCharacterBackground(null!));
    }

    [Fact]
    public void PublicAndPrivateBackgroundsShareTheDocumentedBound()
    {
        Assert.Equal(
            PublicCharacterBackground.MaximumLength,
            PrivateCharacterBackground.MaximumLength);
    }
}

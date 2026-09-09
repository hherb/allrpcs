using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelTextDeltaTests
{
    [Fact]
    public void RetainsExactTextIncludingWhitespace()
    {
        string text = " \t healer response \r\n ";

        CharacterModelTextDelta delta = new(text);

        Assert.Same(text, delta.Text);
    }

    [Fact]
    public void AcceptsWhitespaceOnlyTextBetweenProviderTokens()
    {
        string text = " \t\r\n ";

        CharacterModelTextDelta delta = new(text);

        Assert.Same(text, delta.Text);
    }

    [Fact]
    public void AcceptsTheMaximumUtteranceLength()
    {
        string text = new('x', DialogueUtterance.MaximumTextLength);

        CharacterModelTextDelta delta = new(text);

        Assert.Same(text, delta.Text);
        Assert.Equal(DialogueUtterance.MaximumTextLength, delta.Text.Length);
    }

    [Fact]
    public void RejectsTextAboveTheMaximumUtteranceLength()
    {
        string text = new('x', DialogueUtterance.MaximumTextLength + 1);

        Assert.Throws<ArgumentException>(() => new CharacterModelTextDelta(text));
    }

    [Fact]
    public void RejectsNullAndEmptyText()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelTextDelta(null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelTextDelta(string.Empty));
    }

    [Fact]
    public void ExposesOnlyTheImmutableTextValue()
    {
        Type deltaType = typeof(CharacterModelTextDelta);

        Assert.True(deltaType.IsSealed);
        Assert.Equal(typeof(CharacterModelStreamUpdate), deltaType.BaseType);
        Assert.Single(deltaType.GetConstructors());
        Assert.Equal(
            [typeof(string)],
            deltaType.GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Collection(
            deltaType.GetProperties(),
            property =>
            {
                Assert.Equal(nameof(CharacterModelTextDelta.Text), property.Name);
                Assert.Equal(typeof(string), property.PropertyType);
                Assert.NotNull(property.GetMethod);
                Assert.Null(property.SetMethod);
            });
    }
}

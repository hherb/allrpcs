using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueUtteranceTests
{
    [Fact]
    public void RetainsExactSpeakerIdentifierAndUnicodeTextWithoutNormalization()
    {
        CharacterId speakerCharacterId = new("seeker");
        const string Text = "  Could you examine the mōonwort?\r\n不要忽略这句话。  ";

        DialogueUtterance utterance = new(speakerCharacterId, Text);

        Assert.Same(speakerCharacterId, utterance.SpeakerCharacterId);
        Assert.Equal(Text, utterance.Text);
    }

    [Fact]
    public void EqualityUsesSpeakerAndExactTextValues()
    {
        DialogueUtterance first = new(new CharacterId("seeker"), "Examine the flowers.");
        DialogueUtterance same = new(new CharacterId("seeker"), "Examine the flowers.");
        DialogueUtterance differentSpeaker = new(
            new CharacterId("healer"),
            "Examine the flowers.");
        DialogueUtterance differentText = new(
            new CharacterId("seeker"),
            "Examine the flowers first.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentSpeaker);
        Assert.NotEqual(first, differentText);
    }

    [Fact]
    public void AcceptsTextAtTheMaximumLength()
    {
        string text = new('語', DialogueUtterance.MaximumTextLength);

        DialogueUtterance utterance = new(new CharacterId("seeker"), text);

        Assert.Same(text, utterance.Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void RejectsEmptyOrWhitespaceOnlyText(string text)
    {
        Assert.Throws<ArgumentException>(
            () => new DialogueUtterance(new CharacterId("seeker"), text));
    }

    [Fact]
    public void RejectsOversizedOrNullInputs()
    {
        string oversized = new('a', DialogueUtterance.MaximumTextLength + 1);

        Assert.Throws<ArgumentException>(
            () => new DialogueUtterance(new CharacterId("seeker"), oversized));
        Assert.Throws<ArgumentNullException>(() => new DialogueUtterance(null!, "Hello."));
        Assert.Throws<ArgumentNullException>(
            () => new DialogueUtterance(new CharacterId("seeker"), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableUtteranceSurface()
    {
        Type utteranceType = typeof(DialogueUtterance);
        (string Name, Type Type)[] expectedProperties =
        [
            ("SpeakerCharacterId", typeof(CharacterId)),
            ("Text", typeof(string)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = utteranceType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            utteranceType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(string), parameter.ParameterType));
        Assert.Empty(utteranceType.GetEvents());
        Assert.True(utteranceType.IsSealed);
    }
}

using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class DialogueAudienceSnapshotTests
{
    [Fact]
    public void RetainsExactSpeakerAndStableListenerIdentifierInstances()
    {
        CharacterId speakerCharacterId = new("seeker");
        CharacterId healerCharacterId = new("healer");
        CharacterId hermitCharacterId = new("hermit");

        DialogueAudienceSnapshot audience = new(
            speakerCharacterId,
            [healerCharacterId, hermitCharacterId]);

        Assert.Same(speakerCharacterId, audience.SpeakerCharacterId);
        Assert.Collection(
            audience.ListenerCharacterIds,
            listener => Assert.Same(healerCharacterId, listener),
            listener => Assert.Same(hermitCharacterId, listener));
    }

    [Fact]
    public void EmptyListenerSetIsExplicitlyValid()
    {
        DialogueAudienceSnapshot audience = new(
            new CharacterId("seeker"),
            Array.Empty<CharacterId>());

        Assert.Empty(audience.ListenerCharacterIds);
    }

    [Fact]
    public void CopiesCallerCollectionAndExposesNoMutableCollection()
    {
        CharacterId original = new("healer");
        CharacterId[] source = [original];
        DialogueAudienceSnapshot audience = new(new CharacterId("seeker"), source);

        source[0] = new CharacterId("hermit");
        IList<CharacterId> exposed = Assert.IsAssignableFrom<IList<CharacterId>>(
            audience.ListenerCharacterIds);

        Assert.Same(original, Assert.Single(audience.ListenerCharacterIds));
        Assert.True(exposed.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => exposed.Add(new CharacterId("sage")));
    }

    [Fact]
    public void AcceptsListenerSetAtTheBound()
    {
        CharacterId[] listeners = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => new CharacterId($"listener-{index}"))
            .ToArray();

        DialogueAudienceSnapshot audience = new(new CharacterId("seeker"), listeners);

        Assert.Equal(listeners, audience.ListenerCharacterIds);
    }

    [Fact]
    public void RejectsListenerSetAboveTheBound()
    {
        CharacterId[] listeners = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount + 1)
            .Select(index => new CharacterId($"listener-{index}"))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DialogueAudienceSnapshot(new CharacterId("seeker"), listeners));
    }

    [Fact]
    public void RejectsDuplicateOrSpeakerListenerIdentifiersByValue()
    {
        Assert.Throws<ArgumentException>(() => new DialogueAudienceSnapshot(
            new CharacterId("seeker"),
            [new CharacterId("healer"), new CharacterId("healer")]));
        Assert.Throws<ArgumentException>(() => new DialogueAudienceSnapshot(
            new CharacterId("seeker"),
            [new CharacterId("seeker")]));
    }

    [Fact]
    public void RejectsNullInputsAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DialogueAudienceSnapshot(null!, Array.Empty<CharacterId>()));
        Assert.Throws<ArgumentNullException>(
            () => new DialogueAudienceSnapshot(new CharacterId("seeker"), null!));
        Assert.Throws<ArgumentException>(() => new DialogueAudienceSnapshot(
            new CharacterId("seeker"),
            [null!]));
    }

    [Fact]
    public void ExposesOnlyTheImmutableAudienceSurface()
    {
        Type audienceType = typeof(DialogueAudienceSnapshot);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ListenerCharacterIds", typeof(IReadOnlyList<CharacterId>)),
            ("SpeakerCharacterId", typeof(CharacterId)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = audienceType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(
            audienceType.GetMethods().Where(method =>
                method.DeclaringType == audienceType && !method.IsSpecialName));
        Assert.Empty(audienceType.GetEvents());
        Assert.True(audienceType.IsSealed);
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemorySnapshotTests
{
    [Fact]
    public void PreservesExactAggregatesAndCopiesStableOrder()
    {
        CharacterMemoryAggregate first = Empty("healer");
        CharacterMemoryAggregate second = Empty("sage");
        CharacterMemoryAggregate[] aggregates = [first, second];

        CharacterMemorySnapshot snapshot = new(aggregates);
        aggregates[0] = second;

        Assert.Collection(
            snapshot.Characters,
            aggregate => Assert.Same(first, aggregate),
            aggregate => Assert.Same(second, aggregate));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryAggregate>)snapshot.Characters).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterMemoryAggregate[] maximum = Enumerable
            .Range(0, CharacterMemorySnapshot.MaximumCharacterCount)
            .Select(index => Empty($"character-{index}"))
            .ToArray();

        Assert.Empty(new CharacterMemorySnapshot([]).Characters);
        Assert.Equal(
            CharacterMemorySnapshot.MaximumCharacterCount,
            new CharacterMemorySnapshot(maximum).Characters.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterMemorySnapshot(
            maximum.Append(Empty("extra"))));
    }

    [Fact]
    public void RejectsDuplicateCharacterIdentityNullEntriesAndNullCollection()
    {
        CharacterMemoryAggregate first = Empty("healer");
        CharacterMemoryAggregate duplicate = Empty("healer");

        Assert.Throws<ArgumentException>(() => new CharacterMemorySnapshot([first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterMemorySnapshot([null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemorySnapshot(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterMemorySnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());
        string[] publicMethods = snapshotType
            .GetMethods()
            .Where(method => method.DeclaringType == snapshotType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal("Characters", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterMemoryAggregate>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterMemoryAggregate>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    internal static CharacterMemoryAggregate Empty(string characterId)
    {
        CharacterId identity = new(characterId);
        return new CharacterMemoryAggregate(
            identity,
            new CharacterRecentConversationWindow(new CharacterId(characterId), []),
            new CharacterConversationSummarySnapshot(new CharacterId(characterId), []),
            new CharacterEpisodicMemorySnapshot(new CharacterId(characterId), []),
            new CharacterEpistemicMemorySnapshot(new CharacterId(characterId), []),
            new CharacterBeliefAssessmentSnapshot(new CharacterId(characterId), []),
            new CharacterRelationshipMemorySnapshot(new CharacterId(characterId), []),
            new CharacterCommitmentMemorySnapshot(new CharacterId(characterId), []),
            new CharacterMemoryIndex(new CharacterId(characterId), []));
    }
}

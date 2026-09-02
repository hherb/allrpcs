using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Perception;

public sealed class CharacterPerceptionSnapshotTests
{
    [Fact]
    public void RetainsCharacterIdentifierExactly()
    {
        CharacterId characterId = new("travelling-healer");

        CharacterPerceptionSnapshot snapshot = new(
            characterId,
            Array.Empty<WorldObjectVisibility>());

        Assert.Same(characterId, snapshot.CharacterId);
    }

    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        CharacterPerceptionSnapshot snapshot = new(
            new CharacterId("travelling-healer"),
            Array.Empty<WorldObjectVisibility>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesEntryOrderAndInstances()
    {
        WorldObjectVisibility first = Create("visible-herb", VisibilityState.Noticed);
        WorldObjectVisibility second = Create("concealed-herb", VisibilityState.Unnoticed);

        CharacterPerceptionSnapshot snapshot = new(
            new CharacterId("travelling-healer"),
            new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        WorldObjectVisibility original = Create("visible-herb", VisibilityState.Noticed);
        WorldObjectVisibility replacement = Create("concealed-herb", VisibilityState.Unnoticed);
        WorldObjectVisibility[] source = [original];

        CharacterPerceptionSnapshot snapshot = new(
            new CharacterId("travelling-healer"),
            source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateObjectIdentifiersEvenWhenStatesDiffer()
    {
        WorldObjectVisibility first = Create("visible-herb", VisibilityState.Noticed);
        WorldObjectVisibility duplicate = Create("visible-herb", VisibilityState.Understood);

        Assert.Throws<ArgumentException>(
            () => new CharacterPerceptionSnapshot(
                new CharacterId("travelling-healer"),
                new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCharacterIdentifierCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CharacterPerceptionSnapshot(
                null!,
                Array.Empty<WorldObjectVisibility>()));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterPerceptionSnapshot(
                new CharacterId("travelling-healer"),
                null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterPerceptionSnapshot(
                new CharacterId("travelling-healer"),
                new WorldObjectVisibility[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        WorldObjectVisibility entry = Create("visible-herb", VisibilityState.Noticed);
        CharacterPerceptionSnapshot snapshot = new(
            new CharacterId("travelling-healer"),
            new[] { entry });
        Type snapshotType = typeof(CharacterPerceptionSnapshot);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CharacterId", typeof(CharacterId)),
            ("Entries", typeof(IReadOnlyList<WorldObjectVisibility>)),
        ];

        Assert.Throws<NotSupportedException>(
            () => ((IList<WorldObjectVisibility>)snapshot.Entries).Add(entry));

        (string Name, Type Type, bool CanWrite)[] actualProperties = snapshotType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = snapshotType
            .GetMethods()
            .Where(method => method.DeclaringType == snapshotType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            snapshotType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(IEnumerable<WorldObjectVisibility>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static WorldObjectVisibility Create(string objectId, VisibilityState state) =>
        new(new ObjectId(objectId), state);
}

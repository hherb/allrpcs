using System.Collections;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageActionConsiderationRequestTests
{
    [Fact]
    public void RetainsExactActorAndStableCallerOrderedActionKinds()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionKind[] supplied =
        [
            CharacterSabotageActionKind.Delay,
            CharacterSabotageActionKind.Conceal,
        ];

        CharacterSabotageActionConsiderationRequest request = new(actor, supplied);
        ListIdentityContractTests.AssertIdentity(request);

        supplied[0] = CharacterSabotageActionKind.Influence;
        Assert.Same(actor, request.Actor);
        Assert.Collection(
            request.ActionKinds,
            kind => Assert.Same(CharacterSabotageActionKind.Delay, kind),
            kind => Assert.Same(CharacterSabotageActionKind.Conceal, kind));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterSabotageActionKind>>(request.ActionKinds);
    }

    [Fact]
    public void AcceptsEmptyAndEveryCanonicalKindWithOneEnumeration()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        SingleUseEnumerable<CharacterSabotageActionKind> onePass = new(
            CharacterSabotageActionKind.All);

        CharacterSabotageActionConsiderationRequest empty = new(actor, []);
        CharacterSabotageActionConsiderationRequest full = new(actor, onePass);

        Assert.Empty(empty.ActionKinds);
        Assert.Equal(CharacterSabotageActionKind.All, full.ActionKinds);
        Assert.Equal(1, onePass.EnumerationCount);
    }

    [Fact]
    public void RejectsNullAndDuplicateActionKinds()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionConsiderationRequest(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionConsiderationRequest(actor, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterSabotageActionConsiderationRequest(actor, [null!]));
        Assert.Throws<ArgumentException>(() => new CharacterSabotageActionConsiderationRequest(
            actor,
            [CharacterSabotageActionKind.Move, CharacterSabotageActionKind.Move]));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralRequestSurface()
    {
        Type type = typeof(CharacterSabotageActionConsiderationRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ActionKinds", typeof(IReadOnlyList<CharacterSabotageActionKind>)),
            ("Actor", typeof(Character)),
        ];
        System.Reflection.PropertyInfo[] properties = type.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(type.GetConstructors());
        Assert.Empty(type.GetEvents());
        Assert.True(type.IsSealed);
    }

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException("The source was enumerated more than once.");
            }

            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

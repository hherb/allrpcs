using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageActionCandidateBindingTests
{
    [Fact]
    public void RetainsExactSingletonRequestKindAndTypedCandidate()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Move]);
        object candidate = new();

        CharacterSabotageActionCandidateBinding<object> binding = new(
            request,
            CharacterSabotageActionKind.Move,
            candidate);

        Assert.Same(request, binding.Request);
        Assert.Same(CharacterSabotageActionKind.Move, binding.ActionKind);
        Assert.Same(candidate, binding.Candidate);
    }

    [Fact]
    public void RejectsNullOrNonExactSingletonInputs()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest move = new(
            actor,
            [CharacterSabotageActionKind.Move]);
        CharacterSabotageActionConsiderationRequest empty = new(actor, []);
        CharacterSabotageActionConsiderationRequest multiple = new(
            actor,
            [CharacterSabotageActionKind.Move, CharacterSabotageActionKind.Conceal]);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(
                null!, CharacterSabotageActionKind.Move, new object()));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(move, null!, new object()));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(
                move, CharacterSabotageActionKind.Move, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(
                empty, CharacterSabotageActionKind.Move, new object()));
        Assert.Throws<ArgumentException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(
                multiple, CharacterSabotageActionKind.Move, new object()));
        Assert.Throws<ArgumentException>(() =>
            new CharacterSabotageActionCandidateBinding<object>(
                move, CharacterSabotageActionKind.Conceal, new object()));
    }

    [Fact]
    public void ExposesOnlyImmutableProviderNeutralBindingSurface()
    {
        Type baseType = typeof(CharacterSabotageActionCandidateBinding);
        Type bindingType = typeof(CharacterSabotageActionCandidateBinding<object>);

        Assert.Equal(
            [
                ("ActionKind", typeof(CharacterSabotageActionKind)),
                ("Request", typeof(CharacterSabotageActionConsiderationRequest)),
            ],
            baseType.GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType))
                .ToArray());
        Assert.All(baseType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.True(baseType.IsAbstract);
        Assert.Empty(baseType.GetEvents());

        System.Reflection.PropertyInfo candidate = Assert.Single(
            bindingType.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        Assert.Equal("Candidate", candidate.Name);
        Assert.Equal(typeof(object), candidate.PropertyType);
        Assert.False(candidate.CanWrite);
        Assert.True(bindingType.IsSealed);
        Assert.Single(bindingType.GetConstructors());
        Assert.Empty(bindingType.GetEvents());
    }
}

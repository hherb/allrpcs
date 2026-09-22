using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageActionDecisionTests
{
    [Fact]
    public void AcceptsExactActorMemberSelectionAndAbstention()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Delay, CharacterSabotageActionKind.Conceal]);
        CharacterSabotageActionDecision selection = new(
            actor.Identity.Id,
            CharacterSabotageActionKind.Conceal);
        CharacterSabotageActionDecision abstention = new(actor.Identity.Id, null);

        Assert.True(CharacterSabotageActionDecisionPolicy.CanAccept(request, selection));
        Assert.True(CharacterSabotageActionDecisionPolicy.CanAccept(request, abstention));
        Assert.Same(actor.Identity.Id, selection.ActorId);
        Assert.Same(CharacterSabotageActionKind.Conceal, selection.SelectedActionKind);
        Assert.Null(abstention.SelectedActionKind);
    }

    [Fact]
    public void RejectsValueEqualActorAndUnavailableSelection()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Delay]);

        Assert.False(CharacterSabotageActionDecisionPolicy.CanAccept(
            request,
            new CharacterSabotageActionDecision(
                new CharacterId(actor.Identity.Id.Value),
                CharacterSabotageActionKind.Delay)));
        Assert.False(CharacterSabotageActionDecisionPolicy.CanAccept(
            request,
            new CharacterSabotageActionDecision(
                actor.Identity.Id,
                CharacterSabotageActionKind.Influence)));
        Assert.Same(CharacterSabotageActionKind.Delay, Assert.Single(request.ActionKinds));
    }

    [Fact]
    public void ResultRetainsOnlyAnAcceptedExactDecision()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(
            actor,
            [CharacterSabotageActionKind.Misquote]);
        CharacterSabotageActionDecision selection = new(
            actor.Identity.Id,
            CharacterSabotageActionKind.Misquote);
        CharacterSabotageActionDecision abstention = new(actor.Identity.Id, null);

        CharacterSabotageActionConsiderationResult selected = new(request, selection);
        CharacterSabotageActionConsiderationResult abstained = new(request, abstention);

        Assert.Same(request, selected.Request);
        Assert.Same(selection, selected.Decision);
        Assert.Same(CharacterSabotageActionKind.Misquote, selected.SelectedActionKind);
        Assert.Same(request, abstained.Request);
        Assert.Same(abstention, abstained.Decision);
        Assert.Null(abstained.SelectedActionKind);
        Assert.Throws<ArgumentException>(() => new CharacterSabotageActionConsiderationResult(
            request,
            new CharacterSabotageActionDecision(
                actor.Identity.Id,
                CharacterSabotageActionKind.Move)));
    }

    [Fact]
    public void ContractsRejectNullsAndExposeClosedReadOnlySurfaces()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest request = new(actor, []);
        CharacterSabotageActionDecision decision = new(actor.Identity.Id, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionDecision(null!, null));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageActionDecisionPolicy.CanAccept(request, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionConsiderationResult(request, null!));

        AssertReadOnlySurface(
            typeof(CharacterSabotageActionDecision),
            [
                ("ActorId", typeof(CharacterId)),
                ("SelectedActionKind", typeof(CharacterSabotageActionKind)),
            ]);
        AssertReadOnlySurface(
            typeof(CharacterSabotageActionConsiderationResult),
            [
                ("Decision", typeof(CharacterSabotageActionDecision)),
                ("Request", typeof(CharacterSabotageActionConsiderationRequest)),
                ("SelectedActionKind", typeof(CharacterSabotageActionKind)),
            ]);

        Type policyType = typeof(CharacterSabotageActionDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterSabotageActionConsiderationRequest), typeof(CharacterSabotageActionDecision)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static void AssertReadOnlySurface(
        Type type,
        IReadOnlyList<(string Name, Type Type)> expectedProperties)
    {
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
}

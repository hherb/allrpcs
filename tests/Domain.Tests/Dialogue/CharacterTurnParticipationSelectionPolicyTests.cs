using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationSelectionPolicyTests
{
    [Theory]
    [InlineData("respond")]
    [InlineData("interject")]
    public void SelectsTheFirstSpeakingDecisionInScheduledOrder(string kindValue)
    {
        CharacterTurnParticipationDecision silent = Decision(
            "woodland-hermit",
            CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationDecision speaking = Decision(
            "village-sage",
            CharacterTurnParticipationKind.Parse(kindValue));
        CharacterTurnParticipationDecision laterSpeaking = Decision(
            "travelling-healer",
            CharacterTurnParticipationKind.Respond);

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationSelectionPolicy.SelectNext(
                [silent, speaking, laterSpeaking]);

        Assert.Same(speaking, selected);
    }

    [Fact]
    public void ReturnsNoSelectionWhenEveryScheduledCharacterMeaningfullyRemainsSilent()
    {
        CharacterTurnParticipationDecision first = Decision(
            "woodland-hermit",
            CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationDecision second = Decision(
            "village-sage",
            CharacterTurnParticipationKind.RemainSilent);

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationSelectionPolicy.SelectNext([first, second]);

        Assert.Null(selected);
        Assert.Null(CharacterTurnParticipationSelectionPolicy.SelectNext([]));
    }

    [Fact]
    public void RetainsCallerOrderAndExactDecisionInstances()
    {
        CharacterTurnParticipationDecision first = Decision(
            "woodland-hermit",
            CharacterTurnParticipationKind.Interject);
        CharacterTurnParticipationDecision second = Decision(
            "village-sage",
            CharacterTurnParticipationKind.Interject);
        CharacterTurnParticipationDecision[] decisions = [first, second];

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationSelectionPolicy.SelectNext(decisions);

        Assert.Same(first, selected);
        Assert.Same(second, decisions[1]);
    }

    [Fact]
    public void RejectsNullDuplicateOrOversizedDecisionInputs()
    {
        CharacterTurnParticipationDecision duplicate = Decision(
            "woodland-hermit",
            CharacterTurnParticipationKind.Interject);
        CharacterTurnParticipationDecision[] maximum = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => Decision(
                $"speaker-{index}",
                CharacterTurnParticipationKind.Interject))
            .ToArray();

        Assert.NotNull(CharacterTurnParticipationSelectionPolicy.SelectNext(maximum));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationSelectionPolicy.SelectNext(null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationSelectionPolicy.SelectNext([null!]));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationSelectionPolicy.SelectNext(
                [duplicate, Decision("woodland-hermit", CharacterTurnParticipationKind.Respond)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationSelectionPolicy.SelectNext(
                maximum.Append(Decision(
                    "extra-speaker",
                    CharacterTurnParticipationKind.Interject))));
    }

    [Fact]
    public void ExposesOnlyThePureSelectionOperation()
    {
        Type policyType = typeof(CharacterTurnParticipationSelectionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("SelectNext", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnParticipationDecision), method.ReturnType);
        Assert.Equal(
            [typeof(IEnumerable<CharacterTurnParticipationDecision>)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterTurnParticipationDecision Decision(
        string characterId,
        CharacterTurnParticipationKind kind)
    {
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : new CharacterGoal($"motive-{characterId}", "Participate for an authored motive.");
        return new CharacterTurnParticipationDecision(new CharacterId(characterId), kind, motive);
    }
}

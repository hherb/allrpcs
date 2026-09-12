using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationParticipationPolicyTests
{
    [Fact]
    public void ReturnsTheExactValidatedSpeakerWhileTheBudgetCanContinue()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterTurnParticipationDecision response = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);

        CharacterTurnParticipationDecision? selected =
            AutonomousConversationParticipationPolicy.SelectNext(
                new AutonomousConversationBudget(0, 2, false),
                [sage],
                [response],
                [new CharacterId(sage.Identity.Id.Value)]);

        Assert.Same(response, selected);
    }

    [Theory]
    [InlineData(2, 2, false)]
    [InlineData(0, 2, true)]
    public void ReturnsNoSpeakerWhenTheBudgetCannotContinue(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterTurnParticipationDecision interjection = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Interject,
            Assert.Single(sage.Goals.Entries));
        Character[] characters = [sage];
        CharacterTurnParticipationDecision[] decisions = [interjection];
        CharacterId[] addresses = [];

        CharacterTurnParticipationDecision? selected =
            AutonomousConversationParticipationPolicy.SelectNext(
                new AutonomousConversationBudget(
                    completedTurnCount,
                    maximumTurnCount,
                    hasPlayerInterrupted),
                characters,
                decisions,
                addresses);

        Assert.Null(selected);
        Assert.Same(sage, characters[0]);
        Assert.Same(interjection, decisions[0]);
        Assert.Empty(addresses);
    }

    [Fact]
    public void ReturnsNoSpeakerWhenTheValidatedRoundIsMeaningfullySilent()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterTurnParticipationDecision silence = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.RemainSilent,
            Assert.Single(sage.Goals.Entries));

        Assert.Null(AutonomousConversationParticipationPolicy.SelectNext(
            new AutonomousConversationBudget(0, 2, false),
            [sage],
            [silence],
            []));
        Assert.Null(AutonomousConversationParticipationPolicy.SelectNext(
            new AutonomousConversationBudget(0, 2, false),
            [],
            [],
            []));
    }

    [Fact]
    public void ValidatesTheCompleteRoundEvenWhenTheBudgetIsClosed()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterTurnParticipationDecision invalidResponse = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);

        Assert.Throws<ArgumentException>(() =>
            AutonomousConversationParticipationPolicy.SelectNext(
                new AutonomousConversationBudget(1, 1, false),
                [sage],
                [invalidResponse],
                []));
    }

    [Fact]
    public void RejectsNullBudgetBeforeInspectingRoundInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationParticipationPolicy.SelectNext(
                null!,
                null!,
                null!,
                null!));
    }

    [Fact]
    public void ExposesOnlyThePureBudgetedSelectionOperation()
    {
        Type policyType = typeof(AutonomousConversationParticipationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("SelectNext", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnParticipationDecision), method.ReturnType);
        Assert.Equal(
            [
                typeof(AutonomousConversationBudget),
                typeof(IEnumerable<Character>),
                typeof(IEnumerable<CharacterTurnParticipationDecision>),
                typeof(IEnumerable<CharacterId>),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}

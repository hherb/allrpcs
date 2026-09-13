using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationConsiderationSelectionPolicyTests
{
    [Fact]
    public void ReturnsTheExactFirstSpeakingResultWhileTheBudgetCanContinue()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id, sage.Identity.Id);
        CharacterTurnParticipationConsiderationResult silent = Result(
            new CharacterTurnParticipationConsiderationRequest(hermit, heardEvent, false),
            CharacterTurnParticipationKind.RemainSilent);
        CharacterTurnParticipationConsiderationResult interjection = Result(
            new CharacterTurnParticipationConsiderationRequest(sage, heardEvent, false),
            CharacterTurnParticipationKind.Interject);
        CharacterTurnParticipationConsiderationRoundResult round = new(
            [silent.Request, interjection.Request],
            [silent, interjection]);

        CharacterTurnParticipationConsiderationResult? selected =
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(
                new AutonomousConversationBudget(0, 2, false),
                round);

        Assert.NotNull(selected);
        Assert.Same(interjection, selected);
        Assert.Same(interjection.Decision, selected.Decision);
        Assert.Same(sage.Identity.Id, selected.Decision.ScheduledCharacterId);
    }

    [Fact]
    public void RetainsStableFirstSpeakerOrderForResponsesAndInterjections()
    {
        Character sage = CreateCharacter("village-sage");
        Character hermit = CreateCharacter("woodland-hermit");
        DialogueEvent heardEvent = CreateEvent(sage.Identity.Id, hermit.Identity.Id);
        CharacterTurnParticipationConsiderationResult response = Result(
            new CharacterTurnParticipationConsiderationRequest(sage, heardEvent, true),
            CharacterTurnParticipationKind.Respond);
        CharacterTurnParticipationConsiderationResult interjection = Result(
            new CharacterTurnParticipationConsiderationRequest(hermit, heardEvent, false),
            CharacterTurnParticipationKind.Interject);
        CharacterTurnParticipationConsiderationRoundResult round = new(
            [response.Request, interjection.Request],
            [response, interjection]);

        CharacterTurnParticipationConsiderationResult? selected =
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(
                new AutonomousConversationBudget(1, 3, false),
                round);

        Assert.NotNull(selected);
        Assert.Same(response, selected);
        Assert.Same(response.Decision, selected.Decision);
    }

    [Fact]
    public void ReturnsNoSelectionForEmptyOrAllSilentRounds()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        DialogueEvent heardEvent = CreateEvent(hermit.Identity.Id);
        CharacterTurnParticipationConsiderationResult silent = Result(
            new CharacterTurnParticipationConsiderationRequest(hermit, heardEvent, false),
            CharacterTurnParticipationKind.RemainSilent);
        AutonomousConversationBudget budget = new(0, 2, false);

        Assert.Null(AutonomousConversationConsiderationSelectionPolicy.SelectNext(
            budget,
            new CharacterTurnParticipationConsiderationRoundResult([], [])));
        Assert.Null(AutonomousConversationConsiderationSelectionPolicy.SelectNext(
            budget,
            new CharacterTurnParticipationConsiderationRoundResult(
                [silent.Request],
                [silent])));
    }

    [Theory]
    [InlineData(2, 2, false)]
    [InlineData(0, 2, true)]
    public void ReturnsNoSelectionWhenTheBudgetCannotContinueWithoutChangingInputs(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        Character sage = CreateCharacter("village-sage");
        DialogueEvent heardEvent = CreateEvent(sage.Identity.Id);
        CharacterTurnParticipationConsiderationResult response = Result(
            new CharacterTurnParticipationConsiderationRequest(sage, heardEvent, true),
            CharacterTurnParticipationKind.Respond);
        CharacterTurnParticipationConsiderationRoundResult round = new(
            [response.Request],
            [response]);
        AutonomousConversationBudget budget = new(
            completedTurnCount,
            maximumTurnCount,
            hasPlayerInterrupted);

        CharacterTurnParticipationConsiderationResult? selected =
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(budget, round);

        Assert.Null(selected);
        Assert.Equal(completedTurnCount, budget.CompletedTurnCount);
        Assert.Equal(maximumTurnCount, budget.MaximumTurnCount);
        Assert.Equal(hasPlayerInterrupted, budget.HasPlayerInterrupted);
        Assert.Same(response.Request, Assert.Single(round.Requests));
        Assert.Same(response, Assert.Single(round.Results));
        Assert.Same(response.Decision, Assert.Single(round.Results).Decision);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureSelectionOperation()
    {
        AutonomousConversationBudget budget = new(0, 2, false);
        CharacterTurnParticipationConsiderationRoundResult round = new([], []);

        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(null!, round));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationConsiderationSelectionPolicy.SelectNext(budget, null!));

        Type policyType = typeof(AutonomousConversationConsiderationSelectionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("SelectNext", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnParticipationConsiderationResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(AutonomousConversationBudget),
                typeof(CharacterTurnParticipationConsiderationRoundResult),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterTurnParticipationConsiderationResult Result(
        CharacterTurnParticipationConsiderationRequest request,
        CharacterTurnParticipationKind kind)
    {
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : Assert.Single(request.ScheduledCharacter.Goals.Entries);

        return new CharacterTurnParticipationConsiderationResult(
            request,
            new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                kind,
                motive));
    }

    private static DialogueEvent CreateEvent(params CharacterId[] listeners)
    {
        CharacterId speakerCharacterId = new("seeker");

        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerCharacterId, listeners));
    }

    private static Character CreateCharacter(string id)
    {
        CharacterId characterId = new(id);

        return new Character(
            new CharacterIdentity(characterId, id.ToUpperInvariant()),
            new PublicCharacterBackground("A scheduled participant."),
            new PrivateCharacterBackground("A private scheduled participant."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [new CharacterGoal($"goal-{id}", "Participate for an authored reason.")]));
    }
}

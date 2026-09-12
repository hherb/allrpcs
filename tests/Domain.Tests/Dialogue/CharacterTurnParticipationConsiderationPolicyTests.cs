using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationPolicyTests
{
    [Theory]
    [InlineData("respond", true)]
    [InlineData("interject", false)]
    [InlineData("remain-silent", true)]
    [InlineData("remain-silent", false)]
    public void AcceptsAChoiceAllowedByTheExactConsiderationRequest(
        string kindValue,
        bool isDirectlyAddressed)
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(
            character,
            isDirectlyAddressed);
        CharacterTurnParticipationDecision decision = Decision(character, kindValue);

        bool accepted = CharacterTurnParticipationConsiderationPolicy.CanAccept(
            request,
            decision);

        Assert.True(accepted);
        Assert.Same(character, request.ScheduledCharacter);
        Assert.Same(character.Identity.Id, decision.ScheduledCharacterId);
    }

    [Theory]
    [InlineData("respond", false)]
    [InlineData("interject", true)]
    public void RejectsAChoiceIncompatibleWithDirectAddress(
        string kindValue,
        bool isDirectlyAddressed)
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(
            character,
            isDirectlyAddressed);
        CharacterTurnParticipationDecision decision = Decision(character, kindValue);

        Assert.False(CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision));
    }

    [Fact]
    public void RejectsADistinctValueEqualDecisionIdentifier()
    {
        Character character = CreateCharacter("woodland-hermit");
        CharacterTurnParticipationConsiderationRequest request = Request(character, false);
        CharacterTurnParticipationDecision decision = new(
            new CharacterId(character.Identity.Id.Value),
            CharacterTurnParticipationKind.Interject,
            Assert.Single(character.Goals.Entries));

        Assert.False(CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision));
        Assert.Equal(character.Identity.Id, decision.ScheduledCharacterId);
        Assert.NotSame(character.Identity.Id, decision.ScheduledCharacterId);
    }

    [Fact]
    public void RejectsAMotiveOutsideTheExactScheduledCharacter()
    {
        Character character = CreateCharacter("woodland-hermit");
        Character otherCharacter = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(character, false);
        CharacterGoal otherMotive = Assert.Single(otherCharacter.Goals.Entries);
        CharacterTurnParticipationDecision decision = new(
            character.Identity.Id,
            CharacterTurnParticipationKind.Interject,
            otherMotive);

        Assert.False(CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision));
        Assert.Same(otherMotive, decision.Motive);
        Assert.Same(character, request.ScheduledCharacter);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(character, true);
        CharacterTurnParticipationDecision decision = Decision(character, "respond");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationConsiderationPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationConsiderationPolicy.CanAccept(request, null!));
    }

    [Fact]
    public void ExposesOnlyThePureAcceptanceOperation()
    {
        Type policyType = typeof(CharacterTurnParticipationConsiderationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTurnParticipationConsiderationRequest),
                typeof(CharacterTurnParticipationDecision),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterTurnParticipationConsiderationRequest Request(
        Character character,
        bool isDirectlyAddressed)
    {
        CharacterId speakerCharacterId = new("seeker");
        DialogueEvent heardEvent = new(
            new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
            new DialogueAudienceSnapshot(speakerCharacterId, [character.Identity.Id]));

        return new(character, heardEvent, isDirectlyAddressed);
    }

    private static CharacterTurnParticipationDecision Decision(
        Character character,
        string kindValue)
    {
        CharacterTurnParticipationKind kind = CharacterTurnParticipationKind.Parse(kindValue);
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : Assert.Single(character.Goals.Entries);

        return new(character.Identity.Id, kind, motive);
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

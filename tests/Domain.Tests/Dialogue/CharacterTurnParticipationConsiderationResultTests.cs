using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationConsiderationResultTests
{
    [Theory]
    [InlineData("respond", true)]
    [InlineData("interject", false)]
    [InlineData("remain-silent", true)]
    [InlineData("remain-silent", false)]
    public void RetainsAnExactAcceptedRequestAndDecision(
        string kindValue,
        bool isDirectlyAddressed)
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(
            character,
            isDirectlyAddressed);
        CharacterTurnParticipationDecision decision = Decision(character, kindValue);

        CharacterTurnParticipationConsiderationResult result = new(request, decision);

        Assert.Same(request, result.Request);
        Assert.Same(decision, result.Decision);
    }

    [Theory]
    [InlineData("respond", false)]
    [InlineData("interject", true)]
    public void RejectsAChoiceIncompatibleWithTheExactRequest(
        string kindValue,
        bool isDirectlyAddressed)
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(
            character,
            isDirectlyAddressed);
        CharacterTurnParticipationDecision decision = Decision(character, kindValue);

        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationResult(request, decision));
        Assert.Same(character, request.ScheduledCharacter);
        Assert.Same(character.Identity.Id, decision.ScheduledCharacterId);
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

        Assert.Throws<ArgumentException>(() =>
            new CharacterTurnParticipationConsiderationResult(request, decision));
        Assert.NotSame(character.Identity.Id, decision.ScheduledCharacterId);
        Assert.Equal(character.Identity.Id, decision.ScheduledCharacterId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Character character = CreateCharacter("village-sage");
        CharacterTurnParticipationConsiderationRequest request = Request(character, true);
        CharacterTurnParticipationDecision decision = Decision(character, "respond");

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnParticipationConsiderationResult(request, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResultSurface()
    {
        Type resultType = typeof(CharacterTurnParticipationConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterTurnParticipationConsiderationRequest),
                typeof(CharacterTurnParticipationDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterTurnParticipationDecision)),
                ("Request", typeof(CharacterTurnParticipationConsiderationRequest)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
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

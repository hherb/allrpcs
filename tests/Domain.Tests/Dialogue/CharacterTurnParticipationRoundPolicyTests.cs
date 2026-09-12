using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationRoundPolicyTests
{
    [Fact]
    public void SelectsTheExactFirstAcceptedResponseUsingValueEqualDirectAddressMembership()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnParticipationDecision silence = SilentDecision(hermit);
        CharacterTurnParticipationDecision response = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit, sage],
                [silence, response],
                [new CharacterId(sage.Identity.Id.Value)]);

        Assert.Same(response, selected);
    }

    [Fact]
    public void SelectsTheExactFirstAcceptedInterjection()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnParticipationDecision interjection = InterjectionDecision(hermit);
        CharacterTurnParticipationDecision laterInterjection = InterjectionDecision(sage);

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit, sage],
                [interjection, laterInterjection],
                []);

        Assert.Same(interjection, selected);
    }

    [Fact]
    public void ReturnsNoSelectionForAnEmptyOrAllSilentAcceptedRound()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");

        Assert.Null(CharacterTurnParticipationRoundPolicy.SelectNext([], [], []));
        Assert.Null(CharacterTurnParticipationRoundPolicy.SelectNext(
            [hermit, sage],
            [SilentDecision(hermit), SilentDecision(sage)],
            []));
    }

    [Fact]
    public void RequiresOneExactlyAlignedDecisionForEveryScheduledCharacter()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnParticipationDecision hermitDecision = SilentDecision(hermit);
        CharacterTurnParticipationDecision sageDecision = SilentDecision(sage);
        CharacterTurnParticipationDecision replacementIdentityDecision = new(
            new CharacterId(hermit.Identity.Id.Value),
            CharacterTurnParticipationKind.RemainSilent,
            Assert.Single(hermit.Goals.Entries));

        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit, sage],
                [hermitDecision],
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit],
                [hermitDecision, sageDecision],
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit, sage],
                [sageDecision, hermitDecision],
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit],
                [replacementIdentityDecision],
                []));
    }

    [Fact]
    public void ValidatesLateDecisionsBeforeSelectingAnEarlierSpeaker()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnParticipationDecision response = new(
            hermit.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);
        CharacterTurnParticipationDecision invalidLateResponse = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);

        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [hermit, sage],
                [response, invalidLateResponse],
                [hermit.Identity.Id]));
    }

    [Fact]
    public void RejectsNullDuplicateOrOversizedInputs()
    {
        Character character = CreateCharacter("speaker");
        Character otherCharacter = CreateCharacter("other-speaker");
        CharacterTurnParticipationDecision decision = SilentDecision(character);
        Character[] maximumCharacters = Enumerable
            .Range(0, DialogueAudienceSnapshot.MaximumListenerCount)
            .Select(index => CreateCharacter($"speaker-{index}"))
            .ToArray();
        CharacterTurnParticipationDecision[] maximumDecisions = maximumCharacters
            .Select(SilentDecision)
            .ToArray();
        CharacterId[] maximumAddresses = maximumCharacters
            .Select(candidate => candidate.Identity.Id)
            .ToArray();

        Assert.Null(CharacterTurnParticipationRoundPolicy.SelectNext(
            maximumCharacters,
            maximumDecisions,
            maximumAddresses));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(null!, [], []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext([], null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext([], [], null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext([null!], [decision], []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext([character], [null!], []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext([], [], [null!]));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [character, character],
                [decision, decision],
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [character, otherCharacter],
                [decision, decision],
                []));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                [character],
                [decision],
                [character.Identity.Id, new CharacterId(character.Identity.Id.Value)]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                maximumCharacters.Append(CreateCharacter("extra-speaker")),
                maximumDecisions,
                maximumAddresses));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                maximumCharacters,
                maximumDecisions.Append(SilentDecision(CreateCharacter("extra-decision"))),
                maximumAddresses));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterTurnParticipationRoundPolicy.SelectNext(
                maximumCharacters,
                maximumDecisions,
                maximumAddresses.Append(new CharacterId("extra-address"))));
    }

    [Fact]
    public void LeavesInputsUnchangedAndRetainsTheirExactInstances()
    {
        Character hermit = CreateCharacter("woodland-hermit");
        Character sage = CreateCharacter("village-sage");
        CharacterTurnParticipationDecision silence = SilentDecision(hermit);
        CharacterTurnParticipationDecision interjection = InterjectionDecision(sage);
        Character[] characters = [hermit, sage];
        CharacterTurnParticipationDecision[] decisions = [silence, interjection];
        CharacterId address = new("absent-character");
        CharacterId[] addresses = [address];

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationRoundPolicy.SelectNext(
                characters,
                decisions,
                addresses);

        Assert.Same(interjection, selected);
        Assert.Same(hermit, characters[0]);
        Assert.Same(sage, characters[1]);
        Assert.Same(silence, decisions[0]);
        Assert.Same(interjection, decisions[1]);
        Assert.Same(address, addresses[0]);
    }

    [Fact]
    public void ExposesOnlyThePureRoundSelectionOperation()
    {
        Type policyType = typeof(CharacterTurnParticipationRoundPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("SelectNext", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnParticipationDecision), method.ReturnType);
        Assert.Equal(
            [
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

    private static CharacterTurnParticipationDecision SilentDecision(Character character) =>
        new(
            character.Identity.Id,
            CharacterTurnParticipationKind.RemainSilent,
            Assert.Single(character.Goals.Entries));

    private static CharacterTurnParticipationDecision InterjectionDecision(Character character) =>
        new(
            character.Identity.Id,
            CharacterTurnParticipationKind.Interject,
            Assert.Single(character.Goals.Entries));

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

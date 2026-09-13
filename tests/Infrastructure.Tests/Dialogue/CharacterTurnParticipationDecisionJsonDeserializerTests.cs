using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterTurnParticipationDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnExactDirectResponse()
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(
            isDirectlyAddressed: true);

        CharacterTurnParticipationDecision decision =
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(
                request,
                """{"kind":"respond","motiveKey":null}""");

        Assert.Same(request.ScheduledCharacter.Identity.Id, decision.ScheduledCharacterId);
        Assert.Same(CharacterTurnParticipationKind.Respond, decision.Kind);
        Assert.Null(decision.Motive);
        Assert.True(CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision));
    }

    [Theory]
    [InlineData("interject")]
    [InlineData("remain-silent")]
    public void RetainsTheExactAuthoredMotiveForUndirectedChoices(string kind)
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(
            isDirectlyAddressed: false);
        CharacterGoal motive = Assert.Single(request.ScheduledCharacter.Goals.Entries);

        CharacterTurnParticipationDecision decision =
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(
                request,
                $$"""{"kind":"{{kind}}","motiveKey":"{{motive.Key}}"}""");

        Assert.Same(request.ScheduledCharacter.Identity.Id, decision.ScheduledCharacterId);
        Assert.Same(CharacterTurnParticipationKind.Parse(kind), decision.Kind);
        Assert.Same(motive, decision.Motive);
        Assert.True(CharacterTurnParticipationConsiderationPolicy.CanAccept(request, decision));
    }

    [Theory]
    [InlineData(true, "interject", "compare-evidence")]
    [InlineData(false, "respond", null)]
    [InlineData(true, "respond", "compare-evidence")]
    [InlineData(false, "remain-silent", null)]
    [InlineData(false, "interject", "absent-goal")]
    public void RejectsChoicesOutsideTheExactConsideration(
        bool isDirectlyAddressed,
        string kind,
        string? motiveKey)
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(
            isDirectlyAddressed);
        string motive = motiveKey is null ? "null" : $"\"{motiveKey}\"";
        string document = $$"""{"kind":"{{kind}}","motiveKey":{{motive}}}""";

        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(request, document));
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        """{"kind":"respond","motiveKey":null} trailing""",
        """{/*comment*/"kind":"respond","motiveKey":null}""",
        """{"kind":"respond","motiveKey":null,"unknown":null}""",
        """{"kind":"respond","kind":"respond","motiveKey":null}""",
        """{"kind":"respond"}""",
        """{"kind":"RESPOND","motiveKey":null}""",
        """{"kind":5,"motiveKey":null}""",
        """{"kind":"respond","motiveKey":5}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedUnknownDuplicateMissingAndInvalidValues(string document)
    {
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(
                CreateRequest(isDirectlyAddressed: true),
                document));
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        CharacterTurnParticipationConsiderationRequest request = CreateRequest(
            isDirectlyAddressed: true);
        const string document = """{"kind":"respond","motiveKey":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(request, null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationDecisionJsonDeserializer.Deserialize(
                request,
                new string(
                    ' ',
                    CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength + 1)));
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterTurnParticipationDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(
                candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(
            deserializerType.GetFields(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnParticipationDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTurnParticipationConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumDocumentLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(deserializerType.GetConstructors());
        Assert.Empty(deserializerType.GetProperties());
        Assert.Empty(deserializerType.GetEvents());
        Assert.True(deserializerType.IsAbstract && deserializerType.IsSealed);
    }

    private static CharacterTurnParticipationConsiderationRequest CreateRequest(
        bool isDirectlyAddressed)
    {
        Character character = CreateCharacter();
        CharacterId speakerCharacterId = new("seeker");
        DialogueEvent heardEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(speakerCharacterId, "What do you make of this evidence?"),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerCharacterId, [character.Identity.Id]));

        return new CharacterTurnParticipationConsiderationRequest(
            character,
            heardEvent,
            isDirectlyAddressed);
    }

    private static Character CreateCharacter()
    {
        CharacterId characterId = new("village-sage");

        return new Character(
            new CharacterIdentity(characterId, "Village Sage"),
            new PublicCharacterBackground("An evidence-comparison consultant."),
            new PrivateCharacterBackground("Challenges unsupported assumptions."),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Llm),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet(
                [new CharacterGoal("compare-evidence", "Compare the available evidence.")]));
    }
}

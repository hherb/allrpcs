using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromValueMatchingActorTextAndIndex()
    {
        Input input = CreateInput();
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);

        CharacterTargetSabotageInfluenceDecision decision =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedCandidateIndex":0}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(input.Candidate, decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterTargetSabotageInfluenceDecision decision =
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageInfluenceDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "0")]
    [InlineData("enemy-agent", "-1")]
    [InlineData("enemy-agent", "1")]
    [InlineData("INVALID", "0")]
    public void RejectsInvalidActorAndSelectionValuesWithOneBoundedError(
        string actorId,
        string selectedCandidateIndex)
    {
        Input input = CreateInput();
        string document =
            $$"""{"actorId":"{{actorId}}","selectedCandidateIndex":{{selectedCandidateIndex}}}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(input.Request, document));

        Assert.Equal("serialized", exception.ParamName);
        Assert.DoesNotContain(actorId, exception.Message, StringComparison.Ordinal);
        AssertRequestUnchanged(input);
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        """{"actorId":"enemy-agent","selectedCandidateIndex":null} trailing""",
        """{/*comment*/"actorId":"enemy-agent","selectedCandidateIndex":null}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":null,"unknown":null}""",
        """{"actorId":"enemy-agent","actorId":"enemy-agent","selectedCandidateIndex":null}""",
        """{"actorId":"enemy-agent"}""",
        """{"selectedCandidateIndex":null}""",
        """{"actorId":5,"selectedCandidateIndex":null}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":"0"}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":0.0}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":2147483648}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":{}}""",
        """{"actorId":"enemy-agent","selectedCandidateIndex":[[[[null]]]]}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedTrailingOverDepthUnknownDuplicateMissingAndWrongTypeInput(string document)
    {
        Input input = CreateInput();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(input.Request, document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        Input input = CreateInput();
        const string document = """{"actorId":"enemy-agent","selectedCandidateIndex":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(input.Request, null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterTargetSabotageInfluenceDecisionJsonDeserializer.MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterTargetSabotageInfluenceDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(deserializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageInfluenceConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumDocumentLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(deserializerType.GetConstructors());
        Assert.Empty(deserializerType.GetProperties());
        Assert.Empty(deserializerType.GetEvents());
        Assert.True(deserializerType.IsAbstract && deserializerType.IsSealed);
    }

    private static void AssertRequestUnchanged(Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.InteractionAccess, input.Request.InteractionAccess);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.Change, input.Candidate.RelationshipChange);
    }

    internal static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            target);
        CharacterRelationshipChange change = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("influence-attempt"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("enemy-agent-influence"),
                "The target accepted the influence."));
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            actor,
            interactionAccess,
            relationship,
            [candidate]);

        return new(actor, interactionAccess, relationship, occurrence, change, candidate, request);
    }

    internal sealed record Input(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageOccurrence Occurrence,
        CharacterRelationshipChange Change,
        CharacterTargetSabotageInfluenceCandidate Candidate,
        CharacterTargetSabotageInfluenceConsiderationRequest Request);
}

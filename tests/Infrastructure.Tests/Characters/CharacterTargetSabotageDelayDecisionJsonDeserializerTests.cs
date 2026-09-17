using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageDelayDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromValueMatchingActorTextAndIndex()
    {
        Input input = CreateInput();
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);

        CharacterTargetSabotageDelayDecision decision =
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedCandidateIndex":0}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(input.Candidate, decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(input.Request, decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterTargetSabotageDelayDecision decision =
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageDelayDecisionPolicy.CanAccept(input.Request, decision));
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
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(input.Request, document));

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
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(input.Request, document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        Input input = CreateInput();
        const string document = """{"actorId":"enemy-agent","selectedCandidateIndex":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(input.Request, null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterTargetSabotageDelayDecisionJsonDeserializer.MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(CharacterTargetSabotageDelayDecisionJsonDeserializer);
        System.Reflection.MethodInfo method = Assert.Single(
            deserializerType.GetMethods().Where(candidate => candidate.DeclaringType == deserializerType));
        System.Reflection.FieldInfo field = Assert.Single(deserializerType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal("Deserialize", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTargetSabotageDelayDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayConsiderationRequest), typeof(string)],
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
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.WorldEvent, input.Candidate.WorldEvent);
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
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent worldEvent = new(
            new WorldEventId("enemy-agent-delayed-sage"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayCandidate candidate = new(occurrence, worldEvent);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTargetSabotageDelayConsiderationRequest request = new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            [candidate]);

        return new(
            actor,
            interactionAccess,
            worldEvents,
            eventLog,
            worldTime,
            occurrence,
            worldEvent,
            candidate,
            request);
    }

    internal sealed record Input(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTargetSabotageOccurrence Occurrence,
        WorldEvent WorldEvent,
        CharacterTargetSabotageDelayCandidate Candidate,
        CharacterTargetSabotageDelayConsiderationRequest Request);
}

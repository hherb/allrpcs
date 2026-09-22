using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnExactSelectionAmongDistinctEquivalentCandidateState()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create(2);
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);

        CharacterTargetSabotageDelayRecoveryDecision decision =
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedCandidateIndex":1}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.NotSame(input.Candidates[0], input.Candidates[1]);
        Assert.Same(input.DelayResult, input.Candidates[1].DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidates[1].RecoveryEvent);
        Assert.Same(input.Candidates[1], decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();

        CharacterTargetSabotageDelayRecoveryDecision decision =
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesTheLastExactCandidateAtTheMaximumBoundary()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create(
                CharacterTargetSabotageDelayRecoverySelection.MaximumCandidateCount);
        int selectedIndex = input.Candidates.Count - 1;

        CharacterTargetSabotageDelayRecoveryDecision decision =
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":{{selectedIndex}}}""");

        Assert.Same(input.Candidates[selectedIndex], decision.SelectedCandidate);
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "0")]
    [InlineData("sage", "-1")]
    [InlineData("sage", "1")]
    [InlineData("INVALID", "0")]
    public void RejectsInvalidActorAndSelectionValuesWithOneBoundedError(
        string actorId,
        string selectedCandidateIndex)
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        string document =
            $$"""{"actorId":"{{actorId}}","selectedCandidateIndex":{{selectedCandidateIndex}}}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        Assert.DoesNotContain(actorId, exception.ToString(), StringComparison.Ordinal);
        AssertRequestUnchanged(input);
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        """{"actorId":"sage","selectedCandidateIndex":null} trailing""",
        """{/*comment*/"actorId":"sage","selectedCandidateIndex":null}""",
        """{"actorId":"sage","selectedCandidateIndex":null,"unknown":null}""",
        """{"actorId":"sage","actorId":"sage","selectedCandidateIndex":null}""",
        """{"actorId":"sage"}""",
        """{"selectedCandidateIndex":null}""",
        """{"actorId":5,"selectedCandidateIndex":null}""",
        """{"actorId":"sage","selectedCandidateIndex":"0"}""",
        """{"actorId":"sage","selectedCandidateIndex":0.0}""",
        """{"actorId":"sage","selectedCandidateIndex":2147483648}""",
        """{"actorId":"sage","selectedCandidateIndex":{}}""",
        """{"actorId":"sage","selectedCandidateIndex":[[[[null]]]]}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedTrailingOverDepthUnknownDuplicateMissingAndWrongTypeInput(
        string document)
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsPolicyInvalidWrongOwnerAndStaleRequests()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        Character wrongOwner = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageDelayRecoveryConsiderationRequest wrongOwnerRequest = new(
            wrongOwner,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.Candidates);
        WorldEventLogSnapshot staleEventLog = new(input.EventLog.Entries);
        CharacterTargetSabotageDelayRecoveryConsiderationRequest staleRequest = new(
            input.Actor,
            input.WorldEvents,
            staleEventLog,
            input.WorldTime,
            input.Candidates);

        Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                wrongOwnerRequest,
                $$"""{"actorId":"{{wrongOwner.Identity.Id}}","selectedCandidateIndex":0}"""));
        Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                staleRequest,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":0}"""));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        const string document = """{"actorId":"sage","selectedCandidateIndex":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(null!, document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer
                        .MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(
            CharacterTargetSabotageDelayRecoveryDecisionJsonDeserializer);
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
        Assert.Equal(typeof(CharacterTargetSabotageDelayRecoveryDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumDocumentLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(deserializerType.GetConstructors());
        Assert.Empty(deserializerType.GetProperties());
        Assert.Empty(deserializerType.GetEvents());
        Assert.True(deserializerType.IsAbstract && deserializerType.IsSealed);
    }

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.WorldEvents, input.Request.WorldEvents);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.WorldTime, input.Request.WorldTime);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.Same(input.DelayResult, input.Candidates[0].DelayResult);
        Assert.Same(input.RecoveryEvent, input.Candidates[0].RecoveryEvent);
        Assert.Same(input.DelayEvent.EventId, input.EventLog.Entries[^1]);
        Assert.Same(input.DelayEvent.EventId, input.WorldTime.CurrentEventId);
    }
}

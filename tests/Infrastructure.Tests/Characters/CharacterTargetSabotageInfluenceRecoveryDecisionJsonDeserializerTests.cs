using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnExactSelectionAmongDistinctEquivalentCandidateState()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create(2);
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);

        CharacterTargetSabotageInfluenceRecoveryDecision decision =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedCandidateIndex":1}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.NotSame(input.Candidates[0], input.Candidates[1]);
        Assert.Same(input.Candidates[0].InfluenceResult, input.Candidates[1].InfluenceResult);
        Assert.Same(input.Candidates[0].RecoveryChange, input.Candidates[1].RecoveryChange);
        Assert.Same(input.Candidates[1], decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();

        CharacterTargetSabotageInfluenceRecoveryDecision decision =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedCandidate);
        Assert.True(CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesTheLastExactCandidateAtTheMaximumBoundary()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create(
                CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount);
        int selectedIndex = input.Candidates.Count - 1;

        CharacterTargetSabotageInfluenceRecoveryDecision decision =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
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
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        string document =
            $$"""{"actorId":"{{actorId}}","selectedCandidateIndex":{{selectedCandidateIndex}}}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
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
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsPolicyInvalidWrongOwnerAndStaleRequests()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        Character wrongOwner = RepresentativeHermitCharacterSource.Create()[0];
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest wrongOwnerRequest = new(
            wrongOwner,
            input.Relationship,
            input.Candidates);
        CharacterRelationshipMemory staleRelationship = new(
            input.Relationship.CharacterId,
            input.Relationship.RelatedCharacterId,
            input.Relationship.Dimensions,
            []);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest staleRequest = new(
            input.Actor,
            staleRelationship,
            input.Candidates);

        Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                wrongOwnerRequest,
                $$"""{"actorId":"{{wrongOwner.Identity.Id}}","selectedCandidateIndex":0}"""));
        Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                staleRequest,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedCandidateIndex":0}"""));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        const string document = """{"actorId":"sage","selectedCandidateIndex":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                null!,
                document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer
                        .MaximumDocumentLength + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonDeserializer);
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
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceRecoveryDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest), typeof(string)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal("MaximumDocumentLength", field.Name);
        Assert.True(field.IsLiteral);
        Assert.Empty(deserializerType.GetConstructors());
        Assert.Empty(deserializerType.GetProperties());
        Assert.Empty(deserializerType.GetEvents());
        Assert.True(deserializerType.IsAbstract && deserializerType.IsSealed);
    }

    private static void AssertRequestUnchanged(
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input)
    {
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Equal(input.Candidates, input.Request.Candidates);
        Assert.Same(input.InfluenceResult, input.Candidates[0].InfluenceResult);
        Assert.Same(input.RecoveryChange, input.Candidates[0].RecoveryChange);
        Assert.Same(input.InfluenceChange.Reason, input.Relationship.Reasons.Single());
        Assert.Equal(11, input.Relationship.Dimensions.Single().Value);
    }
}

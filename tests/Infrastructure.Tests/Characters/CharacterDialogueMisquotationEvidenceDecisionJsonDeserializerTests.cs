using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceDecisionJsonDeserializerTests
{
    [Fact]
    public void MaterializesAnAcceptedExactSelectionFromValueMatchingActorTextAndIndex()
    {
        Input input = CreateInput();
        CharacterId distinctActorId = CharacterId.Parse(input.Actor.Identity.Id.Value);

        CharacterDialogueMisquotationEvidenceDecision decision =
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{distinctActorId}}","selectedEvidenceIndex":0}""");

        Assert.NotSame(distinctActorId, decision.ActorId);
        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(input.Evidence, decision.SelectedEvidence);
        Assert.True(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void MaterializesAnAcceptedExactAbstention()
    {
        Input input = CreateInput();

        CharacterDialogueMisquotationEvidenceDecision decision =
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                $$"""{"actorId":"{{input.Actor.Identity.Id}}","selectedEvidenceIndex":null}""");

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Null(decision.SelectedEvidence);
        Assert.True(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            decision));
        AssertRequestUnchanged(input);
    }

    [Theory]
    [InlineData("other-character", "0")]
    [InlineData("enemy-agent", "-1")]
    [InlineData("enemy-agent", "1")]
    [InlineData("INVALID", "0")]
    public void RejectsInvalidActorAndSelectionValuesWithOneBoundedError(
        string actorId,
        string selectedEvidenceIndex)
    {
        Input input = CreateInput();
        string document =
            $$"""{"actorId":"{{actorId}}","selectedEvidenceIndex":{{selectedEvidenceIndex}}}""";

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

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
        """{"actorId":"enemy-agent","selectedEvidenceIndex":null} trailing""",
        """{/*comment*/"actorId":"enemy-agent","selectedEvidenceIndex":null}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":null,"unknown":null}""",
        """{"actorId":"enemy-agent","actorId":"enemy-agent","selectedEvidenceIndex":null}""",
        """{"actorId":"enemy-agent"}""",
        """{"selectedEvidenceIndex":null}""",
        """{"actorId":5,"selectedEvidenceIndex":null}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":"0"}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":0.0}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":2147483648}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":{}}""",
        """{"actorId":"enemy-agent","selectedEvidenceIndex":[[[[null]]]]}""",
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedTrailingOverDepthUnknownDuplicateMissingAndWrongTypeInput(
        string document)
    {
        Input input = CreateInput();

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                document));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void RejectsNullsAndDocumentsBeyondTheTransportBoundary()
    {
        Input input = CreateInput();
        const string document =
            """{"actorId":"enemy-agent","selectedEvidenceIndex":null}""";

        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                null!,
                document));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                null!));
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.Deserialize(
                input.Request,
                new string(
                    ' ',
                    CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer.MaximumDocumentLength
                        + 1)));

        Assert.Equal("serialized", exception.ParamName);
        AssertRequestUnchanged(input);
    }

    [Fact]
    public void ExposesOnlyTheStrictDeserializationOperationAndBound()
    {
        Type deserializerType = typeof(
            CharacterDialogueMisquotationEvidenceDecisionJsonDeserializer);
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
        Assert.Equal(typeof(CharacterDialogueMisquotationEvidenceDecision), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest), typeof(string)],
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
        Assert.Same(input.Evidence, Assert.Single(input.Request.Candidates));
        Assert.Same(input.SourceEvent, input.Evidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.SourceAudience, input.SourceEvent.Audience);
        Assert.Same(input.DeliveredEvent, input.Evidence.Occurrence.DeliveredEvent);
        Assert.Same(input.DeliveredAudience, input.DeliveredEvent.Audience);
    }

    internal static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterId sourceSpeakerId = new("sage");
        DialogueAudienceSnapshot sourceAudience = new(
            sourceSpeakerId,
            [actor.Identity.Id, witnessId]);
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            sourceAudience);
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        DialogueAudienceSnapshot deliveredAudience = new(
            proposal.ActorId,
            [witnessId]);
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            deliveredAudience);
        CharacterDialogueMisquotationEvidence evidence = new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            actor,
            [evidence]);

        return new(
            actor,
            sourceAudience,
            sourceEvent,
            deliveredAudience,
            deliveredEvent,
            evidence,
            request);
    }

    internal sealed record Input(
        Character Actor,
        DialogueAudienceSnapshot SourceAudience,
        DialogueEvent SourceEvent,
        DialogueAudienceSnapshot DeliveredAudience,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}

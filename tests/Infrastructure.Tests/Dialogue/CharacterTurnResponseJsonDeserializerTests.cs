using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterTurnResponseJsonDeserializerTests
{
    private const string CompleteDocument = """
        {
          "disposition":"conditionally-accept",
          "speech":"I will inspect it before collecting it.",
          "immediateIntention":{"type":"examine-object","targetObjectId":"herb-patch"},
          "commitments":[
            {"proposedIntentions":[
              {"type":"collect-object","targetObjectId":"herb-patch"},
              {"type":"wait","targetObjectId":null}
            ]}
          ],
          "stateProposals":[{"key":"herb-safety","value":"Inspection is required."}]
        }
        """;

    [Fact]
    public void MaterializesACompleteProviderNeutralResponseForTheExactRequest()
    {
        CharacterTurnRequest request = CreateRequest();

        CharacterTurnResponse response = CharacterTurnResponseJsonDeserializer.Deserialize(
            request,
            CompleteDocument);

        Assert.Same(request, response.Request);
        Assert.Same(CharacterTurnDisposition.ConditionallyAccept, response.Disposition);
        Assert.Equal(request.RespondingCharacterId, response.Speech?.SpeakerCharacterId);
        Assert.Equal("I will inspect it before collecting it.", response.Speech?.Text);
        Assert.Same(CharacterIntentionType.ExamineObject, response.ImmediateIntention.Type);
        Assert.Equal(new ObjectId("herb-patch"), response.ImmediateIntention.TargetObjectId);
        CharacterCommitment commitment = Assert.Single(response.Commitments);
        Assert.Collection(
            commitment.ProposedIntentions,
            intention => Assert.Same(CharacterIntentionType.CollectObject, intention.Type),
            intention => Assert.Same(CharacterIntentionType.Wait, intention.Type));
        CharacterStateProposal proposal = Assert.Single(response.StateProposals);
        Assert.Equal("herb-safety", proposal.Key);
        Assert.Equal("Inspection is required.", proposal.Value);
    }

    [Fact]
    public void AcceptsNullSpeechAndEmptyBoundedCollections()
    {
        CharacterTurnRequest request = CreateRequest();
        const string document = """
            {
              "disposition":"clarify",
              "speech":null,
              "immediateIntention":{"type":"wait","targetObjectId":null},
              "commitments":[],
              "stateProposals":[]
            }
            """;

        CharacterTurnResponse response = CharacterTurnResponseJsonDeserializer.Deserialize(
            request,
            document);

        Assert.Null(response.Speech);
        Assert.Empty(response.Commitments);
        Assert.Empty(response.StateProposals);
    }

    public static TheoryData<string> InvalidDocuments => new()
    {
        string.Empty,
        "null",
        "[]",
        "not-json",
        CompleteDocument + " trailing",
        CompleteDocument.Replace("{", "{/*comment*/", StringComparison.Ordinal),
        CompleteDocument.Replace("\"stateProposals\":", "\"unknown\":[],\"stateProposals\":", StringComparison.Ordinal),
        CompleteDocument.Replace("\"speech\":", "\"speech\":null,\"speech\":", StringComparison.Ordinal),
        CompleteDocument.Replace(
            "\"targetObjectId\":\"herb-patch\"",
            "\"unknown\":null,\"targetObjectId\":\"herb-patch\"",
            StringComparison.Ordinal),
        CompleteDocument.Replace(
            "\"key\":\"herb-safety\"",
            "\"key\":\"herb-safety\",\"key\":\"herb-safety\"",
            StringComparison.Ordinal),
        CompleteDocument.Replace(
            "\"speech\":\"I will inspect it before collecting it.\",",
            string.Empty,
            StringComparison.Ordinal),
        CompleteDocument.Replace(
            "\"disposition\":\"conditionally-accept\"",
            "\"disposition\":\"ACCEPT\"",
            StringComparison.Ordinal),
        CompleteDocument.Replace("\"type\":\"examine-object\"", "\"type\":\"teleport\"", StringComparison.Ordinal),
        CompleteDocument.Replace("\"targetObjectId\":\"herb-patch\"", "\"targetObjectId\":5", StringComparison.Ordinal),
        CompleteDocument.Replace("\"key\":\"herb-safety\"", "\"key\":\"Not Canonical\"", StringComparison.Ordinal),
    };

    [Theory]
    [MemberData(nameof(InvalidDocuments))]
    public void RejectsMalformedUnknownDuplicateMissingAndInvalidValues(string document)
    {
        Assert.Throws<ArgumentException>(() => CharacterTurnResponseJsonDeserializer.Deserialize(
            CreateRequest(),
            document));
    }

    [Theory]
    [InlineData("wait", "herb-patch")]
    [InlineData("examine-object", null)]
    [InlineData("collect-object", null)]
    public void RejectsStructurallyInvalidIntentions(string type, string? targetObjectId)
    {
        string target = targetObjectId is null ? "null" : $"\"{targetObjectId}\"";
        string document = $$"""
            {
              "disposition":"accept",
              "speech":null,
              "immediateIntention":{"type":"{{type}}","targetObjectId":{{target}}},
              "commitments":[],
              "stateProposals":[]
            }
            """;

        Assert.Throws<ArgumentException>(() => CharacterTurnResponseJsonDeserializer.Deserialize(
            CreateRequest(),
            document));
    }

    [Fact]
    public void RejectsNullInputsAndDocumentsBeyondTheTransportBoundary()
    {
        CharacterTurnRequest request = CreateRequest();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseJsonDeserializer.Deserialize(null!, CompleteDocument));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseJsonDeserializer.Deserialize(request, null!));
        Assert.Throws<ArgumentException>(() => CharacterTurnResponseJsonDeserializer.Deserialize(
            request,
            new string(' ', CharacterTurnResponseJsonDeserializer.MaximumDocumentLength + 1)));
    }

    [Fact]
    public void RejectsExcessiveCommitmentsAndIntentions()
    {
        string intention = "{\"type\":\"wait\",\"targetObjectId\":null}";
        string excessiveIntentions = string.Join(
            ',',
            Enumerable.Repeat(
                intention,
                CharacterTurnResponseJsonDeserializer.MaximumIntentionsPerCommitment + 1));
        string excessiveCommitments = string.Join(
            ',',
            Enumerable.Repeat(
                $"{{\"proposedIntentions\":[{intention}]}}",
                CharacterTurnResponseJsonDeserializer.MaximumCommitmentCount + 1));

        Assert.Throws<ArgumentException>(() => CharacterTurnResponseJsonDeserializer.Deserialize(
            CreateRequest(),
            DocumentWith($"[{{\"proposedIntentions\":[{excessiveIntentions}]}}]")));
        Assert.Throws<ArgumentException>(() => CharacterTurnResponseJsonDeserializer.Deserialize(
            CreateRequest(),
            DocumentWith($"[{excessiveCommitments}]")));
    }

    private static string DocumentWith(string commitments) => $$"""
        {
          "disposition":"accept",
          "speech":null,
          "immediateIntention":{"type":"wait","targetObjectId":null},
          "commitments":{{commitments}},
          "stateProposals":[]
        }
        """;

    private static CharacterTurnRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        DialogueUtterance utterance = new(seekerId, "Please inspect the herb.");
        DialogueEvent dialogueEvent = new(
            utterance,
            new DialogueAudienceSnapshot(seekerId, [new CharacterId("healer")]));
        return new CharacterTurnRequest(
            new CharacterId("healer"),
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
    }
}

using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnResponseTests
{
    [Fact]
    public void RetainsExactRequestDispositionSpeechAndIntentionInstances()
    {
        CharacterTurnRequest request = CreateRequest();
        DialogueUtterance speech = new(
            request.RespondingCharacterId,
            "I will examine the flowers first.");
        CharacterIntention intention = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort"));
        CharacterCommitment commitment = new(
            [new CharacterIntention(CharacterIntentionType.Wait, null)]);
        CharacterStateProposal proposal = new(
            "moonwort-safety-belief",
            "Moonwort requires inspection.");

        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            speech,
            intention,
            [commitment],
            [proposal]);

        Assert.Same(request, response.Request);
        Assert.Same(CharacterTurnDisposition.Accept, response.Disposition);
        Assert.Same(speech, response.Speech);
        Assert.Same(intention, response.ImmediateIntention);
        Assert.Same(commitment, Assert.Single(response.Commitments));
        Assert.Same(proposal, Assert.Single(response.StateProposals));
    }

    [Fact]
    public void AcceptsAnAbsentSpokenResponse()
    {
        CharacterTurnRequest request = CreateRequest();
        CharacterIntention intention = new(CharacterIntentionType.Wait, null);

        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Clarify,
            null,
            intention,
            [],
            []);

        Assert.Null(response.Speech);
        Assert.Same(intention, response.ImmediateIntention);
    }

    [Fact]
    public void AcceptsSpeechFromADistinctValueEqualResponderIdentifier()
    {
        CharacterTurnRequest request = CreateRequest();
        DialogueUtterance speech = new(
            new CharacterId(request.RespondingCharacterId.Value),
            "Please identify which flowers you mean.");

        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Clarify,
            speech,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);

        Assert.Same(speech, response.Speech);
    }

    [Fact]
    public void RejectsSpeechFromAnotherCharacter()
    {
        CharacterTurnRequest request = CreateRequest();
        DialogueUtterance speech = new(
            new CharacterId("seeker"),
            "I will examine the flowers.");

        Assert.Throws<ArgumentException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            speech,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterTurnRequest request = CreateRequest();
        CharacterIntention intention = new(CharacterIntentionType.Wait, null);

        Assert.Throws<ArgumentNullException>(() => new CharacterTurnResponse(
            null!,
            CharacterTurnDisposition.Accept,
            null,
            intention,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnResponse(
            request,
            null!,
            null,
            intention,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            null!,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            intention,
            null!,
            []));
        Assert.Throws<ArgumentException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            intention,
            [null!],
            []));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            intention,
            [],
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            intention,
            [],
            [null!]));
    }

    [Fact]
    public void CopiesTheCallerCommitmentCollection()
    {
        CharacterTurnRequest request = CreateRequest();
        CharacterCommitment first = new(
            [new CharacterIntention(CharacterIntentionType.Wait, null)]);
        CharacterCommitment replacement = new(
            [new CharacterIntention(CharacterIntentionType.Wait, null)]);
        List<CharacterCommitment> commitments = [first];
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            commitments,
            []);

        commitments[0] = replacement;
        commitments.Add(replacement);

        Assert.Single(response.Commitments);
        Assert.Same(first, response.Commitments[0]);
    }

    [Fact]
    public void CopiesAndBoundsTheCallerStateProposalCollection()
    {
        CharacterTurnRequest request = CreateRequest();
        CharacterStateProposal first = new("belief-1", "First.");
        CharacterStateProposal replacement = new("belief-2", "Second.");
        List<CharacterStateProposal> proposals = [first];
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            proposals);

        proposals[0] = replacement;
        proposals.Add(replacement);

        Assert.Single(response.StateProposals);
        Assert.Same(first, response.StateProposals[0]);

        CharacterStateProposal[] maximum = Enumerable.Range(
                0,
                CharacterTurnResponse.MaximumStateProposalCount)
            .Select(index => new CharacterStateProposal($"belief-{index}", "Value."))
            .ToArray();
        CharacterTurnResponse maximumResponse = new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            maximum);

        Assert.Equal(CharacterTurnResponse.MaximumStateProposalCount, maximumResponse.StateProposals.Count);

        Assert.Throws<ArgumentException>(() => new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            maximum.Append(new CharacterStateProposal("overflow", "Value."))));
    }

    [Fact]
    public void ExposesOnlyTheProviderNeutralResponseCore()
    {
        Type responseType = typeof(CharacterTurnResponse);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Commitments", typeof(IReadOnlyList<CharacterCommitment>)),
            ("Disposition", typeof(CharacterTurnDisposition)),
            ("ImmediateIntention", typeof(CharacterIntention)),
            ("Request", typeof(CharacterTurnRequest)),
            ("Speech", typeof(DialogueUtterance)),
            ("StateProposals", typeof(IReadOnlyList<CharacterStateProposal>)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = responseType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(actualProperties, property =>
            property.Name.Contains("Provider", StringComparison.Ordinal)
            || property.Name.Contains("Model", StringComparison.Ordinal));
        Assert.Empty(responseType.GetEvents());
        Assert.True(responseType.IsSealed);
    }

    private static CharacterTurnRequest CreateRequest()
    {
        DialogueUtterance utterance = new(
            new CharacterId("seeker"),
            "Please examine the flowers first.");
        CharacterId healerId = new("healer");

        return new CharacterTurnRequest(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(
                    utterance.SpeakerCharacterId,
                    [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
    }
}

using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnActivityTests
{
    [Fact]
    public void RetainsAwaitingResponseStateWithoutAResponse()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");

        CharacterTurnActivity activity = new(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);

        Assert.Same(request, activity.Request);
        Assert.Null(activity.Response);
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, activity.State);
    }

    [Theory]
    [InlineData("speaking")]
    [InlineData("completed")]
    public void RetainsResponseForSpeakingAndCompletedStates(string stateValue)
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnResponse response = CreateResponse(request);
        CharacterTurnActivityState state = CharacterTurnActivityState.Parse(stateValue);

        CharacterTurnActivity activity = new(request, response, state);

        Assert.Same(request, activity.Request);
        Assert.Same(response, activity.Response);
        Assert.Same(state, activity.State);
    }

    [Fact]
    public void CancelledStateAcceptsCancellationBeforeOrAfterAResponse()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnResponse response = CreateResponse(request);

        CharacterTurnActivity beforeResponse = new(
            request,
            null,
            CharacterTurnActivityState.Cancelled);
        CharacterTurnActivity afterResponse = new(
            request,
            response,
            CharacterTurnActivityState.Cancelled);

        Assert.Null(beforeResponse.Response);
        Assert.Same(response, afterResponse.Response);
    }

    [Fact]
    public void RejectsInvalidResponseShapes()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnResponse response = CreateResponse(request);

        Assert.Throws<ArgumentException>(() => new CharacterTurnActivity(
            request,
            response,
            CharacterTurnActivityState.AwaitingResponse));
        Assert.Throws<ArgumentException>(() => new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.Speaking));
        Assert.Throws<ArgumentException>(() => new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.Completed));
    }

    [Fact]
    public void RejectsAResponseForAnotherRequest()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnResponse otherResponse = CreateResponse(CreateRequest("Please follow."));

        Assert.Throws<ArgumentException>(() => new CharacterTurnActivity(
            request,
            otherResponse,
            CharacterTurnActivityState.Speaking));
    }

    [Fact]
    public void RejectsAResponseWithADistinctStructurallyMatchingRequest()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnRequest equalRequest = new(
            request.RespondingCharacterId,
            request.DialogueEvent,
            request.DialogueAct);
        CharacterTurnResponse response = CreateResponse(equalRequest);

        Assert.NotSame(request, equalRequest);
        Assert.Equal(request, equalRequest);
        Assert.Throws<ArgumentException>(() => new CharacterTurnActivity(
            request,
            response,
            CharacterTurnActivityState.Speaking));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");

        Assert.Throws<ArgumentNullException>(() => new CharacterTurnActivity(
            null!,
            null,
            CharacterTurnActivityState.AwaitingResponse));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnActivity(
            request,
            null,
            null!));
    }

    [Fact]
    public void ExposesOnlyImmutableActivityState()
    {
        Type activityType = typeof(CharacterTurnActivity);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Request", typeof(CharacterTurnRequest)),
            ("Response", typeof(CharacterTurnResponse)),
            ("State", typeof(CharacterTurnActivityState)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = activityType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(activityType.GetEvents());
        Assert.True(activityType.IsSealed);
    }

    private static CharacterTurnRequest CreateRequest(string text)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, text);

        return new CharacterTurnRequest(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
    }

    private static CharacterTurnResponse CreateResponse(CharacterTurnRequest request) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(request.RespondingCharacterId, "I will wait."),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
}

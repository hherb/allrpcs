using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterRecentConversationTurnTests
{
    [Fact]
    public void RetainsExactIdentifierAndCompletedActivity()
    {
        CharacterTurnId turnId = new("healer-turn-1");
        CharacterTurnActivity activity = CompletedActivity("Please wait.", "I will wait.");

        CharacterRecentConversationTurn turn = new(turnId, activity);

        Assert.Same(turnId, turn.TurnId);
        Assert.Same(activity, turn.Activity);
        Assert.Same(activity.Request.RespondingCharacterId, turn.CharacterId);
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("speaking")]
    [InlineData("cancelled")]
    public void RejectsAnActivityThatDidNotComplete(string stateValue)
    {
        CharacterTurnRequest request = CreateRequest("Please wait.");
        CharacterTurnActivityState state = CharacterTurnActivityState.Parse(stateValue);
        CharacterTurnResponse? response = state == CharacterTurnActivityState.AwaitingResponse
            ? null
            : CreateResponse(request, "I will wait.");
        CharacterTurnActivity activity = new(request, response, state);

        Assert.Throws<ArgumentException>(() => new CharacterRecentConversationTurn(
            new CharacterTurnId("healer-turn-1"),
            activity));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnId turnId = new("healer-turn-1");
        CharacterTurnActivity activity = CompletedActivity("Please wait.", "I will wait.");

        Assert.Throws<ArgumentNullException>(() => new CharacterRecentConversationTurn(
            null!,
            activity));
        Assert.Throws<ArgumentNullException>(() => new CharacterRecentConversationTurn(
            turnId,
            null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableCompletedTurnSurface()
    {
        Type turnType = typeof(CharacterRecentConversationTurn);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Activity", typeof(CharacterTurnActivity)),
            ("CharacterId", typeof(CharacterId)),
            ("TurnId", typeof(CharacterTurnId)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = turnType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(turnType.GetEvents());
        Assert.True(turnType.IsSealed);
    }

    internal static CharacterTurnActivity CompletedActivity(string heardText, string responseText)
    {
        CharacterTurnRequest request = CreateRequest(heardText);
        return new CharacterTurnActivity(
            request,
            CreateResponse(request, responseText),
            CharacterTurnActivityState.Completed);
    }

    internal static CharacterTurnRequest CreateRequest(string text)
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

    internal static CharacterTurnResponse CreateResponse(
        CharacterTurnRequest request,
        string responseText) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(request.RespondingCharacterId, responseText),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
}

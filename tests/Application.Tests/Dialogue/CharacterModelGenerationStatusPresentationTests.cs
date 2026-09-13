using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationStatusPresentationTests
{
    [Fact]
    public void ProjectsEmptyAndTextOnlyStreamsAsGenerating()
    {
        CharacterModelStreamSnapshot empty = new([]);
        CharacterModelTextDelta text = new("private partial response");
        CharacterModelStreamSnapshot partial = new([text]);

        CharacterModelGenerationStatusPresentation emptyResult =
            CharacterModelGenerationStatusPresentation.Project(empty);
        CharacterModelGenerationStatusPresentation partialResult =
            CharacterModelGenerationStatusPresentation.Project(partial);

        Assert.Same(CharacterModelGenerationStatusPresentation.Generating, emptyResult);
        Assert.Same(emptyResult, partialResult);
        Assert.Null(emptyResult.TerminationStatus);
        Assert.False(emptyResult.IsTerminal);
        Assert.Equal("Character response: GENERATING", emptyResult.Text);
        Assert.DoesNotContain(text.Text, emptyResult.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(TerminalCases))]
    public void ProjectsEveryTerminalStatusToFixedSafePresentation(
        CharacterModelStreamTermination termination,
        CharacterModelGenerationStatusPresentation expected,
        string expectedText)
    {
        CharacterModelTextDelta partialText = new("private response content");
        CharacterModelStreamSnapshot snapshot = new([partialText, termination]);

        CharacterModelGenerationStatusPresentation result =
            CharacterModelGenerationStatusPresentation.Project(snapshot);

        Assert.Same(expected, result);
        Assert.Equal(termination.Status, result.TerminationStatus);
        Assert.True(result.IsTerminal);
        Assert.Equal(expectedText, result.Text);
        Assert.Equal(expectedText, result.ToString());
        Assert.DoesNotContain(partialText.Text, result.Text, StringComparison.Ordinal);
    }

    public static TheoryData<
        CharacterModelStreamTermination,
        CharacterModelGenerationStatusPresentation,
        string> TerminalCases => new()
    {
        {
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                Response("private completed speech"),
                null),
            CharacterModelGenerationStatusPresentation.Completed,
            "Character response: COMPLETE"
        },
        {
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Cancelled,
                null,
                null),
            CharacterModelGenerationStatusPresentation.Cancelled,
            "Character response: CANCELLED"
        },
        {
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.TimedOut,
                null,
                null),
            CharacterModelGenerationStatusPresentation.TimedOut,
            "Character response: TIMED OUT"
        },
        {
            new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.ProviderRejected)),
            CharacterModelGenerationStatusPresentation.Failed,
            "Character response: FAILED"
        },
    };

    [Fact]
    public void RejectsNullSnapshotAndHasOnlyClosedCanonicalInstances()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelGenerationStatusPresentation.Project(null!));

        Assert.Equal(
            new[]
            {
                CharacterModelGenerationStatusPresentation.Generating,
                CharacterModelGenerationStatusPresentation.Completed,
                CharacterModelGenerationStatusPresentation.Cancelled,
                CharacterModelGenerationStatusPresentation.TimedOut,
                CharacterModelGenerationStatusPresentation.Failed,
            },
            CharacterModelGenerationStatusPresentation.All);
        Assert.Equal(5, CharacterModelGenerationStatusPresentation.All.Distinct().Count());
    }

    private static CharacterTurnResponse Response(string speechText)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance playerSpeech = new(seekerId, "Please inspect the herb.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(playerSpeech, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(playerSpeech, DialogueActType.Request));
        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(healerId, speechText),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
    }
}

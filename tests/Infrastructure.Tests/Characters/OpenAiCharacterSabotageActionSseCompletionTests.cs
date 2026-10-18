using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterSabotageActionSseCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        string document = OpenAiCharacterSabotageActionStreamAccumulatorTests.DecisionDocument(
            input.Actor.Identity.Id.Value,
            abstain ? null : input.SelectedKind.Value);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterSabotageActionDecision decision =
            await CompleteCharacterSabotageActionAsync(input.Request, stream);

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(abstain ? null : input.SelectedKind, decision.SelectedActionKind);
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData(": heartbeat\n\n")]
    [InlineData("data: {\"type\":\"response.created\"}\n\n")]
    [InlineData("data: not-json\n\n")]
    [InlineData("data: {\"type\":\"response.completed\"}\n\n")]
    [InlineData("data: {\"type\":\"response.failed\",\"detail\":\"secret\"}\n\n")]
    [InlineData("data: {}")]
    public async Task RejectsIncompleteMalformedFailedAndUnframedInput(string events)
    {
        using MemoryStream stream = Utf8Stream(events);

        InvalidDataException exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            CompleteCharacterSabotageActionAsync(
                CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput().Request,
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsAtCompletionBeforeTrailingEventsAndPreservesCallerCancellation()
    {
        CharacterSabotageActionDecisionJsonDeserializerTests.Input input =
            CharacterSabotageActionDecisionJsonDeserializerTests.CreateInput();
        string document = OpenAiCharacterSabotageActionStreamAccumulatorTests.DecisionDocument(
            input.Actor.Identity.Id.Value,
            null);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" })
            + Frame(new { type = "response.created" });
        using MemoryStream trailing = Utf8Stream(events);
        using MemoryStream empty = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.NotNull(await CompleteCharacterSabotageActionAsync(input.Request, trailing));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CompleteCharacterSabotageActionAsync(
                input.Request,
                empty,
                cancellation.Token));
    }



    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static MemoryStream Utf8Stream(string value) => new(Encoding.UTF8.GetBytes(value));
}

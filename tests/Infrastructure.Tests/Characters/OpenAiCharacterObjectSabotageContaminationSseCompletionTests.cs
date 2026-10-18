using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageContaminationSseCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.Input input =
            CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.CreateInput();
        string? evidenceId = abstain ? null : input.Candidate.Evidence.EvidenceId.Value;
        string document = OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests.DecisionDocument(
            input.Character.Identity.Id.Value,
            evidenceId);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterObjectSabotageContaminationDecision decision =
            await CompleteCharacterObjectSabotageContaminationAsync(
                input.Request,
                stream);

        Assert.Same(input.Character.Identity.Id, decision.CharacterId);
        Assert.Equal(evidenceId, decision.SelectedEvidenceId?.Value);
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
            CompleteCharacterObjectSabotageContaminationAsync(
                CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.CreateInput().Request,
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsAtCompletionBeforeTrailingEventsAndPreservesCallerCancellation()
    {
        CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.Input input =
            CharacterObjectSabotageContaminationDecisionJsonDeserializerTests.CreateInput();
        string document = OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests.DecisionDocument(
            input.Character.Identity.Id.Value,
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

        Assert.NotNull(await CompleteCharacterObjectSabotageContaminationAsync(input.Request, trailing));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CompleteCharacterObjectSabotageContaminationAsync(
                input.Request,
                empty,
                cancellation.Token));
    }



    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static MemoryStream Utf8Stream(string value) => new(Encoding.UTF8.GetBytes(value));
}

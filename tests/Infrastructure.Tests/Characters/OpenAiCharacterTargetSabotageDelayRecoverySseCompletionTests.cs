using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageDelayRecoverySseCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        string document = OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulatorTests
            .DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterTargetSabotageDelayRecoveryDecision decision =
            await OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(
                input.Request,
                stream);

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(abstain ? null : input.Candidates[0], decision.SelectedCandidate);
        Assert.True(stream.CanRead);
        Assert.Same(input.EventLog, input.Request.EventLog);
        Assert.Same(input.RecoveryEvent, input.Candidates[0].RecoveryEvent);
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
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(
                CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request,
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RejectsTrailingEventsAfterCompletionAndPreservesCallerCancellation()
    {
        CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create();
        string document = OpenAiCharacterTargetSabotageDelayRecoveryStreamAccumulatorTests
            .DecisionDocument(input.Actor.Identity.Id.Value, null);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" })
            + Frame(new { type = "response.created" });
        using MemoryStream trailing = Utf8Stream(events);
        using MemoryStream empty = Utf8Stream(string.Empty);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(
                input.Request,
                trailing));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(
                input.Request,
                empty,
                cancellation.Token));
    }

    [Fact]
    public async Task RejectsNullDependenciesAndExposesOnlyOneBoundedAsyncCompletionOperation()
    {
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request =
            CharacterTargetSabotageDelayRecoveryDecisionJsonTestInput.Create().Request;
        using MemoryStream stream = Utf8Stream(string.Empty);
        Type completionType = typeof(OpenAiCharacterTargetSabotageDelayRecoverySseCompletion);
        System.Reflection.MethodInfo method = Assert.Single(
            completionType.GetMethods().Where(candidate => candidate.DeclaringType == completionType));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(null!, stream));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageDelayRecoverySseCompletion.CompleteAsync(request, null!));
        Assert.Equal("CompleteAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<CharacterTargetSabotageDelayRecoveryDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageDelayRecoveryConsiderationRequest),
                typeof(Stream),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(completionType.GetConstructors());
        Assert.Empty(completionType.GetProperties());
        Assert.Empty(completionType.GetEvents());
        Assert.True(completionType.IsAbstract && completionType.IsSealed);
    }

    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static MemoryStream Utf8Stream(string value) => new(Encoding.UTF8.GetBytes(value));
}

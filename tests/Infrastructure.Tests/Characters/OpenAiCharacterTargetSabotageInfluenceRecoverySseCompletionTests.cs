using System.Text;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletesTheExactDecisionFromFramedSemanticEvents(bool abstain)
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        string document = OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulatorTests
            .DecisionDocument(input.Actor.Identity.Id.Value, abstain ? null : 0);
        string events = Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
        using MemoryStream stream = Utf8Stream(events);

        CharacterTargetSabotageInfluenceRecoveryDecision decision =
            await OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(
                input.Request,
                stream);

        Assert.Same(input.Actor.Identity.Id, decision.ActorId);
        Assert.Same(abstain ? null : input.Candidates[0], decision.SelectedCandidate);
        Assert.True(stream.CanRead);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.RecoveryChange.Reason, input.Candidates[0].RecoveryChange.Reason);
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
            OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(
                CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request,
                stream));

        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReturnsAtCompletionBeforeTrailingEventsAndPreservesCallerCancellation()
    {
        CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Input input =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create();
        string document = OpenAiCharacterTargetSabotageInfluenceRecoveryStreamAccumulatorTests
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

        Assert.NotNull(await OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(
                input.Request,
                trailing));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(
                input.Request,
                empty,
                cancellation.Token));
    }

    [Fact]
    public async Task RejectsNullDependenciesAndExposesOnlyOneBoundedAsyncCompletionOperation()
    {
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request =
            CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput.Create().Request;
        using MemoryStream stream = Utf8Stream(string.Empty);
        Type completionType = typeof(OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion);
        System.Reflection.MethodInfo method = Assert.Single(
            completionType.GetMethods().Where(candidate => candidate.DeclaringType == completionType));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(null!, stream));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            OpenAiCharacterTargetSabotageInfluenceRecoverySseCompletion.CompleteAsync(request, null!));
        Assert.Equal("CompleteAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTargetSabotageInfluenceRecoveryDecision>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest),
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

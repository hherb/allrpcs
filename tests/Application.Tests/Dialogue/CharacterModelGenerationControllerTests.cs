using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationControllerTests
{
    [Fact]
    public async Task RetainsExactCompletedStreamUpdates()
    {
        CharacterModelTextDelta text = new("Complete response.");
        CharacterModelStreamTermination completed = Terminal(
            CharacterModelStreamTerminationStatus.Completed);
        StubModel model = new(Updates(text, completed));
        CharacterModelGenerationRequest request = Request();

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Collection(
            result.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(completed, update));
        Assert.Equal(1, model.CallCount);
        Assert.Same(request, model.LastRequest);
    }

    [Fact]
    public async Task RetriesOneTransientTextFreeFailureWithoutExposingIt()
    {
        CharacterModelStreamTermination failed = Failed(CharacterModelFailureCode.Transport);
        CharacterModelTextDelta text = new("Retry succeeded.");
        CharacterModelStreamTermination completed = Terminal(
            CharacterModelStreamTerminationStatus.Completed);
        StubModel model = new(Updates(failed), Updates(text, completed));

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            Request(),
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Collection(
            result.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(completed, update));
        Assert.Equal(2, model.CallCount);
        Assert.DoesNotContain(failed, result.Updates);
    }

    [Fact]
    public async Task DoesNotRetryAfterTextWasProduced()
    {
        CharacterModelTextDelta text = new("Visible partial text.");
        CharacterModelStreamTermination failed = Failed(CharacterModelFailureCode.Transport);
        StubModel model = new(Updates(text, failed));

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            Request(),
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Collection(
            result.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(failed, update));
        Assert.Equal(1, model.CallCount);
    }

    [Fact]
    public async Task ConvertsMissingTerminationToInvalidResponse()
    {
        CharacterModelTextDelta text = new("Incomplete response.");
        StubModel model = new(Updates(text));

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            Request(),
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Same(text, result.Updates[0]);
        CharacterModelStreamTermination termination =
            Assert.IsType<CharacterModelStreamTermination>(result.Updates[1]);
        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, termination.Status);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, termination.Failure?.Code);
    }

    [Fact]
    public async Task ConvertsElapsedDeadlineToTimedOut()
    {
        StubModel model = new(WaitForCancellation());

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            Request(),
            TimeSpan.FromMilliseconds(10),
            CancellationToken.None);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.TimedOut, termination.Status);
        Assert.Null(termination.Failure);
    }

    [Fact]
    public async Task ConvertsCallerCancellationToCancelled()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        StubModel model = new(WaitForCancellation());

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            Request(),
            TimeSpan.FromSeconds(1),
            cancellation.Token);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Cancelled, termination.Status);
        Assert.Null(termination.Failure);
    }

    [Fact]
    public async Task RejectsNullsAndNonPositiveOrInfiniteDeadline()
    {
        StubModel model = new(Updates(Terminal(CharacterModelStreamTerminationStatus.Completed)));
        CharacterModelGenerationRequest request = Request();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                null!, request, TimeSpan.FromSeconds(1), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model, null!, TimeSpan.FromSeconds(1), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model, request, TimeSpan.Zero, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model, request, Timeout.InfiniteTimeSpan, CancellationToken.None));
    }

    private static CharacterModelGenerationRequest Request()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        KnowledgeSnapshot knowledge = new(
            character.Knowledge.KnowledgeReferences.Select(reference =>
                new KnowledgeEntry(reference, $"Knowledge for {reference.Value}.")));
        CharacterModelGenerationRequest request = CharacterModelContextAssembler.Assemble(
            CharacterModelPlayerDialogueContextProjectionTests.CreateRequest(),
            character,
            new CharacterPerceptionSnapshot(character.Identity.Id, []),
            new WorldObjectSnapshot([]),
            knowledge,
            [],
            [],
            []);
        return request;
    }

    private static CharacterModelStreamTermination Terminal(
        CharacterModelStreamTerminationStatus status) =>
        new(status, null);

    private static CharacterModelStreamTermination Failed(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            new CharacterModelFailure(code));

    private static async IAsyncEnumerable<CharacterModelStreamUpdate> Updates(
        params CharacterModelStreamUpdate[] updates)
    {
        await Task.CompletedTask;
        foreach (CharacterModelStreamUpdate update in updates)
        {
            yield return update;
        }
    }

    private static async IAsyncEnumerable<CharacterModelStreamUpdate> WaitForCancellation(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        yield break;
    }

    private sealed class StubModel : ICharacterModel
    {
        private readonly Queue<IAsyncEnumerable<CharacterModelStreamUpdate>> attempts;

        public StubModel(params IAsyncEnumerable<CharacterModelStreamUpdate>[] attempts)
        {
            this.attempts = new Queue<IAsyncEnumerable<CharacterModelStreamUpdate>>(attempts);
        }

        public int CallCount { get; private set; }

        public CharacterModelGenerationRequest? LastRequest { get; private set; }

        public IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return ObserveCancellation(attempts.Dequeue(), cancellationToken);
        }

        private static async IAsyncEnumerable<CharacterModelStreamUpdate> ObserveCancellation(
            IAsyncEnumerable<CharacterModelStreamUpdate> attempt,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (CharacterModelStreamUpdate update in attempt.WithCancellation(
                cancellationToken))
            {
                yield return update;
            }
        }
    }
}

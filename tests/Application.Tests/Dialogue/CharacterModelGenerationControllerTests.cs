using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.TestSupport;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationControllerTests
{
    [Theory]
    [InlineData("complete")]
    [InlineData("cancel")]
    public async Task BackoffWaitsForBoundedHintAndHonoursCancellation(string mode)
    {
        ManualTimeProvider clock = new();
        using CancellationTokenSource caller = new();
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination failed = new(CharacterModelStreamTerminationStatus.Failed,
            null, new CharacterModelFailure(CharacterModelFailureCode.Unavailable, TimeSpan.FromSeconds(17)));
        StubModel model = new(Updates(failed), Updates(Completed(request)));
        Task<CharacterModelStreamSnapshot> task = CharacterModelGenerationController.GenerateAsync(
            model, request, new WorldObjectSnapshot([]), new CharacterIntentionAffordanceSnapshot([]),
            TimeSpan.FromSeconds(120), caller.Token, clock);
        Assert.Equal(1, model.CallCount);
        Assert.False(task.IsCompleted);
        clock.Advance(TimeSpan.FromSeconds(9));
        Assert.Equal(1, model.CallCount);
        if (mode == "cancel") caller.Cancel();
        else clock.Advance(TimeSpan.FromSeconds(8));
        CharacterModelStreamSnapshot result = await task;
        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(result.Updates[^1]);
        Assert.Equal(mode == "cancel"
            ? CharacterModelStreamTerminationStatus.Cancelled
            : CharacterModelStreamTerminationStatus.Completed, termination.Status);
        Assert.Equal(mode == "complete" ? 2 : 1, model.CallCount);
    }

    [Fact]
    public async Task HintBeyondDeadlineReturnsProviderFailureWithoutWaiting()
    {
        ManualTimeProvider clock = new();
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination failed = new(CharacterModelStreamTerminationStatus.Failed,
            null, new CharacterModelFailure(CharacterModelFailureCode.RateLimited, TimeSpan.FromSeconds(17)));
        StubModel model = new(Updates(failed), Updates(Completed(request)));
        Task<CharacterModelStreamSnapshot> task = CharacterModelGenerationController.GenerateAsync(
            model, request, new WorldObjectSnapshot([]), new CharacterIntentionAffordanceSnapshot([]),
            TimeSpan.FromSeconds(10), default, clock);
        Assert.True(task.IsCompleted);
        CharacterModelStreamSnapshot result = await task;
        Assert.Equal(CharacterModelFailureCode.RateLimited,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(result.Updates)).Failure?.Code);
        Assert.Equal(1, model.CallCount);
        Assert.Equal(0, clock.ActiveTimerCount);
    }

    [Fact]
    public async Task MissingHintUsesOneSecondBackoffAndStopsAfterSecondFailure()
    {
        ManualTimeProvider clock = new();
        CharacterModelGenerationRequest request = Request();
        StubModel model = new(Updates(Failed(CharacterModelFailureCode.Transport)),
            Updates(Failed(CharacterModelFailureCode.Transport)), Updates(Completed(request)));
        Task<CharacterModelStreamSnapshot> task = CharacterModelGenerationController.GenerateAsync(
            model, request, new WorldObjectSnapshot([]), new CharacterIntentionAffordanceSnapshot([]),
            TimeSpan.FromSeconds(120), default, clock);
        clock.Advance(TimeSpan.FromMilliseconds(999));
        Assert.Equal(1, model.CallCount);
        Assert.False(task.IsCompleted);
        clock.Advance(TimeSpan.FromMilliseconds(1));
        CharacterModelStreamSnapshot result = await task;
        Assert.Equal(CharacterModelFailureCode.Transport,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(result.Updates)).Failure?.Code);
        Assert.Equal(2, model.CallCount);
        Assert.Equal(0, clock.ActiveTimerCount);
    }

    [Fact]
    public async Task RetainsExactCompletedStreamUpdates()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelTextDelta text = new("Complete response.");
        CharacterModelStreamTermination completed = Completed(request);
        StubModel model = new(Updates(text, completed));

        CharacterModelStreamSnapshot result = await GenerateAsync(
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
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination failed = new(CharacterModelStreamTerminationStatus.Failed,
            null, new CharacterModelFailure(CharacterModelFailureCode.Transport, TimeSpan.Zero));
        CharacterModelTextDelta text = new("Retry succeeded.");
        CharacterModelStreamTermination completed = Completed(request);
        StubModel model = new(Updates(failed), Updates(text, completed));

        CharacterModelStreamSnapshot result = await GenerateAsync(
            model,
            request,
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

        CharacterModelStreamSnapshot result = await GenerateAsync(
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

        CharacterModelStreamSnapshot result = await GenerateAsync(
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
    public async Task RejectsACompletedResponseForADifferentRequestWithoutExposingIt()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelGenerationRequest differentRequest = Request();
        CharacterModelTextDelta text = new("Visible response text.");
        CharacterModelStreamTermination mismatched = Completed(differentRequest);
        StubModel model = new(Updates(text, mismatched));

        CharacterModelStreamSnapshot result = await GenerateAsync(
            model,
            request,
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Same(text, result.Updates[0]);
        CharacterModelStreamTermination termination =
            Assert.IsType<CharacterModelStreamTermination>(result.Updates[1]);
        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, termination.Status);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, termination.Failure?.Code);
        Assert.Null(termination.Response);
        Assert.DoesNotContain(mismatched, result.Updates);
        Assert.NotSame(request.TurnRequest, differentRequest.TurnRequest);
    }

    [Fact]
    public async Task RejectsAnUnknownCompletedIntentionWithoutChangingSnapshots()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination completed = Completed(
            request,
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("herb-patch")));
        StubModel model = new(Updates(completed));
        WorldObjectSnapshot worldObjects = new([]);
        CharacterIntentionAffordance affordance = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("herb-patch"));
        CharacterIntentionAffordanceSnapshot affordances = new([affordance]);

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            worldObjects,
            affordances,
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, termination.Status);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, termination.Failure?.Code);
        Assert.Null(termination.Response);
        Assert.Empty(worldObjects.Entries);
        Assert.Same(affordance, Assert.Single(affordances.Entries));
        Assert.DoesNotContain(completed, result.Updates);
    }

    [Fact]
    public async Task RetainsACompletedKnownAndCurrentlyAvailableIntention()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination completed = Completed(
            request,
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("herb-patch")));
        StubModel model = new(Updates(completed));
        WorldObjectSnapshot worldObjects = new(
            [
                CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
                    "herb-patch",
                    "Herb patch",
                    "A patch of herbs."),
            ]);
        CharacterIntentionAffordanceSnapshot affordances = new(
            [
                new CharacterIntentionAffordance(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch")),
            ]);

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            worldObjects,
            affordances,
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        CharacterModelStreamTermination resultTermination =
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(result.Updates));
        Assert.Same(completed, resultTermination);
        Assert.Same(completed.Response, resultTermination.Response);
    }

    [Fact]
    public async Task RejectsAKnownButCurrentlyUnavailableCompletedIntention()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination completed = Completed(
            request,
            new CharacterIntention(
                CharacterIntentionType.CollectObject,
                new ObjectId("herb-patch")));
        StubModel model = new(Updates(completed));
        WorldObjectSnapshot worldObjects = new(
            [
                CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
                    "herb-patch",
                    "Herb patch",
                    "A patch of herbs."),
            ]);

        CharacterModelStreamSnapshot result = await CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            worldObjects,
            new CharacterIntentionAffordanceSnapshot([]),
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, termination.Failure?.Code);
        Assert.Null(termination.Response);
        Assert.DoesNotContain(completed, result.Updates);
    }

    [Fact]
    public async Task ConvertsElapsedDeadlineToTimedOut()
    {
        StubModel model = new(WaitForCancellation());

        CharacterModelStreamSnapshot result = await GenerateAsync(
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

        CharacterModelStreamSnapshot result = await GenerateAsync(
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
        CharacterModelGenerationRequest request = Request();
        StubModel model = new(Updates(Completed(request)));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                null!,
                request,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                null!,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                request,
                null!,
                new CharacterIntentionAffordanceSnapshot([]),
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                request,
                new WorldObjectSnapshot([]),
                null!,
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                request,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                TimeSpan.Zero,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                request,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                Timeout.InfiniteTimeSpan,
                CancellationToken.None));
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
            new CharacterIntentionAffordanceSnapshot([]));
        return request;
    }

    private static CharacterModelStreamTermination Completed(
        CharacterModelGenerationRequest request,
        CharacterIntention? immediateIntention = null) =>
        new(
            CharacterModelStreamTerminationStatus.Completed,
            Response(request.TurnRequest, immediateIntention),
            null);

    private static CharacterModelStreamTermination Failed(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));

    private static CharacterTurnResponse Response(
        CharacterTurnRequest request,
        CharacterIntention? immediateIntention = null) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            immediateIntention ?? new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);

    private static Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        TimeSpan deadline,
        CancellationToken cancellationToken) =>
        CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            new WorldObjectSnapshot([]),
            new CharacterIntentionAffordanceSnapshot([]),
            deadline,
            cancellationToken);

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

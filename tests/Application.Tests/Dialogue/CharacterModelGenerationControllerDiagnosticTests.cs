using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.TestSupport;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationControllerDiagnosticTests
{
    [Fact]
    public async Task RecordsCallerCancellationWithIndependentDiagnosticCancellation()
    {
        using CancellationTokenSource generationCancellation = new();
        generationCancellation.Cancel();
        using CancellationTokenSource diagnosticCancellation = new();
        RecordingSink sink = new();
        CharacterModelGenerationControllerDiagnostics diagnostics = new(
            CharacterModelProvider.Anthropic,
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(25)),
            sink,
            diagnosticCancellation.Token);

        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            diagnostics,
            null,
            generationCancellation.Token);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Cancelled, termination.Status);
        CharacterModelGenerationDiagnostic diagnostic = Assert.Single(sink.Diagnostics);
        Assert.Same(CharacterModelProvider.Anthropic, diagnostic.Provider);
        Assert.Equal(CharacterModelStreamTerminationStatus.Cancelled, diagnostic.Status);
        Assert.Equal(TimeSpan.FromMilliseconds(25), diagnostic.Elapsed);
        Assert.Null(diagnostic.FailureCode);
        Assert.Null(diagnostic.InputTokenCount);
        Assert.Null(diagnostic.OutputTokenCount);
        Assert.Null(diagnostic.TotalTokenCount);
        Assert.Equal(diagnosticCancellation.Token, sink.CancellationToken);
        Assert.NotEqual(generationCancellation.Token, sink.CancellationToken);
    }

    [Fact]
    public async Task RecordsDeadlineTimeout()
    {
        RecordingSink sink = new();
        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromMilliseconds(10),
            Diagnostics(
                new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(10)),
                sink),
            null,
            CancellationToken.None);

        CharacterModelStreamTermination termination = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(result.Updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.TimedOut, termination.Status);
        CharacterModelGenerationDiagnostic diagnostic = Assert.Single(sink.Diagnostics);
        Assert.Equal(CharacterModelStreamTerminationStatus.TimedOut, diagnostic.Status);
        Assert.Equal(TimeSpan.FromMilliseconds(10), diagnostic.Elapsed);
    }

    [Theory]
    [MemberData(nameof(BoundedElapsedCases))]
    public async Task ClampsSynthesizedDiagnosticElapsedTime(
        TimeSpan start,
        TimeSpan end,
        TimeSpan expected)
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        RecordingSink sink = new();

        await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            Diagnostics(new SequenceTimeProvider(start, end), sink),
            null,
            cancellation.Token);

        Assert.Equal(expected, Assert.Single(sink.Diagnostics).Elapsed);
    }

    public static TheoryData<TimeSpan, TimeSpan, TimeSpan> BoundedElapsedCases => new()
    {
        { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), TimeSpan.Zero },
        { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero },
        {
            TimeSpan.Zero,
            CharacterModelGenerationDiagnostic.MaximumElapsed,
            CharacterModelGenerationDiagnostic.MaximumElapsed
        },
        {
            TimeSpan.Zero,
            CharacterModelGenerationDiagnostic.MaximumElapsed + TimeSpan.FromTicks(1),
            CharacterModelGenerationDiagnostic.MaximumElapsed
        },
    };

    [Fact]
    public async Task DoesNotDuplicateProviderTerminalDiagnostics()
    {
        CharacterModelGenerationRequest request = Request();
        RecordingSink completedSink = new();
        RecordingSink failedSink = new();

        CharacterModelStreamSnapshot completed = await GenerateAsync(
            new StubModel(Updates(Completed(request))),
            TimeSpan.FromSeconds(1),
            Diagnostics(new SequenceTimeProvider(TimeSpan.Zero), completedSink),
            request,
            CancellationToken.None);
        CharacterModelStreamSnapshot failed = await GenerateAsync(
            new StubModel(Updates(Failed(CharacterModelFailureCode.Authentication))),
            TimeSpan.FromSeconds(1),
            Diagnostics(new SequenceTimeProvider(TimeSpan.Zero), failedSink),
            request,
            CancellationToken.None);

        Assert.Equal(
            CharacterModelStreamTerminationStatus.Completed,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(completed.Updates)).Status);
        Assert.Equal(
            CharacterModelFailureCode.Authentication,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(failed.Updates))
                .Failure?.Code);
        Assert.Empty(completedSink.Diagnostics);
        Assert.Empty(failedSink.Diagnostics);
    }

    [Fact]
    public async Task RecordsOnlyFinalSynthesizedTimeoutAfterSafeRetry()
    {
        RecordingSink sink = new();
        ManualTimeProvider generationClock = new();
        bool secondAttemptEntered = false;
        StubModel model = new(
            Updates(new CharacterModelStreamTermination(CharacterModelStreamTerminationStatus.Failed,
                null, new CharacterModelFailure(CharacterModelFailureCode.Transport, TimeSpan.Zero))),
            WaitForCancellation(() => secondAttemptEntered = true));

        Task<CharacterModelStreamSnapshot> generation = GenerateAsync(
            model,
            TimeSpan.FromMilliseconds(10),
            Diagnostics(
                new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(10)),
                sink),
            null,
            CancellationToken.None,
            generationClock);

        Assert.Equal(2, model.CallCount);
        Assert.True(secondAttemptEntered);
        Assert.False(generation.IsCompleted);
        Assert.Empty(sink.Diagnostics);
        generationClock.Advance(TimeSpan.FromMilliseconds(10));
        CharacterModelStreamSnapshot result = await generation;

        Assert.Equal(2, model.CallCount);
        Assert.Equal(
            CharacterModelStreamTerminationStatus.TimedOut,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(result.Updates)).Status);
        Assert.Single(sink.Diagnostics);
    }

    [Fact]
    public async Task DiagnosticCancellationDoesNotChangeSynthesizedResult()
    {
        using CancellationTokenSource generationCancellation = new();
        generationCancellation.Cancel();
        using CancellationTokenSource diagnosticCancellation = new();
        diagnosticCancellation.Cancel();
        CancellationObservingSink sink = new();
        CharacterModelGenerationControllerDiagnostics diagnostics = new(
            CharacterModelProvider.OpenAi,
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(1)),
            sink,
            diagnosticCancellation.Token);

        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            diagnostics,
            null,
            generationCancellation.Token);

        Assert.Equal(
            CharacterModelStreamTerminationStatus.Cancelled,
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(result.Updates)).Status);
        Assert.Equal(diagnosticCancellation.Token, sink.CancellationToken);
    }

    [Fact]
    public async Task SinkAndTimeSourceFailuresDoNotChangeSynthesizedResult()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        CharacterModelStreamSnapshot sinkFailure = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            Diagnostics(
                new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(1)),
                new ThrowingSink()),
            null,
            cancellation.Token);
        CharacterModelStreamSnapshot startFailure = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            Diagnostics(new ThrowingTimeProvider(throwOnStart: true), new RecordingSink()),
            null,
            cancellation.Token);
        CharacterModelStreamSnapshot elapsedFailure = await GenerateAsync(
            new StubModel(WaitForCancellation()),
            TimeSpan.FromSeconds(1),
            Diagnostics(new ThrowingTimeProvider(throwOnStart: false), new RecordingSink()),
            null,
            cancellation.Token);

        Assert.All(
            new[] { sinkFailure, startFailure, elapsedFailure },
            snapshot => Assert.Equal(
                CharacterModelStreamTerminationStatus.Cancelled,
                Assert.IsType<CharacterModelStreamTermination>(Assert.Single(snapshot.Updates))
                    .Status));
    }

    [Fact]
    public void RejectsNullDiagnosticDependencies()
    {
        RecordingSink sink = new();

        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationControllerDiagnostics(
            null!,
            TimeProvider.System,
            sink,
            CancellationToken.None));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationControllerDiagnostics(
            CharacterModelProvider.OpenAi,
            null!,
            sink,
            CancellationToken.None));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelGenerationControllerDiagnostics(
            CharacterModelProvider.OpenAi,
            TimeProvider.System,
            null!,
            CancellationToken.None));
    }

    private static CharacterModelGenerationControllerDiagnostics Diagnostics(
        TimeProvider timeProvider,
        ICharacterModelGenerationDiagnosticSink sink) =>
        new(
            CharacterModelProvider.OpenAi,
            timeProvider,
            sink,
            CancellationToken.None);

    private static Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        TimeSpan deadline,
        CharacterModelGenerationControllerDiagnostics diagnostics,
        CharacterModelGenerationRequest? request,
        CancellationToken cancellationToken,
        TimeProvider? generationTimeProvider = null) =>
        CharacterModelGenerationController.GenerateAsync(
            model,
            request ?? Request(),
            new WorldObjectSnapshot([]),
            new CharacterIntentionAffordanceSnapshot([]),
            deadline,
            diagnostics,
            cancellationToken,
            generationTimeProvider);

    private static CharacterModelGenerationRequest Request()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        KnowledgeSnapshot knowledge = new(
            character.Knowledge.KnowledgeReferences.Select(reference =>
                new KnowledgeEntry(reference, $"Knowledge for {reference.Value}.")));
        return CharacterModelContextAssembler.Assemble(
            CharacterModelPlayerDialogueContextProjectionTests.CreateRequest(),
            character,
            new CharacterPerceptionSnapshot(character.Identity.Id, []),
            new WorldObjectSnapshot([]),
            knowledge,
            [],
            [],
            new CharacterIntentionAffordanceSnapshot([]));
    }

    private static CharacterModelStreamTermination Completed(
        CharacterModelGenerationRequest request) =>
        new(
            CharacterModelStreamTerminationStatus.Completed,
            new CharacterTurnResponse(
                request.TurnRequest,
                CharacterTurnDisposition.Accept,
                null,
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []),
            null);

    private static CharacterModelStreamTermination Failed(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
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
        Action? onEntered = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Task pendingCancellation = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        onEntered?.Invoke();
        await pendingCancellation;
        yield break;
    }

    private sealed class StubModel(
        params IAsyncEnumerable<CharacterModelStreamUpdate>[] attempts) : ICharacterModel
    {
        private readonly Queue<IAsyncEnumerable<CharacterModelStreamUpdate>> attempts = new(attempts);

        public int CallCount { get; private set; }

        public IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return attempts.Dequeue();
        }
    }

    private sealed class RecordingSink : ICharacterModelGenerationDiagnosticSink
    {
        public List<CharacterModelGenerationDiagnostic> Diagnostics { get; } = [];

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            Diagnostics.Add(diagnostic);
            CancellationToken = cancellationToken;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CancellationObservingSink : ICharacterModelGenerationDiagnosticSink
    {
        public CancellationToken CancellationToken { get; private set; }

        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingSink : ICharacterModelGenerationDiagnosticSink
    {
        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("private sink detail");
    }

    private sealed class SequenceTimeProvider(params TimeSpan[] timestamps) : TimeProvider
    {
        private readonly Queue<long> values = new(timestamps.Select(value => value.Ticks));

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => values.Dequeue();
    }

    private sealed class ThrowingTimeProvider(bool throwOnStart) : TimeProvider
    {
        private int callCount;

        public override long GetTimestamp()
        {
            callCount++;
            if (throwOnStart || callCount > 1)
            {
                throw new InvalidOperationException("private time detail");
            }

            return 0;
        }
    }
}

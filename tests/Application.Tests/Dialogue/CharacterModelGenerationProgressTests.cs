using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelGenerationProgressTests
{
    [Fact]
    public async Task ObservesExactTextAndOnlyTheValidatedTerminalUpdate()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelTextDelta text = new("Visible response.");
        CharacterModelStreamTermination completed = Completed(request);
        RecordingSink sink = new();

        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(Updates(text, completed)),
            request,
            new CharacterModelStreamObservation(sink, CancellationToken.None));

        Assert.Collection(
            sink.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(completed, update));
        Assert.Collection(
            result.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(completed, update));
    }

    [Fact]
    public async Task HidesTextFreeRetryFailureFromProgress()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelStreamTermination retryFailure = new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(CharacterModelFailureCode.Transport, TimeSpan.Zero));
        CharacterModelTextDelta text = new("Retried response.");
        CharacterModelStreamTermination completed = Completed(request);
        RecordingSink sink = new();

        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(Updates(retryFailure), Updates(text, completed)),
            request,
            new CharacterModelStreamObservation(sink, CancellationToken.None));

        Assert.DoesNotContain(retryFailure, sink.Updates);
        Assert.Equal(result.Updates, sink.Updates);
        Assert.Collection(
            sink.Updates,
            update => Assert.Same(text, update),
            update => Assert.Same(completed, update));
    }

    [Fact]
    public async Task ReplacesInvalidCompletedTerminalBeforeProgressObservation()
    {
        CharacterModelGenerationRequest request = Request();
        CharacterModelTextDelta text = new("Unsafe structured result.");
        CharacterModelStreamTermination invalidCompleted = new(
            CharacterModelStreamTerminationStatus.Completed,
            new CharacterTurnResponse(
                request.TurnRequest,
                CharacterTurnDisposition.Accept,
                null,
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new Allrpcs.Domain.Identifiers.ObjectId("absent-object")),
                [],
                []),
            null);
        RecordingSink sink = new();

        CharacterModelStreamSnapshot result = await GenerateAsync(
            new StubModel(Updates(text, invalidCompleted)),
            request,
            new CharacterModelStreamObservation(sink, CancellationToken.None));

        CharacterModelStreamTermination observed =
            Assert.IsType<CharacterModelStreamTermination>(sink.Updates[1]);
        Assert.Same(result.Updates[1], observed);
        Assert.Equal(CharacterModelStreamTerminationStatus.Failed, observed.Status);
        Assert.Equal(CharacterModelFailureCode.InvalidResponse, observed.Failure?.Code);
        Assert.Null(observed.Response);
        Assert.DoesNotContain(invalidCompleted, sink.Updates);
    }

    [Fact]
    public async Task ObserverFailureOrIndependentCancellationCannotChangeGeneration()
    {
        CharacterModelGenerationRequest firstRequest = Request();
        CharacterModelTextDelta firstText = new("First response.");
        CharacterModelStreamTermination firstCompleted = Completed(firstRequest);
        CharacterModelStreamSnapshot failedObserverResult = await GenerateAsync(
            new StubModel(Updates(firstText, firstCompleted)),
            firstRequest,
            new CharacterModelStreamObservation(
                new ThrowingSink(),
                CancellationToken.None));

        using CancellationTokenSource observationCancellation = new();
        observationCancellation.Cancel();
        CharacterModelGenerationRequest secondRequest = Request();
        CharacterModelTextDelta secondText = new("Second response.");
        CharacterModelStreamTermination secondCompleted = Completed(secondRequest);
        RecordingSink cancelledSink = new();
        CharacterModelStreamSnapshot cancelledObserverResult = await GenerateAsync(
            new StubModel(Updates(secondText, secondCompleted)),
            secondRequest,
            new CharacterModelStreamObservation(
                cancelledSink,
                observationCancellation.Token));

        Assert.Collection(
            failedObserverResult.Updates,
            update => Assert.Same(firstText, update),
            update => Assert.Same(firstCompleted, update));
        Assert.Collection(
            cancelledObserverResult.Updates,
            update => Assert.Same(secondText, update),
            update => Assert.Same(secondCompleted, update));
        Assert.Empty(cancelledSink.Updates);
    }

    [Fact]
    public async Task RejectsNullProgressBoundaries()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterModelStreamObservation(null!, CancellationToken.None));

        CharacterModelGenerationRequest request = Request();
        StubModel model = new(Updates(Completed(request)));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CharacterModelGenerationController.GenerateAsync(
                model,
                request,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                TimeSpan.FromSeconds(1),
                (CharacterModelStreamObservation)null!,
                CancellationToken.None));
    }

    private static Task<CharacterModelStreamSnapshot> GenerateAsync(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        CharacterModelStreamObservation observation) =>
        CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            new WorldObjectSnapshot([]),
            new CharacterIntentionAffordanceSnapshot([]),
            TimeSpan.FromSeconds(1),
            observation,
            CancellationToken.None);

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

    private static async IAsyncEnumerable<CharacterModelStreamUpdate> Updates(
        params CharacterModelStreamUpdate[] updates)
    {
        await Task.CompletedTask;
        foreach (CharacterModelStreamUpdate update in updates)
        {
            yield return update;
        }
    }

    private sealed class StubModel(
        params IAsyncEnumerable<CharacterModelStreamUpdate>[] attempts) : ICharacterModel
    {
        private readonly Queue<IAsyncEnumerable<CharacterModelStreamUpdate>> attempts =
            new(attempts);

        public IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            return ObserveCancellation(attempts.Dequeue(), cancellationToken);
        }

        private static async IAsyncEnumerable<CharacterModelStreamUpdate> ObserveCancellation(
            IAsyncEnumerable<CharacterModelStreamUpdate> updates,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (CharacterModelStreamUpdate update in updates.WithCancellation(
                cancellationToken))
            {
                yield return update;
            }
        }
    }

    private sealed class RecordingSink : ICharacterModelStreamUpdateSink
    {
        public List<CharacterModelStreamUpdate> Updates { get; } = [];

        public ValueTask RecordAsync(
            CharacterModelStreamUpdate update,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Updates.Add(update);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingSink : ICharacterModelStreamUpdateSink
    {
        public ValueTask RecordAsync(
            CharacterModelStreamUpdate update,
            CancellationToken cancellationToken)
        {
            _ = update;
            _ = cancellationToken;
            throw new InvalidOperationException("Expected observer failure.");
        }
    }
}

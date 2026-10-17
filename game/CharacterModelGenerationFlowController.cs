using System.Collections.Concurrent;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Owns one scene model-generation task and applies observed updates on the Godot main thread.
/// </summary>
public partial class CharacterModelGenerationFlowController : Node, ICharacterModelStreamUpdateSink
{
    private ConcurrentQueue<CharacterModelStreamUpdate> pendingUpdates = new();
    private CancellationTokenSource? generationCancellation;
    private Task<CharacterModelStreamSnapshot>? generationTask;
    private bool cancellationRequested;
    private CancellationTokenSource? observationCancellation;
    private CharacterModelStreamController? streamController;
    private TemporaryCharacterModelGenerationStatusLabel? statusLabel;
    private TemporaryCharacterModelStreamTextLabel? textLabel;

    /// <summary>Gets whether the one scene generation remains active.</summary>
    public bool IsGenerating => generationTask is not null;

    /// <summary>Gets whether the current scene flow has started.</summary>
    public bool HasStarted => generationTask is not null || Result is not null;

    /// <summary>Gets the exact completed generation result, if available.</summary>
    public CharacterModelStreamSnapshot? Result { get; private set; }

    /// <summary>Initializes the flow once over exact scene-owned presentation dependencies.</summary>
    public bool TryInitialize(
        CharacterModelStreamController stream,
        TemporaryCharacterModelGenerationStatusLabel status,
        TemporaryCharacterModelStreamTextLabel text)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(status);
        ArgumentNullException.ThrowIfNull(text);

        if (streamController is not null)
        {
            return false;
        }

        streamController = stream;
        statusLabel = status;
        textLabel = text;
        return true;
    }

    /// <summary>Starts one bounded generation without constructing or retaining provider settings.</summary>
    public bool TryStart(
        ICharacterModel model,
        CharacterModelGenerationRequest request,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances,
        TimeSpan deadline,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(affordances);
        if (deadline <= TimeSpan.Zero || deadline == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }

        CharacterModelStreamController? stream = streamController;
        if (stream is null
            || generationTask is not null
            || Result is not null
            || stream.Snapshot is not { Updates.Count: 0 })
        {
            return false;
        }

        cancellationRequested = false;
        generationCancellation = new CancellationTokenSource();
        observationCancellation = new CancellationTokenSource();
        CharacterModelStreamObservation observation = new(
            new GenerationSink(pendingUpdates),
            observationCancellation.Token);
        generationTask = CharacterModelGenerationController.GenerateAsync(
            model,
            request,
            worldObjects,
            affordances,
            deadline,
            observation,
            generationCancellation.Token,
            timeProvider);
        _ = statusLabel!.Observe(stream);
        _ = textLabel!.Observe(stream);
        return true;
    }

    /// <summary>Requests caller cancellation once while generation remains active.</summary>
    public bool TryCancel()
    {
        if (generationTask is null
            || generationTask.IsCompleted
            || cancellationRequested
            || generationCancellation is null)
        {
            return false;
        }

        cancellationRequested = true;
        generationCancellation.Cancel();
        return true;
    }

    /// <summary>Clears one completed terminal flow so another generation can start.</summary>
    public bool TryReset()
    {
        CharacterModelStreamController? stream = streamController;
        CharacterModelStreamSnapshot? result = Result;
        if (stream is null
            || generationTask is not null
            || result is null
            || result.Updates.Count == 0
            || result.Updates[^1] is not CharacterModelStreamTermination
            || stream.Snapshot is not { } sceneSnapshot
            || !HasSameExactUpdates(sceneSnapshot, result)
            || !pendingUpdates.IsEmpty
            || !stream.TryReset())
        {
            return false;
        }

        Result = null;
        cancellationRequested = false;
        statusLabel!.ClearPresentation();
        textLabel!.ClearPresentation();
        return true;
    }

    internal PreparedRestore PrepareSessionReset()
    {
        Task? retiredTask = generationTask;
        CancellationTokenSource? retiredGeneration = generationCancellation;
        CancellationTokenSource? retiredObservation = observationCancellation;
        ConcurrentQueue<CharacterModelStreamUpdate> freshUpdates = new();
        CharacterModelStreamSnapshot empty = new([]);
        return new PreparedRestore(() =>
        {
            generationTask = null;
            generationCancellation = null;
            observationCancellation = null;
            pendingUpdates = freshUpdates;
            Result = null;
            cancellationRequested = false;
            streamController?.InstallPreparedRestoration(empty);
        }, () =>
        {
            RetiredRestoreTask.Retire(retiredTask, retiredGeneration, retiredObservation);
            statusLabel?.ClearPresentation();
            textLabel?.ClearPresentation();
        });
    }

    private sealed class GenerationSink(ConcurrentQueue<CharacterModelStreamUpdate> updates)
        : ICharacterModelStreamUpdateSink
    {
        public ValueTask RecordAsync(CharacterModelStreamUpdate update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            updates.Enqueue(update);
            return ValueTask.CompletedTask;
        }
    }

    /// <inheritdoc />
    public ValueTask RecordAsync(
        CharacterModelStreamUpdate update,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        cancellationToken.ThrowIfCancellationRequested();
        pendingUpdates.Enqueue(update);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        DrainObservedUpdates();
        CompleteFinishedGeneration();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        generationCancellation?.Cancel();
        observationCancellation?.Cancel();
        DisposeCancellationSources();
    }

    private void DrainObservedUpdates()
    {
        CharacterModelStreamController? stream = streamController;
        if (stream is null)
        {
            return;
        }

        while (pendingUpdates.TryDequeue(out CharacterModelStreamUpdate? update))
        {
            if (!stream.TryAppend(update))
            {
                throw new InvalidOperationException(
                    "The scene model stream rejected an accepted generation update.");
            }

            _ = statusLabel!.Observe(stream);
            _ = textLabel!.Observe(stream);
        }
    }

    private void CompleteFinishedGeneration()
    {
        Task<CharacterModelStreamSnapshot>? task = generationTask;
        if (task is null || !task.IsCompleted)
        {
            return;
        }

        DrainObservedUpdates();
        _ = task.Exception;
        CharacterModelStreamSnapshot result = task.IsCompletedSuccessfully
            ? task.Result
            : InstallUnexpectedFailure();
        CharacterModelStreamSnapshot sceneSnapshot = streamController!.Snapshot
            ?? throw new InvalidOperationException("The scene model stream became uninitialized.");
        if (!HasSameExactUpdates(sceneSnapshot, result))
        {
            throw new InvalidOperationException(
                "The scene model stream diverged from the generation result.");
        }

        Result = result;
        generationTask = null;
        DisposeCancellationSources();
    }

    private CharacterModelStreamSnapshot InstallUnexpectedFailure()
    {
        CharacterModelStreamController stream = streamController!;
        CharacterModelStreamSnapshot snapshot = stream.Snapshot!;
        if (snapshot.Updates.Count == 0
            || snapshot.Updates[^1] is not CharacterModelStreamTermination)
        {
            CharacterModelStreamTermination failed = new(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.InvalidResponse));
            if (!stream.TryAppend(failed))
            {
                throw new InvalidOperationException(
                    "The scene model stream could not install a safe unexpected failure.");
            }

            _ = statusLabel!.Observe(stream);
            _ = textLabel!.Observe(stream);
            snapshot = stream.Snapshot!;
        }

        return snapshot;
    }

    private static bool HasSameExactUpdates(
        CharacterModelStreamSnapshot first,
        CharacterModelStreamSnapshot second) =>
        first.Updates.Count == second.Updates.Count
        && first.Updates.Zip(second.Updates).All(pair => ReferenceEquals(pair.First, pair.Second));

    private void DisposeCancellationSources()
    {
        generationCancellation?.Dispose();
        observationCancellation?.Dispose();
        generationCancellation = null;
        observationCancellation = null;
    }
}

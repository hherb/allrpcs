using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains and resolves a move decision.</summary>
public partial class CharacterObjectSabotageMoveTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterObjectSabotageMoveTaskController task = null!;
    private CharacterObjectSabotageMoveController state = null!;
    private TaskInput input = null!;
    private DeferredModel? deferredModel;
    private WorldObjectPlacementSnapshot? staleAppliedPlacements;
    private DiscoveryEvidenceSnapshot? staleAppliedEvidence;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready() => SetProcess(true);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Move task did not finish within its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndStartCancellation();
                break;
            case 3:
                ObserveCancellationAndStartFailure();
                break;
            case 4:
                ObserveFailureAndStartStaleResult();
                break;
            case 5:
                ObserveStaleResult();
                break;
            default:
                Fail("Move task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CreateScenario();
        CharacterObjectSabotageMoveDecision decision = SelectionDecision();
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request, state)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Running
            && task.Result is null
            && !task.TryStart(new ImmediateModel(decision), input.Request, state);
        if (!valid)
        {
            Fail("Move task rejected its exact start, overlap, or closed-surface contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(
                result.Decision.SelectedEvidenceId,
                input.Candidate.Evidence.Evidence.EvidenceId)
            || state.Placements is not { } appliedPlacements
            || state.Evidence is not { } appliedEvidence
            || !ReferenceEquals(appliedPlacements.Entries.Single().WorldObject, input.Target)
            || !ReferenceEquals(appliedPlacements.Entries.Single().Location, input.Destination)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), input.Candidate.Evidence.Evidence)
            || !task.TryReset()
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Move task failed exact selection completion and reset.");
            return;
        }

        DisposeScenario();
        CreateScenario();
        CharacterObjectSabotageMoveDecision abstention = new(input.Character.Identity.Id, null);
        if (!task.TryStart(new ImmediateModel(abstention), input.Request, state))
        {
            Fail("Move task rejected an accepted abstention start.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartCancellation()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Move task failed exact abstention completion.");
            return;
        }

        WaitingModel waiting = new();
        if (!task.TryStart(waiting, input.Request, state)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Move task failed bounded caller cancellation.");
            return;
        }

        stage = 3;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Cancelled
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request, state))
        {
            Fail("Move task failed safe cancellation or failure start.");
            return;
        }

        stage = 4;
    }

    private void ObserveFailureAndStartStaleResult()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Failed
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Move task exposed a failed model result or changed state.");
            return;
        }

        deferredModel = new DeferredModel();
        if (!task.TryStart(deferredModel, input.Request, state)
            || !state.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Placements,
                input.Evidence,
                input.Candidate))
        {
            Fail("Move task could not establish its stale-result scenario.");
            return;
        }

        staleAppliedPlacements = state.Placements;
        staleAppliedEvidence = state.Evidence;
        deferredModel.Complete(SelectionDecision());
        stage = 5;
    }

    private void ObserveStaleResult()
    {
        if (task.Status == CharacterObjectSabotageMoveTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Placements, staleAppliedPlacements)
            || !ReferenceEquals(state.Evidence, staleAppliedEvidence)
            || !InputsAreUnchanged())
        {
            Fail("Move task partially installed a stale successful model result.");
            return;
        }

        GD.Print(
            "Move task controller smoke passed: exact selection and abstention resolution, "
            + "main-thread installation, overlap rejection, cancellation, safe failure, stale "
            + "result rejection, reset, closed surface, and unchanged surrounding state.");
        DisposeScenario();
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterObjectSabotageMoveDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request, state))
            && ThrowsArgumentNull(() => task.TryStart(model, null!, state))
            && ThrowsArgumentNull(() => task.TryStart(model, input.Request, null!));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageMoveTaskController);
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return properties.SequenceEqual(["Result", "Status"])
            && methods.SequenceEqual(["TryCancel", "TryReset", "TryStart", "_ExitTree", "_Process"])
            && Enum.GetValues<CharacterObjectSabotageMoveTaskStatus>().SequenceEqual(
                [
                    CharacterObjectSabotageMoveTaskStatus.Idle,
                    CharacterObjectSabotageMoveTaskStatus.Running,
                    CharacterObjectSabotageMoveTaskStatus.Completed,
                    CharacterObjectSabotageMoveTaskStatus.Cancelled,
                    CharacterObjectSabotageMoveTaskStatus.Failed,
                ]);
    }

    private void CreateScenario()
    {
        input = CreateInput();
        state = new CharacterObjectSabotageMoveController();
        task = new CharacterObjectSabotageMoveTaskController();
        AddChild(state);
        AddChild(task);
        if (!state.TryInitialize(input.Placements, input.Evidence))
        {
            throw new InvalidOperationException("The smoke state could not initialize.");
        }
    }

    private void DisposeScenario()
    {
        task.Free();
        state.Free();
    }

    private CharacterObjectSabotageMoveDecision SelectionDecision() =>
        new(input.Character.Identity.Id, input.Candidate.Evidence.Evidence.EvidenceId);

    private bool HasInitialState() =>
        ReferenceEquals(state.Placements, input.Placements)
        && ReferenceEquals(state.Evidence, input.Evidence);

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Placements.Entries.Single(), input.SourcePlacement)
        && input.Evidence.Entries.Count == 0
        && ReferenceEquals(input.Request.Character, input.Character)
        && ReferenceEquals(input.Request.PhysicalAccess, input.PhysicalAccess)
        && ReferenceEquals(input.Request.WorldObjects, input.WorldObjects)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Candidate);

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private static TaskInput CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterPhysicalAccessSnapshot access = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacement sourcePlacement = new(target, source);
        WorldObjectPlacementSnapshot placements = new([sourcePlacement]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            placements,
            evidence,
            [candidate]);

        return new(
            character,
            target,
            destination,
            access,
            worldObjects,
            placements,
            evidence,
            sourcePlacement,
            candidate,
            request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), $"Location {id}"),
            new LocationDescription($"Description for {id}."));

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterObjectSabotageMoveDecision decision)
        : ICharacterObjectSabotageMoveDecisionModel
    {
        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class WaitingModel : ICharacterObjectSabotageMoveDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageMoveDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterObjectSabotageMoveDecisionModel
    {
        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return ValueTask.FromException<CharacterObjectSabotageMoveDecision>(
                new InvalidOperationException("Expected smoke failure."));
        }
    }

    private sealed class DeferredModel : ICharacterObjectSabotageMoveDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageMoveDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return new ValueTask<CharacterObjectSabotageMoveDecision>(source.Task);
        }

        public void Complete(CharacterObjectSabotageMoveDecision decision) =>
            source.SetResult(decision);
    }

    private sealed record TaskInput(
        Character Character,
        WorldObject Target,
        Location Destination,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectPlacementSnapshot Placements,
        DiscoveryEvidenceSnapshot Evidence,
        WorldObjectPlacement SourcePlacement,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}

using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains and resolves a concealment decision.</summary>
public partial class CharacterObjectSabotageConcealmentTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterObjectSabotageConcealmentTaskController task = null!;
    private CharacterObjectSabotageConcealmentController state = null!;
    private TaskInput input = null!;
    private DeferredModel? deferredModel;
    private WorldObjectConcealmentSnapshot? staleAppliedConcealment;
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
            Fail("Concealment task did not finish within its bounded smoke duration.");
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
                Fail("Concealment task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CreateScenario();
        CharacterObjectSabotageConcealmentDecision decision = SelectionDecision();
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request, state)
            && task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running
            && task.Result is null
            && !task.TryStart(new ImmediateModel(decision), input.Request, state);
        if (!valid)
        {
            Fail("Concealment task rejected its exact start, overlap, or closed-surface contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.Decision.SelectedEvidenceId, input.Candidate.Evidence.EvidenceId)
            || state.Concealment is not { } appliedConcealment
            || state.Evidence is not { } appliedEvidence
            || !ReferenceEquals(
                appliedConcealment.ConcealedObjectIds.Single(),
                input.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), input.Candidate.Evidence)
            || !task.TryReset()
            || task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Concealment task failed exact selection completion and reset.");
            return;
        }

        DisposeScenario();
        CreateScenario();
        CharacterObjectSabotageConcealmentDecision abstention = new(input.Character.Identity.Id, null);
        if (!task.TryStart(new ImmediateModel(abstention), input.Request, state))
        {
            Fail("Concealment task rejected an accepted abstention start.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartCancellation()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Concealment task failed exact abstention completion.");
            return;
        }

        WaitingModel waiting = new();
        if (!task.TryStart(waiting, input.Request, state)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Concealment task failed bounded caller cancellation.");
            return;
        }

        stage = 3;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Cancelled
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request, state))
        {
            Fail("Concealment task failed safe cancellation or failure start.");
            return;
        }

        stage = 4;
    }

    private void ObserveFailureAndStartStaleResult()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Failed
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Concealment task exposed a failed model result or changed state.");
            return;
        }

        deferredModel = new DeferredModel();
        if (!task.TryStart(deferredModel, input.Request, state)
            || !state.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Concealment,
                input.Evidence,
                input.Candidate))
        {
            Fail("Concealment task could not establish its stale-result scenario.");
            return;
        }

        staleAppliedConcealment = state.Concealment;
        staleAppliedEvidence = state.Evidence;
        deferredModel.Complete(SelectionDecision());
        stage = 5;
    }

    private void ObserveStaleResult()
    {
        if (task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageConcealmentTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Concealment, staleAppliedConcealment)
            || !ReferenceEquals(state.Evidence, staleAppliedEvidence)
            || !InputsAreUnchanged())
        {
            Fail("Concealment task partially installed a stale successful model result.");
            return;
        }

        GD.Print(
            "Concealment task controller smoke passed: exact selection and abstention resolution, "
            + "main-thread installation, overlap rejection, cancellation, safe failure, stale "
            + "result rejection, reset, closed surface, and unchanged surrounding state.");
        DisposeScenario();
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterObjectSabotageConcealmentDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request, state))
            && ThrowsArgumentNull(() => task.TryStart(model, null!, state))
            && ThrowsArgumentNull(() => task.TryStart(model, input.Request, null!));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageConcealmentTaskController);
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
            && Enum.GetValues<CharacterObjectSabotageConcealmentTaskStatus>().SequenceEqual(
                [
                    CharacterObjectSabotageConcealmentTaskStatus.Idle,
                    CharacterObjectSabotageConcealmentTaskStatus.Running,
                    CharacterObjectSabotageConcealmentTaskStatus.Completed,
                    CharacterObjectSabotageConcealmentTaskStatus.Cancelled,
                    CharacterObjectSabotageConcealmentTaskStatus.Failed,
                ]);
    }

    private void CreateScenario()
    {
        input = CreateInput();
        state = new CharacterObjectSabotageConcealmentController();
        task = new CharacterObjectSabotageConcealmentTaskController();
        AddChild(state);
        AddChild(task);
        if (!state.TryInitialize(input.Concealment, input.Evidence))
        {
            throw new InvalidOperationException("The smoke state could not initialize.");
        }
    }

    private void DisposeScenario()
    {
        task.Free();
        state.Free();
    }

    private CharacterObjectSabotageConcealmentDecision SelectionDecision() =>
        new(input.Character.Identity.Id, input.Candidate.Evidence.EvidenceId);

    private bool HasInitialState() =>
        ReferenceEquals(state.Concealment, input.Concealment)
        && ReferenceEquals(state.Evidence, input.Evidence);

    private bool InputsAreUnchanged() =>
        input.Concealment.ConcealedObjectIds.Count == 0
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
        CharacterPhysicalAccessSnapshot access = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            concealment,
            evidence,
            [candidate]);

        return new(character, target, access, worldObjects, concealment, evidence, candidate, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterObjectSabotageConcealmentDecision decision)
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class WaitingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageConcealmentDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return ValueTask.FromException<CharacterObjectSabotageConcealmentDecision>(
                new InvalidOperationException("Expected smoke failure."));
        }
    }

    private sealed class DeferredModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageConcealmentDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return new ValueTask<CharacterObjectSabotageConcealmentDecision>(source.Task);
        }

        public void Complete(CharacterObjectSabotageConcealmentDecision decision) =>
            source.SetResult(decision);
    }

    private sealed record TaskInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}

using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains and resolves a contamination decision.</summary>
public partial class CharacterObjectSabotageContaminationTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterObjectSabotageContaminationTaskController task = null!;
    private CharacterObjectSabotageContaminationController state = null!;
    private TaskInput input = null!;
    private DeferredModel? deferredModel;
    private WorldObjectContaminationSnapshot? staleContamination;
    private DiscoveryEvidenceSnapshot? staleEvidence;
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
            Fail("Contamination task did not finish within its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartCancellation();
                break;
            case 2:
                ObserveCancellationAndStartFailure();
                break;
            case 3:
                ObserveFailureAndStartStaleResult();
                break;
            case 4:
                ObserveStaleResult();
                break;
            default:
                Fail("Contamination task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CreateScenario();
        CharacterObjectSabotageContaminationDecision decision = SelectionDecision();
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterObjectSabotageContaminationTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request, state)
            && task.Status == CharacterObjectSabotageContaminationTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request, state);
        if (!valid)
        {
            Fail("Contamination task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.SelectedCandidate, input.Candidate)
            || state.Contamination is not { } appliedContamination
            || state.Evidence is not { } appliedEvidence
            || !ReferenceEquals(appliedContamination.ContaminatedObjectIds.Single(), input.Target.Identity.Id)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), input.Candidate.Evidence)
            || !task.TryReset())
        {
            Fail("Contamination task failed exact selection completion and reset.");
            return;
        }

        DisposeScenario();
        CreateScenario();
        if (!task.TryStart(new WaitingModel(), input.Request, state)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Contamination task failed bounded caller cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Cancelled
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request, state))
        {
            Fail("Contamination task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartStaleResult()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Failed
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Contamination task exposed a failed result or changed state.");
            return;
        }

        deferredModel = new DeferredModel();
        if (!task.TryStart(deferredModel, input.Request, state)
            || !state.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Contamination,
                input.Evidence,
                input.Candidate))
        {
            Fail("Contamination task could not establish its stale-result scenario.");
            return;
        }

        staleContamination = state.Contamination;
        staleEvidence = state.Evidence;
        deferredModel.Complete(SelectionDecision());
        stage = 4;
    }

    private void ObserveStaleResult()
    {
        if (task.Status == CharacterObjectSabotageContaminationTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageContaminationTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Contamination, staleContamination)
            || !ReferenceEquals(state.Evidence, staleEvidence)
            || !InputsAreUnchanged())
        {
            Fail("Contamination task partially installed a stale successful model result.");
            return;
        }

        GD.Print(
            "Contamination task controller smoke passed: exact selection resolution, main-thread "
            + "installation, overlap rejection, cancellation, safe failure, stale-result rejection, "
            + "reset, closed surface, and unchanged request state.");
        DisposeScenario();
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterObjectSabotageContaminationDecisionModel model = new FailingModel();
        CharacterObjectSabotageContaminationConsiderationRequest staleRequest = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new WorldObjectContaminationSnapshot(input.Contamination.ContaminatedObjectIds),
            input.Evidence,
            [input.Candidate]);

        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request, state))
            && ThrowsArgumentNull(() => task.TryStart(model, null!, state))
            && ThrowsArgumentNull(() => task.TryStart(model, input.Request, null!))
            && !task.TryStart(model, staleRequest, state)
            && HasInitialState();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageContaminationTaskController);
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
            && Enum.GetValues<CharacterObjectSabotageContaminationTaskStatus>().SequenceEqual(
                [
                    CharacterObjectSabotageContaminationTaskStatus.Idle,
                    CharacterObjectSabotageContaminationTaskStatus.Running,
                    CharacterObjectSabotageContaminationTaskStatus.Completed,
                    CharacterObjectSabotageContaminationTaskStatus.Cancelled,
                    CharacterObjectSabotageContaminationTaskStatus.Failed,
                ]);
    }

    private void CreateScenario()
    {
        input = CreateInput();
        state = new CharacterObjectSabotageContaminationController();
        task = new CharacterObjectSabotageContaminationTaskController();
        AddChild(state);
        AddChild(task);
        if (!state.TryInitialize(input.Contamination, input.Evidence))
        {
            throw new InvalidOperationException("The smoke state could not initialize.");
        }
    }

    private void DisposeScenario()
    {
        task.Free();
        state.Free();
    }

    private CharacterObjectSabotageContaminationDecision SelectionDecision() =>
        new(input.Character.Identity.Id, input.Candidate.Evidence.EvidenceId);

    private bool HasInitialState() =>
        ReferenceEquals(state.Contamination, input.Contamination)
        && ReferenceEquals(state.Evidence, input.Evidence);

    private bool InputsAreUnchanged() =>
        input.Contamination.ContaminatedObjectIds.Count == 0
        && input.Evidence.Entries.Count == 0
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
        WorldObjectContaminationSnapshot contamination = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        CharacterObjectSabotageEvidence candidate = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageContaminationConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            contamination,
            evidence,
            [candidate]);

        return new(character, target, access, worldObjects, contamination, evidence, candidate, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterObjectSabotageContaminationDecision decision)
        : ICharacterObjectSabotageContaminationDecisionModel
    {
        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(decision);
    }

    private sealed class WaitingModel : ICharacterObjectSabotageContaminationDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageContaminationDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterObjectSabotageContaminationDecisionModel
    {
        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterObjectSabotageContaminationDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed class DeferredModel : ICharacterObjectSabotageContaminationDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageContaminationDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
            CharacterObjectSabotageContaminationConsiderationRequest request,
            CancellationToken cancellationToken) =>
            new(source.Task);

        public void Complete(CharacterObjectSabotageContaminationDecision decision) =>
            source.SetResult(decision);
    }

    private sealed record TaskInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageContaminationConsiderationRequest Request);
}

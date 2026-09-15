using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains and resolves a substitution decision.</summary>
public partial class CharacterObjectSabotageSubstitutionTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterObjectSabotageSubstitutionTaskController task = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;
    private TaskInput input = null!;
    private DeferredModel? deferredModel;
    private CharacterObjectSabotageSubstitutionSnapshot? staleSubstitutions;
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
            Fail("Substitution task did not finish within its bounded smoke duration.");
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
                Fail("Substitution task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CreateScenario();
        CharacterObjectSabotageSubstitutionDecision decision = SelectionDecision();
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request, state)
            && task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running
            && task.Result is null
            && !task.TryStart(new ImmediateModel(decision), input.Request, state);
        if (!valid)
        {
            Fail("Substitution task rejected its exact start, overlap, or closed-surface contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.SelectedCandidate, input.Candidate)
            || state.Substitutions is not { } appliedSubstitutions
            || state.Evidence is not { } appliedEvidence
            || !ReferenceEquals(appliedSubstitutions.Entries.Single(), input.Candidate.Substitution)
            || !ReferenceEquals(appliedEvidence.Entries.Single(), input.Candidate.Evidence.Evidence)
            || !task.TryReset()
            || task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Idle
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Substitution task failed exact selection completion and reset.");
            return;
        }

        DisposeScenario();
        CreateScenario();
        CharacterObjectSabotageSubstitutionDecision abstention = new(
            input.Character.Identity.Id,
            null);
        if (!task.TryStart(new ImmediateModel(abstention), input.Request, state))
        {
            Fail("Substitution task rejected an accepted abstention start.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartCancellation()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Substitution task failed exact abstention completion.");
            return;
        }

        if (!task.TryStart(new WaitingModel(), input.Request, state)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Substitution task failed bounded caller cancellation.");
            return;
        }

        stage = 3;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Cancelled
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request, state))
        {
            Fail("Substitution task failed safe cancellation or failure start.");
            return;
        }

        stage = 4;
    }

    private void ObserveFailureAndStartStaleResult()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Failed
            || task.Result is not null
            || !HasInitialState()
            || !task.TryReset())
        {
            Fail("Substitution task exposed a failed model result or changed state.");
            return;
        }

        deferredModel = new DeferredModel();
        if (!task.TryStart(deferredModel, input.Request, state)
            || !state.TryApply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Substitutions,
                input.Evidence,
                input.Candidate))
        {
            Fail("Substitution task could not establish its stale-result scenario.");
            return;
        }

        staleSubstitutions = state.Substitutions;
        staleEvidence = state.Evidence;
        deferredModel.Complete(SelectionDecision());
        stage = 5;
    }

    private void ObserveStaleResult()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Substitutions, staleSubstitutions)
            || !ReferenceEquals(state.Evidence, staleEvidence)
            || !InputsAreUnchanged())
        {
            Fail("Substitution task partially installed a stale successful model result.");
            return;
        }

        GD.Print(
            "Substitution task controller smoke passed: exact selection and abstention resolution, "
            + "main-thread installation, overlap rejection, cancellation, safe failure, stale "
            + "result rejection, reset, closed surface, and unchanged surrounding state.");
        DisposeScenario();
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterObjectSabotageSubstitutionDecisionModel model = new FailingModel();
        CharacterObjectSabotageSubstitutionConsiderationRequest staleRequest = new(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            new CharacterObjectSabotageSubstitutionSnapshot(input.Substitutions.Entries),
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
        Type type = typeof(CharacterObjectSabotageSubstitutionTaskController);
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
            && Enum.GetValues<CharacterObjectSabotageSubstitutionTaskStatus>().SequenceEqual(
                [
                    CharacterObjectSabotageSubstitutionTaskStatus.Idle,
                    CharacterObjectSabotageSubstitutionTaskStatus.Running,
                    CharacterObjectSabotageSubstitutionTaskStatus.Completed,
                    CharacterObjectSabotageSubstitutionTaskStatus.Cancelled,
                    CharacterObjectSabotageSubstitutionTaskStatus.Failed,
                ]);
    }

    private void CreateScenario()
    {
        input = CreateInput();
        state = new CharacterObjectSabotageSubstitutionController();
        task = new CharacterObjectSabotageSubstitutionTaskController();
        AddChild(state);
        AddChild(task);
        if (!state.TryInitialize(input.Substitutions, input.Evidence))
        {
            throw new InvalidOperationException("The smoke state could not initialize.");
        }
    }

    private void DisposeScenario()
    {
        task.Free();
        state.Free();
    }

    private CharacterObjectSabotageSubstitutionDecision SelectionDecision() =>
        new(input.Character.Identity.Id, input.Candidate.Evidence.Evidence.EvidenceId);

    private bool HasInitialState() =>
        ReferenceEquals(state.Substitutions, input.Substitutions)
        && ReferenceEquals(state.Evidence, input.Evidence);

    private bool InputsAreUnchanged() =>
        input.Substitutions.Entries.Count == 0
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
        WorldObject target = WorldObject("village-water-cask", "Village water cask");
        CharacterPhysicalAccessSnapshot access = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(
                occurrence,
                WorldObject("tainted-water-cask", "Tainted water cask")),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            substitutions,
            evidence,
            [candidate]);

        return new(character, access, worldObjects, substitutions, evidence, candidate, request);
    }

    private static WorldObject WorldObject(string id, string name) =>
        new(new WorldObjectIdentity(new ObjectId(id), name), new WorldObjectDescription(name));

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterObjectSabotageSubstitutionDecision decision)
        : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class WaitingModel : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageSubstitutionDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return ValueTask.FromException<CharacterObjectSabotageSubstitutionDecision>(
                new InvalidOperationException("Expected smoke failure."));
        }
    }

    private sealed class DeferredModel : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        private readonly TaskCompletionSource<CharacterObjectSabotageSubstitutionDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return new ValueTask<CharacterObjectSabotageSubstitutionDecision>(source.Task);
        }

        public void Complete(CharacterObjectSabotageSubstitutionDecision decision) =>
            source.SetResult(decision);
    }

    private sealed record TaskInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}

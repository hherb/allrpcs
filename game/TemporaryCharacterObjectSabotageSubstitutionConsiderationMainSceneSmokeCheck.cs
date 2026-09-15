using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored substitution consideration in the main scene.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionConsiderationMainSceneSmokeCheck
    : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_CONSIDERATION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSubstitutionSource source = null!;
    private CharacterObjectSabotageSubstitutionController state = null!;
    private TemporaryCharacterObjectSabotageSubstitutionModelSource modelSource = null!;
    private CharacterObjectSabotageSubstitutionTaskController task = null!;
    private TemporaryCharacterObjectSabotageSubstitutionConsiderationFlowController flow = null!;
    private RecordingModel? selectionModel;
    private RecordingModel? overlapModel;
    private CharacterObjectSabotageSubstitutionSnapshot initialSubstitutions = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private CharacterObjectSabotageSubstitutionSnapshot? appliedSubstitutions;
    private DiscoveryEvidenceSnapshot? appliedEvidence;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        if (!isEnabled)
        {
            SetProcess(false);
            return;
        }

        source = GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionSource");
        state = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        modelSource = GetNode<TemporaryCharacterObjectSabotageSubstitutionModelSource>(
            "../TemporaryCharacterObjectSabotageSubstitutionModelSource");
        task = GetNode<CharacterObjectSabotageSubstitutionTaskController>(
            "../CharacterObjectSabotageSubstitutionTaskController");
        flow = GetNode<TemporaryCharacterObjectSabotageSubstitutionConsiderationFlowController>(
            "../TemporaryCharacterObjectSabotageSubstitutionConsiderationFlowController");
        initialSubstitutions = source.Substitutions;
        initialEvidence = source.Evidence;
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Temporary substitution consideration did not finish within its smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartExactSelection();
                break;
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Temporary substitution consideration entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactSelection()
    {
        selectionModel = new RecordingModel(request => new(
            request.Character.Identity.Id,
            request.Candidates.Single().Evidence.Evidence.EvidenceId));
        overlapModel = new RecordingModel(request => new(request.Character.Identity.Id, null));

        bool unchangedBeforeRequest = ReferenceEquals(state.Substitutions, initialSubstitutions)
            && ReferenceEquals(state.Evidence, initialEvidence)
            && task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Idle
            && task.Result is null;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running;
        bool overlapRetained = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!unchangedBeforeRequest || !started || !overlapRetained || !HasClosedSurface())
        {
            Fail("Temporary substitution consideration failed explicit start or overlap handling.");
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

        appliedSubstitutions = state.Substitutions;
        appliedEvidence = state.Evidence;
        CharacterObjectSabotageSubstitutionConsiderationRequest? request = selectionModel!.Request;
        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || task.Result is not { } result
            || request is null
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Substitutions, initialSubstitutions)
            || !ReferenceEquals(request.Evidence, initialEvidence)
            || !ReferenceEquals(request.Candidates.Single(), source.Candidate)
            || appliedSubstitutions is null
            || appliedEvidence is null
            || !ReferenceEquals(
                appliedSubstitutions.Entries.Single(),
                source.Candidate.Substitution)
            || !ReferenceEquals(
                appliedEvidence.Entries.Single(),
                source.Candidate.Evidence.Evidence)
            || overlapModel!.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart())
        {
            Fail("Temporary substitution consideration failed exact request or resolution.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || overlapModel!.CallCount != 1
            || !ReferenceEquals(state.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !task.TryReset()
            || !flow.TryStart())
        {
            Fail("Temporary substitution abstention changed exact scene state.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterObjectSabotageSubstitutionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterObjectSabotageSubstitutionTaskStatus.Failed
            || task.Result is not null
            || !ReferenceEquals(state.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || source.Substitutions.Entries.Count != 0
            || source.Evidence.Entries.Count != 0)
        {
            Fail("Temporary substitution consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Temporary substitution consideration main-scene smoke passed: no automatic effect, "
            + "exact request composition, one-shot selection and abstention, atomic main-thread "
            + "resolution, overlap preservation, stable offline failure, closed surface, and "
            + "unchanged source state.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSubstitutionConsiderationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterObjectSabotageSubstitutionConsiderationRequest,
            CharacterObjectSabotageSubstitutionDecision> decide)
        : ICharacterObjectSabotageSubstitutionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageSubstitutionConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
            CharacterObjectSabotageSubstitutionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}

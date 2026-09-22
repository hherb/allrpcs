using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact composed sabotage-action consideration through main-scene dependencies.</summary>
public partial class CharacterSabotageActionRequestMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_ACTION_REQUEST_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterObjectSabotageSource source = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private CharacterSabotageActionConcealmentRequestController concealmentAuthor = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterSabotageActionRequestConsiderationFlowController flow = null!;
    private CharacterSabotageActionSelectionCoordinator selection = null!;
    private WorldObjectConcealmentSnapshot initialConcealment = null!;
    private DiscoveryEvidenceSnapshot initialEvidence = null!;
    private RecordingModel model = null!;
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

        source = GetNode<TemporaryCharacterObjectSabotageSource>(
            "../TemporaryCharacterObjectSabotageSource");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        concealmentAuthor = GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController");
        authors = GetAuthors();
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        flow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        selection = GetNode<CharacterSabotageActionSelectionCoordinator>(
            "../CharacterSabotageActionSelectionCoordinator");
        initialConcealment = concealmentState.Concealment!;
        initialEvidence = concealmentState.Evidence!;
        model = new(value => new(value.Actor.Identity.Id, CharacterSabotageActionKind.Conceal));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Main-scene composed sabotage-action request exceeded its bounded duration.");
            return;
        }

        if (stage == 0)
        {
            StartExactRequest();
            return;
        }

        if (stage == 1)
        {
            ObserveCompletedSelection();
            return;
        }

        Fail("Main-scene composed sabotage-action request entered an unknown stage.");
    }

    private void StartExactRequest()
    {
        bool inertStartup = authors.All(author => author.Request is null)
            && composer.Request is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && selection.ObservedResult is null
            && StateIsUnchanged();
        bool authored = concealmentAuthor.TryCreate(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            initialConcealment,
            initialEvidence,
            source.Candidate);
        CharacterSabotageActionConsiderationRequest?[] outputs =
            authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(source.Character, outputs);
        if (!composed || composer.Request is not { } request)
        {
            Fail("Main-scene exact author output did not compose one action request.");
            return;
        }

        bool started = modelSource.TrySupplyModel(model)
            && flow.TryStart(request)
            && task.Status == CharacterSabotageActionTaskStatus.Running;

        if (!inertStartup
            || !authored
            || outputs.Count(output => output is not null) != 1
            || !ReferenceEquals(outputs[0], concealmentAuthor.Request)
            || !started
            || !RequestIsExact(request)
            || !StateIsUnchanged())
        {
            Fail("Main-scene sabotage-action request failed inert exact composition or start.");
            return;
        }

        stage = 1;
    }

    private void ObserveCompletedSelection()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        CharacterSabotageActionConsiderationRequest request = composer.Request!;
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Conceal)
            || model.CallCount != 1
            || !ReferenceEquals(model.Request, request)
            || selection.ObservedResult is not null
            || !RequestIsExact(request)
            || !StateIsUnchanged())
        {
            Fail("Main-scene composed sabotage-action selection changed authoritative state.");
            return;
        }

        GD.Print(
            "Main-scene composed sabotage-action request smoke passed: ordinary startup stayed "
            + "inert; one exact live conceal request completed selection without observation, "
            + "acknowledgement, dispatch, application, or authoritative-state mutation.");
        GetTree().Quit();
    }

    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool RequestIsExact(CharacterSabotageActionConsiderationRequest request) =>
        ReferenceEquals(request.Actor, source.Character)
        && request.ActionKinds.Count == 1
        && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(authors[0].Request, concealmentAuthor.Request)
        && authors.Skip(1).All(author => author.Request is null);

    private bool StateIsUnchanged() =>
        ReferenceEquals(concealmentState.Concealment, initialConcealment)
        && ReferenceEquals(concealmentState.Evidence, initialEvidence)
        && initialConcealment.ConcealedObjectIds.Count == 0
        && initialEvidence.Entries.Count == 0;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterSabotageActionConsiderationRequest, CharacterSabotageActionDecision> decide)
        : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }
}

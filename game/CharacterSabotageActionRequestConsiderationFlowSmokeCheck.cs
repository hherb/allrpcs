using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit consideration of one exact composed sabotage-action request.</summary>
public partial class CharacterSabotageActionRequestConsiderationFlowSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterSabotageActionRequestComposer composer = null!;
    private CharacterSabotageActionRequestConsiderationFlowController flow = null!;
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterSabotageActionConsiderationRequest request = null!;
    private CharacterSabotageActionConsiderationRequest?[] authorOutputs = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel abstentionModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        flow = GetNode<CharacterSabotageActionRequestConsiderationFlowController>(
            "../CharacterSabotageActionRequestConsiderationFlowController");
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        selectionModel = new(input => new(input.Actor.Identity.Id, input.ActionKinds[^1]));
        abstentionModel = new(input => new(input.Actor.Identity.Id, null));

        StartExactComposedRequest();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Composed sabotage-action request flow exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndRepeatedStart();
                break;
            default:
                Fail("Composed sabotage-action request flow entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactComposedRequest()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionRequestAuthorStub[] authors = GetAuthors();
        authors[0].Request = new(actor, [CharacterSabotageActionKind.Conceal]);
        authors[5].Request = new(actor, [CharacterSabotageActionKind.Delay]);
        authorOutputs = authors.Select(author => author.Request).ToArray();
        bool composed = composer.TryCompose(actor, authorOutputs);
        if (!composed || composer.Request is not { } composedRequest)
        {
            Fail("Exact author outputs did not compose one sabotage-action request.");
            return;
        }

        request = composedRequest;
        CharacterSabotageActionConsiderationRequest stale = new(actor, request.ActionKinds);
        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));

        bool unavailableAndNullRejected = modelSource.TrySupplyModel(selectionModel)
            && !flow.TryStart(stale)
            && ThrowsArgumentNull(() => flow.TryStart(null!))
            && !modelSource.TrySupplyModel(preservationProbe)
            && task.Status == CharacterSabotageActionTaskStatus.Idle;
        bool started = flow.TryStart(request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && modelSource.TrySupplyModel(abstentionModel)
            && !flow.TryStart(request)
            && !modelSource.TrySupplyModel(preservationProbe);

        if (!unavailableAndNullRejected
            || !started
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Composed sabotage-action request did not fail closed before exact start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || flow.TryStart(request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Composed sabotage-action selection did not preserve exact queued ownership.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndRepeatedStart()
    {
        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { SelectedActionKind: null }
            || !ReferenceEquals(task.Result.Request, request)
            || abstentionModel.CallCount != 1
            || !ReferenceEquals(abstentionModel.Request, request)
            || !modelSource.TrySupplyModel(preservationProbe)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(selectionModel)
            || preservationProbe.CallCount != 0
            || !InputsAreUnchanged())
        {
            Fail("Composed sabotage-action abstention changed state after explicit completion.");
            return;
        }

        GD.Print(
            "Composed sabotage-action request flow smoke passed: unavailable, stale, null, overlap, "
            + "and repeated starts preserved queued models; exact selection and abstention completed "
            + "without dispatching or applying an action.");
        GetTree().Quit();
    }

    private CharacterSabotageActionRequestAuthorStub[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionRequestAuthorStub>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private bool InputsAreUnchanged() =>
        ReferenceEquals(composer.Request, request)
        && authorOutputs.Select((output, index) =>
            ReferenceEquals(output, GetAuthors()[index].Request)).All(value => value)
        && request.ActionKinds.Count == 2
        && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Conceal)
        && ReferenceEquals(request.ActionKinds[1], CharacterSabotageActionKind.Delay);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionRequestConsiderationFlowController);
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

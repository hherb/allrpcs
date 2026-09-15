using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit misquotation-evidence consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController flow = null!;
    private TaskInput input = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        flow = GetNode<TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController>(
            "../TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController");
        input = CreateInput();
        selectionModel = new RecordingModel(request => new(
            request.Actor.Identity.Id,
            request.Candidates.Single()));
        overlapModel = new RecordingModel(request => new(request.Actor.Identity.Id, null));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Misquotation-evidence consideration flow exceeded its bounded smoke duration.");
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
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Misquotation-evidence consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0
            && ThrowsArgumentNull(() => flow.TryStart(null!));
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart(input.Request)
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart(input.Request);

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Misquotation-evidence consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, input.Request)
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.SelectedEvidence, input.Evidence)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart(input.Request))
        {
            Fail("Misquotation-evidence consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { SelectedEvidence: null }
            || overlapModel.CallCount != 1
            || !ReferenceEquals(overlapModel.Request, input.Request)
            || !task.TryReset()
            || !flow.TryStart(input.Request))
        {
            Fail("Misquotation-evidence consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Misquotation-evidence consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Misquotation-evidence consideration flow smoke passed: inert startup, exact request "
            + "and one-shot selection, overlap preservation, explicit reset, stable offline "
            + "failure, closed surface, and unchanged dialogue evidence.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController);
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

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Evidence)
        && ReferenceEquals(input.Evidence.Occurrence.Proposal.SourceEvent, input.SourceEvent)
        && ReferenceEquals(input.Evidence.Occurrence.DeliveredEvent, input.DeliveredEvent);

    private static TaskInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witness = new("seeker");
        CharacterId sourceSpeaker = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeaker, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(sourceSpeaker, [actor.Identity.Id, witness]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(proposal.ActorId, [witness]));
        CharacterDialogueMisquotationEvidence evidence = new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witness);
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(actor, [evidence]);
        return new(actor, sourceEvent, deliveredEvent, evidence, request);
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
        Func<CharacterDialogueMisquotationEvidenceConsiderationRequest,
            CharacterDialogueMisquotationEvidenceDecision> decide)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }

    private sealed record TaskInput(
        Character Actor,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}

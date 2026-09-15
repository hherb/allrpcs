using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one bounded scene task that obtains an exact misquotation-evidence decision.</summary>
public partial class CharacterDialogueMisquotationEvidenceTaskControllerSmokeCheck : Node
{
    private const int MaximumProcessFrames = 300;

    private CharacterDialogueMisquotationEvidenceTaskController task = null!;
    private TaskInput input = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        task = new CharacterDialogueMisquotationEvidenceTaskController();
        AddChild(task);
        input = CreateInput();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Misquotation-evidence task did not finish within its bounded smoke duration.");
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
                ObserveFailureAndStartInvalidDecision();
                break;
            case 4:
                ObserveInvalidDecision();
                break;
            default:
                Fail("Misquotation-evidence task entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Evidence);
        bool valid = HasClosedSurface()
            && RejectsInvalidInputs()
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && task.Result is null
            && !task.TryCancel()
            && !task.TryReset()
            && task.TryStart(new ImmediateModel(decision), input.Request)
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running
            && !task.TryStart(new ImmediateModel(decision), input.Request);
        if (!valid)
        {
            Fail("Misquotation-evidence task rejected its exact start or overlap contract.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartCancellation()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, input.Request)
            || !ReferenceEquals(result.Decision.ActorId, input.Actor.Identity.Id)
            || !ReferenceEquals(result.SelectedEvidence, input.Evidence)
            || !task.TryReset()
            || task.Result is not null)
        {
            Fail("Misquotation-evidence task failed exact completion and reset.");
            return;
        }

        if (!task.TryStart(new WaitingModel(), input.Request)
            || !task.TryCancel()
            || task.TryCancel())
        {
            Fail("Misquotation-evidence task failed bounded caller cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Cancelled
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new FailingModel(), input.Request))
        {
            Fail("Misquotation-evidence task failed safe cancellation or failure start.");
            return;
        }

        stage = 3;
    }

    private void ObserveFailureAndStartInvalidDecision()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        CharacterDialogueMisquotationEvidenceDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Evidence);
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || task.Result is not null
            || !task.TryReset()
            || !task.TryStart(new ImmediateModel(invalid), input.Request))
        {
            Fail("Misquotation-evidence task exposed a failed result or rejected its reset.");
            return;
        }

        stage = 4;
    }

    private void ObserveInvalidDecision()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || task.Result is not null
            || !InputsAreUnchanged())
        {
            Fail("Misquotation-evidence task accepted invalid output or changed request state.");
            return;
        }

        GD.Print(
            "Misquotation-evidence task controller smoke passed: exact main-thread completion, "
            + "overlap rejection, cancellation, safe model and validation failure, reset, "
            + "closed surface, and unchanged request state.");
        GetTree().Quit();
    }

    private bool RejectsInvalidInputs()
    {
        ICharacterDialogueMisquotationEvidenceDecisionModel model = new FailingModel();
        return ThrowsArgumentNull(() => task.TryStart(null!, input.Request))
            && ThrowsArgumentNull(() => task.TryStart(model, null!))
            && InputsAreUnchanged();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceTaskController);
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
            && Enum.GetValues<CharacterDialogueMisquotationEvidenceTaskStatus>().SequenceEqual(
                [
                    CharacterDialogueMisquotationEvidenceTaskStatus.Idle,
                    CharacterDialogueMisquotationEvidenceTaskStatus.Running,
                    CharacterDialogueMisquotationEvidenceTaskStatus.Completed,
                    CharacterDialogueMisquotationEvidenceTaskStatus.Cancelled,
                    CharacterDialogueMisquotationEvidenceTaskStatus.Failed,
                ]);
    }

    private bool InputsAreUnchanged() =>
        ReferenceEquals(input.Request.Actor, input.Actor)
        && ReferenceEquals(input.Request.Candidates.Single(), input.Evidence)
        && ReferenceEquals(input.Evidence.Occurrence.Proposal.SourceEvent, input.SourceEvent)
        && ReferenceEquals(input.Evidence.Occurrence.DeliveredEvent, input.DeliveredEvent);

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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterDialogueMisquotationEvidenceDecision decision)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed class WaitingModel : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        private readonly TaskCompletionSource<CharacterDialogueMisquotationEvidenceDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class FailingModel : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterDialogueMisquotationEvidenceDecision>(
                new InvalidOperationException("Expected smoke failure."));
    }

    private sealed record TaskInput(
        Character Actor,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}

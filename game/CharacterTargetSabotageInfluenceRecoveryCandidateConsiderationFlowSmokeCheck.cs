using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit consideration of the exact live recovery-candidate request.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController flow =
        null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController requests = null!;
    private TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource modelSource = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController task = null!;
    private CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidate candidate = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel abstentionModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        flow =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController");
        requests = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource>(
            "../TemporaryCharacterTargetSabotageInfluenceRecoveryModelSource");
        task = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        selectionModel = new(input => new(
            input.Actor.Identity.Id,
            input.Candidates.Single()));
        abstentionModel = new(input => new(input.Actor.Identity.Id, null));

        StartExactLiveRequest();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Live influence-recovery request flow exceeded its bounded smoke duration.");
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
                Fail("Live influence-recovery request flow entered an unknown smoke stage.");
                break;
        }
    }

    private void StartExactLiveRequest()
    {
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest staleRequest =
            CreateStandaloneRequest();
        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        bool unavailableAndNullRejected = modelSource.TrySupplyModel(selectionModel)
            && !flow.TryStart(staleRequest)
            && ThrowsArgumentNull(() => flow.TryStart(null!))
            && !modelSource.TrySupplyModel(preservationProbe)
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle;

        bool liveRequestCreated = TryCreateLiveRequest();
        request = requests.Request!;
        candidate = request.Candidates.Single();
        bool started = !flow.TryStart(staleRequest)
            && flow.TryStart(request)
            && task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running
            && modelSource.TrySupplyModel(abstentionModel)
            && !flow.TryStart(request)
            && !modelSource.TrySupplyModel(preservationProbe);

        if (!unavailableAndNullRejected
            || !liveRequestCreated
            || !started
            || !HasClosedSurface()
            || !LiveRequestIsUnchanged())
        {
            Fail("Live influence-recovery request did not fail closed before exact start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, candidate)
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(preservationProbe)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Live influence-recovery request lost its queued model after selection.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndRepeatedStart()
    {
        if (task.Status == CharacterTargetSabotageInfluenceRecoveryTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || task.Result is not { SelectedCandidate: null }
            || abstentionModel.CallCount != 1
            || !ReferenceEquals(abstentionModel.Request, request)
            || !modelSource.TrySupplyModel(preservationProbe)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(selectionModel)
            || preservationProbe.CallCount != 0
            || !LiveRequestIsUnchanged())
        {
            Fail("Live influence-recovery request changed state after explicit abstention.");
            return;
        }

        GD.Print(
            "Live influence-recovery candidate consideration flow smoke passed: unavailable, "
            + "stale, null, overlap, and repeated starts preserved the queued model; exact "
            + "selection and abstention completed for the one live request.");
        GetTree().Quit();
    }

    private bool TryCreateLiveRequest()
    {
        CharacterTargetSabotageInfluenceController influence =
            GetNode<CharacterTargetSabotageInfluenceController>(
                "../CharacterTargetSabotageInfluenceController");
        CharacterTargetSabotageInfluenceRecoveryCandidateController candidates =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceConsiderationResult selection = new(
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                [input.Candidate]),
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                input.Candidate));
        return influence.TryInitialize(input.Relationship)
            && influence.TryResolveConsideration(selection)
            && candidates.TryCreate(influence.LatestAppliedResult!, RecoveryChange(input))
            && requests.TryCreate(candidates.Candidate!);
    }

    private bool LiveRequestIsUnchanged() =>
        ReferenceEquals(requests.Request, request)
        && request.Candidates.Count == 1
        && ReferenceEquals(request.Candidates.Single(), candidate)
        && ReferenceEquals(request.Actor, candidate.InfluenceResult.Candidate.Occurrence.Target)
        && ReferenceEquals(request.Relationship, candidate.InfluenceResult.Relationship)
        && CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
            request.Relationship,
            candidate);

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController);
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

    private static CharacterTargetSabotageInfluenceRecoveryConsiderationRequest
        CreateStandaloneRequest()
    {
        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceResult result =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            result,
            RecoveryChange(input));
        return new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
            input.Target,
            result.Relationship,
            [candidate]);
    }

    private static CharacterRelationshipChange RecoveryChange(InfluenceInput input) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("live-influence-recovery"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("live-influence-recovery"),
                "The target reconsidered the enemy agent's influence."));

    private static InfluenceInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            target);
        CharacterTargetSabotageInfluenceCandidate candidate = new(
            occurrence,
            new CharacterRelationshipChange(
                target.Identity.Id,
                actor.Identity.Id,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId("live-influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("live-influence-attempt"),
                    "The enemy agent attempted to gain influence.")));
        return new InfluenceInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(
                    CharacterRelationshipDimension.Trust,
                    10)],
                []),
            candidate);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel(
        Func<CharacterTargetSabotageInfluenceRecoveryConsiderationRequest,
            CharacterTargetSabotageInfluenceRecoveryDecision> decide)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request
        {
            get;
            private set;
        }

        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(decide(value));
        }
    }

    private sealed record InfluenceInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}

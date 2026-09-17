using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoveryConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterTargetSabotageInfluenceRecoveryConsiderationResult result =
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token);

        Assert.Equal(1, model.CallCount);
        Assert.Same(input.Request, model.Request);
        Assert.Equal(cancellation.Token, model.CancellationToken);
        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Candidate, result.SelectedCandidate);
    }

    [Fact]
    public async Task RetainsExactAcceptedAbstention()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageInfluenceRecoveryDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageInfluenceRecoveryConsiderationResult result =
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None);

        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public async Task RejectsInvalidOrNullDecisionWithoutChangingRequest()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageInfluenceRecoveryDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                new RecordingModel(invalid),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                new NullModel(),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Same(input.InfluenceReason, input.Relationship.Reasons.Single());
        Assert.Same(input.RecoveryReason, input.Candidate.RecoveryChange.Reason);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        ControllerInput input = CreateInput();
        RecordingModel model = new(new(input.Actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                model,
                input.Request,
                cancellation.Token));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public async Task PropagatesModelCancellationAndFailureUnchanged()
    {
        ControllerInput input = CreateInput();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        InvalidOperationException expected = new("model failed");

        OperationCanceledException cancelled = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
                await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                    new CancellingModel(cancellation.Token),
                    input.Request,
                    CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await CharacterTargetSabotageInfluenceRecoveryConsiderationController.DecideAsync(
                    new FailingModel(expected),
                    input.Request,
                    CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterTargetSabotageInfluenceRecoveryDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision>),
            modelMethod.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest),
                typeof(CancellationToken),
            ],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(
            CharacterTargetSabotageInfluenceRecoveryConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTargetSabotageInfluenceRecoveryConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterTargetSabotageInfluenceRecoveryDecisionModel),
                typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput CreateInput()
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            actor);
        CharacterRelationshipReason influenceReason = new(
            new RelationshipReasonId("influence-attempt"),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId("enemy-agent-influence"),
            "The enemy agent attempted to gain influence.");
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            influenceReason);
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangeTransition.Apply(priorRelationship, influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            relationship);
        CharacterRelationshipReason recoveryReason = new(
            new RelationshipReasonId("influence-recovered"),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId("sage-recovery"),
            "The sage recovered from the influence attempt.");
        CharacterRelationshipChange recoveryChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            -1,
            recoveryReason);
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            influenceResult,
            recoveryChange);
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            actor,
            relationship,
            [candidate]);

        return new(
            actor,
            relationship,
            influenceReason,
            recoveryReason,
            candidate,
            request);
    }

    private sealed class RecordingModel(CharacterTargetSabotageInfluenceRecoveryDecision decision)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterTargetSabotageInfluenceRecoveryDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterTargetSabotageInfluenceRecoveryDecision>(
                cancellationToken);
    }

    private sealed class FailingModel(Exception exception)
        : ICharacterTargetSabotageInfluenceRecoveryDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
            CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageInfluenceRecoveryDecision>(exception);
    }

    private sealed record ControllerInput(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipReason InfluenceReason,
        CharacterRelationshipReason RecoveryReason,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request);
}

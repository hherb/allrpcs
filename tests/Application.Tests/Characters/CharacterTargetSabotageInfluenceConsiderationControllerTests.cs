using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = CreateInput();
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Candidate);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterTargetSabotageInfluenceConsiderationResult result =
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
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
        CharacterTargetSabotageInfluenceDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterTargetSabotageInfluenceConsiderationResult result =
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
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
        CharacterTargetSabotageInfluenceDecision invalid = new(
            new CharacterId(input.Actor.Identity.Id.Value),
            input.Candidate);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                new RecordingModel(invalid),
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                new NullModel(),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.Actor, input.Request.Actor);
        Assert.Same(input.InteractionAccess, input.Request.InteractionAccess);
        Assert.Same(input.Relationship, input.Request.Relationship);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public async Task RejectsNullsAndPreCancellationBeforeModelInvocation()
    {
        ControllerInput input = CreateInput();
        RecordingModel model = new(new(input.Actor.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
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
            async () => await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                new CancellingModel(cancellation.Token),
                input.Request,
                CancellationToken.None));
        InvalidOperationException failed = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CharacterTargetSabotageInfluenceConsiderationController.DecideAsync(
                new FailingModel(expected),
                input.Request,
                CancellationToken.None));

        Assert.Equal(cancellation.Token, cancelled.CancellationToken);
        Assert.Same(expected, failed);
    }

    [Fact]
    public void ExposesOnlyOneProviderNeutralModelAndStaticDecisionOperation()
    {
        Type modelType = typeof(ICharacterTargetSabotageInfluenceDecisionModel);
        System.Reflection.MethodInfo modelMethod = Assert.Single(modelType.GetMethods());
        Assert.Equal("DecideAsync", modelMethod.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterTargetSabotageInfluenceDecision>),
            modelMethod.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTargetSabotageInfluenceConsiderationRequest),
                typeof(CancellationToken),
            ],
            modelMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());

        Type controllerType = typeof(CharacterTargetSabotageInfluenceConsiderationController);
        System.Reflection.MethodInfo controllerMethod = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));
        Assert.Equal("DecideAsync", controllerMethod.Name);
        Assert.True(controllerMethod.IsStatic);
        Assert.Equal(
            typeof(Task<CharacterTargetSabotageInfluenceConsiderationResult>),
            controllerMethod.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterTargetSabotageInfluenceDecisionModel),
                typeof(CharacterTargetSabotageInfluenceConsiderationRequest),
                typeof(CancellationToken),
            ],
            controllerMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput CreateInput()
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
        CharacterRelationshipChange change = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("influence-attempt"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("enemy-agent-influence"),
                "The enemy agent attempted to gain influence."));
        CharacterTargetSabotageInfluenceCandidate candidate = new(occurrence, change);
        CharacterInteractionAccessSnapshot interactionAccess = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            actor,
            interactionAccess,
            relationship,
            [candidate]);

        return new(actor, interactionAccess, relationship, candidate, request);
    }

    private sealed class RecordingModel(CharacterTargetSabotageInfluenceDecision decision)
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterTargetSabotageInfluenceConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class NullModel : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult<CharacterTargetSabotageInfluenceDecision>(null!);
    }

    private sealed class CancellingModel(CancellationToken cancellationToken)
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken ignoredCancellationToken) =>
            ValueTask.FromCanceled<CharacterTargetSabotageInfluenceDecision>(cancellationToken);
    }

    private sealed class FailingModel(Exception exception)
        : ICharacterTargetSabotageInfluenceDecisionModel
    {
        public ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
            CharacterTargetSabotageInfluenceConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromException<CharacterTargetSabotageInfluenceDecision>(exception);
    }

    private sealed record ControllerInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate,
        CharacterTargetSabotageInfluenceConsiderationRequest Request);
}

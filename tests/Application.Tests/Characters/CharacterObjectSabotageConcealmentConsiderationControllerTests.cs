using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.EvidenceId);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterObjectSabotageConcealmentConsiderationResult result =
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
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
        ControllerInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageConcealmentConsiderationResult result =
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None);

        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public async Task RejectsInvalidDecisionWithoutChangingRequestState()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageConcealmentDecision decision = new(
            new CharacterId(input.Character.Identity.Id.Value),
            input.Candidate.Evidence.EvidenceId);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None));
        Assert.Empty(input.Request.Concealment.ConcealedObjectIds);
        Assert.Empty(input.Request.Evidence.Entries);
        Assert.Same(input.Candidate, Assert.Single(input.Request.Candidates));
    }

    [Fact]
    public async Task RejectsNullsBeforeModelInvocationAndPropagatesCancellation()
    {
        ControllerInput input = Input();
        RecordingModel model = new(new(input.Character.Identity.Id, null));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        Assert.Equal(0, model.CallCount);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterObjectSabotageConcealmentConsiderationController.DecideAsync(
                new CancellingModel(),
                input.Request,
                cancellation.Token));
    }

    [Fact]
    public void ExposesOnlyOneStaticDecisionOperation()
    {
        Type controllerType = typeof(CharacterObjectSabotageConcealmentConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("DecideAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<CharacterObjectSabotageConcealmentConsiderationResult>), method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterObjectSabotageConcealmentDecisionModel),
                typeof(CharacterObjectSabotageConcealmentConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(controllerType.GetProperties());
        Assert.Empty(controllerType.GetConstructors());
        Assert.True(controllerType.IsAbstract && controllerType.IsSealed);
    }

    private static ControllerInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterPhysicalAccessSnapshot physicalAccess = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("disturbed-cask-cover"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            physicalAccess,
            worldObjects,
            concealment,
            evidence,
            [candidate]);

        return new(character, candidate, request);
    }

    private sealed class RecordingModel(CharacterObjectSabotageConcealmentDecision decision)
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageConcealmentConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class CancellingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromCanceled<CharacterObjectSabotageConcealmentDecision>(cancellationToken);
    }

    private sealed record ControllerInput(
        Character Character,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}

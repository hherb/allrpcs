using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterObjectSabotageMoveConsiderationControllerTests
{
    [Fact]
    public async Task ObtainsOneDecisionForExactRequestAndReturnsValidatedResult()
    {
        ControllerInput input = Input();
        CharacterObjectSabotageMoveDecision decision = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);
        RecordingModel model = new(decision);
        using CancellationTokenSource cancellation = new();

        CharacterObjectSabotageMoveConsiderationResult result =
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
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
        CharacterObjectSabotageMoveDecision decision = new(input.Character.Identity.Id, null);

        CharacterObjectSabotageMoveConsiderationResult result =
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
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
        CharacterObjectSabotageMoveDecision decision = new(
            new CharacterId(input.Character.Identity.Id.Value),
            input.Candidate.Evidence.Evidence.EvidenceId);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
                new RecordingModel(decision),
                input.Request,
                CancellationToken.None));
        Assert.Same(input.SourcePlacement, Assert.Single(input.Request.Placements.Entries));
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
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
                null!,
                input.Request,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
                model,
                null!,
                CancellationToken.None));
        Assert.Equal(0, model.CallCount);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await CharacterObjectSabotageMoveConsiderationController.DecideAsync(
                new CancellingModel(),
                input.Request,
                cancellation.Token));
    }

    [Fact]
    public void ExposesOnlyOneStaticDecisionOperation()
    {
        Type controllerType = typeof(CharacterObjectSabotageMoveConsiderationController);
        System.Reflection.MethodInfo method = Assert.Single(controllerType.GetMethods().Where(
            candidate => candidate.DeclaringType == controllerType));

        Assert.Equal("DecideAsync", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(Task<CharacterObjectSabotageMoveConsiderationResult>), method.ReturnType);
        Assert.Equal(
            [
                typeof(ICharacterObjectSabotageMoveDecisionModel),
                typeof(CharacterObjectSabotageMoveConsiderationRequest),
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
        Location source = Location("village-square");
        Location destination = Location("abandoned-wagon");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Move),
            target);
        CharacterObjectSabotageMoveCandidate candidate = new(
            new CharacterObjectSabotageMoveDestination(occurrence, destination),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("cask-drag-marks"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectPlacement sourcePlacement = new(target, source);
        CharacterObjectSabotageMoveConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            new WorldObjectPlacementSnapshot([sourcePlacement]),
            new DiscoveryEvidenceSnapshot([]),
            [candidate]);

        return new(character, sourcePlacement, candidate, request);
    }

    private static Location Location(string id) =>
        new(
            new LocationIdentity(new LocationId(id), id),
            new LocationDescription($"The authored {id} location."));

    private sealed class RecordingModel(CharacterObjectSabotageMoveDecision decision)
        : ICharacterObjectSabotageMoveDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterObjectSabotageMoveConsiderationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class CancellingModel : ICharacterObjectSabotageMoveDecisionModel
    {
        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken) =>
            ValueTask.FromCanceled<CharacterObjectSabotageMoveDecision>(cancellationToken);
    }

    private sealed record ControllerInput(
        Character Character,
        WorldObjectPlacement SourcePlacement,
        CharacterObjectSabotageMoveCandidate Candidate,
        CharacterObjectSabotageMoveConsiderationRequest Request);
}

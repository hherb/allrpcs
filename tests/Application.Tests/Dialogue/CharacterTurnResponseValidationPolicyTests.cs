using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnResponseValidationPolicyTests
{
    [Fact]
    public void AcceptsWaitAndKnownImmediateAndCommitmentTargets()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention immediate = Targeted(CharacterIntentionType.ExamineObject, "herb-patch");
        CharacterIntention committed = Targeted(CharacterIntentionType.CollectObject, "stone-marker");
        CharacterTurnResponse response = Response(
            request,
            immediate,
            [new CharacterCommitment([new CharacterIntention(CharacterIntentionType.Wait, null), committed])]);
        WorldObjectSnapshot worldObjects = Objects("herb-patch", "stone-marker");

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                response,
                worldObjects,
                Affordances((CharacterIntentionType.ExamineObject, "herb-patch")));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Same(request, response.Request);
        Assert.Same(immediate, response.ImmediateIntention);
        Assert.Same(committed, response.Commitments[0].ProposedIntentions[1]);
        Assert.Equal(2, worldObjects.Entries.Count);
    }

    [Fact]
    public void AcceptsDistinctValueEqualTargetIdentifier()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        ObjectId proposedId = new("herb-patch");
        WorldObject known = Object("herb-patch");
        CharacterIntention intention = new(CharacterIntentionType.ExamineObject, proposedId);

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                Response(request, intention),
                new WorldObjectSnapshot([known]),
                Affordances((CharacterIntentionType.ExamineObject, "herb-patch")));

        Assert.True(result.IsValid);
        Assert.NotSame(proposedId, known.Identity.Id);
        Assert.Equal(proposedId, known.Identity.Id);
    }

    [Fact]
    public void RejectsUnknownImmediateTarget()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention intention = Targeted(CharacterIntentionType.ExamineObject, "missing-object");

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                Response(request, intention),
                Objects("known-object"),
                Affordances((CharacterIntentionType.ExamineObject, "missing-object")));

        CharacterTurnResponseValidationError error = Assert.Single(result.Errors);
        Assert.False(result.IsValid);
        Assert.Equal(CharacterTurnResponseValidationErrorCode.UnknownTargetObject, error.Code);
        Assert.Same(intention, error.Intention);
    }

    [Fact]
    public void RejectsUnknownCommitmentTarget()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention intention = Targeted(CharacterIntentionType.CollectObject, "missing-object");
        CharacterTurnResponse response = Response(
            request,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [new CharacterCommitment([intention])]);

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                response,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]));

        CharacterTurnResponseValidationError error = Assert.Single(result.Errors);
        Assert.Equal(CharacterTurnResponseValidationErrorCode.UnknownTargetObject, error.Code);
        Assert.Same(intention, error.Intention);
    }

    [Fact]
    public void RejectsAKnownButCurrentlyUnavailableImmediateIntention()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention intention = Targeted(CharacterIntentionType.CollectObject, "herb-patch");

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                Response(request, intention),
                Objects("herb-patch"),
                Affordances((CharacterIntentionType.ExamineObject, "herb-patch")));

        CharacterTurnResponseValidationError error = Assert.Single(result.Errors);
        Assert.Equal(
            CharacterTurnResponseValidationErrorCode.UnavailableImmediateIntention,
            error.Code);
        Assert.Same(intention, error.Intention);
    }

    [Fact]
    public void DelegatesStructuralValidationForEveryIntention()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention invalidImmediate = new(CharacterIntentionType.ExamineObject, null);
        CharacterIntention invalidCommitted = new(
            CharacterIntentionType.Wait,
            new ObjectId("unexpected-target"));
        CharacterTurnResponse response = Response(
            request,
            invalidImmediate,
            [new CharacterCommitment([invalidCommitted])]);

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                request,
                response,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]));

        Assert.False(result.IsValid);
        Assert.Collection(
            result.Errors,
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.MissingTargetObject, error.Code);
                Assert.Same(invalidImmediate, error.Intention);
            },
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.UnexpectedTargetObject, error.Code);
                Assert.Same(invalidCommitted, error.Intention);
            });
    }

    [Fact]
    public void RejectsDistinctValueEqualRequestAndReportsAllIntentionErrorsInStableOrder()
    {
        CharacterTurnRequest expectedRequest = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterTurnRequest equalRequest = new(
            new CharacterId(expectedRequest.RespondingCharacterId.Value),
            expectedRequest.DialogueEvent,
            expectedRequest.DialogueAct);
        CharacterIntention invalidImmediate = new(CharacterIntentionType.CollectObject, null);
        CharacterIntention unknownFirst = Targeted(CharacterIntentionType.ExamineObject, "first-missing");
        CharacterIntention unknownSecond = Targeted(CharacterIntentionType.CollectObject, "second-missing");
        CharacterTurnResponse response = Response(
            equalRequest,
            invalidImmediate,
            [new CharacterCommitment([unknownFirst, unknownSecond])]);

        CharacterTurnResponseValidationResult result =
            CharacterTurnResponseValidationPolicy.Validate(
                expectedRequest,
                response,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]));

        Assert.False(result.IsValid);
        Assert.Collection(
            result.Errors,
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.RequestMismatch, error.Code);
                Assert.Null(error.Intention);
            },
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.MissingTargetObject, error.Code);
                Assert.Same(invalidImmediate, error.Intention);
            },
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.UnknownTargetObject, error.Code);
                Assert.Same(unknownFirst, error.Intention);
            },
            error =>
            {
                Assert.Equal(CharacterTurnResponseValidationErrorCode.UnknownTargetObject, error.Code);
                Assert.Same(unknownSecond, error.Intention);
            });
        Assert.Same(equalRequest, response.Request);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterTurnResponse response = Response(
            request,
            new CharacterIntention(CharacterIntentionType.Wait, null));
        WorldObjectSnapshot worldObjects = new([]);
        CharacterIntentionAffordanceSnapshot affordances = new([]);

        Assert.Throws<ArgumentNullException>(
            () => CharacterTurnResponseValidationPolicy.Validate(
                null!, response, worldObjects, affordances));
        Assert.Throws<ArgumentNullException>(
            () => CharacterTurnResponseValidationPolicy.Validate(
                request, null!, worldObjects, affordances));
        Assert.Throws<ArgumentNullException>(
            () => CharacterTurnResponseValidationPolicy.Validate(
                request, response, null!, affordances));
        Assert.Throws<ArgumentNullException>(
            () => CharacterTurnResponseValidationPolicy.Validate(
                request, response, worldObjects, null!));
    }

    private static CharacterTurnResponse Response(
        CharacterTurnRequest request,
        CharacterIntention immediateIntention,
        IEnumerable<CharacterCommitment>? commitments = null) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            immediateIntention,
            commitments ?? [],
            []);

    private static CharacterIntention Targeted(CharacterIntentionType type, string target) =>
        new(type, new ObjectId(target));

    private static WorldObjectSnapshot Objects(params string[] ids) =>
        new(ids.Select(Object));

    private static CharacterIntentionAffordanceSnapshot Affordances(
        params (CharacterIntentionType Type, string Target)[] entries) =>
        new(entries.Select(entry => new CharacterIntentionAffordance(
            entry.Type,
            new ObjectId(entry.Target))));

    private static WorldObject Object(string id) =>
        CharacterModelPerceptionContextProjectionTests.CreateWorldObject(
            id,
            $"Display name for {id}",
            $"Description for {id}.");
}

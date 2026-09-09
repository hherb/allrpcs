using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnImmediateIntentionEligibilityPolicyTests
{
    [Fact]
    public void AcceptsWaitWithoutAnAffordance()
    {
        CharacterTurnResponse response = Response(
            new CharacterIntention(CharacterIntentionType.Wait, null));

        bool eligible = CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            response,
            new CharacterIntentionAffordanceSnapshot([]));

        Assert.True(eligible);
    }

    [Fact]
    public void AcceptsAValueEqualTypeAndTargetAffordance()
    {
        ObjectId proposedTarget = new("herb-patch");
        CharacterIntention intention = new(CharacterIntentionType.ExamineObject, proposedTarget);
        CharacterIntentionAffordance affordance = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("herb-patch"));
        CharacterTurnResponse response = Response(intention);

        bool eligible = CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            response,
            new CharacterIntentionAffordanceSnapshot([affordance]));

        Assert.True(eligible);
        Assert.NotSame(proposedTarget, affordance.TargetObjectId);
        Assert.Same(intention, response.ImmediateIntention);
    }

    [Fact]
    public void RejectsMissingAndWrongTypeAffordancesWithoutChangingInputs()
    {
        CharacterIntention intention = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("herb-patch"));
        CharacterTurnResponse response = Response(intention);
        CharacterIntentionAffordance wrongType = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("herb-patch"));
        CharacterIntentionAffordance unrelated = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("stone-marker"));
        CharacterIntentionAffordanceSnapshot affordances = new([wrongType, unrelated]);

        Assert.False(CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            response,
            affordances));
        Assert.Same(intention, response.ImmediateIntention);
        Assert.Collection(
            affordances.Entries,
            entry => Assert.Same(wrongType, entry),
            entry => Assert.Same(unrelated, entry));
    }

    [Fact]
    public void RejectsStructurallyInvalidImmediateIntentions()
    {
        CharacterTurnResponse missingTarget = Response(
            new CharacterIntention(CharacterIntentionType.ExamineObject, null));
        CharacterTurnResponse unexpectedTarget = Response(
            new CharacterIntention(CharacterIntentionType.Wait, new ObjectId("herb-patch")));
        CharacterIntentionAffordanceSnapshot affordances = new(
            [new CharacterIntentionAffordance(
                CharacterIntentionType.ExamineObject,
                new ObjectId("herb-patch"))]);

        Assert.False(CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            missingTarget,
            affordances));
        Assert.False(CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            unexpectedTarget,
            affordances));
    }

    [Fact]
    public void DoesNotApplyCurrentEligibilityToFutureCommitments()
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        CharacterIntention future = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("herb-patch"));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            null,
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [new CharacterCommitment([future])],
            []);

        Assert.True(CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(
            response,
            new CharacterIntentionAffordanceSnapshot([])));
        Assert.Same(future, Assert.Single(response.Commitments).ProposedIntentions[0]);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnResponse response = Response(
            new CharacterIntention(CharacterIntentionType.Wait, null));
        CharacterIntentionAffordanceSnapshot affordances = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(null!, affordances));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionEligibilityPolicy.IsEligible(response, null!));
    }

    private static CharacterTurnResponse Response(CharacterIntention immediateIntention)
    {
        CharacterTurnRequest request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Accept,
            null,
            immediateIntention,
            [],
            []);
    }
}

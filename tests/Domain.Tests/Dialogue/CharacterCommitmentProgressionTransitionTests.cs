using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterCommitmentProgressionTransitionTests
{
    [Fact]
    public void AdvancesOneEligibleIntentionAndRetainsTheExactCommitment()
    {
        CharacterIntention first = Examine("flowers");
        CharacterIntention second = Collect("flowers");
        CharacterCommitment commitment = new([first, second]);
        CharacterCommitmentProgress progress = new(commitment, 0);

        CharacterCommitmentProgress result =
            CharacterCommitmentProgressionTransition.Advance(progress, first);

        Assert.NotSame(progress, result);
        Assert.Same(commitment, result.Commitment);
        Assert.Equal(1, result.NextIntentionIndex);
        Assert.Same(second, result.NextIntention);
        Assert.False(result.IsComplete);
        Assert.Equal(0, progress.NextIntentionIndex);
        Assert.Same(first, progress.NextIntention);
    }

    [Fact]
    public void AdvancesOnceWhenConsecutiveIntentionsAreValueEqual()
    {
        CharacterIntention first = Examine("flowers");
        CharacterIntention second = Examine("flowers");
        CharacterCommitment commitment = new([first, second]);
        CharacterCommitmentProgress progress = new(commitment, 0);

        CharacterCommitmentProgress result =
            CharacterCommitmentProgressionTransition.Advance(progress, Examine("flowers"));

        Assert.Equal(1, result.NextIntentionIndex);
        Assert.Same(second, result.NextIntention);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public void CompletesAfterObservingTheLastPendingIntention()
    {
        CharacterIntention intention = Examine("flowers");
        CharacterCommitmentProgress progress = new(
            new CharacterCommitment([intention]),
            0);

        CharacterCommitmentProgress result =
            CharacterCommitmentProgressionTransition.Advance(progress, intention);

        Assert.Equal(1, result.NextIntentionIndex);
        Assert.True(result.IsComplete);
        Assert.Null(result.NextIntention);
    }

    [Fact]
    public void RejectsOutOfOrderNearMatchAndCompletedObservations()
    {
        CharacterIntention first = Examine("flowers");
        CharacterIntention second = Collect("flowers");
        CharacterCommitment commitment = new([first, second]);
        CharacterCommitmentProgress initial = new(commitment, 0);
        CharacterCommitmentProgress complete = new(commitment, 2);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterCommitmentProgressionTransition.Advance(initial, second));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterCommitmentProgressionTransition.Advance(initial, Examine("herbs")));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterCommitmentProgressionTransition.Advance(complete, second));
        Assert.Equal(0, initial.NextIntentionIndex);
        Assert.Equal(2, complete.NextIntentionIndex);
    }

    [Fact]
    public void RejectsNullInputsThroughThePolicy()
    {
        CharacterIntention intention = Examine("flowers");
        CharacterCommitmentProgress progress = new(
            new CharacterCommitment([intention]),
            0);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterCommitmentProgressionTransition.Advance(null!, intention));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterCommitmentProgressionTransition.Advance(progress, null!));
    }

    [Fact]
    public void ExposesOnlyThePureAdvanceTransition()
    {
        Type transitionType = typeof(CharacterCommitmentProgressionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Advance", method.Name);
        Assert.Equal(typeof(CharacterCommitmentProgress), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterCommitmentProgress),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterIntention), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static CharacterIntention Collect(string objectId) =>
        new(CharacterIntentionType.CollectObject, new ObjectId(objectId));

    private static CharacterIntention Examine(string objectId) =>
        new(CharacterIntentionType.ExamineObject, new ObjectId(objectId));
}

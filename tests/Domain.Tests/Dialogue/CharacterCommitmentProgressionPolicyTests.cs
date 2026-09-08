using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterCommitmentProgressionPolicyTests
{
    [Fact]
    public void AcceptsTheExactPendingIntention()
    {
        CharacterIntention pending = Examine("flowers");
        CharacterCommitmentProgress progress = Progress(0, pending, Collect("flowers"));

        bool canAdvance = CharacterCommitmentProgressionPolicy.CanAdvance(progress, pending);

        Assert.True(canAdvance);
        Assert.Same(pending, progress.NextIntention);
        Assert.Equal(0, progress.NextIntentionIndex);
    }

    [Fact]
    public void AcceptsAValueEqualDistinctObservation()
    {
        CharacterIntention pending = Examine("flowers");
        CharacterIntention observation = Examine("flowers");
        CharacterCommitmentProgress progress = Progress(0, pending);

        bool canAdvance = CharacterCommitmentProgressionPolicy.CanAdvance(progress, observation);

        Assert.NotSame(pending, observation);
        Assert.NotSame(pending.TargetObjectId, observation.TargetObjectId);
        Assert.Equal(pending, observation);
        Assert.True(canAdvance);
    }

    [Fact]
    public void RejectsAnOutOfOrderObservation()
    {
        CharacterIntention pending = Examine("flowers");
        CharacterIntention later = Collect("flowers");
        CharacterCommitmentProgress progress = Progress(0, pending, later);

        Assert.False(CharacterCommitmentProgressionPolicy.CanAdvance(progress, later));
        Assert.Same(pending, progress.NextIntention);
    }

    [Fact]
    public void RejectsNearMatchObservations()
    {
        CharacterCommitmentProgress progress = Progress(0, Examine("flowers"));

        Assert.False(CharacterCommitmentProgressionPolicy.CanAdvance(
            progress,
            Collect("flowers")));
        Assert.False(CharacterCommitmentProgressionPolicy.CanAdvance(
            progress,
            Examine("herbs")));
    }

    [Fact]
    public void RejectsAnObservationAfterCompletion()
    {
        CharacterIntention intention = Examine("flowers");
        CharacterCommitmentProgress progress = Progress(1, intention);

        Assert.True(progress.IsComplete);
        Assert.False(CharacterCommitmentProgressionPolicy.CanAdvance(progress, intention));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterIntention intention = Examine("flowers");
        CharacterCommitmentProgress progress = Progress(0, intention);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterCommitmentProgressionPolicy.CanAdvance(null!, intention));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterCommitmentProgressionPolicy.CanAdvance(progress, null!));
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityQuery()
    {
        Type policyType = typeof(CharacterCommitmentProgressionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAdvance", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterCommitmentProgress),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterIntention), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static CharacterIntention Collect(string objectId) =>
        new(CharacterIntentionType.CollectObject, new ObjectId(objectId));

    private static CharacterIntention Examine(string objectId) =>
        new(CharacterIntentionType.ExamineObject, new ObjectId(objectId));

    private static CharacterCommitmentProgress Progress(
        int nextIntentionIndex,
        params CharacterIntention[] intentions) =>
        new(new CharacterCommitment(intentions), nextIntentionIndex);
}

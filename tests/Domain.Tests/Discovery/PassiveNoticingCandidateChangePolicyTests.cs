using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateChangePolicyTests
{
    [Fact]
    public void AllowsApplicableUnnoticedTargetToChange()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.True(result);
    }

    [Fact]
    public void RejectsEveryStateOtherThanUnnoticed()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");

        foreach (VisibilityState currentState in VisibilityState.All)
        {
            CharacterPerceptionSnapshot perception = Snapshot(
                "seeker",
                Visibility("weathered-inscription", currentState));

            bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

            Assert.Equal(currentState == VisibilityState.Unnoticed, result);
        }
    }

    [Fact]
    public void RejectsInapplicableCandidateForDifferentObserver()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Snapshot(
            "travelling-healer",
            Visibility("weathered-inscription", VisibilityState.Unnoticed));

        bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.False(result);
    }

    [Fact]
    public void RejectsInapplicableCandidateForAbsentTarget()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("healing-herb", VisibilityState.Unnoticed));

        bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.False(result);
    }

    [Fact]
    public void FindsTheExactTargetAmongUnrelatedEntries()
    {
        ObjectId targetObjectId = new("weathered-inscription");
        WorldObjectVisibility target = new(targetObjectId, VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = Snapshot(
            "seeker",
            Visibility("wagon-residue", VisibilityState.Unnoticed),
            target,
            Visibility("healing-herb", VisibilityState.Identified));
        PassiveNoticingCandidate candidate = new(new CharacterId("seeker"), targetObjectId);

        bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.True(result);
        Assert.Same(targetObjectId, target.ObjectId);
    }

    [Fact]
    public void AcceptsDistinctValueEqualIdentifiers()
    {
        CharacterId candidateCharacterId = new("seeker");
        ObjectId candidateObjectId = new("weathered-inscription");
        CharacterId perceptionCharacterId = new("seeker");
        ObjectId perceptionObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(candidateCharacterId, candidateObjectId);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[]
            {
                new WorldObjectVisibility(perceptionObjectId, VisibilityState.Unnoticed),
            });

        bool result = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.True(result);
        Assert.NotSame(candidateCharacterId, perceptionCharacterId);
        Assert.Equal(candidateCharacterId, perceptionCharacterId);
        Assert.NotSame(candidateObjectId, perceptionObjectId);
        Assert.Equal(candidateObjectId, perceptionObjectId);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);
        CharacterId perceptionCharacterId = new("seeker");
        WorldObjectVisibility unrelated = Visibility(
            "wagon-residue",
            VisibilityState.Noticed);
        VisibilityState currentVisibilityState = VisibilityState.Unnoticed;
        WorldObjectVisibility target = new(targetObjectId, currentVisibilityState);
        CharacterPerceptionSnapshot perception = new(
            perceptionCharacterId,
            new[] { unrelated, target });

        _ = PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception);

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(perceptionCharacterId, perception.CharacterId);
        Assert.Collection(
            perception.Entries,
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(target, entry));
        Assert.Same(targetObjectId, target.ObjectId);
        Assert.Same(currentVisibilityState, target.VisibilityState);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        PassiveNoticingCandidate candidate = Candidate("seeker", "weathered-inscription");
        CharacterPerceptionSnapshot perception = Snapshot("seeker");

        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateChangePolicy.WouldChange(null!, perception));
        Assert.Throws<ArgumentNullException>(
            () => PassiveNoticingCandidateChangePolicy.WouldChange(candidate, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureChangeOperation()
    {
        Type policyType = typeof(PassiveNoticingCandidateChangePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("WouldChange", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidate),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static PassiveNoticingCandidate Candidate(
        string observerCharacterId,
        string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));

    private static CharacterPerceptionSnapshot Snapshot(
        string characterId,
        params WorldObjectVisibility[] entries) =>
        new(new CharacterId(characterId), entries);

    private static WorldObjectVisibility Visibility(
        string objectId,
        VisibilityState visibilityState) =>
        new(new ObjectId(objectId), visibilityState);
}

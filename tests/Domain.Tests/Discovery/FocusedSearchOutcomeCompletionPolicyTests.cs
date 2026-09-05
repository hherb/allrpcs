using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeCompletionPolicyTests
{
    [Fact]
    public void AllowsActiveUnrecordedOutcomeAmongUnrelatedEntries()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity matchingActivity = new(outcome.Request);
        FocusedSearchActivitySnapshot activities = new(
            new[]
            {
                Activity("travelling-healer", "concealed-healing-herb"),
                matchingActivity,
                Activity("sage", "wagon-residue"),
            });
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(
            Outcome("travelling-healer", "concealed-healing-herb", VisibilityState.Identified));

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            activities,
            outcomes);

        Assert.True(result);
        Assert.Same(outcome.Request, matchingActivity.Request);
    }

    [Fact]
    public void RejectsWhenActivitySnapshotIsEmpty()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            Activities(),
            Outcomes());

        Assert.False(result);
    }

    [Fact]
    public void RejectsDifferentTargetForSameActiveSearcher()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            Activities(("seeker", "concealed-healing-herb")),
            Outcomes());

        Assert.False(result);
    }

    [Fact]
    public void RejectsDifferentActiveSearcherForSameTarget()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            Activities(("travelling-healer", "weathered-inscription")),
            Outcomes());

        Assert.False(result);
    }

    [Fact]
    public void RejectsUnrelatedActivities()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            Activities(
                ("travelling-healer", "concealed-healing-herb"),
                ("sage", "wagon-residue")),
            Outcomes());

        Assert.False(result);
    }

    [Fact]
    public void RejectsTheExactRecordedRequestInstance()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = new(outcome.Request, VisibilityState.Identified);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            new FocusedSearchActivitySnapshot(new[] { new FocusedSearchActivity(outcome.Request) }),
            Outcomes(retained));

        Assert.False(result);
    }

    [Fact]
    public void RejectsAValueEqualDistinctRecordedRequest()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchOutcome retained = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Noticed);

        bool result = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            new FocusedSearchActivitySnapshot(new[] { new FocusedSearchActivity(outcome.Request) }),
            Outcomes(retained));

        Assert.NotSame(outcome.Request, retained.Request);
        Assert.Equal(outcome.Request, retained.Request);
        Assert.False(result);
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        FocusedSearchActivity firstActivity = Activity(
            "travelling-healer",
            "concealed-healing-herb");
        FocusedSearchActivity matchingActivity = new(request);
        FocusedSearchActivitySnapshot activities = new(new[] { firstActivity, matchingActivity });
        FocusedSearchOutcome retainedOutcome = Outcome(
            "sage",
            "wagon-residue",
            VisibilityState.Noticed);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        _ = FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
            outcome,
            activities,
            outcomes);

        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, outcome.Request.SearcherCharacterId);
        Assert.Same(targetObjectId, outcome.Request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(firstActivity, entry),
            entry => Assert.Same(matchingActivity, entry));
        Assert.Same(request, matchingActivity.Request);
        FocusedSearchOutcome exactRetainedOutcome = Assert.Single(outcomes.Entries);
        Assert.Same(retainedOutcome, exactRetainedOutcome);
        Assert.Same(retainedOutcome.Request, exactRetainedOutcome.Request);
        Assert.Same(
            retainedOutcome.ResultingVisibilityState,
            exactRetainedOutcome.ResultingVisibilityState);
    }

    [Fact]
    public void RejectsNullInputsBeforeDelegation()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivitySnapshot activities = Activities();
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
                null!,
                activities,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
                outcome,
                null!,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
                outcome,
                activities,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureCompletionEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchOutcomeCompletionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanCompleteWithOutcome", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchOutcome), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static FocusedSearchOutcome Outcome(
        string characterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(Request(characterId, targetObjectId), resultingVisibilityState);

    private static FocusedSearchActivity Activity(
        string characterId,
        string targetObjectId) =>
        new(Request(characterId, targetObjectId));

    private static FocusedSearchActivitySnapshot Activities(
        params (string CharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Activity(entry.CharacterId, entry.TargetObjectId)));

    private static FocusedSearchOutcomeSnapshot Outcomes(
        params FocusedSearchOutcome[] entries) =>
        new(entries);

    private static FocusedSearchRequest Request(
        string characterId,
        string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));
}

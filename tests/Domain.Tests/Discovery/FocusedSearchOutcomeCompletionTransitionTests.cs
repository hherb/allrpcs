using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeCompletionTransitionTests
{
    [Fact]
    public void CompletesTheExactRequestAndAppendsTheExactOutcome()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity matching = new(outcome.Request);
        FocusedSearchActivitySnapshot activities = new(new[] { matching });
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        FocusedSearchOutcomeCompletionResult result =
            FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                outcomes);

        Assert.Empty(result.Activities.Entries);
        Assert.Collection(result.Outcomes.Entries, entry => Assert.Same(outcome, entry));
        Assert.Same(outcome.Request, matching.Request);
    }

    [Fact]
    public void PreservesStableOrderAndExactUnrelatedInstances()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity firstActivity = Activity(
            "travelling-healer",
            "concealed-healing-herb");
        FocusedSearchActivity matchingActivity = new(outcome.Request);
        FocusedSearchActivity lastActivity = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(
            new[] { firstActivity, matchingActivity, lastActivity });
        FocusedSearchOutcome firstOutcome = Outcome(
            "archivist",
            "faded-map",
            VisibilityState.Noticed);
        FocusedSearchOutcome lastOutcome = Outcome(
            "scout",
            "hidden-trail",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(firstOutcome, lastOutcome);

        FocusedSearchOutcomeCompletionResult result =
            FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                outcomes);

        Assert.Collection(
            result.Activities.Entries,
            entry => Assert.Same(firstActivity, entry),
            entry => Assert.Same(lastActivity, entry));
        Assert.Collection(
            result.Outcomes.Entries,
            entry => Assert.Same(firstOutcome, entry),
            entry => Assert.Same(lastOutcome, entry),
            entry => Assert.Same(outcome, entry));
    }

    [Fact]
    public void LeavesEveryInputAndRetainedInstanceUnchanged()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        VisibilityState resultingVisibilityState = VisibilityState.Understood;
        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);
        FocusedSearchActivity matchingActivity = new(request);
        FocusedSearchActivity retainedActivity = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(
            new[] { matchingActivity, retainedActivity });
        FocusedSearchOutcome retainedOutcome = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        FocusedSearchOutcomeCompletionResult result =
            FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                outcomes);

        Assert.NotSame(activities, result.Activities);
        Assert.NotSame(outcomes, result.Outcomes);
        Assert.Same(request, outcome.Request);
        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(matchingActivity, entry),
            entry => Assert.Same(retainedActivity, entry));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retainedOutcome, entry));
        Assert.Collection(result.Activities.Entries, entry => Assert.Same(retainedActivity, entry));
        Assert.Collection(
            result.Outcomes.Entries,
            entry => Assert.Same(retainedOutcome, entry),
            entry => Assert.Same(outcome, entry));
    }

    [Fact]
    public void RejectsAnInactiveRequestWithoutChangingInputs()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity retainedActivity = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { retainedActivity });
        FocusedSearchOutcome retainedOutcome = Outcome(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                outcomes));
        Assert.Collection(activities.Entries, entry => Assert.Same(retainedActivity, entry));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retainedOutcome, entry));
    }

    [Fact]
    public void RejectsAnAlreadyRecordedRequestWithoutChangingInputs()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivity matchingActivity = new(outcome.Request);
        FocusedSearchActivitySnapshot activities = new(new[] { matchingActivity });
        FocusedSearchOutcome retainedOutcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Noticed);
        FocusedSearchOutcomeSnapshot outcomes = Outcomes(retainedOutcome);

        Assert.NotSame(outcome.Request, retainedOutcome.Request);
        Assert.Equal(outcome.Request, retainedOutcome.Request);
        Assert.Throws<InvalidOperationException>(
            () => FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                outcomes));
        Assert.Collection(activities.Entries, entry => Assert.Same(matchingActivity, entry));
        Assert.Collection(outcomes.Entries, entry => Assert.Same(retainedOutcome, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughCompletionPolicy()
    {
        FocusedSearchOutcome outcome = Outcome(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);
        FocusedSearchActivitySnapshot activities = Activities(("seeker", "weathered-inscription"));
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                null!,
                activities,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                null!,
                outcomes));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome(
                outcome,
                activities,
                null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureCompletionTransitionOperation()
    {
        Type transitionType = typeof(FocusedSearchOutcomeCompletionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("CompleteWithOutcome", method.Name);
        Assert.Equal(typeof(FocusedSearchOutcomeCompletionResult), method.ReturnType);
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
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
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

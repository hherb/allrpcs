using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskDepartureTransitionTests
{
    [Fact]
    public void StartsFirstEligibleTaskAndSeparatesSameCharacterAtomically()
    {
        CharacterIndependentTaskActivity other =
            CharacterIndependentTaskSelectionPolicyTests.Activity("other", "sage", "assigned");
        CharacterIndependentTaskActivity selected =
            CharacterIndependentTaskSelectionPolicyTests.Activity("first", "healer", "assigned");
        CharacterIndependentTaskActivity later =
            CharacterIndependentTaskSelectionPolicyTests.Activity("later", "healer", "assigned");
        CharacterIndependentTaskActivitySnapshot activities = new([other, selected, later]);
        CharacterId healer = selected.Assignment.CharacterId;
        CharacterId sage = new("sage");
        CharacterPartySnapshot party = new([healer, sage]);

        CharacterIndependentTaskDepartureResult result =
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                new CharacterId("healer"),
                activities,
                party);

        Assert.Same(selected.Assignment, result.ActiveActivity.Assignment);
        Assert.Same(CharacterIndependentTaskActivityState.Active, result.ActiveActivity.State);
        Assert.Collection(
            result.Activities.Activities,
            activity => Assert.Same(other, activity),
            activity => Assert.Same(result.ActiveActivity, activity),
            activity => Assert.Same(later, activity));
        Assert.Collection(result.Party.MemberCharacterIds, member => Assert.Same(sage, member));
        Assert.Same(selected, activities.Activities[1]);
        Assert.Collection(
            party.MemberCharacterIds,
            member => Assert.Same(healer, member),
            member => Assert.Same(sage, member));
    }

    [Fact]
    public void RejectsMissingPartyMemberBeforeStartingTask()
    {
        CharacterIndependentTaskActivity assigned =
            CharacterIndependentTaskSelectionPolicyTests.Activity("task", "healer", "assigned");
        CharacterIndependentTaskActivitySnapshot activities = new([assigned]);
        CharacterPartySnapshot party = new([new CharacterId("sage")]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                new CharacterId("healer"),
                activities,
                party));
        Assert.Same(assigned, activities.Activities[0]);
        Assert.Equal("sage", Assert.Single(party.MemberCharacterIds).Value);
    }

    [Fact]
    public void RejectsMissingOrBlockedTaskBeforeSeparatingMember()
    {
        CharacterIndependentTaskActivity active =
            CharacterIndependentTaskSelectionPolicyTests.Activity("task", "healer", "active");
        CharacterIndependentTaskActivitySnapshot activities = new([active]);
        CharacterId healer = active.Assignment.CharacterId;
        CharacterPartySnapshot party = new([healer]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                new CharacterId("healer"),
                activities,
                party));
        Assert.Same(active, activities.Activities[0]);
        Assert.Same(healer, Assert.Single(party.MemberCharacterIds));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheAtomicTransition()
    {
        CharacterIndependentTaskActivitySnapshot activities = new([]);
        CharacterPartySnapshot party = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                null!, activities, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                new CharacterId("healer"), null!, party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskDepartureTransition.StartAndSeparate(
                new CharacterId("healer"), activities, null!));

        Type transitionType = typeof(CharacterIndependentTaskDepartureTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("StartAndSeparate", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskDepartureResult), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}

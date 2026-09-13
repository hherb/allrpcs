using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskSelectionPolicyTests
{
    [Fact]
    public void SelectsFirstAssignedExactActivityForValueEqualCharacter()
    {
        CharacterIndependentTaskActivity other = Activity("other", "sage", "assigned");
        CharacterIndependentTaskActivity first = Activity("first", "healer", "assigned");
        CharacterIndependentTaskActivity second = Activity("second", "healer", "assigned");
        CharacterIndependentTaskActivitySnapshot snapshot = new([other, first, second]);

        CharacterIndependentTaskActivity? selected =
            CharacterIndependentTaskSelectionPolicy.SelectNext(
                new CharacterId("healer"),
                snapshot);

        Assert.Same(first, selected);
    }

    [Fact]
    public void ActiveTaskForCharacterBlocksAnotherSelection()
    {
        CharacterIndependentTaskActivity assigned = Activity("assigned", "healer", "assigned");
        CharacterIndependentTaskActivity active = Activity("active", "healer", "active");
        CharacterIndependentTaskActivitySnapshot snapshot = new([assigned, active]);

        Assert.Null(CharacterIndependentTaskSelectionPolicy.SelectNext(
            new CharacterId("healer"),
            snapshot));
    }

    [Fact]
    public void IgnoresTerminalAndOtherCharacterActivities()
    {
        CharacterIndependentTaskActivity completed = Activity("done", "healer", "completed");
        CharacterIndependentTaskActivity cancelled = Activity("cancelled", "healer", "cancelled");
        CharacterIndependentTaskActivity activeOther = Activity("active-other", "sage", "active");
        CharacterIndependentTaskActivity assigned = Activity("next", "healer", "assigned");
        CharacterIndependentTaskActivitySnapshot snapshot = new(
            [completed, cancelled, activeOther, assigned]);

        Assert.Same(
            assigned,
            CharacterIndependentTaskSelectionPolicy.SelectNext(
                new CharacterId("healer"),
                snapshot));
        Assert.Null(CharacterIndependentTaskSelectionPolicy.SelectNext(
            new CharacterId("enemy-agent"),
            snapshot));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureSelection()
    {
        CharacterIndependentTaskActivitySnapshot snapshot = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskSelectionPolicy.SelectNext(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskSelectionPolicy.SelectNext(
                new CharacterId("healer"),
                null!));

        Type policyType = typeof(CharacterIndependentTaskSelectionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));
        Assert.Equal("SelectNext", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskActivity), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    internal static CharacterIndependentTaskActivity Activity(
        string taskId,
        string characterId,
        string stateValue) =>
        new(
            CharacterIndependentTaskAssignmentTests.Assignment(
                taskId,
                characterId,
                $"{taskId}-object"),
            CharacterIndependentTaskActivityState.Parse(stateValue));
}

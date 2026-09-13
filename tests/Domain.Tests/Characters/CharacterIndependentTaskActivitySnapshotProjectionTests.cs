using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskActivitySnapshotProjectionTests
{
    [Fact]
    public void ProjectsEveryExactAssignmentIntoStableAssignedActivity()
    {
        CharacterIndependentTaskAssignment first =
            CharacterIndependentTaskAssignmentTests.Assignment("first", "healer", "moonwort");
        CharacterIndependentTaskAssignment second =
            CharacterIndependentTaskAssignmentTests.Assignment("second", "sage", "stream");
        CharacterIndependentTaskAssignmentSnapshot assignments = new([first, second]);

        CharacterIndependentTaskActivitySnapshot result =
            CharacterIndependentTaskActivitySnapshotProjection.Project(assignments);

        Assert.Collection(
            result.Activities,
            activity =>
            {
                Assert.Same(first, activity.Assignment);
                Assert.Same(CharacterIndependentTaskActivityState.Assigned, activity.State);
            },
            activity =>
            {
                Assert.Same(second, activity.Assignment);
                Assert.Same(CharacterIndependentTaskActivityState.Assigned, activity.State);
            });
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePureProjection()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskActivitySnapshotProjection.Project(null!));

        Type projectionType = typeof(CharacterIndependentTaskActivitySnapshotProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));
        Assert.Equal("Project", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskActivitySnapshot), method.ReturnType);
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract && projectionType.IsSealed);
    }
}

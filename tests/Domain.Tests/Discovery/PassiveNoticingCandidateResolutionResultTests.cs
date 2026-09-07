using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateResolutionResultTests
{
    [Fact]
    public void RetainsExactPerceptionAndCandidateSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        PassiveNoticingCandidateSnapshot candidates = Candidates();

        PassiveNoticingCandidateResolutionResult result = new(perception, candidates);

        Assert.Same(perception, result.Perception);
        Assert.Same(candidates, result.Candidates);
    }

    [Fact]
    public void EqualityUsesRetainedSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        PassiveNoticingCandidateSnapshot candidates = Candidates();
        PassiveNoticingCandidateResolutionResult first = new(perception, candidates);
        PassiveNoticingCandidateResolutionResult same = new(perception, candidates);
        PassiveNoticingCandidateResolutionResult differentPerception =
            new(Perception(), candidates);
        PassiveNoticingCandidateResolutionResult differentCandidates =
            new(perception, Candidates());

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentPerception);
        Assert.NotEqual(first, differentCandidates);
    }

    [Fact]
    public void RejectsNullSnapshots()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PassiveNoticingCandidateResolutionResult(null!, Candidates()));
        Assert.Throws<ArgumentNullException>(
            () => new PassiveNoticingCandidateResolutionResult(Perception(), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResolutionResultSurface()
    {
        Type resultType = typeof(PassiveNoticingCandidateResolutionResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidates", typeof(PassiveNoticingCandidateSnapshot)),
            ("Perception", typeof(CharacterPerceptionSnapshot)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            resultType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(PassiveNoticingCandidateSnapshot),
                parameter.ParameterType));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static CharacterPerceptionSnapshot Perception() =>
        new(new CharacterId("seeker"), Array.Empty<WorldObjectVisibility>());

    private static PassiveNoticingCandidateSnapshot Candidates() =>
        new(Array.Empty<PassiveNoticingCandidate>());
}

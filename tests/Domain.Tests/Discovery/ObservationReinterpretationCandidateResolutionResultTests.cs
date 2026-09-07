using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateResolutionResultTests
{
    [Fact]
    public void RetainsExactPerceptionAndCandidateSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();

        ObservationReinterpretationCandidateResolutionResult result =
            new(perception, candidates);

        Assert.Same(perception, result.Perception);
        Assert.Same(candidates, result.Candidates);
    }

    [Fact]
    public void EqualityUsesRetainedSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        ObservationReinterpretationCandidateSnapshot candidates = Candidates();
        ObservationReinterpretationCandidateResolutionResult first =
            new(perception, candidates);
        ObservationReinterpretationCandidateResolutionResult same =
            new(perception, candidates);
        ObservationReinterpretationCandidateResolutionResult differentPerception =
            new(Perception(), candidates);
        ObservationReinterpretationCandidateResolutionResult differentCandidates =
            new(perception, Candidates());

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentPerception);
        Assert.NotEqual(first, differentCandidates);
    }

    [Fact]
    public void RejectsNullSnapshots()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ObservationReinterpretationCandidateResolutionResult(null!, Candidates()));
        Assert.Throws<ArgumentNullException>(() =>
            new ObservationReinterpretationCandidateResolutionResult(Perception(), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResolutionResultSurface()
    {
        Type resultType = typeof(ObservationReinterpretationCandidateResolutionResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Candidates", typeof(ObservationReinterpretationCandidateSnapshot)),
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
                typeof(CharacterPerceptionSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(ObservationReinterpretationCandidateSnapshot), parameter.ParameterType));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static CharacterPerceptionSnapshot Perception() =>
        new(new CharacterId("seeker"), Array.Empty<WorldObjectVisibility>());

    private static ObservationReinterpretationCandidateSnapshot Candidates() =>
        new(Array.Empty<ObservationReinterpretationCandidate>());
}

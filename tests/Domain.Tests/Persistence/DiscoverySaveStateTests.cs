using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class DiscoverySaveStateTests
{
    [Fact]
    public void RetainsEveryExactDiscoverySnapshot()
    {
        CharacterPerceptionSnapshot perception = new(new CharacterId("seeker"), []);
        FocusedSearchActivitySnapshot activities = new([]);
        FocusedSearchOutcomeSnapshot outcomes = new([]);
        PassiveNoticingCandidateSnapshot passiveCandidates = new([]);
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates = new([]);

        DiscoverySaveState state = new(
            perception,
            activities,
            outcomes,
            passiveCandidates,
            reinterpretationCandidates);

        Assert.Same(perception, state.Perception);
        Assert.Same(activities, state.FocusedSearchActivities);
        Assert.Same(outcomes, state.FocusedSearchOutcomes);
        Assert.Same(passiveCandidates, state.PassiveNoticingCandidates);
        Assert.Same(reinterpretationCandidates, state.ObservationReinterpretationCandidates);
    }

    [Fact]
    public void EqualityUsesTheRetainedSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = new(new CharacterId("seeker"), []);
        FocusedSearchActivitySnapshot activities = new([]);
        FocusedSearchOutcomeSnapshot outcomes = new([]);
        PassiveNoticingCandidateSnapshot passiveCandidates = new([]);
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates = new([]);
        DiscoverySaveState first = new(
            perception,
            activities,
            outcomes,
            passiveCandidates,
            reinterpretationCandidates);
        DiscoverySaveState same = new(
            perception,
            activities,
            outcomes,
            passiveCandidates,
            reinterpretationCandidates);
        DiscoverySaveState distinctEmptySnapshots = new(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, distinctEmptySnapshots);
    }

    [Fact]
    public void RejectsEveryNullSnapshot()
    {
        CharacterPerceptionSnapshot perception = new(new CharacterId("seeker"), []);
        FocusedSearchActivitySnapshot activities = new([]);
        FocusedSearchOutcomeSnapshot outcomes = new([]);
        PassiveNoticingCandidateSnapshot passiveCandidates = new([]);
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates = new([]);

        Assert.Throws<ArgumentNullException>(() => new DiscoverySaveState(
            null!, activities, outcomes, passiveCandidates, reinterpretationCandidates));
        Assert.Throws<ArgumentNullException>(() => new DiscoverySaveState(
            perception, null!, outcomes, passiveCandidates, reinterpretationCandidates));
        Assert.Throws<ArgumentNullException>(() => new DiscoverySaveState(
            perception, activities, null!, passiveCandidates, reinterpretationCandidates));
        Assert.Throws<ArgumentNullException>(() => new DiscoverySaveState(
            perception, activities, outcomes, null!, reinterpretationCandidates));
        Assert.Throws<ArgumentNullException>(() => new DiscoverySaveState(
            perception, activities, outcomes, passiveCandidates, null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDiscoveryStateSurface()
    {
        Type stateType = typeof(DiscoverySaveState);
        (string Name, Type Type)[] expectedProperties =
        [
            ("FocusedSearchActivities", typeof(FocusedSearchActivitySnapshot)),
            ("FocusedSearchOutcomes", typeof(FocusedSearchOutcomeSnapshot)),
            (
                "ObservationReinterpretationCandidates",
                typeof(ObservationReinterpretationCandidateSnapshot)),
            ("PassiveNoticingCandidates", typeof(PassiveNoticingCandidateSnapshot)),
            ("Perception", typeof(CharacterPerceptionSnapshot)),
        ];

        System.Reflection.PropertyInfo[] properties = stateType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedProperties, properties.Select(property =>
            (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(stateType.GetEvents());
        Assert.True(stateType.IsSealed);
    }
}

using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Investigations;

public sealed class StreamResidueArrivalNoticingTests
{
    [Fact]
    public void ArrivingAtTheStreamNoticesOnlyTheResidueForTheObserver()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        CharacterEvidenceViewSnapshot? view =
            StreamResidueArrivalNoticing.Notice(scenario, new LocationId("stream"));

        Assert.NotNull(view);
        Assert.Same(scenario.ObserverCharacterId, view.CharacterId);
        EvidenceId residue = Assert.Single(view.EvidenceIds);
        Assert.Same(
            scenario.Evidence.Entries.Single(evidence => evidence.EvidenceId.Value == "residue").EvidenceId,
            residue);
    }

    [Theory]
    [InlineData("abandoned-laundry")]
    [InlineData("old-shrine")]
    [InlineData("southern-road")]
    public void ArrivingAnywhereElseNoticesNothing(string locationId)
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        Assert.Null(StreamResidueArrivalNoticing.Notice(scenario, new LocationId(locationId)));
    }

    [Fact]
    public void IsStatelessSoEveryArrivalGivesAnEqualFreshView()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        LocationId stream = new("stream");

        CharacterEvidenceViewSnapshot first = StreamResidueArrivalNoticing.Notice(scenario, stream)!;
        CharacterEvidenceViewSnapshot second = StreamResidueArrivalNoticing.Notice(scenario, stream)!;

        Assert.NotSame(first, second);
        Assert.Equal(first.EvidenceIds, second.EvidenceIds);
    }

    [Fact]
    public void RejectsNulls()
    {
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();

        Assert.Throws<ArgumentNullException>(() =>
            StreamResidueArrivalNoticing.Notice(null!, new LocationId("stream")));
        Assert.Throws<ArgumentNullException>(() => StreamResidueArrivalNoticing.Notice(scenario, null!));
    }
}

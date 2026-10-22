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
    [InlineData("abandoned-wagon")]
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
    public void FailsLoudlyWhenTheStreamNoticingExposesNoResidue()
    {
        DiscoveryScenario authored = RepresentativeDiscoveryScenarioSource.Create();
        DiscoveryScenario withoutResidue = new(
            authored.ObserverCharacterId,
            authored.WorldObjects,
            new DiscoveryEvidenceSnapshot(
                authored.Evidence.Entries.Where(evidence => evidence.EvidenceId.Value != "residue")),
            authored.InitialPerception,
            authored.Conditions,
            authored.AvailableCapabilities,
            authored.AvailableInventory,
            authored.AvailableKnowledge,
            authored.PassiveNoticingCandidates,
            authored.ReinterpretationCandidates);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            StreamResidueArrivalNoticing.Notice(withoutResidue, new LocationId("stream")));
        Assert.Contains("residue", error.Message, StringComparison.Ordinal);
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

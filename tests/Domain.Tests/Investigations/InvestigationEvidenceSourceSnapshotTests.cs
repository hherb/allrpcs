using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationEvidenceSourceSnapshotTests
{
    [Fact]
    public void CopiesSourcesInStableOrderAndRetainsExactInstances()
    {
        InvestigationEvidenceSource first = Source("laundry", "laundry-residue", false);
        InvestigationEvidenceSource second = Source("healer", "healer-testimony", true);
        InvestigationEvidenceSource replacement = Source("replacement", "replacement", false);
        InvestigationEvidenceSource[] supplied = [first, second];

        InvestigationEvidenceSourceSnapshot snapshot = new(supplied);
        supplied[0] = replacement;

        Assert.Collection(
            snapshot.Sources,
            source => Assert.Same(first, source),
            source => Assert.Same(second, source));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InvestigationEvidenceSource>)snapshot.Sources).Add(replacement));
    }

    [Fact]
    public void AllowsSharedSourcesButRejectsDuplicateEvidenceAndInvalidCollections()
    {
        InvestigationEvidenceSource first = Source("healer", "first-note", true);
        InvestigationEvidenceSource second = Source("healer", "second-note", true);
        InvestigationEvidenceSource[] maximum = Enumerable
            .Range(0, InvestigationEvidenceSourceSnapshot.MaximumSourceCount)
            .Select(index => Source($"source-{index}", $"evidence-{index}", false))
            .ToArray();

        Assert.Equal(2, new InvestigationEvidenceSourceSnapshot([first, second]).Sources.Count);
        Assert.Equal(
            InvestigationEvidenceSourceSnapshot.MaximumSourceCount,
            new InvestigationEvidenceSourceSnapshot(maximum).Sources.Count);
        Assert.Throws<ArgumentNullException>(() => new InvestigationEvidenceSourceSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new InvestigationEvidenceSourceSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new InvestigationEvidenceSourceSnapshot(
            [first, Source("different-source", first.EvidenceId.Value, false)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InvestigationEvidenceSourceSnapshot(
            maximum.Append(Source("extra-source", "extra-evidence", false))));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySourceSurface()
    {
        Type snapshotType = typeof(InvestigationEvidenceSourceSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Sources", property.Name);
        Assert.Equal(typeof(IReadOnlyList<InvestigationEvidenceSource>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal(
            typeof(IEnumerable<InvestigationEvidenceSource>),
            Assert.Single(constructor.GetParameters()).ParameterType);
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    internal static InvestigationEvidenceSource Source(
        string sourceId,
        string evidenceId,
        bool requiresCooperation) =>
        new(
            new InvestigationEvidenceSourceId(sourceId),
            new EvidenceId(evidenceId),
            requiresCooperation);
}

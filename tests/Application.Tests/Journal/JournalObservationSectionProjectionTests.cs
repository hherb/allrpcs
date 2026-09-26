using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalObservationSectionProjectionTests
{
    [Fact]
    public void BindsTheExactObservationsSectionToExactFilteredSeekerPerception()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        WorldObjectVisibility noticed = new(new ObjectId("herb-patch"), VisibilityState.Noticed);
        WorldObjectVisibility identified = new(new ObjectId("old-shrine"), VisibilityState.Identified);
        CharacterPerceptionSnapshot perception = new(seekerId, [noticed, identified]);

        JournalObservationSectionBinding binding = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            perception);

        Assert.Same(catalogue, binding.Catalogue);
        Assert.Same(catalogue.Sections[0], binding.Section);
        Assert.Equal("observations", binding.Section.Key);
        Assert.Same(seekerId, binding.SeekerId);
        Assert.Same(perception, binding.Perception);
        Assert.Collection(
            binding.Perception.Entries,
            entry => Assert.Same(noticed, entry),
            entry => Assert.Same(identified, entry));
    }

    [Fact]
    public void AcceptsAnExactEmptyFilteredSnapshotWithoutCreatingContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        CharacterPerceptionSnapshot perception = new(seekerId, []);

        JournalObservationSectionBinding binding = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            perception);

        Assert.Same(perception, binding.Perception);
        Assert.Empty(binding.Perception.Entries);
    }

    [Fact]
    public void RejectsValueEqualRebuiltOrDifferentCharacterPerception()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        CharacterPerceptionSnapshot rebuilt = new(new CharacterId("seeker"), []);
        CharacterPerceptionSnapshot different = new(new CharacterId("healer"), []);

        Assert.Throws<ArgumentException>(() => JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            rebuilt));
        Assert.Throws<ArgumentException>(() => JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            different));
    }

    [Fact]
    public void RejectsSnapshotsThatStillContainUnnoticedEntries()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        WorldObjectVisibility unnoticed = new(
            new ObjectId("hidden-specimen"),
            VisibilityState.Unnoticed);
        CharacterPerceptionSnapshot perception = new(seekerId, [unnoticed]);

        Assert.Throws<ArgumentException>(() => JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            perception));
        Assert.Same(unnoticed, Assert.Single(perception.Entries));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        CharacterPerceptionSnapshot perception = new(seekerId, []);

        Assert.Throws<ArgumentNullException>(() => JournalObservationSectionProjection.Bind(
            null!,
            seekerId,
            perception));
        Assert.Throws<ArgumentNullException>(() => JournalObservationSectionProjection.Bind(
            catalogue,
            null!,
            perception));
        Assert.Throws<ArgumentNullException>(() => JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            null!));
    }
}

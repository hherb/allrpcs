using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSectionPresentationProjectionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ProjectsOnlyTheExactSelectedSafeSection(int sectionIndex)
    {
        JournalSnapshot journal = CompleteJournal();
        JournalSectionDefinition section = journal.Catalogue.Sections[sectionIndex];

        JournalSectionPresentationSnapshot result =
            JournalSectionPresentationProjection.Project(journal, section);

        Assert.Same(section, result.Section);
        Assert.Equal(sectionIndex == 0, ReferenceEquals(journal.Observations, result.Observations));
        Assert.Equal(sectionIndex == 1, ReferenceEquals(journal.Testimony, result.Testimony));
        Assert.Equal(sectionIndex == 2, ReferenceEquals(journal.Hypotheses, result.Hypotheses));
        Assert.Equal(sectionIndex == 3, ReferenceEquals(journal.Experiments, result.Experiments));
        Assert.Equal(sectionIndex == 4, ReferenceEquals(journal.OpenQuestions, result.OpenQuestions));
    }

    [Fact]
    public void RejectsIncompleteForeignRebuiltAndNullInputs()
    {
        JournalSnapshot journal = CompleteJournal();
        JournalSnapshot foreign = CompleteJournal();
        JournalSnapshot incomplete = ObservationsOnlyJournal();
        JournalSectionDefinition exact = journal.Catalogue.Sections[0];
        JournalSectionDefinition rebuilt = new(exact.Key, exact.DisplayLabel);

        Assert.Throws<InvalidOperationException>(
            () => JournalSectionPresentationProjection.Project(incomplete, exact));
        Assert.Throws<InvalidOperationException>(
            () => JournalSectionPresentationProjection.Project(journal, foreign.Catalogue.Sections[0]));
        Assert.Throws<InvalidOperationException>(
            () => JournalSectionPresentationProjection.Project(journal, rebuilt));
        Assert.Throws<ArgumentNullException>(
            () => JournalSectionPresentationProjection.Project(null!, exact));
        Assert.Throws<ArgumentNullException>(
            () => JournalSectionPresentationProjection.Project(journal, null!));
    }

    [Fact]
    public void ExposesOnlyTheSelectedSafeContentUnion()
    {
        Type snapshotType = typeof(JournalSectionPresentationSnapshot);
        System.Reflection.PropertyInfo[] properties = snapshotType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            snapshotType.GetConstructors(
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic));
        System.Reflection.MethodInfo method = Assert.Single(
            typeof(JournalSectionPresentationProjection).GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(
            [
                typeof(JournalSectionDefinition),
                typeof(JournalObservationSectionSnapshot),
                typeof(JournalTestimonySectionSnapshot),
                typeof(JournalHypothesisSectionSnapshot),
                typeof(JournalExperimentSectionSnapshot),
                typeof(JournalOpenQuestionSectionSnapshot),
            ],
            properties.Select(property => property.PropertyType));
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(6, constructor.GetParameters().Length);
        Assert.Equal(nameof(JournalSectionPresentationProjection.Project), method.Name);
        Assert.Equal(typeof(JournalSectionPresentationSnapshot), method.ReturnType);
        Assert.True(snapshotType.IsSealed);
        Assert.True(typeof(JournalSectionPresentationProjection).IsAbstract);
        Assert.True(typeof(JournalSectionPresentationProjection).IsSealed);
    }

    private static JournalSnapshot CompleteJournal()
    {
        JournalSnapshot journal = ObservationsOnlyJournal();
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(journal, []));
        journal = JournalTestimonySnapshotProjection.Compose(journal, testimony);
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot([])));
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        journal = JournalExperimentSnapshotProjection.Compose(journal, experiments);
        JournalOpenQuestionSectionSnapshot questions =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(
                    journal,
                    new OpenInvestigationQuestionSnapshot([])));
        return JournalOpenQuestionSnapshotProjection.Compose(journal, questions);
    }

    private static JournalSnapshot ObservationsOnlyJournal()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        return JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
    }
}

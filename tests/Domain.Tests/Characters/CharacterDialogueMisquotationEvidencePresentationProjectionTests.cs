using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidencePresentationProjectionTests
{
    [Fact]
    public void ProjectsExactSelectedEvidenceAsTruthNeutralEventPair()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot view = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidencePresentationSelection selection = new(
            view,
            input.Evidence);

        CharacterDialogueMisquotationEvidencePresentationSnapshot? presentation =
            CharacterDialogueMisquotationEvidencePresentationProjection.Project(view, selection);

        Assert.NotNull(presentation);
        Assert.Same(selection, presentation.Selection);
        Assert.Same(input.WitnessId, presentation.WitnessCharacterId);
        Assert.Same(
            input.Evidence.Occurrence.Proposal.SourceEvent,
            presentation.SourceEvent);
        Assert.Same(input.Evidence.Occurrence.DeliveredEvent, presentation.DeliveredEvent);
        Assert.Same(view, selection.WitnessView);
        Assert.Same(input.Evidence, selection.SelectedEvidence);
        Assert.Single(view.Entries);
        Assert.Same(input.Evidence, view.Entries[0]);
    }

    [Fact]
    public void ReturnsNoPresentationForAbstentionOrNonCurrentView()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot selectedView = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidenceViewSnapshot distinctEquivalentView = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidenceViewSnapshot advancedView = new(
            input.WitnessId,
            []);
        CharacterDialogueMisquotationEvidencePresentationSelection selection = new(
            selectedView,
            input.Evidence);
        CharacterDialogueMisquotationEvidencePresentationSelection abstention = new(
            selectedView,
            null);

        Assert.Null(CharacterDialogueMisquotationEvidencePresentationProjection.Project(
            selectedView,
            abstention));
        Assert.Null(CharacterDialogueMisquotationEvidencePresentationProjection.Project(
            distinctEquivalentView,
            selection));
        Assert.Null(CharacterDialogueMisquotationEvidencePresentationProjection.Project(
            advancedView,
            selection));
        Assert.NotSame(selectedView, distinctEquivalentView);
        Assert.Same(selectedView.WitnessCharacterId, distinctEquivalentView.WitnessCharacterId);
        Assert.Same(selectedView.Entries[0], distinctEquivalentView.Entries[0]);
        Assert.Empty(advancedView.Entries);
    }

    [Fact]
    public void RejectsNullsAndAbstainingSnapshotConstruction()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot view = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidencePresentationSelection selection = new(
            view,
            input.Evidence);
        CharacterDialogueMisquotationEvidencePresentationSelection abstention = new(view, null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidencePresentationSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidencePresentationSnapshot(abstention));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePresentationProjection.Project(null!, selection));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePresentationProjection.Project(view, null!));
    }

    [Fact]
    public void ExposesOnlyClosedImmutableSnapshotAndPureProjectionSurfaces()
    {
        Type snapshotType = typeof(CharacterDialogueMisquotationEvidencePresentationSnapshot);
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());
        System.Reflection.PropertyInfo[] properties = snapshotType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [typeof(CharacterDialogueMisquotationEvidencePresentationSelection)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["DeliveredEvent", "Selection", "SourceEvent", "WitnessCharacterId"],
            properties.Select(property => property.Name));
        Assert.Equal(
            [
                typeof(DialogueEvent),
                typeof(CharacterDialogueMisquotationEvidencePresentationSelection),
                typeof(DialogueEvent),
                typeof(CharacterId),
            ],
            properties.Select(property => property.PropertyType));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);

        Type projectionType = typeof(CharacterDialogueMisquotationEvidencePresentationProjection);
        System.Reflection.MethodInfo method = Assert.Single(projectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == projectionType));
        Assert.Equal("Project", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(CharacterDialogueMisquotationEvidencePresentationSnapshot),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterDialogueMisquotationEvidenceViewSnapshot),
                typeof(CharacterDialogueMisquotationEvidencePresentationSelection),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract && projectionType.IsSealed);
    }
}

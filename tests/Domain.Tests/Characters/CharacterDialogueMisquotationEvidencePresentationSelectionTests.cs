using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidencePresentationSelectionTests
{
    [Fact]
    public void RetainsExactViewAndMemberEvidenceOrExplicitAbstention()
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

        Assert.Same(view, selection.WitnessView);
        Assert.Same(input.Evidence, selection.SelectedEvidence);
        Assert.NotNull(selection.SelectedEvidence);
        Assert.Same(input.Evidence.Occurrence, selection.SelectedEvidence.Occurrence);
        Assert.Same(view, abstention.WitnessView);
        Assert.Null(abstention.SelectedEvidence);
    }

    [Fact]
    public void RejectsNonMemberAndDistinctValueEqualEvidence()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot view = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidence distinctValueEqual = new(
            input.Evidence.Occurrence,
            new CharacterId(input.WitnessId.Value));

        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidencePresentationSelection(
                view,
                CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateEvidence(
                    input.Actor,
                    input.WitnessId,
                    "A different exposed misquotation.")));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidencePresentationSelection(
                view,
                distinctValueEqual));
        Assert.NotSame(input.Evidence, distinctValueEqual);
        Assert.Same(input.Evidence.Occurrence, distinctValueEqual.Occurrence);
        Assert.Equal(input.WitnessId, distinctValueEqual.WitnessCharacterId);
        Assert.NotSame(input.WitnessId, distinctValueEqual.WitnessCharacterId);
    }

    [Fact]
    public void AcceptsCurrentSelectionAndAbstentionButRejectsStaleView()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot currentView = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidenceViewSnapshot staleValueEqualView = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidencePresentationSelection selection = new(
            currentView,
            input.Evidence);
        CharacterDialogueMisquotationEvidencePresentationSelection abstention = new(
            currentView,
            null);

        Assert.True(CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            currentView,
            selection));
        Assert.True(CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            currentView,
            abstention));
        Assert.False(CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
            staleValueEqualView,
            selection));
        Assert.NotSame(currentView, staleValueEqualView);
        Assert.Same(currentView.WitnessCharacterId, staleValueEqualView.WitnessCharacterId);
        Assert.Same(currentView.Entries[0], staleValueEqualView.Entries[0]);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyClosedImmutableSurfaces()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceViewSnapshot view = new(
            input.WitnessId,
            [input.Evidence]);
        CharacterDialogueMisquotationEvidencePresentationSelection selection = new(
            view,
            input.Evidence);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidencePresentationSelection(null!, null));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
                null!,
                selection));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
                view,
                null!));

        Type selectionType = typeof(CharacterDialogueMisquotationEvidencePresentationSelection);
        System.Reflection.ConstructorInfo constructor = Assert.Single(selectionType.GetConstructors());
        System.Reflection.PropertyInfo[] properties = selectionType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterDialogueMisquotationEvidenceViewSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterDialogueMisquotationEvidence),
                parameter.ParameterType));
        Assert.Equal(
            ["SelectedEvidence", "WitnessView"],
            properties.Select(property => property.Name));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(selectionType.GetEvents());
        Assert.True(selectionType.IsSealed);

        Type policyType = typeof(CharacterDialogueMisquotationEvidencePresentationSelectionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceViewProjectionTests
{
    [Fact]
    public void ProjectsOnlyExactWitnessEvidenceInStableExposureOrder()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterId otherWitnessId = new("healer");
        CharacterDialogueMisquotationEvidence first =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateEvidence(
                input.Actor,
                input.WitnessId,
                "First visible misquotation.");
        CharacterDialogueMisquotationEvidence excluded =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateEvidence(
                input.Actor,
                otherWitnessId,
                "Other witness misquotation.");
        CharacterDialogueMisquotationEvidence last =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateEvidence(
                input.Actor,
                input.WitnessId,
                "Last visible misquotation.");
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure = new(
            [first, excluded, last]);

        CharacterDialogueMisquotationEvidenceViewSnapshot view =
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(
                exposure,
                input.WitnessId);

        Assert.Same(input.WitnessId, view.WitnessCharacterId);
        Assert.Collection(
            view.Entries,
            evidence =>
            {
                Assert.Same(first, evidence);
                Assert.Same(first.Occurrence, evidence.Occurrence);
                Assert.Same(first.Occurrence.DeliveredEvent, evidence.Occurrence.DeliveredEvent);
            },
            evidence =>
            {
                Assert.Same(last, evidence);
                Assert.Same(last.Occurrence, evidence.Occurrence);
                Assert.Same(last.Occurrence.DeliveredEvent, evidence.Occurrence.DeliveredEvent);
            });
    }

    [Fact]
    public void ReturnsEmptyViewForValueEqualDistinctOrUnknownWitness()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure = new([input.Evidence]);
        CharacterId valueEqualWitnessId = new(input.WitnessId.Value);
        CharacterId unknownWitnessId = new("healer");

        CharacterDialogueMisquotationEvidenceViewSnapshot valueEqualView =
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(
                exposure,
                valueEqualWitnessId);
        CharacterDialogueMisquotationEvidenceViewSnapshot unknownView =
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(
                exposure,
                unknownWitnessId);

        Assert.Equal(input.WitnessId, valueEqualWitnessId);
        Assert.NotSame(input.WitnessId, valueEqualWitnessId);
        Assert.Same(valueEqualWitnessId, valueEqualView.WitnessCharacterId);
        Assert.Empty(valueEqualView.Entries);
        Assert.Same(unknownWitnessId, unknownView.WitnessCharacterId);
        Assert.Empty(unknownView.Entries);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyPureProjection()
    {
        CharacterDialogueMisquotationEvidenceViewSnapshotTests.ViewInput input =
            CharacterDialogueMisquotationEvidenceViewSnapshotTests.CreateInput();
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure = new([input.Evidence]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(null!, input.WitnessId));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(exposure, null!));

        Type projectionType = typeof(CharacterDialogueMisquotationEvidenceViewProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("ForWitness", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterDialogueMisquotationEvidenceViewSnapshot), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterDialogueMisquotationEvidenceExposureSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract && projectionType.IsSealed);
    }
}

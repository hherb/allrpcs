using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Diseases;

public sealed class DiseaseConditionProjectionTests
{
    public static TheoryData<DiseaseStage, string, string> ObservableConditions => new()
    {
        { DiseaseStage.EarlyIllness, "appears-unwell", "Appears unwell." },
        { DiseaseStage.Drowsing, "appears-drowsy", "Appears increasingly drowsy." },
        { DiseaseStage.DeepSleep, "deeply-asleep", "Is deeply asleep and cannot be roused." },
        { DiseaseStage.Wasting, "severely-weakened", "Appears severely weakened." },
        { DiseaseStage.FinalDecline, "near-death", "Appears close to death." },
    };

    [Fact]
    public void KeepsExposureUnobservable()
    {
        (CharacterDiseaseState Infection, DiseaseTruthSnapshot Truth) input =
            Create(DiseaseStage.Exposed);

        Assert.Null(DiseaseConditionProjection.Project(input.Infection, input.Truth));
    }

    [Theory]
    [MemberData(nameof(ObservableConditions))]
    public void ProjectsOnlyStableQualitativeConditionText(
        DiseaseStage stage,
        string expectedKey,
        string expectedDescription)
    {
        (CharacterDiseaseState Infection, DiseaseTruthSnapshot Truth) input = Create(stage);

        CharacterCondition condition = Assert.IsType<CharacterCondition>(
            DiseaseConditionProjection.Project(input.Infection, input.Truth));

        Assert.Equal(expectedKey, condition.Key);
        Assert.Equal(expectedDescription, condition.Description);
        Assert.DoesNotContain(input.Infection.Disease.DiseaseId.Value, condition.Key);
        Assert.DoesNotContain(input.Infection.Disease.DiseaseId.Value, condition.Description);
        Assert.DoesNotContain(condition.Description, char.IsDigit);
    }

    [Fact]
    public void RejectsAValueEqualReplacementAndNullInputs()
    {
        (CharacterDiseaseState Infection, DiseaseTruthSnapshot Truth) input =
            Create(DiseaseStage.EarlyIllness);
        CharacterDiseaseState replacement = new(
            new CharacterId(input.Infection.CharacterId.Value),
            input.Infection.Disease,
            input.Infection.Stage,
            input.Infection.LastProgressionEventId);

        Assert.Throws<ArgumentException>(() =>
            DiseaseConditionProjection.Project(replacement, input.Truth));
        Assert.Throws<ArgumentNullException>(() =>
            DiseaseConditionProjection.Project(null!, input.Truth));
        Assert.Throws<ArgumentNullException>(() =>
            DiseaseConditionProjection.Project(input.Infection, null!));
    }

    [Fact]
    public void ExposesOnlyThePureProjectionOperation()
    {
        Type projectionType = typeof(DiseaseConditionProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("Project", method.Name);
        Assert.Equal(typeof(CharacterCondition), method.ReturnType);
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static (CharacterDiseaseState Infection, DiseaseTruthSnapshot Truth) Create(
        DiseaseStage stage)
    {
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        CharacterDiseaseState infection = new(
            new CharacterId("patient"),
            disease,
            stage,
            null);

        return (
            infection,
            new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([disease]), [infection]));
    }
}

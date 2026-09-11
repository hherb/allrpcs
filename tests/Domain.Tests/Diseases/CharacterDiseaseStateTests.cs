using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class CharacterDiseaseStateTests
{
    [Fact]
    public void RetainsExactCharacterDiseaseAndStageInstances()
    {
        CharacterId characterId = new("patient");
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));
        DiseaseStage stage = DiseaseStage.EarlyIllness;
        WorldEventId lastProgressionEventId = new("patient-became-ill");

        CharacterDiseaseState state = new(
            characterId,
            disease,
            stage,
            lastProgressionEventId,
            DiseaseCareState.Unassisted);

        Assert.Same(characterId, state.CharacterId);
        Assert.Same(disease, state.Disease);
        Assert.Same(stage, state.Stage);
        Assert.Same(lastProgressionEventId, state.LastProgressionEventId);
        Assert.Same(DiseaseCareState.Unassisted, state.CareState);
    }

    [Fact]
    public void RejectsNullValues()
    {
        CharacterId characterId = new("patient");
        DiseaseDefinition disease = new(new DiseaseId("grey-sleep"));

        Assert.Throws<ArgumentNullException>(() => new CharacterDiseaseState(
            null!, disease, DiseaseStage.Exposed, null, DiseaseCareState.Unassisted));
        Assert.Throws<ArgumentNullException>(() => new CharacterDiseaseState(
            characterId, null!, DiseaseStage.Exposed, null, DiseaseCareState.Unassisted));
        Assert.Throws<ArgumentNullException>(() => new CharacterDiseaseState(
            characterId, disease, null!, null, DiseaseCareState.Unassisted));
        Assert.Throws<ArgumentNullException>(() => new CharacterDiseaseState(
            characterId, disease, DiseaseStage.Exposed, null, null!));
        Assert.Null(new CharacterDiseaseState(
            characterId,
            disease,
            DiseaseStage.Exposed,
            null,
            DiseaseCareState.Unassisted).LastProgressionEventId);
    }

    [Fact]
    public void ExposesOnlyImmutableAuthoritativeState()
    {
        Type stateType = typeof(CharacterDiseaseState);
        (string Name, Type Type, bool CanWrite)[] properties = stateType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(stateType.GetConstructors());

        Assert.Equal(
            [
                ("CareState", typeof(DiseaseCareState), false),
                ("CharacterId", typeof(CharacterId), false),
                ("Disease", typeof(DiseaseDefinition), false),
                ("LastProgressionEventId", typeof(WorldEventId), false),
                ("Stage", typeof(DiseaseStage), false),
            ],
            properties);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DiseaseDefinition), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DiseaseStage), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DiseaseCareState), parameter.ParameterType));
        Assert.Empty(stateType.GetEvents());
        Assert.True(stateType.IsSealed);
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterControllerAssignmentTests
{
    [Fact]
    public void RetainsCharacterAndControllerTypeExactly()
    {
        CharacterId characterId = new("travelling-healer");

        CharacterControllerAssignment assignment = new(characterId, ControllerType.Llm);

        Assert.Same(characterId, assignment.CharacterId);
        Assert.Same(ControllerType.Llm, assignment.ControllerType);
    }

    [Fact]
    public void EqualityUsesCharacterAndControllerTypeValues()
    {
        CharacterControllerAssignment first = new(
            new CharacterId("healer"),
            ControllerType.Llm);
        CharacterControllerAssignment same = new(
            new CharacterId("healer"),
            ControllerType.Llm);
        CharacterControllerAssignment differentCharacter = new(
            new CharacterId("sage"),
            ControllerType.Llm);
        CharacterControllerAssignment differentController = new(
            new CharacterId("healer"),
            ControllerType.Human);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentCharacter);
        Assert.NotEqual(first, differentController);
    }

    [Fact]
    public void AcceptsEveryDefinedControllerType()
    {
        CharacterId characterId = new("healer");

        CharacterControllerAssignment[] assignments = ControllerType.All
            .Select(type => new CharacterControllerAssignment(characterId, type))
            .ToArray();

        Assert.Collection(
            assignments,
            assignment => Assert.Same(ControllerType.Human, assignment.ControllerType),
            assignment => Assert.Same(ControllerType.Llm, assignment.ControllerType),
            assignment => Assert.Same(ControllerType.DeterministicTest, assignment.ControllerType),
            assignment => Assert.Same(ControllerType.Dormant, assignment.ControllerType));
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CharacterControllerAssignment(null!, ControllerType.Human));
        Assert.Throws<ArgumentNullException>(
            () => new CharacterControllerAssignment(new CharacterId("seeker"), null!));
    }

    [Fact]
    public void ExposesNoProviderModelConfigurationOrBehaviorSurface()
    {
        string[] publicProperties = typeof(CharacterControllerAssignment)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("CharacterId", property),
            property => Assert.Equal("ControllerType", property));
    }
}

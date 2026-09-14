using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageMoveDecisionTests
{
    [Fact]
    public void RetainsExactSelectedEvidenceDecision()
    {
        CharacterId characterId = new("enemy-agent");
        EvidenceId evidenceId = new("cask-drag-marks");

        CharacterObjectSabotageMoveDecision decision = new(characterId, evidenceId);

        Assert.Same(characterId, decision.CharacterId);
        Assert.Same(evidenceId, decision.SelectedEvidenceId);
    }

    [Fact]
    public void RetainsExplicitAbstention()
    {
        CharacterId characterId = new("enemy-agent");

        CharacterObjectSabotageMoveDecision decision = new(characterId, null);

        Assert.Same(characterId, decision.CharacterId);
        Assert.Null(decision.SelectedEvidenceId);
    }

    [Fact]
    public void RejectsNullCharacterIdentifier()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveDecision(null!, null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralDecisionSurface()
    {
        Type decisionType = typeof(CharacterObjectSabotageMoveDecision);
        (string Name, Type Type, bool CanWrite)[] properties = decisionType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                ("CharacterId", typeof(CharacterId)),
                ("SelectedEvidenceId", typeof(EvidenceId)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(decisionType.GetConstructors());
        Assert.Empty(decisionType.GetEvents());
        Assert.True(decisionType.IsSealed);
    }
}

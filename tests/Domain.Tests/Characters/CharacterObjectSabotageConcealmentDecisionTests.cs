using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentDecisionTests
{
    [Fact]
    public void RetainsExactSelectedEvidenceDecision()
    {
        CharacterId characterId = new("enemy-agent");
        EvidenceId evidenceId = new("disturbed-cask-cover");

        CharacterObjectSabotageConcealmentDecision decision = new(characterId, evidenceId);

        Assert.Same(characterId, decision.CharacterId);
        Assert.Same(evidenceId, decision.SelectedEvidenceId);
    }

    [Fact]
    public void RetainsExplicitAbstentionAndRejectsNullCharacter()
    {
        CharacterId characterId = new("enemy-agent");

        CharacterObjectSabotageConcealmentDecision abstention = new(characterId, null);

        Assert.Same(characterId, abstention.CharacterId);
        Assert.Null(abstention.SelectedEvidenceId);
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentDecision(null!, null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralDecisionSurface()
    {
        Type decisionType = typeof(CharacterObjectSabotageConcealmentDecision);
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
        Assert.Empty(decisionType.GetEvents());
        Assert.True(decisionType.IsSealed);
    }
}

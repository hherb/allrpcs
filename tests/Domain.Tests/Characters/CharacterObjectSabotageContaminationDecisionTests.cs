using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationDecisionTests
{
    [Fact]
    public void RetainsExactSelectionOrExplicitAbstention()
    {
        CharacterId characterId = new("enemy-agent");
        EvidenceId evidenceId = new("bitter-cask-residue");

        CharacterObjectSabotageContaminationDecision selection = new(characterId, evidenceId);
        CharacterObjectSabotageContaminationDecision abstention = new(characterId, null);

        Assert.Same(characterId, selection.CharacterId);
        Assert.Same(evidenceId, selection.SelectedEvidenceId);
        Assert.Same(characterId, abstention.CharacterId);
        Assert.Null(abstention.SelectedEvidenceId);
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationDecision(null!, null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableProviderNeutralDecisionSurface()
    {
        Type decisionType = typeof(CharacterObjectSabotageContaminationDecision);
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

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationDecisionTests
{
    [Fact]
    public void RetainsExactRespondingSpeakerWithoutAMotive()
    {
        CharacterId speaker = new("village-sage");

        CharacterTurnParticipationDecision decision = new(
            speaker,
            CharacterTurnParticipationKind.Respond,
            null);

        Assert.Same(speaker, decision.ScheduledCharacterId);
        Assert.Same(CharacterTurnParticipationKind.Respond, decision.Kind);
        Assert.Null(decision.Motive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RetainsExactMotiveForInterjectionOrMeaningfulSilence(bool interject)
    {
        CharacterId speaker = new("woodland-hermit");
        CharacterGoal motive = new("protect-refuge", "Keep the refuge concealed.");
        CharacterTurnParticipationKind kind = interject
            ? CharacterTurnParticipationKind.Interject
            : CharacterTurnParticipationKind.RemainSilent;

        CharacterTurnParticipationDecision decision = new(speaker, kind, motive);

        Assert.Same(speaker, decision.ScheduledCharacterId);
        Assert.Same(kind, decision.Kind);
        Assert.Same(motive, decision.Motive);
    }

    [Fact]
    public void RejectsInvalidKindAndMotiveCombinations()
    {
        CharacterId speaker = new("village-sage");
        CharacterGoal motive = new("compare-evidence", "Compare available evidence.");

        Assert.Throws<ArgumentException>(() => new CharacterTurnParticipationDecision(
            speaker,
            CharacterTurnParticipationKind.Respond,
            motive));
        Assert.Throws<ArgumentException>(() => new CharacterTurnParticipationDecision(
            speaker,
            CharacterTurnParticipationKind.Interject,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterTurnParticipationDecision(
            speaker,
            CharacterTurnParticipationKind.RemainSilent,
            null));
    }

    [Fact]
    public void RejectsNullRequiredValues()
    {
        CharacterId speaker = new("village-sage");

        Assert.Throws<ArgumentNullException>(() => new CharacterTurnParticipationDecision(
            null!,
            CharacterTurnParticipationKind.Respond,
            null));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnParticipationDecision(
            speaker,
            null!,
            null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDecisionSurface()
    {
        Type decisionType = typeof(CharacterTurnParticipationDecision);
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            decisionType.GetConstructors());

        Assert.Equal(
            [typeof(CharacterId), typeof(CharacterTurnParticipationKind), typeof(CharacterGoal)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Kind", "Motive", "ScheduledCharacterId"],
            decisionType
                .GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(
            decisionType.GetProperties(),
            property => Assert.False(property.SetMethod?.IsPublic ?? false));
        Assert.Empty(decisionType.GetEvents());
    }
}

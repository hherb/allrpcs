using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnParticipationDecisionPolicyTests
{
    [Fact]
    public void AcceptsDirectResponseAndExactGoalMotivatedParticipation()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterId speaker = sage.Identity.Id;
        CharacterGoal motive = Assert.Single(sage.Goals.Entries);

        Assert.True(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.Respond,
                null),
            sage,
            true));
        Assert.True(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.Interject,
                motive),
            sage,
            false));
        Assert.True(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.RemainSilent,
                motive),
            sage,
            true));
        Assert.True(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.RemainSilent,
                motive),
            sage,
            false));
    }

    [Fact]
    public void RejectsResponseWithoutDirectAddressOrAddressedInterjection()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterId speaker = sage.Identity.Id;
        CharacterGoal motive = Assert.Single(sage.Goals.Entries);

        Assert.False(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.Respond,
                null),
            sage,
            false));
        Assert.False(CharacterTurnParticipationDecisionPolicy.CanAccept(
            new CharacterTurnParticipationDecision(
                speaker,
                CharacterTurnParticipationKind.Interject,
                motive),
            sage,
            true));
    }

    [Fact]
    public void RequiresTheExactCharacterIdentityAndAnExactAuthoredMotive()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterGoal motive = Assert.Single(sage.Goals.Entries);
        CharacterTurnParticipationDecision replacementSpeaker = new(
            new CharacterId(sage.Identity.Id.Value),
            CharacterTurnParticipationKind.Interject,
            motive);
        CharacterTurnParticipationDecision replacementMotive = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Interject,
            new CharacterGoal(motive.Key, motive.Description));

        Assert.False(CharacterTurnParticipationDecisionPolicy.CanAccept(
            replacementSpeaker,
            sage,
            false));
        Assert.False(CharacterTurnParticipationDecisionPolicy.CanAccept(
            replacementMotive,
            sage,
            false));
    }

    [Fact]
    public void RejectsNullDecisionOrCharacter()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        CharacterTurnParticipationDecision decision = new(
            sage.Identity.Id,
            CharacterTurnParticipationKind.Respond,
            null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationDecisionPolicy.CanAccept(null!, sage, true));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationDecisionPolicy.CanAccept(decision, null!, true));
    }

    [Fact]
    public void ExposesOnlyThePureAcceptanceQuery()
    {
        Type policyType = typeof(CharacterTurnParticipationDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAccept", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnParticipationDecision), typeof(Character), typeof(bool)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}

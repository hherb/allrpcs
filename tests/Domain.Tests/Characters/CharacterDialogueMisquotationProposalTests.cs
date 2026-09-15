using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationProposalTests
{
    private const string RequiredCapabilityKey = "assess-local-influence";

    [Fact]
    public void RetainsExactSourceAndAcceptsOnlyAnActorWhoHeardItWithCapability()
    {
        Input input = CreateInput();

        CharacterDialogueMisquotationProposal proposal = new(
            input.Character.Identity.Id,
            input.SourceEvent,
            "The sage said the remedy should wait for royal approval.",
            RequiredCapabilityKey);

        Assert.Same(input.Character.Identity.Id, proposal.ActorId);
        Assert.Same(input.SourceEvent, proposal.SourceEvent);
        Assert.Equal(
            "The sage said the remedy should wait for royal approval.",
            proposal.MisquotedText);
        Assert.Equal(RequiredCapabilityKey, proposal.RequiredCapabilityKey);
        Assert.Same(CharacterSabotageActionKind.Misquote, proposal.ActionKind);
        Assert.True(CharacterDialogueMisquotationProposalPolicy.CanPropose(
            input.Character,
            proposal));
        Assert.Equal("Use the clean herb now.", input.SourceEvent.Utterance.Text);
        Assert.Same(input.SourceAudience, input.SourceEvent.Audience);
    }

    [Fact]
    public void RejectsActorAudienceOrCapabilityMismatchWithoutChangingInputs()
    {
        Input input = CreateInput();
        CharacterDialogueMisquotationProposal valid = Proposal(input);
        CharacterDialogueMisquotationProposal wrongActor = new(
            new CharacterId("other-agent"),
            input.SourceEvent,
            valid.MisquotedText,
            RequiredCapabilityKey);
        DialogueEvent unheardEvent = SourceEvent(
            input.SourceSpeakerId,
            new CharacterId("other-listener"));
        CharacterDialogueMisquotationProposal unheard = new(
            input.Character.Identity.Id,
            unheardEvent,
            valid.MisquotedText,
            RequiredCapabilityKey);
        CharacterDialogueMisquotationProposal missingCapability = new(
            input.Character.Identity.Id,
            input.SourceEvent,
            valid.MisquotedText,
            "medical-practice");

        Assert.False(CharacterDialogueMisquotationProposalPolicy.CanPropose(
            input.Character,
            wrongActor));
        Assert.False(CharacterDialogueMisquotationProposalPolicy.CanPropose(
            input.Character,
            unheard));
        Assert.False(CharacterDialogueMisquotationProposalPolicy.CanPropose(
            input.Character,
            missingCapability));
        Assert.Same(input.SourceEvent, valid.SourceEvent);
        Assert.Same(input.SourceAudience, input.SourceEvent.Audience);
        Assert.Equal("Use the clean herb now.", input.SourceEvent.Utterance.Text);
    }

    [Fact]
    public void RejectsTruthEquivalentMalformedAndNullConstruction()
    {
        Input input = CreateInput();
        string excessive = new('x', DialogueUtterance.MaximumTextLength + 1);

        Assert.Throws<ArgumentException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            input.SourceEvent,
            input.SourceEvent.Utterance.Text,
            RequiredCapabilityKey));
        Assert.Throws<ArgumentException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            input.SourceEvent,
            " ",
            RequiredCapabilityKey));
        Assert.Throws<ArgumentException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            input.SourceEvent,
            excessive,
            RequiredCapabilityKey));
        Assert.Throws<ArgumentException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            input.SourceEvent,
            "Different text.",
            "Assess-local-influence"));
        Assert.Throws<ArgumentNullException>(() => new CharacterDialogueMisquotationProposal(
            null!,
            input.SourceEvent,
            "Different text.",
            RequiredCapabilityKey));
        Assert.Throws<ArgumentNullException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            null!,
            "Different text.",
            RequiredCapabilityKey));
        Assert.Throws<ArgumentNullException>(() => new CharacterDialogueMisquotationProposal(
            input.Character.Identity.Id,
            input.SourceEvent,
            null!,
            RequiredCapabilityKey));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationProposalPolicy.CanPropose(null!, Proposal(input)));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationProposalPolicy.CanPropose(input.Character, null!));
    }

    [Fact]
    public void ExposesOnlyTheBoundedProposalAndPurePolicySurface()
    {
        Type proposalType = typeof(CharacterDialogueMisquotationProposal);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ActionKind", typeof(CharacterSabotageActionKind)),
            ("ActorId", typeof(CharacterId)),
            ("MisquotedText", typeof(string)),
            ("RequiredCapabilityKey", typeof(string)),
            ("SourceEvent", typeof(DialogueEvent)),
        ];
        System.Reflection.PropertyInfo[] properties = proposalType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedProperties,
            properties.Select(property => (property.Name, property.PropertyType)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(proposalType.GetMethods().Where(method =>
            method.DeclaringType == proposalType && !method.IsSpecialName));
        Assert.Empty(proposalType.GetEvents());
        Assert.True(proposalType.IsSealed);

        Type policyType = typeof(CharacterDialogueMisquotationProposalPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanPropose", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(Character), typeof(CharacterDialogueMisquotationProposal)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static CharacterDialogueMisquotationProposal Proposal(Input input) => new(
        input.Character.Identity.Id,
        input.SourceEvent,
        "The sage said the remedy should wait for royal approval.",
        RequiredCapabilityKey);

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId speakerId = new("sage");
        DialogueEvent sourceEvent = SourceEvent(speakerId, character.Identity.Id);
        return new(character, speakerId, sourceEvent.Audience, sourceEvent);
    }

    private static DialogueEvent SourceEvent(CharacterId speakerId, CharacterId listenerId) => new(
        new DialogueDelivery(
            new DialogueUtterance(speakerId, "Use the clean herb now."),
            DialogueDeliveryMode.OrdinarySpeech),
        new DialogueAudienceSnapshot(speakerId, [listenerId]));

    private sealed record Input(
        Character Character,
        CharacterId SourceSpeakerId,
        DialogueAudienceSnapshot SourceAudience,
        DialogueEvent SourceEvent);
}

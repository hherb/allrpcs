using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceExposureTransitionTests
{
    [Fact]
    public void ExposeAppendsExactSelectedEvidenceToFreshSnapshot()
    {
        TransitionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence retained = CreateEvidence(
            input.Actor,
            input.WitnessId,
            "The sage said to discard the remedy.");
        CharacterDialogueMisquotationEvidenceExposureSnapshot current = new([retained]);

        CharacterDialogueMisquotationEvidenceExposureSnapshot result =
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(
                current,
                input.Result);

        Assert.NotSame(current, result);
        Assert.Collection(
            result.Entries,
            evidence => Assert.Same(retained, evidence),
            evidence => Assert.Same(input.Evidence, evidence));
        Assert.Same(retained, Assert.Single(current.Entries));
        Assert.Same(input.Evidence.Occurrence.Proposal.SourceEvent, result.Entries[1].Occurrence.Proposal.SourceEvent);
        Assert.Same(input.Evidence.Occurrence.DeliveredEvent, result.Entries[1].Occurrence.DeliveredEvent);
    }

    [Fact]
    public void ExposeReturnsExactCurrentSnapshotForAbstention()
    {
        TransitionInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceExposureSnapshot current = new([]);
        CharacterDialogueMisquotationEvidenceConsiderationResult abstention = new(
            input.Request,
            new CharacterDialogueMisquotationEvidenceDecision(input.Actor.Identity.Id, null));

        CharacterDialogueMisquotationEvidenceExposureSnapshot result =
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(current, abstention);

        Assert.Same(current, result);
        Assert.Empty(current.Entries);
    }

    [Fact]
    public void ExposeRejectsAlreadyExposedExactDeliveredEvent()
    {
        TransitionInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceExposureSnapshot current = new([input.Evidence]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(current, input.Result));
        Assert.Same(input.Evidence, Assert.Single(current.Entries));
    }

    [Fact]
    public void ExposeRejectsOverflowWithoutChangingCurrentSnapshot()
    {
        TransitionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence[] retained = Enumerable.Range(
                0,
                CharacterDialogueMisquotationEvidenceExposureSnapshot.MaximumEvidenceCount)
            .Select(index => CreateEvidence(input.Actor, input.WitnessId, $"Retained {index}."))
            .ToArray();
        CharacterDialogueMisquotationEvidenceExposureSnapshot current = new(retained);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(current, input.Result));
        Assert.Equal(retained.Length, current.Entries.Count);
        Assert.All(retained, evidence => Assert.Contains(evidence, current.Entries));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureOperation()
    {
        TransitionInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceExposureSnapshot current = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(null!, input.Result));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceExposureTransition.Expose(current, null!));

        Type transitionType = typeof(CharacterDialogueMisquotationEvidenceExposureTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Expose", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterDialogueMisquotationEvidenceExposureSnapshot), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterDialogueMisquotationEvidenceExposureSnapshot),
                typeof(CharacterDialogueMisquotationEvidenceConsiderationResult),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static TransitionInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterDialogueMisquotationEvidence evidence = CreateEvidence(
            actor,
            witnessId,
            "The sage said the remedy should wait.");
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(actor, [evidence]);
        CharacterDialogueMisquotationEvidenceConsiderationResult result = new(
            request,
            new CharacterDialogueMisquotationEvidenceDecision(actor.Identity.Id, evidence));
        return new(actor, witnessId, evidence, request, result);
    }

    private static CharacterDialogueMisquotationEvidence CreateEvidence(
        Character actor,
        CharacterId witnessId,
        string misquotedText)
    {
        CharacterId sourceSpeakerId = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(sourceSpeakerId, [actor.Identity.Id, witnessId]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            misquotedText,
            "assess-local-influence");
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(actor.Identity.Id, misquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(actor.Identity.Id, [witnessId]));
        return new CharacterDialogueMisquotationEvidence(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
    }

    private sealed record TransitionInput(
        Character Actor,
        CharacterId WitnessId,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request,
        CharacterDialogueMisquotationEvidenceConsiderationResult Result);
}

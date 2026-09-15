using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceExposureSnapshotTests
{
    [Fact]
    public void CopiesExactEvidenceInStableOrder()
    {
        ExposureInput input = CreateInput();
        CharacterDialogueMisquotationEvidence second = CreateEvidence(
            input.Actor,
            input.WitnessId,
            "The sage said the remedy should be discarded.");
        CharacterDialogueMisquotationEvidence[] supplied = [input.Evidence, second];

        CharacterDialogueMisquotationEvidenceExposureSnapshot snapshot = new(supplied);

        supplied[0] = second;
        Assert.Collection(
            snapshot.Entries,
            evidence => Assert.Same(input.Evidence, evidence),
            evidence => Assert.Same(second, evidence));
    }

    [Fact]
    public void RejectsNullAndDuplicateExactDeliveredEvents()
    {
        ExposureInput input = CreateInput();
        CharacterDialogueMisquotationEvidence sameDelivery = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                input.Evidence.Occurrence.DeliveredEvent),
            input.WitnessId);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceExposureSnapshot(null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceExposureSnapshot([input.Evidence, null!]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceExposureSnapshot([input.Evidence, sameDelivery]));
    }

    [Fact]
    public void AllowsDistinctValueEqualDeliveredEvents()
    {
        ExposureInput input = CreateInput();
        DialogueEvent deliveredEvent = input.Evidence.Occurrence.DeliveredEvent;
        DialogueEvent replacement = new(deliveredEvent.Delivery, deliveredEvent.Audience);
        CharacterDialogueMisquotationEvidence valueEqual = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                replacement),
            input.WitnessId);

        CharacterDialogueMisquotationEvidenceExposureSnapshot snapshot = new(
            [input.Evidence, valueEqual]);

        Assert.Equal(deliveredEvent, replacement);
        Assert.NotSame(deliveredEvent, replacement);
        Assert.Same(input.Evidence, snapshot.Entries[0]);
        Assert.Same(valueEqual, snapshot.Entries[1]);
    }

    [Fact]
    public void RejectsMoreThanMaximumEvidenceCount()
    {
        ExposureInput input = CreateInput();
        CharacterDialogueMisquotationEvidence[] supplied = Enumerable.Range(
                0,
                CharacterDialogueMisquotationEvidenceExposureSnapshot.MaximumEvidenceCount + 1)
            .Select(index => CreateEvidence(input.Actor, input.WitnessId, $"Misquotation {index}."))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterDialogueMisquotationEvidenceExposureSnapshot(supplied));
    }

    [Fact]
    public void ExposesOnlyClosedImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterDialogueMisquotationEvidenceExposureSnapshot);
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());

        Assert.Equal(
            typeof(IEnumerable<CharacterDialogueMisquotationEvidence>),
            Assert.Single(constructor.GetParameters()).ParameterType);
        Assert.Equal("Entries", property.Name);
        Assert.Equal(typeof(IReadOnlyList<CharacterDialogueMisquotationEvidence>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);
    }

    private static ExposureInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        return new(actor, witnessId, CreateEvidence(actor, witnessId, "The sage said to wait."));
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

    private sealed record ExposureInput(
        Character Actor,
        CharacterId WitnessId,
        CharacterDialogueMisquotationEvidence Evidence);
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceViewSnapshotTests
{
    [Fact]
    public void RetainsExactWitnessAndCopiesEvidenceInStableOrder()
    {
        ViewInput input = CreateInput();
        CharacterDialogueMisquotationEvidence second = CreateEvidence(
            input.Actor,
            input.WitnessId,
            "The sage said the remedy should be discarded.");
        CharacterDialogueMisquotationEvidence[] supplied = [input.Evidence, second];

        CharacterDialogueMisquotationEvidenceViewSnapshot snapshot = new(
            input.WitnessId,
            supplied);

        supplied[0] = second;
        Assert.Same(input.WitnessId, snapshot.WitnessCharacterId);
        Assert.Collection(
            snapshot.Entries,
            evidence => Assert.Same(input.Evidence, evidence),
            evidence => Assert.Same(second, evidence));
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<IList<CharacterDialogueMisquotationEvidence>>(snapshot.Entries)
                .Add(input.Evidence));
    }

    [Fact]
    public void RejectsNullMismatchedWitnessAndDuplicateExactDeliveredEvents()
    {
        ViewInput input = CreateInput();
        CharacterId valueEqualWitnessId = new(input.WitnessId.Value);
        CharacterDialogueMisquotationEvidence valueEqualWitness = CreateEvidence(
            input.Actor,
            valueEqualWitnessId,
            "A value-equal witness heard this.");
        CharacterDialogueMisquotationEvidence otherWitness = CreateEvidence(
            input.Actor,
            new CharacterId("healer"),
            "Another witness heard this.");
        CharacterDialogueMisquotationEvidence sameDelivery = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                input.Evidence.Occurrence.DeliveredEvent),
            input.WitnessId);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(input.WitnessId, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(input.WitnessId, [null!]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(
                input.WitnessId,
                [valueEqualWitness]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(
                input.WitnessId,
                [otherWitness]));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(
                input.WitnessId,
                [input.Evidence, sameDelivery]));
    }

    [Fact]
    public void AllowsDistinctValueEqualDeliveredEvents()
    {
        ViewInput input = CreateInput();
        DialogueEvent deliveredEvent = input.Evidence.Occurrence.DeliveredEvent;
        DialogueEvent replacement = new(deliveredEvent.Delivery, deliveredEvent.Audience);
        CharacterDialogueMisquotationEvidence valueEqual = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                replacement),
            input.WitnessId);

        CharacterDialogueMisquotationEvidenceViewSnapshot snapshot = new(
            input.WitnessId,
            [input.Evidence, valueEqual]);

        Assert.Equal(deliveredEvent, replacement);
        Assert.NotSame(deliveredEvent, replacement);
        Assert.Same(input.Evidence, snapshot.Entries[0]);
        Assert.Same(valueEqual, snapshot.Entries[1]);
    }

    [Fact]
    public void RejectsMoreThanMaximumEvidenceCount()
    {
        ViewInput input = CreateInput();
        CharacterDialogueMisquotationEvidence[] supplied = Enumerable.Range(
                0,
                CharacterDialogueMisquotationEvidenceViewSnapshot.MaximumEvidenceCount + 1)
            .Select(index => CreateEvidence(input.Actor, input.WitnessId, $"Misquotation {index}."))
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterDialogueMisquotationEvidenceViewSnapshot(input.WitnessId, supplied));
    }

    [Fact]
    public void ExposesOnlyClosedImmutableViewSurface()
    {
        Type snapshotType = typeof(CharacterDialogueMisquotationEvidenceViewSnapshot);
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());
        System.Reflection.PropertyInfo[] properties = snapshotType.GetProperties();

        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(IEnumerable<CharacterDialogueMisquotationEvidence>),
                parameter.ParameterType));
        Assert.Collection(
            properties.OrderBy(property => property.Name),
            property =>
            {
                Assert.Equal("Entries", property.Name);
                Assert.Equal(
                    typeof(IReadOnlyList<CharacterDialogueMisquotationEvidence>),
                    property.PropertyType);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("WitnessCharacterId", property.Name);
                Assert.Equal(typeof(CharacterId), property.PropertyType);
                Assert.False(property.CanWrite);
            });
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);
    }

    internal static ViewInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        return new(actor, witnessId, CreateEvidence(actor, witnessId, "The sage said to wait."));
    }

    internal static CharacterDialogueMisquotationEvidence CreateEvidence(
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

    internal sealed record ViewInput(
        Character Actor,
        CharacterId WitnessId,
        CharacterDialogueMisquotationEvidence Evidence);
}

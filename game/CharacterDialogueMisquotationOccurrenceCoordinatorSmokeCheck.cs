using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one-shot exact misquotation occurrence through ordinary scene dialogue.</summary>
public partial class CharacterDialogueMisquotationOccurrenceCoordinatorSmokeCheck : Node
{
    private const float OrdinaryHearingRangePixels = 192.0f;
    private const float WhisperHearingRangePixels = 48.0f;
    private const string MisquotedText =
        "The sage said the remedy should wait for royal approval.";

    /// <inheritdoc />
    public override void _Ready()
    {
        DialogueEventController dialogue = GetNode<DialogueEventController>(
            "../DialogueEventController");
        CharacterDialogueMisquotationOccurrenceCoordinator coordinator =
            GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
                "../CharacterDialogueMisquotationOccurrenceCoordinator");
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterDialogueMisquotationProposal proposal = Proposal(actor, witnessId);
        DialogueDelivery delivery = Delivery(
            proposal.ActorId,
            proposal.MisquotedText,
            proposal.SourceEvent.Delivery.Mode);
        DialogueHearingCandidate[] heardWitness =
        [
            new(witnessId, new NumericsVector2(32.0f, 0.0f)),
        ];

        bool invalidHearingQuery = !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                -1.0f,
                WhisperHearingRangePixels,
                heardWitness)
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;
        bool wrongActor = !coordinator.TryOccur(
                proposal,
                Delivery(
                    new CharacterId(proposal.ActorId.Value),
                    proposal.MisquotedText,
                    proposal.SourceEvent.Delivery.Mode),
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness)
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;
        bool wrongText = !coordinator.TryOccur(
                proposal,
                Delivery(
                    proposal.ActorId,
                    "The sage said something else.",
                    proposal.SourceEvent.Delivery.Mode),
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness)
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;
        bool wrongMode = !coordinator.TryOccur(
                proposal,
                Delivery(
                    proposal.ActorId,
                    proposal.MisquotedText,
                    DialogueDeliveryMode.Whispering),
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness)
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;
        bool absentWitness = !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                [new DialogueHearingCandidate(witnessId, new NumericsVector2(193.0f, 0.0f))])
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;
        bool nullsRejected = ThrowsArgumentNull(() => coordinator.TryOccur(
                null!,
                delivery,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness))
            && ThrowsArgumentNull(() => coordinator.TryOccur(
                proposal,
                null!,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness))
            && ThrowsArgumentNull(() => coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                null!))
            && coordinator.LastOccurrence is null
            && dialogue.LastOccurredEvent is null;

        if (!invalidHearingQuery
            || !wrongActor
            || !wrongText
            || !wrongMode
            || !absentWitness
            || !nullsRejected
            || !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                new ChangingHearingCandidates(witnessId))
            || coordinator.LastOccurrence is not { } occurrence
            || !ReferenceEquals(occurrence.Proposal, proposal)
            || !ReferenceEquals(occurrence.DeliveredEvent, dialogue.LastOccurredEvent)
            || !ReferenceEquals(occurrence.DeliveredEvent.Delivery, delivery)
            || !ReferenceEquals(
                occurrence.DeliveredEvent.Audience.ListenerCharacterIds.Single(),
                witnessId))
        {
            Fail("Misquotation occurrence did not accept one exact valid ordinary delivery.");
            return;
        }

        CharacterDialogueMisquotationOccurrence retainedOccurrence = occurrence;
        DialogueEvent retainedEvent = occurrence.DeliveredEvent;
        bool repeatedAndLaterInvalidAreInert = !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                OrdinaryHearingRangePixels,
                WhisperHearingRangePixels,
                heardWitness)
            && !coordinator.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                -1.0f,
                WhisperHearingRangePixels,
                heardWitness)
            && ReferenceEquals(coordinator.LastOccurrence, retainedOccurrence)
            && ReferenceEquals(dialogue.LastOccurredEvent, retainedEvent);

        if (!repeatedAndLaterInvalidAreInert || !HasClosedSurface())
        {
            Fail("Repeated or invalid misquotation occurrence replaced accepted scene state.");
            return;
        }

        GD.Print(
            "Misquotation occurrence coordinator smoke passed: invalid hearing, actor, text, "
            + "mode, absent-witness, null, and repeated inputs failed closed; one explicit "
            + "retained hearing query made an ordinary delivery both the exact dialogue event "
            + "and retained occurrence.");
        GetTree().Quit();
    }

    private static CharacterDialogueMisquotationProposal Proposal(
        Character actor,
        CharacterId witnessId)
    {
        CharacterId sourceSpeakerId = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                sourceSpeakerId,
                [actor.Identity.Id, witnessId]));
        return new CharacterDialogueMisquotationProposal(
            actor.Identity.Id,
            sourceEvent,
            MisquotedText,
            "assess-local-influence");
    }

    private static DialogueDelivery Delivery(
        CharacterId actorId,
        string text,
        DialogueDeliveryMode mode) =>
        new(new DialogueUtterance(actorId, text), mode);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationOccurrenceCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methods.SequenceEqual(["TryOccur", "_Ready"])
            && properties.Length == 1
            && properties[0].Name == "LastOccurrence"
            && properties[0].PropertyType == typeof(CharacterDialogueMisquotationOccurrence)
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private sealed class ChangingHearingCandidates : IReadOnlyList<DialogueHearingCandidate>
    {
        private readonly CharacterId witnessId;
        private int enumerationCount;

        public ChangingHearingCandidates(CharacterId witnessId)
        {
            this.witnessId = witnessId;
        }

        public int Count => 1;

        public DialogueHearingCandidate this[int index] => index == 0
            ? Candidate()
            : throw new ArgumentOutOfRangeException(nameof(index));

        public IEnumerator<DialogueHearingCandidate> GetEnumerator()
        {
            yield return Candidate();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private DialogueHearingCandidate Candidate()
        {
            enumerationCount++;
            float horizontalPosition = enumerationCount == 1 ? 32.0f : 193.0f;
            return new DialogueHearingCandidate(
                witnessId,
                new NumericsVector2(horizontalPosition, 0.0f));
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}

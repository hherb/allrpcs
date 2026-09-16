using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks exact-current occurrence evidence candidate ownership.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterDialogueMisquotationOccurrenceCoordinator occurrences =
            GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
                "../CharacterDialogueMisquotationOccurrenceCoordinator");
        CharacterDialogueMisquotationEvidenceCandidateController candidates =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateController>(
                "../CharacterDialogueMisquotationEvidenceCandidateController");
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterId sourceOnlyWitnessId = new("healer");
        CharacterDialogueMisquotationProposal proposal = Proposal(
            actor,
            witnessId,
            sourceOnlyWitnessId);
        DialogueDelivery delivery = new(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            proposal.SourceEvent.Delivery.Mode);
        CharacterDialogueMisquotationOccurrence unownedOccurrence = new(
            proposal,
            new DialogueEvent(
                delivery,
                new DialogueAudienceSnapshot(proposal.ActorId, [witnessId])));

        bool unavailableOccurrenceRejected = !candidates.TryCreate(
                actor,
                unownedOccurrence,
                witnessId)
            && candidates.Candidate is null;
        bool occurred = occurrences.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                [new DialogueHearingCandidate(witnessId, new NumericsVector2(32.0f, 0.0f))]);
        CharacterDialogueMisquotationOccurrence? occurrence = occurrences.LastOccurrence;
        bool staleAndUnheardRejected = !candidates.TryCreate(
                actor,
                unownedOccurrence,
                witnessId)
            && occurrence is not null
            && !candidates.TryCreate(actor, occurrence, sourceOnlyWitnessId)
            && !candidates.TryCreate(actor, occurrence, new CharacterId("unknown-witness"))
            && candidates.Candidate is null;
        bool nullsRejected = ThrowsArgumentNull(() => candidates.TryCreate(
                null!,
                occurrence!,
                witnessId))
            && ThrowsArgumentNull(() => candidates.TryCreate(
                actor,
                null!,
                witnessId))
            && ThrowsArgumentNull(() => candidates.TryCreate(
                actor,
                occurrence!,
                null!))
            && candidates.Candidate is null;
        bool created = occurrence is not null
            && candidates.TryCreate(actor, occurrence, witnessId)
            && candidates.Candidate is { } candidate
            && ReferenceEquals(candidate.Occurrence, occurrence)
            && ReferenceEquals(candidate.WitnessCharacterId, witnessId)
            && CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, candidate);

        CharacterDialogueMisquotationEvidence retained = candidates.Candidate!;
        bool repeatedIsInert = !candidates.TryCreate(actor, occurrence!, witnessId)
            && !candidates.TryCreate(actor, occurrence!, sourceOnlyWitnessId)
            && ReferenceEquals(candidates.Candidate, retained)
            && ReferenceEquals(occurrences.LastOccurrence, occurrence)
            && ReferenceEquals(occurrence!.Proposal, proposal)
            && ReferenceEquals(occurrence.DeliveredEvent.Delivery, delivery);

        if (!unavailableOccurrenceRejected
            || !occurred
            || !staleAndUnheardRejected
            || !nullsRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Misquotation evidence candidate did not retain one exact current occurrence.");
            return;
        }

        GD.Print(
            "Misquotation evidence candidate controller smoke passed: unavailable, stale, "
            + "unheard, null, and repeated inputs stayed inert; one explicit witness retained "
            + "the exact current occurrence without exposure or mutation.");
        GetTree().Quit();
    }

    private static CharacterDialogueMisquotationProposal Proposal(
        Character actor,
        CharacterId witnessId,
        CharacterId sourceOnlyWitnessId)
    {
        CharacterId sourceSpeakerId = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                sourceSpeakerId,
                [actor.Identity.Id, witnessId, sourceOnlyWitnessId]));
        return new CharacterDialogueMisquotationProposal(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceCandidateController);
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

        return methods.SequenceEqual(["TryCreate", "_Ready"])
            && properties.Length == 1
            && properties[0].Name == "Candidate"
            && properties[0].PropertyType == typeof(CharacterDialogueMisquotationEvidence)
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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}

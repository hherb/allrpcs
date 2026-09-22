using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one misquotation-gated sabotage-action request from exact source state.</summary>
public partial class CharacterSabotageActionMisquotationRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterSabotageActionMisquotationRequestController requests =
            GetNode<CharacterSabotageActionMisquotationRequestController>(
                "../CharacterSabotageActionMisquotationRequestController");

        bool initiallyUnavailable = requests.Request is null;
        DialogueEvent retainedSourceEvent = source.SourceEvent;
        DialogueEvent retainedDeliveredEvent = source.DeliveredEvent;
        CharacterDialogueMisquotationOccurrence retainedOccurrence = source.Occurrence;
        CharacterId retainedWitness = source.WitnessCharacterId;
        CharacterDialogueMisquotationEvidence retainedCandidate = source.Candidate;
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        DialogueEvent replacementSourceEvent = new(
            source.SourceEvent.Delivery,
            source.SourceEvent.Audience);
        DialogueEvent replacementDeliveredEvent = new(
            source.DeliveredEvent.Delivery,
            source.DeliveredEvent.Audience);
        CharacterDialogueMisquotationOccurrence replacementOccurrence = new(
            source.Occurrence.Proposal,
            source.Occurrence.DeliveredEvent);
        CharacterId replacementWitness = new(source.WitnessCharacterId.Value);
        CharacterDialogueMisquotationEvidence replacementCandidate = new(
            source.Candidate.Occurrence,
            source.Candidate.WitnessCharacterId);
        bool replacementsRejected = !TryCreate(requests, source, actor: replacementActor)
            && !TryCreate(requests, source, sourceEvent: replacementSourceEvent)
            && !TryCreate(requests, source, deliveredEvent: replacementDeliveredEvent)
            && !TryCreate(requests, source, occurrence: replacementOccurrence)
            && !TryCreate(requests, source, witnessCharacterId: replacementWitness)
            && !TryCreate(requests, source, candidate: replacementCandidate)
            && requests.Request is null;
        bool nullsRejected = RejectsNulls(requests, source);
        bool created = TryCreate(requests, source)
            && requests.Request is { } request
            && ReferenceEquals(request.Actor, source.Actor)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Misquote);

        CharacterSabotageActionConsiderationRequest retained = requests.Request!;
        bool repeatedIsInert = !TryCreate(requests, source)
            && ReferenceEquals(requests.Request, retained)
            && ReferenceEquals(source.SourceEvent, retainedSourceEvent)
            && ReferenceEquals(source.DeliveredEvent, retainedDeliveredEvent)
            && ReferenceEquals(source.Occurrence, retainedOccurrence)
            && ReferenceEquals(source.WitnessCharacterId, retainedWitness)
            && ReferenceEquals(source.Candidate, retainedCandidate)
            && ReferenceEquals(source.SourceEvent, source.Occurrence.Proposal.SourceEvent)
            && ReferenceEquals(source.DeliveredEvent, source.Occurrence.DeliveredEvent)
            && ReferenceEquals(source.WitnessCharacterId, source.Candidate.WitnessCharacterId)
            && CharacterDialogueMisquotationEvidencePolicy.CanObserve(
                source.Actor,
                source.Candidate);

        if (!initiallyUnavailable
            || !replacementsRejected
            || !nullsRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Misquotation-gated sabotage-action request did not preserve exact source state.");
            return;
        }

        GD.Print(
            "Sabotage-action misquotation request smoke passed: unavailable, replacement, null, "
            + "and repeated inputs stayed inert; one request retained the exact enemy actor and "
            + "canonical misquote action without delivering dialogue or starting work.");
        GetTree().Quit();
    }

    private static bool TryCreate(
        CharacterSabotageActionMisquotationRequestController requests,
        TemporaryCharacterDialogueMisquotationEvidenceSource source,
        Character? actor = null,
        DialogueEvent? sourceEvent = null,
        DialogueEvent? deliveredEvent = null,
        CharacterDialogueMisquotationOccurrence? occurrence = null,
        CharacterId? witnessCharacterId = null,
        CharacterDialogueMisquotationEvidence? candidate = null) =>
        requests.TryCreate(
            actor ?? source.Actor,
            sourceEvent ?? source.SourceEvent,
            deliveredEvent ?? source.DeliveredEvent,
            occurrence ?? source.Occurrence,
            witnessCharacterId ?? source.WitnessCharacterId,
            candidate ?? source.Candidate);

    private static bool RejectsNulls(
        CharacterSabotageActionMisquotationRequestController requests,
        TemporaryCharacterDialogueMisquotationEvidenceSource source) =>
        ThrowsArgumentNull(() => requests.TryCreate(
            null!, source.SourceEvent, source.DeliveredEvent, source.Occurrence,
            source.WitnessCharacterId, source.Candidate))
        && ThrowsArgumentNull(() => requests.TryCreate(
            source.Actor, null!, source.DeliveredEvent, source.Occurrence,
            source.WitnessCharacterId, source.Candidate))
        && ThrowsArgumentNull(() => requests.TryCreate(
            source.Actor, source.SourceEvent, null!, source.Occurrence,
            source.WitnessCharacterId, source.Candidate))
        && ThrowsArgumentNull(() => requests.TryCreate(
            source.Actor, source.SourceEvent, source.DeliveredEvent, null!,
            source.WitnessCharacterId, source.Candidate))
        && ThrowsArgumentNull(() => requests.TryCreate(
            source.Actor, source.SourceEvent, source.DeliveredEvent, source.Occurrence,
            null!, source.Candidate))
        && ThrowsArgumentNull(() => requests.TryCreate(
            source.Actor, source.SourceEvent, source.DeliveredEvent, source.Occurrence,
            source.WitnessCharacterId, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionMisquotationRequestController);
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
            && properties[0].Name == "Request"
            && properties[0].PropertyType
                == typeof(CharacterSabotageActionConsiderationRequest)
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

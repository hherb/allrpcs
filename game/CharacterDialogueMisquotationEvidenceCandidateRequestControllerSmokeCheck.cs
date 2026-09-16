using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one consideration request for the exact current evidence candidate.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateRequestControllerSmokeCheck
    : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationOccurrenceCoordinator occurrences =
            GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
                "../CharacterDialogueMisquotationOccurrenceCoordinator");
        CharacterDialogueMisquotationEvidenceCandidateController candidates =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateController>(
                "../CharacterDialogueMisquotationEvidenceCandidateController");
        CharacterDialogueMisquotationEvidenceCandidateRequestController requests =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateRequestController>(
                "../CharacterDialogueMisquotationEvidenceCandidateRequestController");

        CharacterDialogueMisquotationProposal proposal = source.Occurrence.Proposal;
        DialogueDelivery delivery = new(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            proposal.SourceEvent.Delivery.Mode);
        bool unavailableRejected = !requests.TryCreate(source.Actor, source.Candidate)
            && requests.Request is null;
        bool occurrenceAccepted = occurrences.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                [
                    new DialogueHearingCandidate(
                        source.WitnessCharacterId,
                        new NumericsVector2(32.0f, 0.0f)),
                ]);
        CharacterDialogueMisquotationOccurrence? occurrence = occurrences.LastOccurrence;
        bool occurred = occurrenceAccepted && occurrence is not null;
        bool candidateCreated = candidates.TryCreate(
                source.Actor,
                occurrences.LastOccurrence!,
                source.WitnessCharacterId)
            && candidates.Candidate is not null
            && ReferenceEquals(candidates.Candidate.Occurrence, occurrence);
        CharacterDialogueMisquotationEvidence? candidate = candidates.Candidate;
        bool staleAndNullRejected = !requests.TryCreate(source.Actor, source.Candidate)
            && ThrowsArgumentNull(() => requests.TryCreate(null!, candidate!))
            && ThrowsArgumentNull(() => requests.TryCreate(source.Actor, null!))
            && requests.Request is null;
        bool created = requests.TryCreate(source.Actor, candidate!)
            && requests.Request is { } request
            && ReferenceEquals(request.Actor, source.Actor)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates.Single(), candidate);

        CharacterDialogueMisquotationEvidenceConsiderationRequest retained = requests.Request!;
        bool repeatedIsInert = !requests.TryCreate(source.Actor, candidate!)
            && !requests.TryCreate(source.Actor, source.Candidate)
            && ReferenceEquals(requests.Request, retained)
            && ReferenceEquals(candidates.Candidate, candidate)
            && ReferenceEquals(occurrences.LastOccurrence, occurrence);

        if (!unavailableRejected
            || !occurred
            || !candidateCreated
            || !staleAndNullRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Misquotation evidence request did not retain the exact current candidate.");
            return;
        }

        GD.Print(
            "Misquotation evidence candidate request smoke passed: unavailable, stale, null, "
            + "and repeated inputs stayed inert; one request retained the exact actor and "
            + "current candidate without starting or exposing evidence.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceCandidateRequestController);
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
                == typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest)
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

using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns one explicit misquotation occurrence through ordinary scene dialogue.</summary>
public partial class CharacterDialogueMisquotationOccurrenceCoordinator : Node
{
    private DialogueEventController dialogue = null!;

    /// <summary>Gets the exact one-shot occurrence accepted by this coordinator.</summary>
    public CharacterDialogueMisquotationOccurrence? LastOccurrence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        dialogue = GetNode<DialogueEventController>("../DialogueEventController");
    }

    /// <summary>Occurs one exact eligible delivery through the caller-supplied hearing query.</summary>
    public bool TryOccur(
        CharacterDialogueMisquotationProposal proposal,
        DialogueDelivery delivery,
        NumericsVector2 speakerPosition,
        float ordinaryHearingRangePixels,
        float whisperHearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(candidates);

        if (LastOccurrence is not null
            || !ReferenceEquals(delivery.Utterance.SpeakerCharacterId, proposal.ActorId)
            || !StringComparer.Ordinal.Equals(delivery.Utterance.Text, proposal.MisquotedText)
            || !ReferenceEquals(delivery.Mode, proposal.SourceEvent.Delivery.Mode))
        {
            return false;
        }

        DialogueHearingCandidate[] retainedCandidates;
        DialogueAudienceSnapshot expectedAudience;
        try
        {
            retainedCandidates = candidates.ToArray();
            expectedAudience = DialogueDeliveryHearingSelector.Select(
                delivery,
                speakerPosition,
                ordinaryHearingRangePixels,
                whisperHearingRangePixels,
                retainedCandidates);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (expectedAudience.ListenerCharacterIds.Count == 0
            || !dialogue.TryOccur(
                delivery,
                speakerPosition,
                ordinaryHearingRangePixels,
                whisperHearingRangePixels,
                retainedCandidates)
            || dialogue.LastOccurredEvent is not { } occurred
            || !ReferenceEquals(occurred.Delivery, delivery)
            || !ReferenceEquals(occurred.Utterance.SpeakerCharacterId, proposal.ActorId)
            || !StringComparer.Ordinal.Equals(occurred.Utterance.Text, proposal.MisquotedText)
            || !ReferenceEquals(occurred.Delivery.Mode, proposal.SourceEvent.Delivery.Mode)
            || !HasExactAudience(occurred.Audience, expectedAudience))
        {
            return false;
        }

        LastOccurrence = new CharacterDialogueMisquotationOccurrence(proposal, occurred);
        return true;
    }

    private static bool HasExactAudience(
        DialogueAudienceSnapshot actual,
        DialogueAudienceSnapshot expected) =>
        ReferenceEquals(actual.SpeakerCharacterId, expected.SpeakerCharacterId)
        && actual.ListenerCharacterIds.Count == expected.ListenerCharacterIds.Count
        && actual.ListenerCharacterIds
            .Zip(expected.ListenerCharacterIds)
            .All(pair => ReferenceEquals(pair.First, pair.Second));
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>Resolves the exact bounded LLM participant for one awaiting character turn.</summary>
public static class CharacterTurnParticipantLookup
{
    /// <summary>Preflights the complete roster, then returns its exact value-matching participant.</summary>
    public static Character? Find(
        IEnumerable<Character> participants,
        CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentNullException.ThrowIfNull(activity);

        List<Character> retained = [];
        HashSet<CharacterId> identifiers = [];
        foreach (Character participant in participants)
        {
            if (participant is null)
            {
                throw new ArgumentException(
                    "Turn participants cannot contain null values.",
                    nameof(participants));
            }

            CharacterId participantId = participant.Identity.Id;
            CharacterControllerAssignment assignment = participant.ControllerAssignment;
            if (!identifiers.Add(participantId))
            {
                throw new ArgumentException(
                    "Turn participants must have distinct identifiers.",
                    nameof(participants));
            }

            if (!ReferenceEquals(assignment.CharacterId, participantId)
                || !ReferenceEquals(assignment.ControllerType, ControllerType.Llm))
            {
                throw new ArgumentException(
                    "Every turn participant must be exactly assigned to LLM control.",
                    nameof(participants));
            }

            retained.Add(participant);
            if (retained.Count > DialogueAudienceSnapshot.MaximumListenerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(participants),
                    retained.Count,
                    $"Turn participant lookup accepts at most "
                        + $"{DialogueAudienceSnapshot.MaximumListenerCount} characters.");
            }
        }

        if (!ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse))
        {
            return null;
        }

        CharacterId respondingCharacterId = activity.Request.RespondingCharacterId;
        return retained.SingleOrDefault(participant =>
            participant.Identity.Id.Equals(respondingCharacterId));
    }
}

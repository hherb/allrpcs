using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one complete bounded round of exact participation-consideration requests and results.
/// </summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterTurnParticipationConsiderationRoundResult
{
    /// <summary>
    /// Creates an immutable round after every exact request and accepted result is preflighted.
    /// </summary>
    public CharacterTurnParticipationConsiderationRoundResult(
        IEnumerable<CharacterTurnParticipationConsiderationRequest> requests,
        IEnumerable<CharacterTurnParticipationConsiderationResult> results)
    {
        ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(results);

        List<CharacterTurnParticipationConsiderationRequest> retainedRequests = requests.ToList();
        List<CharacterTurnParticipationConsiderationResult> retainedResults = results.ToList();

        if (retainedRequests.Count > DialogueAudienceSnapshot.MaximumListenerCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requests),
                retainedRequests.Count,
                $"Participation rounds accept at most "
                    + $"{DialogueAudienceSnapshot.MaximumListenerCount} requests.");
        }

        if (retainedRequests.Count != retainedResults.Count)
        {
            throw new ArgumentException(
                "Every consideration request requires one positionally aligned result.",
                nameof(results));
        }

        DialogueEvent? sharedEvent = null;
        HashSet<CharacterId> scheduledCharacterIds = new();
        for (int index = 0; index < retainedRequests.Count; index++)
        {
            CharacterTurnParticipationConsiderationRequest request = retainedRequests[index]
                ?? throw new ArgumentException(
                    "Participation requests cannot contain null values.",
                    nameof(requests));
            CharacterTurnParticipationConsiderationResult result = retainedResults[index]
                ?? throw new ArgumentException(
                    "Participation results cannot contain null values.",
                    nameof(results));

            if (!ReferenceEquals(request, result.Request))
            {
                throw new ArgumentException(
                    "Every result must retain the exact positionally aligned request.",
                    nameof(results));
            }

            if (!scheduledCharacterIds.Add(request.ScheduledCharacter.Identity.Id))
            {
                throw new ArgumentException(
                    "Participation requests must have distinct scheduled character identifiers.",
                    nameof(requests));
            }

            sharedEvent ??= request.HeardDialogueEvent;
            if (!ReferenceEquals(sharedEvent, request.HeardDialogueEvent))
            {
                throw new ArgumentException(
                    "Every participation request must retain the same exact heard dialogue event.",
                    nameof(requests));
            }
        }

        Requests = retainedRequests.AsReadOnly();
        Results = retainedResults.AsReadOnly();
    }

    /// <summary>Gets the copied read-only sequence of exact consideration requests.</summary>
    public IReadOnlyList<CharacterTurnParticipationConsiderationRequest> Requests { get; }

    /// <summary>Gets the copied read-only sequence of exact accepted results.</summary>
    public IReadOnlyList<CharacterTurnParticipationConsiderationResult> Results { get; }
}

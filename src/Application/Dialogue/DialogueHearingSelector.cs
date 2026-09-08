using System.Numerics;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Describes one character position considered by a dialogue hearing query.
/// </summary>
/// <param name="CharacterId">The exact stable identifier of the candidate character.</param>
/// <param name="Position">The candidate's finite current world position.</param>
public readonly record struct DialogueHearingCandidate(
    CharacterId CharacterId,
    Vector2 Position);

/// <summary>
/// Selects every character inside a speaker's inclusive hearing range without a Godot dependency.
/// </summary>
public static class DialogueHearingSelector
{
    /// <summary>
    /// Gets the maximum number of candidates accepted by one bounded hearing query.
    /// </summary>
    public const int MaximumCandidateCount = DialogueAudienceSnapshot.MaximumListenerCount;

    /// <summary>
    /// Selects non-speaker listener identifiers in stable candidate order.
    /// </summary>
    public static DialogueAudienceSnapshot Select(
        CharacterId speakerCharacterId,
        Vector2 speakerPosition,
        float hearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(speakerCharacterId);
        ArgumentNullException.ThrowIfNull(candidates);
        ValidateFinitePosition(speakerPosition, nameof(speakerPosition));
        ValidateHearingRange(hearingRangePixels);

        if (candidates.Count > MaximumCandidateCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidates),
                candidates.Count,
                $"Dialogue hearing accepts at most {MaximumCandidateCount} candidates.");
        }

        HashSet<CharacterId> candidateIdentifiers = new();
        List<CharacterId> listenerIdentifiers = new();
        double maximumDistanceSquared = (double)hearingRangePixels * hearingRangePixels;

        foreach (DialogueHearingCandidate candidate in candidates)
        {
            if (candidate.CharacterId is null)
            {
                throw new ArgumentException(
                    "Hearing candidates cannot contain null character identifiers.",
                    nameof(candidates));
            }

            ValidateFinitePosition(candidate.Position, nameof(candidates));

            if (!candidateIdentifiers.Add(candidate.CharacterId))
            {
                throw new ArgumentException(
                    "Hearing candidates cannot contain duplicate character identifiers.",
                    nameof(candidates));
            }

            if (candidate.CharacterId == speakerCharacterId)
            {
                continue;
            }

            double horizontalDistance = (double)candidate.Position.X - speakerPosition.X;
            double verticalDistance = (double)candidate.Position.Y - speakerPosition.Y;
            double distanceSquared = (horizontalDistance * horizontalDistance)
                + (verticalDistance * verticalDistance);

            if (distanceSquared <= maximumDistanceSquared)
            {
                listenerIdentifiers.Add(candidate.CharacterId);
            }
        }

        return new DialogueAudienceSnapshot(speakerCharacterId, listenerIdentifiers);
    }

    private static void ValidateFinitePosition(Vector2 position, string parameterName)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                position,
                "Dialogue hearing positions must be finite.");
        }
    }

    private static void ValidateHearingRange(float hearingRangePixels)
    {
        if (!float.IsFinite(hearingRangePixels) || hearingRangePixels <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hearingRangePixels),
                hearingRangePixels,
                "Dialogue hearing range must be finite and positive.");
        }
    }
}

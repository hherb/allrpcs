using System.Numerics;

namespace Allrpcs.Application.Interaction;

/// <summary>
/// Describes one temporary presentation candidate considered for interaction focus.
/// </summary>
/// <param name="Position">The finite world position used for proximity selection.</param>
/// <param name="IsVisible">Whether the candidate is currently visible to the player.</param>
/// <param name="IsInteractionEnabled">Whether the candidate may currently receive focus.</param>
public readonly record struct InteractionFocusCandidate(
    Vector2 Position,
    bool IsVisible,
    bool IsInteractionEnabled);

/// <summary>
/// Identifies the selected entry in the caller's bounded candidate list.
/// </summary>
/// <param name="CandidateIndex">The zero-based candidate-list index.</param>
/// <param name="DistancePixels">The finite distance from the Seeker to the candidate.</param>
public readonly record struct InteractionFocusSelection(
    int CandidateIndex,
    float DistancePixels);

/// <summary>
/// Selects the nearest visible, enabled interaction candidate without a Godot dependency.
/// </summary>
public static class InteractionFocusSelector
{
    /// <summary>The maximum candidates accepted by one greybox focus query.</summary>
    public const int MaximumCandidateCount = 32;

    /// <summary>
    /// Selects the nearest eligible candidate inside an inclusive interaction range.
    /// </summary>
    /// <param name="seekerPosition">The finite world position from which range is measured.</param>
    /// <param name="interactionRangePixels">The finite, positive inclusive selection range.</param>
    /// <param name="candidates">
    /// The authored-order candidate list. Authored order resolves exact distance ties.
    /// </param>
    /// <returns>The selected candidate, or <see langword="null"/> when none is eligible.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="candidates"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when positions or range are non-finite, the range is not positive, or the candidate
    /// list exceeds <see cref="MaximumCandidateCount"/>.
    /// </exception>
    public static InteractionFocusSelection? Select(
        Vector2 seekerPosition,
        float interactionRangePixels,
        IReadOnlyList<InteractionFocusCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ValidateFinitePosition(seekerPosition, nameof(seekerPosition));
        ValidateInteractionRange(interactionRangePixels);
        ValidateCandidateCount(candidates);

        double maximumDistanceSquared = (double)interactionRangePixels * interactionRangePixels;
        int selectedIndex = -1;
        double selectedDistanceSquared = double.PositiveInfinity;

        for (int index = 0; index < candidates.Count; index++)
        {
            InteractionFocusCandidate candidate = candidates[index];
            ValidateFinitePosition(candidate.Position, nameof(candidates));

            if (!candidate.IsVisible || !candidate.IsInteractionEnabled)
            {
                continue;
            }

            double distanceSquared = CalculateDistanceSquared(seekerPosition, candidate.Position);
            if (distanceSquared <= maximumDistanceSquared
                && distanceSquared < selectedDistanceSquared)
            {
                selectedIndex = index;
                selectedDistanceSquared = distanceSquared;
            }
        }

        return selectedIndex < 0
            ? null
            : new InteractionFocusSelection(
                selectedIndex,
                (float)Math.Sqrt(selectedDistanceSquared));
    }

    private static double CalculateDistanceSquared(Vector2 origin, Vector2 destination)
    {
        double deltaX = (double)destination.X - origin.X;
        double deltaY = (double)destination.Y - origin.Y;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    private static void ValidateFinitePosition(Vector2 position, string parameterName)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                position,
                "Interaction focus positions must be finite.");
        }
    }

    private static void ValidateInteractionRange(float interactionRangePixels)
    {
        if (!float.IsFinite(interactionRangePixels) || interactionRangePixels <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interactionRangePixels),
                interactionRangePixels,
                "Interaction range must be finite and positive.");
        }
    }

    private static void ValidateCandidateCount(
        IReadOnlyList<InteractionFocusCandidate> candidates)
    {
        if (candidates.Count > MaximumCandidateCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(candidates),
                candidates.Count,
                $"Interaction focus accepts at most {MaximumCandidateCount} candidates.");
        }
    }
}

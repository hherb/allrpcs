namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Advances immutable character-commitment progress by one eligible observed intention.
/// </summary>
public static class CharacterCommitmentProgressionTransition
{
    /// <summary>Returns new progress advanced by exactly one pending intention.</summary>
    public static CharacterCommitmentProgress Advance(
        CharacterCommitmentProgress progress,
        CharacterIntention observedIntention)
    {
        if (!CharacterCommitmentProgressionPolicy.CanAdvance(progress, observedIntention))
        {
            throw new InvalidOperationException(
                "Only the single pending commitment intention may advance progress.");
        }

        return new CharacterCommitmentProgress(
            progress.Commitment,
            progress.NextIntentionIndex + 1);
    }
}

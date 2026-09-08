namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Decides whether one observed intention is the single next step in a character commitment.
/// </summary>
public static class CharacterCommitmentProgressionPolicy
{
    /// <summary>Returns whether the value-equal observed intention may advance the progress.</summary>
    public static bool CanAdvance(
        CharacterCommitmentProgress progress,
        CharacterIntention observedIntention)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(observedIntention);

        return progress.NextIntention is not null
            && progress.NextIntention == observedIntention;
    }
}

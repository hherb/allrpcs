namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains immutable progress through one character commitment without observing or executing it.
/// </summary>
public sealed record CharacterCommitmentProgress
{
    /// <summary>Creates progress positioned at one valid next-intention index.</summary>
    public CharacterCommitmentProgress(
        CharacterCommitment commitment,
        int nextIntentionIndex)
    {
        ArgumentNullException.ThrowIfNull(commitment);

        if (nextIntentionIndex < 0 || nextIntentionIndex > commitment.ProposedIntentions.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextIntentionIndex),
                nextIntentionIndex,
                "The next-intention index must be inside the commitment or at its end.");
        }

        Commitment = commitment;
        NextIntentionIndex = nextIntentionIndex;
    }

    /// <summary>Gets the exact commitment whose progress is represented.</summary>
    public CharacterCommitment Commitment { get; }

    /// <summary>Gets the zero-based index of the next intention to observe.</summary>
    public int NextIntentionIndex { get; }

    /// <summary>Gets whether every proposed intention has been observed.</summary>
    public bool IsComplete => NextIntentionIndex == Commitment.ProposedIntentions.Count;

    /// <summary>Gets the exact next intention, or <see langword="null"/> after completion.</summary>
    public CharacterIntention? NextIntention =>
        IsComplete ? null : Commitment.ProposedIntentions[NextIntentionIndex];
}

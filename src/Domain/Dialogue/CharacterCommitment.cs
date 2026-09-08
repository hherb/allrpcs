namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Declares an ordered non-empty sequence of proposed intentions without advancing or executing it.
/// </summary>
public sealed class CharacterCommitment
{
    /// <summary>Creates an immutable declaration from proposed intentions in authored order.</summary>
    public CharacterCommitment(IEnumerable<CharacterIntention> proposedIntentions)
    {
        ArgumentNullException.ThrowIfNull(proposedIntentions);

        List<CharacterIntention> intentions = new();
        foreach (CharacterIntention intention in proposedIntentions)
        {
            if (intention is null)
            {
                throw new ArgumentException(
                    "Proposed intentions cannot contain null values.",
                    nameof(proposedIntentions));
            }

            intentions.Add(intention);
        }

        if (intentions.Count == 0)
        {
            throw new ArgumentException(
                "A commitment must declare at least one proposed intention.",
                nameof(proposedIntentions));
        }

        ProposedIntentions = intentions.AsReadOnly();
    }

    /// <summary>Gets the exact proposed intentions in declared order.</summary>
    public IReadOnlyList<CharacterIntention> ProposedIntentions { get; }
}

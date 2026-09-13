namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains the immutable provider-neutral core of one proposed character-turn response.
/// </summary>
public sealed record CharacterTurnResponse
{
    /// <summary>Gets the maximum number of state proposals retained by one response.</summary>
    public const int MaximumStateProposalCount = 8;

    /// <summary>
    /// Creates a response core with an optional spoken delivery and one immediate intention.
    /// </summary>
    public CharacterTurnResponse(
        CharacterTurnRequest request,
        CharacterTurnDisposition disposition,
        DialogueDelivery? delivery,
        CharacterIntention immediateIntention,
        IEnumerable<CharacterCommitment> commitments,
        IEnumerable<CharacterStateProposal> stateProposals)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(disposition);
        ArgumentNullException.ThrowIfNull(immediateIntention);
        ArgumentNullException.ThrowIfNull(commitments);
        ArgumentNullException.ThrowIfNull(stateProposals);

        if (delivery is not null
            && delivery.Utterance.SpeakerCharacterId != request.RespondingCharacterId)
        {
            throw new ArgumentException(
                "Response speech must come from the responding character.",
                nameof(delivery));
        }

        List<CharacterCommitment> retainedCommitments = new();
        foreach (CharacterCommitment commitment in commitments)
        {
            if (commitment is null)
            {
                throw new ArgumentException(
                    "Commitments cannot contain null values.",
                    nameof(commitments));
            }

            retainedCommitments.Add(commitment);
        }

        List<CharacterStateProposal> retainedStateProposals = new();
        foreach (CharacterStateProposal stateProposal in stateProposals)
        {
            if (stateProposal is null)
            {
                throw new ArgumentException(
                    "State proposals cannot contain null values.",
                    nameof(stateProposals));
            }

            retainedStateProposals.Add(stateProposal);
            if (retainedStateProposals.Count > MaximumStateProposalCount)
            {
                throw new ArgumentException(
                    $"A response cannot contain more than {MaximumStateProposalCount} state proposals.",
                    nameof(stateProposals));
            }
        }

        Request = request;
        Disposition = disposition;
        Delivery = delivery;
        ImmediateIntention = immediateIntention;
        Commitments = retainedCommitments.AsReadOnly();
        StateProposals = retainedStateProposals.AsReadOnly();
    }

    /// <summary>Gets the exact request being answered.</summary>
    public CharacterTurnRequest Request { get; }

    /// <summary>Gets the exact declared response disposition.</summary>
    public CharacterTurnDisposition Disposition { get; }

    /// <summary>Gets the exact proposed spoken delivery, or <see langword="null"/>.</summary>
    public DialogueDelivery? Delivery { get; }

    /// <summary>Gets the exact delivered utterance, or <see langword="null"/> for no speech.</summary>
    public DialogueUtterance? Speech => Delivery?.Utterance;

    /// <summary>Gets the exact one proposed immediate intention.</summary>
    public CharacterIntention ImmediateIntention { get; }

    /// <summary>Gets exact declared commitments in stable response order.</summary>
    public IReadOnlyList<CharacterCommitment> Commitments { get; }

    /// <summary>Gets exact bounded state proposals in stable response order.</summary>
    public IReadOnlyList<CharacterStateProposal> StateProposals { get; }
}

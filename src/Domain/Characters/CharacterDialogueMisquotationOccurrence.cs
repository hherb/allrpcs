using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact misquotation proposal to one exact in-world dialogue event.</summary>
public sealed class CharacterDialogueMisquotationOccurrence
{
    /// <summary>Creates one immutable occurrence candidate without delivering new dialogue.</summary>
    public CharacterDialogueMisquotationOccurrence(
        CharacterDialogueMisquotationProposal proposal,
        DialogueEvent deliveredEvent)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(deliveredEvent);

        Proposal = proposal;
        DeliveredEvent = deliveredEvent;
    }

    /// <summary>Gets the exact capability- and hearing-bound proposal.</summary>
    public CharacterDialogueMisquotationProposal Proposal { get; }

    /// <summary>Gets the exact already-composed in-world delivery candidate.</summary>
    public DialogueEvent DeliveredEvent { get; }
}

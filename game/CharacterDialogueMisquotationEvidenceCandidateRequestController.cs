using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one consideration request for the exact current evidence candidate.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateRequestController : Node
{
    private CharacterDialogueMisquotationEvidenceCandidateController candidates = null!;

    /// <summary>Gets the exact one-shot consideration request accepted by this controller.</summary>
    public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidates = GetNode<CharacterDialogueMisquotationEvidenceCandidateController>(
            "../CharacterDialogueMisquotationEvidenceCandidateController");
    }

    /// <summary>Creates one request for the exact current observable candidate.</summary>
    public bool TryCreate(
        Character actor,
        CharacterDialogueMisquotationEvidence expectedCandidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(expectedCandidate);

        if (Request is not null
            || !ReferenceEquals(candidates.Candidate, expectedCandidate)
            || !CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, expectedCandidate))
        {
            return false;
        }

        Request = new CharacterDialogueMisquotationEvidenceConsiderationRequest(
            actor,
            [expectedCandidate]);
        return true;
    }
}

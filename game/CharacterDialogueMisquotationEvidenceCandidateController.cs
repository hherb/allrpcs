using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one potential witness-evidence candidate for the exact current occurrence.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateController : Node
{
    private CharacterDialogueMisquotationOccurrenceCoordinator occurrences = null!;

    /// <summary>Gets the exact one-shot candidate accepted by this controller.</summary>
    public CharacterDialogueMisquotationEvidence? Candidate { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        occurrences = GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
            "../CharacterDialogueMisquotationOccurrenceCoordinator");
    }

    /// <summary>Creates one candidate for an explicit witness to the exact current occurrence.</summary>
    public bool TryCreate(
        Character actor,
        CharacterDialogueMisquotationOccurrence expectedOccurrence,
        CharacterId witnessCharacterId)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(expectedOccurrence);
        ArgumentNullException.ThrowIfNull(witnessCharacterId);

        if (Candidate is not null
            || !ReferenceEquals(occurrences.LastOccurrence, expectedOccurrence))
        {
            return false;
        }

        CharacterDialogueMisquotationEvidence candidate = new(
            expectedOccurrence,
            witnessCharacterId);
        if (!CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, candidate))
        {
            return false;
        }

        Candidate = candidate;
        return true;
    }
}

using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact witness-specific view of scene-exposed misquotation evidence.</summary>
public partial class CharacterDialogueMisquotationEvidenceViewController : Node
{
    /// <summary>Gets the exact exposure snapshot from which the current view was projected.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot? SourceExposure { get; private set; }

    /// <summary>Gets the initialized exact witness-specific view, if any.</summary>
    public CharacterDialogueMisquotationEvidenceViewSnapshot? View { get; private set; }

    /// <summary>Initializes one exact witness view from one exact exposure snapshot.</summary>
    public bool TryInitialize(
        CharacterDialogueMisquotationEvidenceExposureSnapshot exposure,
        CharacterId witnessCharacterId)
    {
        ArgumentNullException.ThrowIfNull(exposure);
        ArgumentNullException.ThrowIfNull(witnessCharacterId);

        if (View is not null)
        {
            return false;
        }

        SourceExposure = exposure;
        View = CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(
            exposure,
            witnessCharacterId);
        return true;
    }

    /// <summary>Refreshes the view only from its exact expected source exposure.</summary>
    public bool TryRefresh(
        CharacterDialogueMisquotationEvidenceExposureSnapshot expectedExposure,
        CharacterDialogueMisquotationEvidenceExposureSnapshot currentExposure)
    {
        ArgumentNullException.ThrowIfNull(expectedExposure);
        ArgumentNullException.ThrowIfNull(currentExposure);

        if (View is null
            || !ReferenceEquals(SourceExposure, expectedExposure)
            || ReferenceEquals(expectedExposure, currentExposure))
        {
            return false;
        }

        CharacterDialogueMisquotationEvidenceViewSnapshot currentView =
            CharacterDialogueMisquotationEvidenceViewProjection.ForWitness(
                currentExposure,
                View.WitnessCharacterId);
        SourceExposure = currentExposure;
        View = currentView;
        return true;
    }
}

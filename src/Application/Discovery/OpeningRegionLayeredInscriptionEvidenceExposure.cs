using Allrpcs.Domain.Discovery;

namespace Allrpcs.Application.Discovery;

/// <summary>Retains the exact filtered evidence exposed by one inscription presentation.</summary>
public sealed class OpeningRegionLayeredInscriptionEvidenceExposure
{
    internal OpeningRegionLayeredInscriptionEvidenceExposure(
        OpeningRegionLayeredInscriptionPresentation presentation,
        DiscoveryEvidence evidence,
        CharacterEvidenceViewSnapshot evidenceView)
    {
        Presentation = presentation;
        Evidence = evidence;
        EvidenceView = evidenceView;
    }

    /// <summary>Gets the exact presentation that authorized exposure.</summary>
    public OpeningRegionLayeredInscriptionPresentation Presentation { get; }

    /// <summary>Gets the exact authored partial-inscription evidence definition.</summary>
    public DiscoveryEvidence Evidence { get; }

    /// <summary>Gets the filtered single-identifier Seeker evidence view.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }
}

using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Records one bounded local reason for a relationship dimension.</summary>
public sealed record CharacterRelationshipReason
{
    /// <summary>Gets the maximum reason summary length in UTF-16 code units.</summary>
    public const int MaximumSummaryLength = 500;

    /// <summary>Creates one immutable relationship reason.</summary>
    public CharacterRelationshipReason(
        RelationshipReasonId reasonId,
        CharacterRelationshipDimension dimension,
        CharacterTurnId sourceTurnId,
        string summary)
    {
        ArgumentNullException.ThrowIfNull(reasonId);
        ArgumentNullException.ThrowIfNull(dimension);
        ArgumentNullException.ThrowIfNull(sourceTurnId);

        ReasonId = reasonId;
        Dimension = dimension;
        SourceTurnId = sourceTurnId;
        Summary = AuthoredCharacterText.RequireNonBlank(
            summary,
            MaximumSummaryLength,
            nameof(summary));
    }

    /// <summary>Gets the exact stable reason identity.</summary>
    public RelationshipReasonId ReasonId { get; }

    /// <summary>Gets the exact relationship dimension this reason concerns.</summary>
    public CharacterRelationshipDimension Dimension { get; }

    /// <summary>Gets the exact completed local turn providing provenance.</summary>
    public CharacterTurnId SourceTurnId { get; }

    /// <summary>Gets the recorded bounded reason text exactly as supplied.</summary>
    public string Summary { get; }
}

using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Contains authored measures and bounded active character protections.</summary>
public sealed class DiseaseProtectionSnapshot
{
    /// <summary>Gets the maximum supported number of active character protections.</summary>
    public const int MaximumActiveProtectionCount = 256;

    /// <summary>Creates one immutable active-protection snapshot.</summary>
    public DiseaseProtectionSnapshot(
        DiseaseProtectionDefinitionSnapshot definitions,
        IEnumerable<CharacterDiseaseProtection> activeProtections)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(activeProtections);

        List<CharacterDiseaseProtection> retainedProtections = new();
        HashSet<(CharacterId CharacterId, DiseaseId DiseaseId, EvidenceId EvidenceId)> identities =
            new();
        foreach (CharacterDiseaseProtection protection in activeProtections)
        {
            if (protection is null)
            {
                throw new ArgumentException(
                    "Active disease protections cannot contain null values.",
                    nameof(activeProtections));
            }

            if (!definitions.Definitions.Any(definition =>
                    ReferenceEquals(definition, protection.Definition)))
            {
                throw new ArgumentException(
                    "Each active protection must retain an exact authored definition.",
                    nameof(activeProtections));
            }

            DiseaseProtectionDefinition definition = protection.Definition;
            if (!identities.Add((
                    protection.CharacterId,
                    definition.Disease.DiseaseId,
                    definition.EvidenceId)))
            {
                throw new ArgumentException(
                    "Active protections cannot repeat a character and measure pair.",
                    nameof(activeProtections));
            }

            retainedProtections.Add(protection);
            if (retainedProtections.Count > MaximumActiveProtectionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activeProtections),
                    $"A snapshot accepts at most {MaximumActiveProtectionCount} active protections.");
            }
        }

        Definitions = definitions;
        ActiveProtections = retainedProtections.AsReadOnly();
    }

    /// <summary>Gets the exact authored protective-measure definitions.</summary>
    public DiseaseProtectionDefinitionSnapshot Definitions { get; }

    /// <summary>Gets exact active character protections in stable supplied order.</summary>
    public IReadOnlyList<CharacterDiseaseProtection> ActiveProtections { get; }
}

using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Contains canonical disease definitions and hidden character infection truth.</summary>
public sealed class DiseaseTruthSnapshot
{
    /// <summary>Gets the maximum supported number of character-disease infections.</summary>
    public const int MaximumInfectionCount = 256;

    /// <summary>Creates one immutable canonical disease-truth snapshot.</summary>
    public DiseaseTruthSnapshot(
        DiseaseDefinitionSnapshot definitions,
        IEnumerable<CharacterDiseaseState> infections)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(infections);

        List<CharacterDiseaseState> retainedInfections = new();
        HashSet<(CharacterId CharacterId, DiseaseId DiseaseId)> identities = new();
        foreach (CharacterDiseaseState infection in infections)
        {
            if (infection is null)
            {
                throw new ArgumentException(
                    "Disease infections cannot contain null values.",
                    nameof(infections));
            }

            if (!definitions.Definitions.Any(definition =>
                    ReferenceEquals(definition, infection.Disease)))
            {
                throw new ArgumentException(
                    "Each infection must retain an exact authored disease definition.",
                    nameof(infections));
            }

            if (!identities.Add((infection.CharacterId, infection.Disease.DiseaseId)))
            {
                throw new ArgumentException(
                    "Disease infections cannot repeat a character and disease identity pair.",
                    nameof(infections));
            }

            retainedInfections.Add(infection);
            if (retainedInfections.Count > MaximumInfectionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(infections),
                    $"Disease truth accepts at most {MaximumInfectionCount} infections.");
            }
        }

        Definitions = definitions;
        Infections = retainedInfections.AsReadOnly();
    }

    /// <summary>Gets the exact authored disease-definition snapshot.</summary>
    public DiseaseDefinitionSnapshot Definitions { get; }

    /// <summary>Gets exact hidden infection states in stable supplied order.</summary>
    public IReadOnlyList<CharacterDiseaseState> Infections { get; }
}

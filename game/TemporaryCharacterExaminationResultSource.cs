using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Selects temporary safe results for exact observed examination occurrences.</summary>
public partial class TemporaryCharacterExaminationResultSource : Node
{
    private readonly Func<CharacterTurnWorldEventOccurrence, ObjectId, CharacterExaminationResult?>
        resultSelector = DeterministicExaminationResultSelector.Select;

    /// <summary>Returns a fresh safe result only for the exact supplied examination target.</summary>
    public CharacterExaminationResult? CreateResult(
        CharacterTurnWorldEventOccurrence occurrence,
        ObjectId safeTargetObjectId)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(safeTargetObjectId);

        return resultSelector(occurrence, safeTargetObjectId);
    }
}

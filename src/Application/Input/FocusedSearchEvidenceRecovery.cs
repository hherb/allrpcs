using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Application.Input;

/// <summary>
/// One sabotage presentation that may recover once its evidence object's search resolves.
/// </summary>
/// <param name="EvidenceObjectId">The exact sabotage evidence object.</param>
/// <param name="TryRecover">Recovers the presentation against the resolved perception.</param>
public sealed record FocusedSearchEvidenceRecovery(
    ObjectId EvidenceObjectId,
    Func<CharacterPerceptionSnapshot, bool> TryRecover);

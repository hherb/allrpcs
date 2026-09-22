using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Supplies controlled request-author outputs to the isolated composer smoke.</summary>
public partial class CharacterSabotageActionRequestAuthorStub : Node, ICharacterSabotageActionRequestAuthor
{
    /// <inheritdoc />
    public CharacterSabotageActionConsiderationRequest? Request { get; set; }

    /// <inheritdoc />
    public CharacterSabotageActionCandidateBinding? CandidateBinding { get; set; }
}

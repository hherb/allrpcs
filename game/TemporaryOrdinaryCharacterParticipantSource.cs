using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the stable bounded roster of ordinary LLM dialogue participants.</summary>
public partial class TemporaryOrdinaryCharacterParticipantSource : Node
{
    /// <summary>Gets the exact healer-first read-only ordinary participant roster.</summary>
    public IReadOnlyList<Character> Participants { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Character healer = GetNode<TemporaryHealerCharacterSource>(
            "../TemporaryHealerCharacterSource").Character;
        Character sage = RepresentativeSageCharacterSource.Create();

        Participants = Array.AsReadOnly([healer, sage]);
    }
}

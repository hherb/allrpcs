using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's exact immutable provider-neutral character-memory snapshot.</summary>
public partial class CharacterMemoryController : Node
{
    /// <summary>Gets the exact installed canonical memory, initially an explicit empty snapshot.</summary>
    public CharacterMemorySnapshot Memory { get; private set; } = new([]);

    /// <summary>Installs one exact caller-supplied immutable replacement.</summary>
    public void Install(CharacterMemorySnapshot memory)
    {
        ArgumentNullException.ThrowIfNull(memory);

        Memory = memory;
    }
}

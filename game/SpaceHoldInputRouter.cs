using Godot;

namespace Allrpcs.Game;

/// <summary>Routes a game-owned popup viewport through the same Space intent owner.</summary>
public partial class SpaceHoldInputRouter : Node
{
    internal SpaceHoldSpeechController OwnerController { get; set; } = null!;
    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    { if (GodotObject.IsInstanceValid(OwnerController)) { OwnerController.Handle(@event, GetViewport()); } }
}

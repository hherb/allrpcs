using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one temporary identifier for the greybox-ready world event.</summary>
public partial class TemporaryWorldEventSource : Node
{
    private const string GreyboxReadyEventIdText = "greybox-ready";

    /// <summary>Gets the single retained temporary world-event identifier.</summary>
    public WorldEventId EventId { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        EventId = CreateEventId();
    }

    private static WorldEventId CreateEventId() => new(GreyboxReadyEventIdText);
}

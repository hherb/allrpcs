namespace Allrpcs.Application.Dialogue;

/// <summary>Observes accepted provider-neutral stream updates without owning generation.</summary>
public interface ICharacterModelStreamUpdateSink
{
    /// <summary>Records one exact accepted update under caller-owned cancellation.</summary>
    ValueTask RecordAsync(
        CharacterModelStreamUpdate update,
        CancellationToken cancellationToken);
}

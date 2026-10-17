using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Prepares every explicit owner before committing any owner, in registration order.</summary>
internal sealed class RestoreParticipantRegistry
{
    private readonly List<IRestoreParticipant> participants = [];

    internal IReadOnlyList<string> Owners => participants.Select(participant => participant.Owner).ToArray();

    internal void Register(IRestoreParticipant participant)
    {
        if (participants.Any(existing => existing.Owner == participant.Owner))
        {
            throw new ArgumentException("A restore owner must be registered exactly once.", nameof(participant));
        }
        participants.Add(participant);
    }

    internal bool TryRestore(SaveSnapshot snapshot)
    {
        List<PreparedRestore> prepared = [];
        foreach (IRestoreParticipant participant in participants)
        {
            try { prepared.Add(participant.Prepare(snapshot)); }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                GD.PushWarning($"Restore rejected by {participant.Owner}: {exception}");
                return false;
            }
        }
        foreach (PreparedRestore participant in prepared) { participant.Commit(); }
        foreach (PreparedRestore participant in prepared)
        {
            if (participant.Publish is { } publish) { Publish(publish); }
        }
        return true;
    }

    internal static void Publish(Action publish)
    {
        try { publish(); }
        catch (Exception exception) { GD.PrintErr($"Restoration presentation failed: {exception}"); }
    }
}

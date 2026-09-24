using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one exact fifth-cycle Move output without starting it.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationSuccessorCompositionCoordinator
    : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator authoring =
        null!;

    /// <summary>Gets the exact admission retained by successful composition.</summary>
    public CharacterSabotageActionAuthoringAdmission? Admission { get; private set; }

    /// <summary>Gets the exact guarded Move author output retained by composition.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the fresh generic Move request produced by successful composition.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator");
    }

    /// <summary>Composes once only from the exact still-current guarded Move output.</summary>
    public bool TryCompose(CharacterSabotageActionConsiderationRequest? expectedAuthorRequest)
    {
        CharacterSabotageActionAuthoringAdmission? currentAdmission = authoring.Admission;
        CharacterSabotageActionConsiderationRequest? currentAuthorRequest =
            authoring.AuthorRequest;
        if (Admission is not null
            || AuthorRequest is not null
            || Request is not null
            || currentAdmission is null
            || currentAuthorRequest is null
            || !ReferenceEquals(currentAuthorRequest, expectedAuthorRequest)
            || !authoring.IsCurrentAuthorRequest(currentAuthorRequest))
        {
            return false;
        }

        Admission = currentAdmission;
        AuthorRequest = currentAuthorRequest;
        Request = new CharacterSabotageActionConsiderationRequest(
            currentAdmission.Actor,
            [CharacterSabotageActionKind.Move]);
        return true;
    }

    /// <summary>Checks the fresh request against its complete exact current author graph.</summary>
    internal bool IsCurrentRequest(CharacterSabotageActionConsiderationRequest? expectedRequest) =>
        Admission is not null
        && AuthorRequest is not null
        && Request is not null
        && ReferenceEquals(Request, expectedRequest)
        && ReferenceEquals(Admission, authoring.Admission)
        && ReferenceEquals(AuthorRequest, authoring.AuthorRequest)
        && authoring.IsCurrentAuthorRequest(AuthorRequest)
        && ReferenceEquals(Request.Actor, Admission.Actor)
        && Request.ActionKinds.Count == 1
        && ReferenceEquals(Request.ActionKinds[0], CharacterSabotageActionKind.Move);
}

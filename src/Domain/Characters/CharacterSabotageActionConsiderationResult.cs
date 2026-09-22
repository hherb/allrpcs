namespace Allrpcs.Domain.Characters;

/// <summary>Binds one accepted sabotage-kind decision to its exact request.</summary>
public sealed record CharacterSabotageActionConsiderationResult
{
    /// <summary>Creates one result only from an accepted exact decision.</summary>
    public CharacterSabotageActionConsiderationResult(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!CharacterSabotageActionDecisionPolicy.CanAccept(request, decision))
        {
            throw new ArgumentException(
                "The sabotage action decision is not valid for the exact request.",
                nameof(decision));
        }

        Request = request;
        Decision = decision;
    }

    /// <summary>Gets the exact provider-neutral request.</summary>
    public CharacterSabotageActionConsiderationRequest Request { get; }

    /// <summary>Gets the exact accepted decision.</summary>
    public CharacterSabotageActionDecision Decision { get; }

    /// <summary>Gets the selected action kind, or null when the actor abstained.</summary>
    public CharacterSabotageActionKind? SelectedActionKind => Decision.SelectedActionKind;
}

using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes exact current action-author outputs into one canonical request.</summary>
public partial class CharacterSabotageActionRequestComposer : Node
{
    private static readonly CharacterSabotageActionKind[] CanonicalKinds =
    [
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Move,
        CharacterSabotageActionKind.Substitute,
        CharacterSabotageActionKind.Contaminate,
        CharacterSabotageActionKind.Misquote,
        CharacterSabotageActionKind.Delay,
        CharacterSabotageActionKind.Influence,
    ];

    private ICharacterSabotageActionRequestAuthor[] authors = null!;

    /// <summary>Gets the exact one-shot composed request accepted by this controller.</summary>
    public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authors =
        [
            GetAuthor("CharacterSabotageActionConcealmentRequestController"),
            GetAuthor("CharacterSabotageActionMoveRequestController"),
            GetAuthor("CharacterSabotageActionSubstitutionRequestController"),
            GetAuthor("CharacterSabotageActionContaminationRequestController"),
            GetAuthor("CharacterSabotageActionMisquotationRequestController"),
            GetAuthor("CharacterSabotageActionDelayRequestController"),
            GetAuthor("CharacterSabotageActionInfluenceRequestController"),
        ];
    }

    /// <summary>Creates one request from the supplied exact current author outputs.</summary>
    public bool TryCompose(
        Character actor,
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?> expectedCurrentOutputs)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(expectedCurrentOutputs);

        if (Request is not null || expectedCurrentOutputs.Count != CanonicalKinds.Length)
        {
            return false;
        }

        List<CharacterSabotageActionKind> eligibleKinds = new(CanonicalKinds.Length);
        for (int index = 0; index < CanonicalKinds.Length; index++)
        {
            CharacterSabotageActionConsiderationRequest? current = authors[index].Request;
            CharacterSabotageActionConsiderationRequest? expected = expectedCurrentOutputs[index];
            if (!ReferenceEquals(current, expected))
            {
                return false;
            }

            if (current is null)
            {
                continue;
            }

            if (!ReferenceEquals(current.Actor, actor)
                || current.ActionKinds.Count != 1
                || !ReferenceEquals(current.ActionKinds[0], CanonicalKinds[index]))
            {
                return false;
            }

            eligibleKinds.Add(CanonicalKinds[index]);
        }

        if (eligibleKinds.Count == 0)
        {
            return false;
        }

        Request = new CharacterSabotageActionConsiderationRequest(actor, eligibleKinds);
        return true;
    }

    private ICharacterSabotageActionRequestAuthor GetAuthor(string nodeName)
    {
        Node node = GetNode<Node>($"../{nodeName}");
        return node as ICharacterSabotageActionRequestAuthor
            ?? throw new InvalidOperationException($"Node '{nodeName}' is not a request author.");
    }
}

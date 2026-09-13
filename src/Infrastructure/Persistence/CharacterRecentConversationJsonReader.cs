using System.Text.Json;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly reads the recent-conversation constituent of character memory.</summary>
internal static class CharacterRecentConversationJsonReader
{
    /// <summary>Reads completed local turns in stable oldest-to-newest order.</summary>
    internal static CharacterRecentConversationWindow Read(
        JsonElement element,
        CharacterId characterId,
        bool includesDialogueDeliveryMode)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterRecentConversationTurn> turns = new();
        foreach (JsonElement turnElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(turnElement, "turnId", "heard", "response");
            CharacterTurnRequest request = ReadRequest(
                turnElement.GetProperty("heard"),
                characterId,
                includesDialogueDeliveryMode);
            CharacterTurnResponse response = ReadResponse(
                turnElement.GetProperty("response"),
                request,
                includesDialogueDeliveryMode);
            turns.Add(
                new CharacterRecentConversationTurn(
                    new CharacterTurnId(ReadString(turnElement, "turnId")),
                    new CharacterTurnActivity(
                        request,
                        response,
                        CharacterTurnActivityState.Completed)));
        }

        return new CharacterRecentConversationWindow(characterId, turns);
    }

    private static CharacterTurnRequest ReadRequest(
        JsonElement element,
        CharacterId characterId,
        bool includesDialogueDeliveryMode)
    {
        if (includesDialogueDeliveryMode)
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                element,
                "speakerCharacterId",
                "deliveryMode",
                "listenerCharacterIds",
                "text",
                "dialogueAct");
        }
        else
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                element,
                "speakerCharacterId",
                "listenerCharacterIds",
                "text",
                "dialogueAct");
        }

        CharacterId speakerId = new(ReadString(element, "speakerCharacterId"));
        DialogueUtterance utterance = new(speakerId, ReadString(element, "text"));
        DialogueDeliveryMode deliveryMode = includesDialogueDeliveryMode
            ? DialogueDeliveryMode.Parse(ReadString(element, "deliveryMode"))
            : DialogueDeliveryMode.OrdinarySpeech;
        JsonElement listenersElement = element.GetProperty("listenerCharacterIds");
        SaveSnapshotJsonDeserializer.RequireArray(listenersElement);
        List<CharacterId> listenerIds = new();
        foreach (JsonElement listenerElement in listenersElement.EnumerateArray())
        {
            listenerIds.Add(
                new CharacterId(SaveSnapshotJsonDeserializer.ReadString(listenerElement)));
        }

        return new CharacterTurnRequest(
            characterId,
            new DialogueEvent(
                new DialogueDelivery(utterance, deliveryMode),
                new DialogueAudienceSnapshot(speakerId, listenerIds)),
            new DialogueAct(
                utterance,
                DialogueActType.Parse(ReadString(element, "dialogueAct"))));
    }

    private static CharacterTurnResponse ReadResponse(
        JsonElement element,
        CharacterTurnRequest request,
        bool includesDialogueDeliveryMode)
    {
        if (includesDialogueDeliveryMode)
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                element,
                "disposition",
                "speech",
                "deliveryMode",
                "immediateIntention",
                "commitments",
                "stateProposals");
        }
        else
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                element,
                "disposition",
                "speech",
                "immediateIntention",
                "commitments",
                "stateProposals");
        }

        string? speechText = ReadNullableString(element.GetProperty("speech"));
        DialogueDeliveryMode? deliveryMode = includesDialogueDeliveryMode
            ? ReadNullableDeliveryMode(element.GetProperty("deliveryMode"))
            : speechText is null
                ? null
                : DialogueDeliveryMode.OrdinarySpeech;
        if ((speechText is null) != (deliveryMode is null))
        {
            throw SaveSnapshotJsonDeserializer.InvalidDocument();
        }

        JsonElement commitmentsElement = element.GetProperty("commitments");
        SaveSnapshotJsonDeserializer.RequireArray(commitmentsElement);
        List<CharacterCommitment> commitments = new();
        foreach (JsonElement commitmentElement in commitmentsElement.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireArray(commitmentElement);
            List<CharacterIntention> intentions = new();
            foreach (JsonElement intentionElement in commitmentElement.EnumerateArray())
            {
                intentions.Add(ReadIntention(intentionElement));
            }

            commitments.Add(new CharacterCommitment(intentions));
        }

        JsonElement proposalsElement = element.GetProperty("stateProposals");
        SaveSnapshotJsonDeserializer.RequireArray(proposalsElement);
        List<CharacterStateProposal> proposals = new();
        foreach (JsonElement proposalElement in proposalsElement.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(proposalElement, "key", "value");
            proposals.Add(
                new CharacterStateProposal(
                    ReadString(proposalElement, "key"),
                    ReadString(proposalElement, "value")));
        }

        return new CharacterTurnResponse(
            request,
            CharacterTurnDisposition.Parse(ReadString(element, "disposition")),
            speechText is null
                ? null
                : new DialogueDelivery(
                    new DialogueUtterance(request.RespondingCharacterId, speechText),
                    deliveryMode!),
            ReadIntention(element.GetProperty("immediateIntention")),
            commitments,
            proposals);
    }

    private static CharacterIntention ReadIntention(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(element, "type", "targetObjectId");
        string? targetId = ReadNullableString(element.GetProperty("targetObjectId"));
        return new CharacterIntention(
            CharacterIntentionType.Parse(ReadString(element, "type")),
            targetId is null ? null : new ObjectId(targetId));
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        SaveSnapshotJsonDeserializer.ReadString(element.GetProperty(propertyName));

    private static string? ReadNullableString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => SaveSnapshotJsonDeserializer.ReadString(element),
        _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
    };

    private static DialogueDeliveryMode? ReadNullableDeliveryMode(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => DialogueDeliveryMode.Parse(
                SaveSnapshotJsonDeserializer.ReadString(element)),
            _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
        };
}

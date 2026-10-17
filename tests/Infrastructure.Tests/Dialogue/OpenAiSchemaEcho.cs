namespace Allrpcs.Infrastructure.Tests.Dialogue;

public static class OpenAiSchemaEcho
{
    public static string Event(string eventType, int depth)
    {
        string schema = "{}";
        for (int level = 5; level < depth; level++)
        {
            schema = "{\"properties\":" + schema + "}";
        }

        return "{\"type\":\"" + eventType
            + "\",\"response\":{\"text\":{\"format\":{\"schema\":" + schema + "}}}}";
    }

    public static TheoryData<string> InvalidEvents => new()
    {
        Event("response.created", 65),
        "{\"type\":\"response.created\",/*comment*/\"response\":{}}",
        "{\"type\":\"response.created\",\"response\":{},}",
    };
}

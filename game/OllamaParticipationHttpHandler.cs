using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Game;

/// <summary>Supplies bounded fictional HTTP for ordinary Ollama scene checks, or forwards explicit live diagnostics.</summary>
internal sealed class OllamaParticipationHttpHandler(string mode) : HttpMessageHandler
{
    private static bool IsParticipation(JsonElement root) =>
        root.GetProperty("format").TryGetProperty("properties", out JsonElement properties)
        && properties.TryGetProperty("kind", out _);

    internal List<JsonElement> ReplyRequests { get; } = [];
    internal string ReplyText { get; set; } = "I hear you.";
    internal int ReplyDelayMilliseconds { get; set; }
    internal bool SilentExaminationReply { get; set; }
    internal bool WaitInsteadOfExamination { get; set; }
    public int ParticipationCalls { get; private set; }
    public int ReplyCalls { get; private set; }
    public int? LastHttpStatus { get; private set; }
    public string? ProviderError { get; private set; }
    public string[] NativeMessageFields { get; private set; } = [];
    private readonly HttpMessageInvoker liveTransport = new(new HttpClientHandler());
    protected override void Dispose(bool disposing)
    { if (disposing) { liveTransport.Dispose(); } base.Dispose(disposing); }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        using JsonDocument document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
        JsonElement root = document.RootElement;
        if (!IsParticipation(root)) { ReplyRequests.Add(root.Clone()); }
        if (mode == "live")
        {
            if (IsParticipation(root)) { ParticipationCalls++; }
            else { ReplyCalls++; }
            HttpResponseMessage response = await liveTransport.SendAsync(request, token).ConfigureAwait(false);
            LastHttpStatus = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(token);
                ProviderError = error.Length <= 256 ? error : "provider error exceeds diagnostic bound";
            }
            else
            {
                using HttpContent original = response.Content;
                string contentType = original.Headers.ContentType?.MediaType ?? "application/x-ndjson";
                IReadOnlyList<string> lines = await OllamaNewlineDelimitedJsonReader.ReadAsync(
                    await original.ReadAsStreamAsync(token), token);
                HashSet<string> fields = [];
                foreach (string nativeLine in lines)
                {
                    using JsonDocument native = JsonDocument.Parse(nativeLine);
                    if (native.RootElement.TryGetProperty("message", out JsonElement message))
                    { foreach (JsonProperty field in message.EnumerateObject()) { fields.Add(field.Name); } }
                }
                NativeMessageFields = fields.Order(StringComparer.Ordinal).ToArray();
                response.Content = new StringContent(string.Join("\n", lines) + "\n", Encoding.UTF8, contentType);
            }
            return response;
        }
        string body;
        if (IsParticipation(root))
        {
            ParticipationCalls++;
            if (mode == "failure") { return new(HttpStatusCode.ServiceUnavailable); }
            using JsonDocument context = JsonDocument.Parse(root.GetProperty("messages")[1].GetProperty("content").GetString()!);
            bool respond = mode is "respond" or "repeated" or "examination" && context.RootElement.GetProperty("isDirectlyAddressed").GetBoolean();
            string? motive = respond ? null : context.RootElement.GetProperty("goals")[0].GetProperty("key").GetString();
            body = JsonSerializer.Serialize(new { kind = respond ? "respond" : "remain-silent", motiveKey = motive });
        }
        else
        {
            ReplyCalls++;
            if (ReplyDelayMilliseconds > 0) { await Task.Delay(ReplyDelayMilliseconds, token); }
            body = JsonSerializer.Serialize(new
            {
                disposition = "accept",
                speech = (mode == "examination" && SilentExaminationReply)
                    || (mode == "repeated" && ReplyCalls is 3 or 4 or 6) ? null : ReplyText,
                deliveryMode = (mode == "examination" && SilentExaminationReply)
                    || (mode == "repeated" && ReplyCalls is 3 or 6) ? null
                    : mode == "repeated" && ReplyCalls is 2 or 4 ? "whispering" : "ordinary-speech",
                immediateIntention = new
                {
                    type = mode == "examination" && !WaitInsteadOfExamination ? "examine-object" : "wait",
                    targetObjectId = mode == "examination" && !WaitInsteadOfExamination ? "herb-patch" : (string?)null
                },
                commitments = Array.Empty<string>(),
                stateProposals = Array.Empty<string>()
            });
        }
        string line = JsonSerializer.Serialize(new
        {
            message = new { role = "assistant", content = body },
            done = true,
            done_reason = "stop",
            prompt_eval_count = 20,
            prompt_eval_cached_count = 4,
            eval_count = 10
        });
        return new(HttpStatusCode.OK) { Content = new StringContent(line + "\n", Encoding.UTF8, "application/x-ndjson") };
    }
}

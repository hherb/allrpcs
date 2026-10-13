# Allrpcs model-provider disclosures

Checked 2026-10-13 for the development source. Ordinary character dialogue currently composes
OpenAI or local Ollama when a usable saved selection is available. Anthropic has an adapter and a
settings label, but no ordinary game credential/composition path. None of these source findings
establish current account access, live model availability or packaged-platform support.
See [privacy information](privacy.md) for stored data, keys and deletion.

## Available implementation paths

| Path | Current game behavior | Destination and authentication |
| --- | --- | --- |
| OpenAI | Ordinary reply generation and the installed additional OpenAI role-decision bindings use the selected model | Official OpenAI API over HTTPS; the game loads the API key from the OS credential store |
| Ollama | Ordinary reply generation uses a separately running local server; additional OpenAI-only role-decision bindings are cleared for this selection | Numeric loopback HTTP, default `127.0.0.1:11434`; no cloud API key is sent by this adapter |
| Anthropic | Provider-neutral adapter/connection-test code exists; ordinary game composition does not activate it | Adapter default is Anthropic's HTTPS API with a caller-supplied key; this is a development capability, not a working ordinary settings path |
| No usable binding | The fallback reports model generation unavailable | The fallback makes no provider request and supplies no conversational response |

The game selects an exact configured binding; it does not silently switch a failed OpenAI request
to another provider. “Offline fallback” in historical development descriptions means an unavailable
model result, not a bundled offline conversational model. Local Ollama requires its own running
server and installed suitable model. The game does not install Ollama or fetch its weights through
the inspected adapter. The local server's own behavior still needs separate consideration.

## What a request contains

An ordinary dialogue request contains the selected model name, game instructions, the current
utterance and the supplied bounded character context. That context can contain filtered perception,
knowledge, goals, memory, recent speech and valid actions. The request also carries character and
delivery identifiers. The game serializes these request values rather than uploading the save file
as a file attachment. Local saves can contain more conversation history than a particular request;
relevant retained text may still reappear in later requests.

OpenAI participation decisions include heard dialogue plus the scheduled character's identity,
public/private fictional backgrounds, direct-address status and goals. Other installed role-decision
requests can include candidate object/evidence IDs, descriptions, capabilities, knowledge and goals.
Consequently cloud requests are not limited to the text visible in the most recent dialogue box.
Keys are sent in authentication headers, separate from the ordinary request-body serializer.

OpenAI Responses request serializers ask for `store: false`; that flag describes what the game
requests and does not prove provider-wide zero retention. The Anthropic adapter sends messages and
context in its Messages request. Ollama receives comparable dialogue/context at its local chat
endpoint. This document makes no claim about any service's current retention, training, geography,
pricing or account policy.

## Settings, tests and costs

Provider/model selection, saving that selection, saving/deleting an OpenAI key and Test Connection
are explicit settings operations. Editing a text field does not itself save it. A supported saved
selection can be composed again at startup and used for eligible character work without a new
per-request confirmation. The ordinary UI does not expose a general custom-endpoint editor;
endpoint injection in development adapters does not imply a player setting.

Test Connection checks access to the configured model: OpenAI/Anthropic adapter tests request model
information using the relevant authentication; Ollama asks its local server about the model. Tests
do not submit player dialogue or generate a character reply. An Anthropic adapter test is not
composed through the ordinary game's unsupported Anthropic path.

Provider generation can consume account quota or incur charges under your service agreement.
The game has no implemented provider-spending cap or billing estimate in this checkpoint. A single
player interaction can lead to several character decisions or a bounded retry, so it need not equal
one billable request. Confirm account limits and service terms before using a paid model.

## Responses and failures

The model proposes character speech and intentions. The game validates structured results and
retains authority over allowed actions and world changes. Invalid output, missing credentials,
unavailable services and cancellation can produce failure rather than a completed character reply.
Validation is not a guarantee that generated speech is accurate or suitable.

Typed dialogue remains available as an input surface, and recorded text remains separate from
future speech playback. Without a working model, the unavailable fallback does not fulfill natural
conversation. Provider switching, recovery, credentials, platform packages and complete gameplay
remain limited to their recorded implementation/evidence boundaries.

The separate Pocket TTS component is local speech synthesis, not a character-dialogue model
provider. Its English/German worker, assets and voices are not yet composed into ordinary game
playback; microphone recognition is separate unfinished work. This document does not authorize or
alter the concurrently owned speech implementation.

## Release limits

No fresh physical controller/joystick session has been performed. The owner currently has a
MacBook Pro with built-in keyboard and trackpad and has deferred the named controller evaluation.
A trackpad is not evidence for the proposal's ordinary mouse or named controller qualification.
Native reader evaluation, executable installation/export certification and a released support
matrix are also incomplete. These disclosures accompany development source; release notes,
release-specific known limitations and an actual release candidate remain separate deliverables.

The [disclosure record](privacy-provider-disclosures.json) binds the current source and documents.
It records documentation verification, not live provider tests or new acceptance of closed runtime
owners. Recheck the relevant statements when provider composition or release inputs change.

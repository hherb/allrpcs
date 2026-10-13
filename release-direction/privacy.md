# Allrpcs privacy information

Checked 2026-10-13 for the current development source. This describes the game code;
installed release packages and provider-side policies have not been qualified by this document.
See [model-provider disclosures](model-providers.md) for the current provider limits.

## Your game data

Game progress is saved on your computer. A save can include discovered evidence, world events,
character conversations and memories, patient/remedy state, journal entries, map notes and
investigation progress. Recorded dialogue and character replies can therefore remain in your save.
The game keeps audio, controls and model-provider settings in separate local files. Provider
settings contain provider/model choices; the saved provider-settings document contains no API key.
These game files are readable JSON. The game does not encrypt them itself; computer access,
filesystem protection, backups and any operating-system disk encryption are separate controls.
There is no game-operated cloud-save service in the inspected save implementation.

## When dialogue leaves your computer

Using the OpenAI model path sends the current dialogue text and the character's supplied context
to OpenAI's API. Context can include perceptions, relevant knowledge, goals, memories, recent
speech, delivery information and permitted actions. Role decisions can also send character names,
public/private fictional backgrounds, goals, eligible targets and action/evidence descriptions.
These are in-game data, but dialogue text and notes can contain information you chose to enter.

The current Ollama path sends the dialogue request to a local server on your computer. That server
is a separate program: its own logging, model downloads, plugins or forwarding behavior are outside
the game's local-endpoint restriction. Anthropic adapter code exists, but the ordinary game does
not currently compose an Anthropic credential/model path. Selecting a provider label alone does
not establish a working connection.

A saved, supported provider selection can be used on a later launch. The game does not ask for
permission separately before every eligible model request. Connection tests also contact the
selected service, sending authentication where applicable and the configured model identifier;
they do not send your conversation or generate a reply. Multiple character decisions and retries
can produce multiple requests. Cancellation stops further local work where observed; it cannot
withdraw text already sent to a service.

The OpenAI Responses requests set the API's storage option to false. That request flag does not
establish a promise about all provider logging, retention, training, account access or data location.
Consult the selected service's current terms and account controls for its handling of received data.

## Keys and local controls

The OpenAI key is saved separately using macOS Keychain, Linux Secret Service or Windows
Credential Manager, depending on the operating system. Save and Delete OpenAI key operations are
explicit settings actions. Deleting the game's stored key does not revoke it at the provider and
does not recall an in-flight request. Provider revocation is a separate account operation.
The game clears the key-entry field after submitting a save; this is not a guarantee that all
transient process memory has been erased. No Anthropic key-storage UI is installed in the ordinary
composition described here.

To remove local game data, close the game first and delete the specific files you want to reset
from its user-data folder. On this macOS development configuration, Finder's Go to Folder can open
`~/Library/Application Support/Godot/app_userdata/Allrpcs/`.

| File | Removing it resets |
| --- | --- |
| `save-v1.json` | Saved progress and the conversations/memories contained in that save |
| `character-model-settings-v1.json` | Saved provider/model selection; absent settings leave ordinary model generation unavailable until a usable selection is configured |
| `audio-settings-v1.json` | Saved audio preferences |
| `controls-settings-v1.json` | Saved input assignments and deadzones |

API keys remain in the OS credential store until separately deleted. Logs, exports, screenshots,
manually copied saves and backups are separate files; deleting a save does not erase those copies
or provider-held data. Deletion is ordinary file deletion, not secure erasure. Future package names
or custom data-directory settings may use a different location. There is no dedicated erase-all-data
workflow in this development checkpoint.

## Diagnostics, links and optional speech

The model-generation diagnostic type contains provider, result/failure code, elapsed time and
optional token counts. It has no dialogue, prompt, model response or credential fields. Diagnostic
recording requires a caller-supplied sink; the ordinary inspected composition does not install a
model-diagnostic upload service. This limited diagnostic type is not a guarantee that every Godot,
OS or third-party library log is free of personal content or local paths.

No game-operated analytics or crash-upload service is composed in the inspected source. Crash
reporting and its consent policy remain unfinished roadmap work. Operating-system crash reporting
and external tools have their own settings. Clicking an external source/credit link opens your
browser, where the destination site and browser can receive the usual web-request information.
Sharing support evidence is a separate deliberate action; inspect it before including saves,
logs or captures that may reveal dialogue, secrets or unrelated desktop content.

Microphone capture and speech recognition are not installed in ordinary game dialogue. A separate
local Pocket TTS service component can pass supplied text to a local Python process and return audio;
it is not yet wired into ordinary game playback. Model/voice preparation can require downloads.
The service requests local-only model loading, but has not been evaluated under an OS network block.
This disclosure makes no claim about microphone permission, speech-device qualification or future
ASR/TTS integrations.

## Scope of this information

This document records observed implementation boundaries. It does not certify a release, legal
compliance, provider-side deletion, an encrypted save, or a complete network-isolation guarantee.
The [source/evidence record](privacy-provider-disclosures.json) identifies the inspected revision
and the source supporting each statement. Refresh these disclosures when storage, providers,
telemetry or speech composition changes.

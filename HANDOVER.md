# Allrpcs Handover

## Current state

`main` contains merged PR #167 at `8c1b89c`. The current branch is
`codex/phase-4-ollama-ndjson` and advances Phase 4 through Phase 4j5.

- Phase 4j2 adds strict leave-open UTF-8 Ollama NDJSON framing plus a native chat accumulator. It
  bounds lines, documents, counts, combined response text, and usage; requires ordered assistant
  content and one successful `done:true` stop; and rejects blank/truncated input, invalid UTF-8,
  thinking, tools, malformed metadata, unknown or repeated terminal shapes, and post-terminal data.
- Phases 4j3-4j4 add `OllamaEndpoint` and `OllamaCharacterModel`. Only explicit HTTP IP-loopback
  origins with explicit non-default ports are accepted. The caller-composed adapter posts stateless
  native chat requests, requires NDJSON success content, maps HTTP/transport failures without
  reading error bodies, emits only a complete provider-neutral termination, and optionally records
  bounded redacted Ollama diagnostics.
- Phase 4j5 adds a redacted `/api/show` connection test for the exact configured local model. It
  uses the same validated endpoint, sends no credentials, generates no content, and never reads
  success or failure metadata.
- A live opt-in check against the already-installed `granite4.1:8b-q8_0` model confirmed `/api/show`
  availability and real `/api/chat` NDJSON. The live provider rejected the prior maximum/string-
  bound schema grammar; the request now omits those provider-unsupported keywords while the local
  decoder retains every bound. The model then produced locally invalid gameplay content, which was
  rejected as a redacted `InvalidResponse` without a response or partial presentation. No model was
  installed, pulled, changed, or retained by the game.

OpenAI, Anthropic, and Ollama adapters remain caller-composed only. No production provider binding,
settings UI/persistence, automatic local-server discovery, model process ownership, Anthropic
credential store, Windows/Linux credential store, diagnostic export, generated non-wait intention
execution, or gameplay persistence/mutation was added. Full Phase 4 acceptance remains incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 296 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations; and
- the bounded graphical Compatibility-renderer launch.

The additional explicitly enabled live check passes with
`ALLRPCS_LIVE_OLLAMA_MODEL=granite4.1:8b-q8_0`; without that variable the standard Infrastructure
suite performs no local network access. The restricted sandbox reproduces the known MSBuild/Godot
IPC and log-directory denials; identical host-local commands pass. GitHub Actions budget is
exhausted for the month: do not dispatch hosted workflows, and treat zero-step hosted failures as
account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4k1: immutable caller-composed provider bindings and exact selection**.

Acceptance boundary:

1. Add a provider-neutral Application binding that retains one exact `CharacterModelSettings`,
   `ICharacterModel`, and `ICharacterModelConnectionTester` supplied by a trusted composition root;
   do not construct Infrastructure adapters or credentials in Application.
2. Add an immutable copied snapshot of at most the three canonical provider bindings. Reject null,
   empty, duplicate, non-canonical, or settings/provider-mismatched entries without retaining a
   caller-owned mutable collection.
3. Select only by the exact canonical provider and return the exact registered binding. A missing
   provider must remain explicitly unavailable with no default, provider substitution, credential
   fallback, or model-name rewriting. Prove exact OpenAI/Anthropic/Ollama switching with fakes.
4. Do not wire the Godot scene, persist settings, access credentials or network, discover/start
   Ollama, execute generated intentions, add telemetry/export, or add platform credential stores.

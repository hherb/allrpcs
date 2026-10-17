# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Read first: the continuation process

[AGENTS.md](AGENTS.md) is binding. Before picking work, read `CONTINUATION.json` (persistent
ledger: closed scopes/workstreams, reopenings, the one `active_slice`), then `HANDOVER.md` and the
matching item in `ROADMAP.md`, and run `python3 scripts/check-continuation.py`. In short:

- Work only on the ledger's single active slice. Closed scopes stay closed unless the owner
  explicitly reopens them (record the reopening in the ledger first). Another audit/view/proof of an
  accepted, unchanged area is not progress.
- After ledger edits, run `python3 scripts/check-continuation.py --sync` (regenerates the
  `<!-- active-slice:start/end -->` block in `HANDOVER.md`), then the checker and `--self-test`.
  Never prune closed entries; the checker compares against `HEAD:CONTINUATION.json`.
- Never alter frozen art/audio evidence, acceptance hashes or historical validators to make a
  successor pass. Historical "pending"/"next slice" prose in packages is evidence, not a queue.
- The Godot editor rewrite of `game/project.godot` is committed at `a55e9e8d` (`EDITOR_REVISION` in
  `quality.py` pins historical snapshot overlays); `project.godot` may now change normally. Preserve
  `.DS_Store` and unrelated concurrent work under `speech-direction/`.
- Walking animation work must follow [art-direction/reference-gait-model.md](art-direction/reference-gait-model.md).
- `HANDOVER.md` records a standing owner grant to commit and push when a task is finished.

`README.md` is ~450 KB of accumulated boundary documentation; grep it by heading
(`## Prerequisites`, `## Local quality checks`, `## Project boundaries`, `## … model boundaries`)
rather than reading it whole.

## Toolchain

.NET 8 SDK (pinned by `global.json`), Godot **4.6.3 .NET** editor on `PATH` as `godot` (SDK
version pinned in `game/Allrpcs.Game.csproj`), `actionlint`, Python ≥ 3.12. Change these pins
together.

## Commands

```sh
dotnet restore Allrpcs.sln --disable-parallel -m:1 /nr:false
dotnet format Allrpcs.sln --no-restore --verify-no-changes   # CI rejects formatting drift
dotnet build Allrpcs.sln --no-restore -m:1 /nr:false          # warnings are errors

dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --no-build
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj --no-build
dotnet test tests/Architecture.Tests/Allrpcs.Architecture.Tests.csproj --no-build
# single test (xUnit):
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --filter "FullyQualifiedName~SomeTests.SomeMethod"

dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore

godot --path game                  # run the greybox (main scene res://Main.tscn)
godot --editor --path game
./scripts/smoke-godot.sh           # all headless Godot smoke launches
ALLRPCS_NAVIGATION_CHECK=1 godot --headless --path game --quit-after 120   # one smoke check
./scripts/export-development.sh linux-x64|macos-universal|windows-x64      # unsigned debug export
```

Full quality gate (required for product/CI changes; slow — hundreds of stages). Exception: under
the owner-approved code-health program (decision D2 in
`docs/superpowers/plans/2026-10-17-code-health-and-playable-slice.md`) the full gate runs once per
phase exit; between phase exits run `dotnet format`/`build`/`test` plus the affected smoke launches.
Commands:

```sh
python3 -m venv artifacts/quality-python
artifacts/quality-python/bin/python -m pip install --no-cache-dir -r requirements-quality.txt
./quality.sh                 # runs quality.py; logs/report in artifacts/quality-current/
./quality.sh --self-test     # orchestration regression tests only
```

Pillow must build from source against system zlib (`--no-binary=Pillow`); prebuilt wheels change
PNG bytes and break byte-exact art reviews. `quality.py` replays frozen validators at the pinned
historical revisions listed at its top (`ART_REVISION`, `AUDIO_REVISIONS`, `*_REVISION`) in git
snapshots, then runs current product checks. `scripts/quality.sh` is a frozen historical input —
do not edit it; the root `quality.sh`/`quality.py` is the live entry point. Only the diagnosed
native mutex shutdown abort may be retried, with the identical command. Prose-only changes may
reuse prior passing product evidence instead of rerunning the full gate.

CI (`.github/workflows/ci.yml`) runs the pure .NET steps above, the headless Godot smoke, and
unsigned exports on Linux/macOS/Windows, with no credentials or secrets.

## Architecture

Product: a slow, top-down 2D exploration/conversation/healing game. Every character is a role
with a controller that is human, LLM or dormant; the engine owns all truth (world state,
perception, inventory, discovery, disease, action outcomes) and LLMs only choose what their
characters *attempt*, via an invisible structured-intention protocol.

Project reference directions (enforced by `tests/Architecture.Tests/ProjectReferenceTests.cs`):

- `src/Domain` — authoritative rules and value types (identifiers, characters, knowledge,
  discovery, diseases, experiments, inventory, world objects/events). No dependencies.
- `src/Application` — pure, engine-independent use-case logic (movement, camera framing,
  navigation requests, dialogue, character turns, persistence ports, speech ports). Depends only
  on Domain.
- `src/Infrastructure` — JSON serialization, local-file stores, credential sources (macOS
  Keychain / Linux Secret Service), and LLM provider adapters in `Dialogue/` and `Characters/`:
  OpenAI and Anthropic (SSE) and Ollama (NDJSON), each split into request serializer, stream
  accumulator, completion, and a JSON-schema writer/deserializer per structured decision type.
  Provider selection is `Application/Dialogue/CharacterModelProvider.cs`. Depends on Application.
- `game/` — Godot C# presentation and composition root; depends on Application and
  Infrastructure. Godot owns input mapping, physics, collision, navigation topology, camera and
  scene authoring. Keep game rules out of here and keep Godot types out of `src/`.
- `tools/ScenarioValidator` validates the representative discovery-evidence network;
  `tools/SpeechServiceProbe` probes the local speech service.

`game/` is a flat directory (~1,080 tracked files; ~840 `.cs`, about 60% of those lines are checks). `Main.tscn`/`Main.cs` composes the greybox and wires
many `*Controller` nodes. Engine checks are `*SmokeCheck` / `*Check` nodes (C# or GDScript)
attached to `Main.tscn` or standalone `.tscn` proof scenes; each is inert unless its
`ALLRPCS_<NAME>_CHECK=1` environment variable is set, in which case it drives the real scene and
exits with a pass/fail code. Add new engine checks the same way and register the launch in
`scripts/smoke-godot.sh`. Classes named `Temporary*` are deliberate greybox scaffolding.

Speech: Python workers (`scripts/pocket-speech-worker.py` TTS, `scripts/player-speech-worker.py`
ASR) speak a bounded JSON-lines IPC protocol on stdout with the game; local configuration is
selected via `ALLRPCS_SPEECH_CONFIG` (see `speech-direction/dialogue-integration/README.md`).

Evidence directories (`art-direction/`, `audio-direction/`, `speech-direction/`,
`input-direction/`, `release-direction/`, `quality-review/`) hold per-slice packages: README,
JSON authority/closure/verification records, hash-bound proofs. Their `scripts/validate-*.py`,
`capture-*.gd` and `check-*.gd` companions are frozen once accepted.

## C# conventions (from `.editorconfig` / `Directory.Build.props`)

`TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`, nullable enabled, deterministic
builds. File-scoped namespaces matching folders (`Allrpcs.Domain.…`, game code in
`Allrpcs.Game`), braces on new lines, `System` usings first, 4-space indent (2 for
csproj/json/md/tscn), LF line endings. Tests use xUnit.

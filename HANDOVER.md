# Allrpcs Handover

## Current state

PR #138 merged Phases 2p10–2p12 into `main` at `8ed2efb`. The branch
`codex/phase-2p13-plus-save-capture` completes Phases 2p13–2p16:

- `SaveSnapshotCaptureController` captures fresh version-1 save compositions retaining the exact
  initialized discovery, event-log, and qualitative-time snapshot instances.
- `SaveSnapshotJsonSerializer` writes the complete save root as one stable compact JSON document.
- `SaveSnapshotJsonDeserializer` strictly materializes only the complete known version-1 shape,
  rejecting malformed, unexpected, duplicate, invalid, and future-version data.
- `SaveSnapshotJsonController` composes capture, serialization, deserialization, complete scenario
  preflight, and exact live restoration as an in-memory round trip.

The Godot composition root now references `Infrastructure`, and a dedicated Infrastructure test
project is included in the solution and local/hosted quality spines. No file, path, or storage
provider exists; save documents remain in memory.

After code review, the host `./scripts/quality.sh` passed workflow and formatting validation, a
zero-warning build, 1,087 Domain tests, 107 Application tests, 18 Infrastructure tests, seven
architecture tests, all 32 opt-in Godot smokes, and the bounded graphical launch. The focused
capture smoke first reproduced the known restricted Godot user-log denial; the identical host run
and the complete host quality run are authoritative.

## Exact next bounded slice

Take **Phase 2p17: define the storage-independent save-document port**.

Acceptance boundary:

1. Add one minimal Application-owned port for saving and loading the already-serialized complete
   save document without exposing filesystem paths, Godot types, or Infrastructure implementation
   details.
2. Define explicit missing-document and failed read/write results; do not use empty text or thrown
   provider exceptions as successful data.
3. Prove one exact successful document round trip plus missing, failure, replacement, and unchanged
   caller-input behavior with a deterministic in-memory test adapter. Do not add filesystem access
   yet.
4. Document only this port boundary, run `./scripts/quality.sh`, review intended paths, and prune this
   file again.

Explicit exclusions: no filesystem implementation, user paths, save slots, platform directories,
atomic file replacement, encryption, compression, cloud storage, migration, autosave, replay, final
save UI, startup/load orchestration, providers, or credentials. Do not merge, release, distribute,
sign, notarize, or change repository visibility without explicit authorization.

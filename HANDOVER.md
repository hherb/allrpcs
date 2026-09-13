# Allrpcs Handover

## Current state

`main` contains merged PR #217 at `aa55021`. The current branch is
`codex/phase-7-delivery-transcript`; publication of this branch is still pending.

This branch completes the bounded Phase 7 whisper/private-conversation roadmap item through seven
connected slices:

- Phase 7bd makes spoken transcript entries retain one exact `DialogueDelivery` while preserving
  the exact utterance projection and physical-action behavior.
- Phase 7be-7bf retain exact delivery modes on recent and current dialogue model context and
  serialize them into the shared trusted request context for every ordinary provider dialect.
- Phase 7bg introduces save schema version 4, strict canonical JSON, and migrations that make
  historically mode-free version 1-3 speech explicitly ordinary.
- Phase 7bh makes `CharacterTurnResponse` retain an optional exact delivery and preserves its
  utterance through the existing `Speech` projection.
- Phase 7bi requires provider response JSON to pair non-null speech with exactly one canonical
  `ordinary-speech` or `whispering` mode and null speech with null mode.
- Phase 7bj applies the exact response delivery to scene hearing and transcript occurrence. A
  credential-free smoke proves one explicit whisper reaches only the nearby Seeker.
- Version 4 persists exact heard and response delivery modes; legacy version JSON remains byte-shape
  compatible and migrates non-null speech to ordinary delivery.

The completed roadmap item is distance-governed conversation scope, not a secrecy guarantee. This
branch does not infer privacy or direct address from text; define explicit private addressee
authorization; inspect visibility, occlusion, obstacles, acoustic materials, or eavesdropping;
change the participation dialects; add independent tasks, party separation, off-screen simulation,
sabotage, or Phase 8 work; invoke live providers; dispatch workflows; release; sign; or distribute.

## Validation and review

- Focused Domain, Application, Infrastructure, persistence, provider-schema, transcript, save, and
  response-flow checks were driven through failing compile/test states before implementation.
- Exact delivery identity is retained across input/event/transcript and response/event/transcript;
  recorder deduplication is by delivery identity rather than shared utterance identity.
- Provider decoding rejects missing, duplicate, unknown, invalid, or speech/mode-mismatched fields.
  Shared schemas expose only the two canonical modes, and the corrected developer instruction lists
  every actual closed disposition value.
- Version-4 round trips preserve whispered heard and response speech. Version 3 serialization omits
  delivery metadata, and `DeserializeCurrent` reconstructs its historically implicit ordinary mode.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,118 Domain, 443 Application, 532 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions; the complete headless Godot smoke matrix; and Compatibility-renderer launch.
- Restricted VSTest reproduced the known localhost `SocketException (13)`, and restricted Godot
  reproduced the known `user://logs` crash. Identical checks passed host-local.
- No live-conformance environment variable was enabled, no provider was invoked, and no hosted
  workflow was dispatched.

## Exact next bounded slice

Take **Phase 7bk: bounded independent-task assignment contracts**.

Acceptance boundary:

1. Define a canonical `CharacterIndependentTaskId` and an immutable provider-neutral
   `CharacterIndependentTaskAssignment` that retains one exact task identifier, assigned
   `CharacterId`, and exact non-wait `CharacterIntention`.
2. Add a bounded immutable assignment snapshot that copies caller input, retains stable order and
   exact instances, rejects nulls and value-equal duplicate task identifiers, and exposes only a
   read-only collection.
3. Add focused Domain tests for canonical identifier parsing, supported examine/collect intentions,
   wait rejection, exact/value-equal identity behavior, bounds, mutation resistance, and closed
   public surfaces.
4. Do not yet add task lifecycle or outcomes, queues, scheduling, model context or provider output,
   navigation, scene controllers, party membership or separation, persistence, or new intention
   types.

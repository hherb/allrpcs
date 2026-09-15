# Allrpcs Handover

## Current state

Live GitHub state still reports draft PR #269 as open on
`codex/phase-7-misquotation-model-source`, despite the previous-session completion report. Branch
`codex/phase-7-misquotation-evidence-source` is therefore based directly on #269's exact head
`cfdea44` and advances Phase 7 through Phase 7iu:

- `TemporaryCharacterDialogueMisquotationEvidenceSource` retains one stable representative enemy,
  exact heard source event, exact already-composed delivered occurrence, exact non-actor witness,
  and exact observable evidence candidate;
- `TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController` now owns that source
  and creates a fresh request retaining its exact actor and candidate only on explicit `TryStart()`;
  overlap is rejected before a queued one-shot model is consumed; and
- `Main.tscn` owns the source, model source, task, and flow in dependency order. Ordinary startup is
  inert, while one switch-gated smoke proves exact selection, abstention, reset, overlap
  preservation, and fail-closed offline behavior.

No evidence is exposed, applied, presented, or acknowledged. No live dialogue is created or
delivered, and no source event, delivered event, occurrence, witness, candidate, transcript, truth,
character, world object, perception, or other authoritative state is mutated. No automatic task,
credential or provider I/O, persistence, input, retry, timing, scheduling, navigation, or movement
behavior was added. The unrelated untracked `scripts/code_statistics.py` remains untouched and must
stay unstaged.

## Validation and review

- Each slice began with its expected missing-source, missing-parameterless-flow, or missing-main-node
  failure.
- The isolated source, source-owned consideration flow, and switch-gated main-scene smokes pass.
- Sandboxed `./scripts/quality.sh` produced a zero-warning build, then VSTest was blocked by the known
  loopback `SocketException (13)`. The identical host-local command passes workflow validation,
  restore, formatting, a zero-warning build, 2,536 Domain tests, 542 Application tests, 897
  Infrastructure tests, 8 architecture tests, scenario validation, every credential-free headless
  Godot smoke, and the final Compatibility renderer launch.
- Session-wide review checked exact actor/event/occurrence/witness/candidate retention, all four
  existing policies, fresh request composition, one-shot precedence, overlap preservation, explicit
  reset, offline failure, main-scene dependency order, closed public surfaces, ordinary inert
  startup, file sizes, C# line lengths, and whitespace. No unresolved issue remains.

## Exact next bounded slice

Take **Phase 7iv: retain and append exact exposed misquotation evidence**.

Acceptance boundary:

1. Add one immutable `CharacterDialogueMisquotationEvidenceExposureSnapshot` that copies at most 64
   exact evidence entries into stable caller order and rejects nulls or duplicate exact delivered
   events before retaining anything.
2. Add one pure exposure transition that accepts the exact current snapshot and one validated
   `CharacterDialogueMisquotationEvidenceConsiderationResult`: append its exact selected evidence
   into a fresh snapshot, or return the exact current snapshot for abstention.
3. Reject an already-exposed exact delivered event and overflow, preserve all existing exact entries
   and every nested source/delivered event, and allow a distinct value-equal delivered event under
   the existing exact-reference rule.
4. Do not add a scene controller or `Main.tscn` wiring; change the current source, task, model, or
   flow; present or acknowledge evidence; mutate dialogue, transcripts, truth, characters, world
   state, or perception; or add provider, persistence, input, retry, timing, scheduling, navigation,
   or movement behavior.

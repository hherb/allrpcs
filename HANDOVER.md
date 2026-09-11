# Allrpcs Handover

## Current state

`main` contains merged PR #195 at `6bbe133`. The current branch is
`codex/phase-6-hypothesis-serialization`.

Phase 6 now has a strict caller-composed hypothesis-recognition transport boundary, a universal
experiment journal, reusable prepared-batch accounting, and one atomic ingredient-administration
execution path over those contracts.

`SerializedInvestigationHypothesisInterpreter` accepts only a narrow supplied transport. Its
deterministic JSON sends the exact utterance text marked `untrusted-in-world-text`, open-question
identities and statements, and hypothesis identities, question identities, and statements. It
accepts only explicit `no-match` or one strict question/hypothesis identity pair. Malformed, extra,
duplicate, oversized, unknown-shape, partial, or noncanonical responses fail without retaining
provider text. Well-formed unknown identities reach the existing Domain canonicalization policy and
fail closed. Construction and the standard suite perform no network or credential I/O. There is no
provider binding, retry, scene, UI, or persistence wiring.

The universal experiment journal accepts any exact authored world event rather than only ingredient
administration. A record retains an exact experiment identity, exact event, bounded procedure, and
bounded observed qualitative outcome without interpreting efficacy. The immutable journal holds up
to 256 stable-order records and rejects duplicate experiment or event identities. Recording requires
the exact event in authored state and its exact identity as the newest authoritative occurrence.

A reusable prepared batch begins with one exact completed preparation and its full exact quantity.
Pure consumption permits only a dosage from that exact preparation and unit within the remaining
amount. Partial use returns a new reduced batch; complete use removes it. A bounded prepared-batch
snapshot retains up to 256 distinct exact preparation instances and replaces or removes only the
consumed batch while preserving unrelated exact instances and stable order.

`IngredientAdministrationExperimentExecutionTransition` preflights prepared-batch consumption, the
latest exact authored administration event, and the universal journal before returning new prepared
batch state plus both the rich administration-specific journal and universal journal. Any rejection
exposes no partial state. It does not occur the event, change disease truth, or execute an
investigation consequence.

The roadmap's universal experiment-recording and natural-language hypothesis-recognition items are
now complete. Provider-specific hypothesis composition, Phase 6 scene ownership and persistence,
first-patient use of prepared batches and the universal journal, consequence execution, and final
authored UI remain open.

## Validation and review

- complete formatting verification and zero-warning solution build passed
- 1,909 Domain, 402 Application, 440 Infrastructure, and 8 architecture tests passed
- representative discovery scenario validated 5 evidence definitions
- all 77 credential-free Godot smokes passed
- bounded 60-frame Compatibility-renderer launch passed on Godot 4.6.3 .NET
- focused slice checks passed: 20 new Domain tests and 17 new Infrastructure tests
- the staged diff passed whitespace, 120-character, exact-identity, immutability, redaction,
  no-partial-state, and minimal-surface review

The first aggregate `./scripts/quality.sh` run passed workflow validation, restore, formatting,
zero-warning build, and every .NET suite, then transiently stalled while launching the already-built
scenario validator. The exact validator retry and remaining Godot checks passed, then one clean
aggregate retry passed end to end without a product change. Restricted VSTest focused runs reproduced
the known local-loopback denial; the identical host-local commands passed.

## Exact next bounded slice

Take **Phase 6y1: atomic first-patient prepared-dose and universal-journal integration**.

Acceptance boundary:

1. Extend the first-patient stabilization result and transition so successful treatment returns the
   resulting prepared-batch snapshot and universal experiment journal alongside disease truth and
   the existing administration-specific journal.
2. Require the administration dosage's exact preparation to be retained with enough remaining
   quantity, and reuse `IngredientAdministrationExperimentExecutionTransition` for consumption and
   both journal records rather than duplicating those rules.
3. Preserve the current exact patient, disease, remedy, confirmed-conclusion, recipe, latest-event,
   stabilization, and experiment constraints. Add bounded authored procedure and observed-outcome
   inputs without deriving efficacy from their text.
4. Prove the successful representative path stabilizes disease truth, consumes exactly one dose,
   and records both exact journal forms atomically. Prove depleted, replacement, duplicate, full,
   stale-event, wrong-conclusion, and treatment-ineligible paths leave every input unchanged.
5. Add focused reflection-backed tests and update README/HANDOVER evidence. Do not yet occur the
   administration event, add provider-specific hypothesis composition, execute consequences, wire
   scene/UI ownership or persistence, begin Phase 7, release, or distribute.

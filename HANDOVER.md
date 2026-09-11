# Allrpcs Handover

## Current state

`main` contains merged PR #192 at `3ef69e8`. The current branch is
`codex/phase-6-preparation-recipes`.

Phase 6 now includes the preparation, dosage, administration, experiment-journal, and investigation
foundations after the earlier disease, protection, assessment, and care work.

Canonical preparation recipes retain an exact plan, one to 16 ordered exact input requirements with
unique ingredient identities, and one exact result definition and quantity. Ingredient-state
snapshots are bounded, copied, ordered, and duplicate-free. Availability requires exact definitions,
exact units, and sufficient amounts; consumption, start, exact-step progression, and completion are
pure immutable transitions. A completed activity yields only its exact recipe-authored result.

Dosage is positive, uses the exact prepared unit, and cannot exceed the result amount. Administration
retains exact administrator and patient identities, the exact dosage, and an authored world event;
it does not yet validate occurrence or consume a reusable dose. Experiment records retain an exact
administration and exact qualitative observation. The bounded journal append transition rejects
duplicate experiment identities, but recording is not yet atomically coupled to authoritative event
occurrence.

Investigation questions, hypotheses, evidence links, conclusions, and non-executable consequences
are separate immutable contracts. Conclusion plans retain one to eight distinct alternate evidence
routes of one to 16 evidence identities each. Pure confirmation uses the existing character-filtered
evidence view, requires one complete route without pooling partial routes, then atomically removes the
exact open question and appends the exact authored conclusion. It does not parse natural language,
promote a hypothesis directly, reveal hidden evidence, or execute a consequence.

There are still no recipe tools, duration or temperature values, automatic quality rules, prepared
result storage, reusable-dose accounting, administration eligibility, disease-treatment effects,
scene ownership, persistence, provider wiring, natural-language hypothesis recognition, consequence
execution, authored first-patient scenario, or recovery-path acceptance. The roadmap's universal
experiment-recording item remains unchecked.

## Validation and review

- complete host-local `./scripts/quality.sh` passed: workflow validation, restore, formatting, and
  zero-warning solution build
- 1,817 Domain, 393 Application, 423 Infrastructure, and 8 architecture tests passed
- representative discovery scenario validated 5 evidence definitions
- all 77 credential-free Godot smokes passed
- bounded 60-frame Compatibility-renderer launch passed on Godot 4.6.3 .NET
- focused test-first checks covered every slice; the staged source/test diff passed whitespace and
  120-character review before the full quality run

The first sandboxed focused VSTest command reproduced the known local-IPC restriction; the identical
host-local command passed. Git staging likewise required the already-known host retry for
`.git/index.lock`. Neither restriction required a product change.

## Exact next bounded slice

Take **Phase 6k1: authoritative ingredient-administration experiment recording**.

Acceptance boundary:

1. Add one pure recording policy requiring the administration outcome's exact `WorldEvent` to exist
   in the authoritative `WorldEventSnapshot` and its exact event identity to be the most recently
   recorded entry in `WorldEventLogSnapshot`.
2. Add one pure atomic transition that accepts an exact `ExperimentId`, administration outcome,
   qualitative observation, authoritative event inputs, and current experiment journal; on success,
   construct the exact record and append it once through the existing journal transition.
3. Reject nulls, an absent or value-equal replacement event, a stale or empty event log, duplicate
   experiment identity, and journal capacity without changing any input. Preserve exact instances and
   stable record order.
4. Add focused reflection-backed tests for the policy and transition's minimal public surfaces,
   eligibility boundaries, exact-instance retention, atomic rejection, and unchanged inputs.
5. Do not yet mutate disease truth, infer efficacy, consume dosage, execute consequences, wire scene
   ownership or persistence, recognize natural-language hypotheses, author the first patient, or mark
   universal experiment recording complete.

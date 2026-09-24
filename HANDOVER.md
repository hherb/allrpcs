# Allrpcs Handover

## Current state

`main` includes merged PR #349 at `f5984e3`. Branch
`codex/phase-7-continuation-turn-boundary` continues the exact fourth-cycle Move chain after its
completed-turn admission and implements **Phase 7sj–7sm**.

The admitted continuation completion now:

- binds once to the exact current ordinary-turn reference only while it remains empty and startable;
- retains one fresh scheduling authorization over only that exact completion and turn reference;
- enters one explicit successor boundary only from that exact still-current authorization; and
- passes the existing pure cycle-start gate into one exact successor authoring admission.

Null, rebuilt, malformed, changed-slot, active-slot, stale, and repeated inputs remain inert. The
ordinary turn remains empty and unbound. No fifth Move is authored or started, no slot is reserved,
and no model, provider, credential, persistence, UI/input, navigation, perception, dialogue, timer,
or new Domain behavior was added. Ordinary startup remains inert. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Each of the four seams was introduced test-first with the focused game build failing for the
  deliberately absent coordinator before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact boundary binding, authorization, successor entry, and authoring admission while
  authoritative placement/evidence and all five model-call counts remain unchanged.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Complete-diff review found no correctness, exact-reference, scene-ordering, public-surface,
  formatting, line-length, or documentation issues. Whitespace validation is clean.

## Exact next bounded slice

Guard one fresh fifth-cycle Move author invocation with only the exact successor authoring
admission, without composing or starting consideration.

Acceptance boundary:

1. Test-first add one distinct Move authoring coordinator over the exact current successor
   admission and one caller-supplied fifth eligible Move candidate.
2. Require the admission's complete boundary, authorization, admitted completion, empty-turn, and
   current continuation-recovery graph to remain exact and current.
3. Require the fifth candidate's evidence identifier to be distinct from every earlier Move-cycle
   evidence identifier used by the focused proof.
4. Invoke a fresh `CharacterSabotageActionMoveRequestController` once and retain only the exact
   admission and authored request/candidate binding.
5. Reject null, rebuilt, malformed, foreign-actor, wrong-kind, stale-graph, replaced-authority,
   duplicate-evidence, and repeated inputs without changing any owner or consuming a model.
6. Do not compose or start a generic request, start typed work, reserve the turn, clear prior
   provenance, add cadence, or alter placement/evidence authority.
7. Extend only the focused switch-gated Move proof. Keep ordinary startup inert and every existing
   model call count at one.
8. Preserve all existing exclusions: no timers, provider transport, credentials, persistence,
   UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
9. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to compose one fresh generic Move request from only that exact guarded author output.

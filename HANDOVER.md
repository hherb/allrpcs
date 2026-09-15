# Allrpcs Handover

## Current state

`main` is verified at merged PR #258 (`c2321cf`). Branch
`codex/phase-7-contamination-selection` completes four bounded contamination slices:

- `CharacterObjectSabotageContaminationSelection` fully materializes and validates at most 64
  value-unique evidence candidates before returning the exact first currently eligible instance in
  stable caller order;
- `CharacterObjectSabotageContaminationSequence` applies that exact selection through the existing
  atomic contamination transition and fails without changing inputs when no candidate is eligible;
- `CharacterObjectSabotageContaminationConsiderationRequest` retains the exact character and world
  snapshots plus a copied read-only bounded candidate view; and
- `CharacterObjectSabotageContaminationDecision` represents one exact selection or explicit
  abstention, while its pure policy requires the exact request character and exact candidate
  evidence identifier and rechecks current contamination eligibility.

Implementation commit `1e8274a` is published in draft PR #259. Hosted run `34943602414` failed
before executing any steps in all five credential-free jobs (`steps: []`), so it is Actions/account
infrastructure state rather than product-test evidence and was not retried.

No provider was invoked. The work adds no accepted-result binding, consideration application,
controller, task, model/provider path, scene ownership, presentation, persistence, input, UI,
contamination contents or effects, ingredient-state coupling, timing, scheduling, events, time,
transcripts, navigation, or actor movement. Authoritative world objects and every supplied object,
candidate, occurrence, evidence, snapshot, and candidate collection remain unchanged. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with expected missing selection, sequence, request, decision, and decision-policy
  contract failures before each implementation.
- Focused host-local tests pass 6 selection, 4 sequence, 5 request, and 6 decision/policy cases.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,493 Domain tests, 524 Application tests, 757 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every changed authored C# file is below 500 lines with no lines longer than 120 characters.
  Complete changed-file review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hm: bind and apply one exact accepted contamination decision**.

Acceptance boundary:

1. Add one immutable `CharacterObjectSabotageContaminationConsiderationResult` that retains the
   exact request and accepted decision and exposes the exact selected request candidate, or `null`
   for abstention.
2. Construct it only when
   `CharacterObjectSabotageContaminationDecisionPolicy.CanAccept(request, decision)` is true. Reject
   nulls, value-equal replacement character/evidence identifiers, unknown selections, and candidates
   no longer eligible under the request snapshots without changing any input.
3. Add one pure `CharacterObjectSabotageContaminationConsiderationTransition.Apply` operation that
   applies an exact selection through `CharacterObjectSabotageContaminationTransition` and returns
   `null` for an accepted abstention. Prove exact-instance retention, fresh atomic result state,
   unchanged request inputs, null rejection, and closed immutable/static surfaces.
4. Do not add a controller, task, model/provider seam, decoder, transport, binding, scene ownership,
   presentation, acknowledgement, persistence, input, UI, contamination effect, ingredient coupling,
   timing, scheduling, events, time, transcripts, navigation, or actor movement.

# Allrpcs Handover

## Current state

`main` includes merged PR #259 at `e0bae01`. Branch
`codex/phase-7-contamination-application` completes Phase 7hm through Phase 7hq:

- `CharacterObjectSabotageContaminationConsiderationResult` binds only an accepted exact request
  and decision, retaining the exact selected request candidate or `null` for abstention;
- `CharacterObjectSabotageContaminationConsiderationTransition` applies an exact selection through
  the existing atomic transition and leaves an accepted abstention inert;
- `ICharacterObjectSabotageContaminationDecisionModel` and its Application controller obtain and
  validate one exact cancellation-aware provider-neutral decision;
- `CharacterObjectSabotageContaminationController` owns one exact scene contamination/evidence pair
  and rejects stale, repeated, uninitialized, null, or ineligible apply, recovery, and consideration
  operations without partial installation; and
- `CharacterObjectSabotageContaminationTaskController` owns one bounded decision task, prevents
  overlap, resolves accepted results on the Godot main thread, and closes cancellation, failure,
  stale-result, teardown, and reset behavior.

Implementation commit `7b68e40` is published in draft PR #260. Hosted run `34951601197` failed
before executing any steps in all five credential-free jobs (`steps: []`), so it is Actions/account
infrastructure state rather than product-test evidence and was not retried.

No provider was invoked. The work adds no decoder, serialization, schema, HTTP/SSE transport,
credential or configured binding, model implementation/source, main-scene composition,
presentation, acknowledgement, persistence, input, UI, contamination contents or effects,
ingredient-state coupling, scheduling, events, time, transcripts, navigation, or actor movement.
Authoritative world objects and every supplied object, candidate, occurrence, evidence, snapshot,
request, and collection remain unchanged. The unrelated untracked `scripts/code_statistics.py`
remains untouched and must stay unstaged.

## Validation and review

- Tests began with expected missing result/transition, model/controller, scene-controller, and
  task-controller contract failures before each implementation.
- Focused host-local tests pass 9 Domain result/transition and 6 Application model/controller cases.
- Focused host-local Godot smokes pass exact scene lifecycle/consideration resolution and bounded
  task selection, cancellation, failure, stale-result rejection, and reset.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,502 Domain tests, 530 Application tests, 757 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every changed authored C# file is below 500 lines with no lines longer than 120 characters.
  Complete staged-diff review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hr: strictly decode one exact contamination decision**.

Acceptance boundary:

1. Add one static `CharacterObjectSabotageContaminationDecisionJsonDeserializer` with a 4,096
   UTF-16-code-unit document bound and maximum JSON depth 4.
2. Accept only one complete comment-free object containing exactly one case-sensitive
   `characterId` string and one `selectedEvidenceId` string-or-null. Reject trailing content,
   duplicate, missing, unknown, or extra properties, invalid types/identifiers, over-depth values,
   unknown candidates, and currently ineligible selections behind one bounded `ArgumentException`
   for `serialized`.
3. Resolve value-equal parsed identifiers back to the exact request character and candidate
   evidence identifier before constructing the decision, then require the existing decision policy
   to accept it. Preserve the exact request and every nested input unchanged.
4. Do not add a serializer/schema, SSE/HTTP transport, model implementation, credentials, binding,
   retry/timeout policy, source, main-scene composition, presentation, acknowledgement, persistence,
   input, UI, contamination effect, ingredient coupling, scheduling, events, time, transcripts,
   navigation, or actor movement.

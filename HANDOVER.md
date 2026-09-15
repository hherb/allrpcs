# Allrpcs Handover

## Current state

`main` includes merged PR #260 at `35e5259`. Branch
`codex/phase-7-contamination-decision-decoding` completes Phase 7hr:

- `CharacterObjectSabotageContaminationDecisionJsonDeserializer` accepts one complete,
  comment-free, maximum-4,096-UTF-16-code-unit JSON object with exactly case-sensitive
  `characterId` and `selectedEvidenceId` properties;
- it rejects trailing content, duplicates, missing or extra properties, invalid shapes,
  identifiers, unknown candidates, over-depth JSON, and currently ineligible selections through
  one bounded `ArgumentException` for `serialized`; and
- value-equal parsed identifiers are resolved to the exact request character and candidate
  evidence instances before the existing acceptance policy is applied.

No serializer/schema, SSE or HTTP transport, model implementation, credentials, binding,
retry/timeout policy, source, main-scene composition, presentation, acknowledgement,
persistence, input, UI, contamination effects, ingredient coupling, scheduling, events, time,
transcripts, navigation, or actor movement was added. No provider was invoked. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Focused host-local decoder tests pass: 22 tests.
- Host-local `./scripts/quality.sh` passes with a zero-warning build, 2,502 Domain, 530
  Application, 779 Infrastructure, and 8 architecture tests, scenario validation, and the
  credential-free Godot smoke suite.
- Review confirms both authored C# files are below 500 lines and have no lines over 120
  characters; `git diff --check` is clean. No unresolved findings remain.

## Exact next bounded slice

Take **Phase 7hs: accumulate one strict completed contamination decision stream**.

Acceptance boundary:

1. Add one stateful, bounded OpenAI Responses-style SSE accumulator that accepts only the exact
   current contamination request and invokes the Phase 7hr deserializer only after one complete
   terminal JSON decision document.
2. Fail closed on malformed, duplicate, out-of-order, partial, trailing, oversized, cancelled,
   or failed stream state; expose no partial decision and retain no mutable request inputs.
3. Do not add HTTP sending, model composition, credentials, settings/binding, retry/timeout,
   scene composition, presentation, acknowledgement, persistence, input, UI, contamination
   effects, ingredient coupling, scheduling, events, time, transcripts, navigation, or movement.

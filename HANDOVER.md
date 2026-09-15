# Allrpcs Handover

## Current state

`main` includes merged PR #261 at `ab43fa2`. This branch completes Phase 7hs–7ht:

- `OpenAiCharacterObjectSabotageContaminationStreamAccumulator` binds one exact contamination
  consideration request to one bounded Responses-style semantic-event lifecycle. It requires
  `response.created`, then `response.in_progress`, then one bounded terminal output-text JSON
  document and `response.completed`; malformed, duplicate, out-of-order, partial, failed,
  oversized, and trailing states fail closed without exposing a decision.
- `OpenAiCharacterObjectSabotageContaminationSseCompletion` consumes only complete framed SSE
  documents through the existing reader and returns an exact accepted decision only after the
  completed terminal sequence. It preserves caller cancellation and does not perform transport.

No HTTP sending, model implementation or composition, credentials, settings/binding,
retry/timeout policy, scene composition, presentation, acknowledgement, persistence, input, UI,
contamination effects, ingredient coupling, scheduling, events, time, transcripts, navigation, or
movement was added. No provider was invoked. The unrelated untracked `scripts/code_statistics.py`
remains untouched and must stay unstaged.

## Validation and review

- Host-local focused contamination infrastructure tests pass: 36 accumulator/deserializer tests
  and 25 contamination SSE tests.
- Review confirms the accumulator retains only the exact request and completed exact decision,
  bounds every serialized event/output document, and has no provider text or identifier output
  surface. Authored files are below 500 lines and lines are within 120 characters.

## Exact next bounded slice

Take **Phase 7hu: serialize one exact contamination consideration request and closed decision
schema**.

Acceptance boundary:

1. Add pure, bounded request serialization and a closed JSON-schema writer for the exact current
   contamination request, retaining no filesystem paths, credentials, provider identifiers, or
   mutable caller collections.
2. Reject invalid/null/beyond-bound request inputs before producing transport text; prove exact
   stable candidate ordering and the explicit nullable-selection schema with focused tests.
3. Do not add HTTP sending, model composition, credentials, settings/binding, retry/timeout,
   SSE changes, scene composition, presentation, acknowledgement, persistence, input, UI,
   contamination effects, ingredient coupling, scheduling, events, time, transcripts,
   navigation, or movement.

# Review

Reviewed the changed source, fixture and provider request paths against the owner
request and the existing hearing, turn, action and save owners.

- History is bounded at retention and request selection; filtering precedes the
  character request budget. The current event is omitted by reference, while
  separately repeated words remain distinct occurrences.
- Speaker identity selects native assistant/user history roles. Exact historical
  text occurs once on the wire; indexed context metadata retains identity and
  delivery mode. Both existing provider serializers share this implementation.
- Historical dialogue remains untrusted in-world speech. Engine action outcomes
  have their own context kind and cannot be inferred from a model's promise.
- A result must belong to the responding actor and retain its exact physical
  entry in the current transcript. No pending/failed/cancelled action is promoted
  into a completed result. Existing herb execution bytes remain unchanged.
- Required state, action outcomes, affordances and the current utterance are
  reserved before older speech is fitted to the overall context limit.
- History clears only after successful complete restoration. The exact pending
  pre-load request is fenced at response reception; it cannot create stale speech.
  Rejected loads preserve context. This does not change the existing generation
  deadline or add persistent dialogue storage.
- The existing source fixture's duplicate-current expectation and immutable
  entry reflection contract were updated to the intended new API. Their privacy,
  actor, state and immutability checks remain. Frozen validators and evidence were
  not edited.

The five-submission local trial supplies concrete positive evidence for two
context-dependent name replies and an examination follow-up. Earlier exploratory
clarifications and fixture-assumption failures remain visible in diagnostics.
Participation autonomy remains unchanged; characters can still choose silence or
clarification. This is no general claim about model accuracy, physical ASR/TTS,
echo safety, another platform, save-persistent conversations or packaged releases.

No unresolved implementation finding in this bounded repair. Final gate and
source/evidence bindings are recorded in `verification.json` before closure.

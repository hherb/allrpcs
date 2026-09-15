# Allrpcs Handover

## Current state

`main` includes merged PR #266 (`dc6d9ef`). Branch
`codex/phase-7-misquotation-consideration-result` advances Phase 7 through Phase 7ij:

- an immutable `CharacterDialogueMisquotationEvidenceConsiderationResult` retains the exact request
  and accepted decision, resolving the exact selected request evidence or null for abstention;
- null, wrong-actor, replacement-candidate, and currently ineligible decisions fail before any
  result is retained; and
- one provider-neutral cancellation-aware model seam and Application controller forward the exact
  request and caller token once, then return only the existing validated result.

No evidence is exposed or applied, and no dialogue is created or delivered. The work adds no JSON
decoder, provider request, SSE, adapter, binding, retry, deadline, fallback, task, scene,
persistence, transcript, truth, character, audience, event, or world-state mutation. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing result, model-interface, and controller compile failures.
- All 5 focused new Domain tests and 6 focused new Application tests pass host-locally.
- The sandbox quality run completed restore and a zero-warning build before VSTest hit its known
  blocked loopback socket; the identical host-local `./scripts/quality.sh` passed without product
  changes.
- Full validation passes workflow validation, restore, formatting, a zero-warning build, 2,536
  Domain tests, 539 Application tests, 826 Infrastructure tests, 8 architecture tests, scenario
  validation, every credential-free headless Godot smoke, and the final Compatibility renderer
  launch.
- Every new C# file is below 500 lines with no lines longer than 120 characters.
- Session-wide review confirmed exact request/decision/evidence retention, fail-closed current
  eligibility, one exact model call, pre-cancellation, unchanged model cancellation/failure
  propagation, immutable inputs, and the absence of provider or mutation authority. Review also
  strengthened the model-cancellation test so controller pre-cancellation cannot satisfy it.

## Exact next bounded slice

Take **Phase 7ik: strictly decode one exact misquotation-evidence decision**.

Acceptance boundary:

1. Add one public static Infrastructure JSON deserializer with a 4,096 UTF-16-code-unit transport
   bound. Accept exactly `actorId` and nullable integer `selectedEvidenceIndex`; reject missing,
   unknown, duplicate, trailing, commented, malformed, over-depth, wrong-type, and excessive input.
2. Resolve the value-matching actor text to the request's exact actor identifier and an in-range
   zero-based selection index to the request's exact candidate; null means abstention. Reject
   invalid actor text, negative or out-of-range indices, and every decision the existing Domain
   acceptance policy rejects.
3. Prove exact-reference rebinding, selection and abstention, strict closed structure, one bounded
   public error shape, immutable request/candidates/events/audiences, and a closed public API.
4. Do not add a schema writer or provider request, SSE, adapter, model binding, retry, deadline,
   fallback, task, scene, persistence, dialogue delivery, evidence application, transcript/truth
   mutation, delay, or influence behavior.

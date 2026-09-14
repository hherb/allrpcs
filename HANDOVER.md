# Allrpcs Handover

## Current state

`main` contains merged PR #233 at `710f1ff`. The current branch is
`codex/phase-7-sabotage-provider-flow` and advances four bounded Phase 7 slices:

- Phase 7dy strictly decodes one complete concealment decision JSON document against its exact
  consideration request, resolving valid parsed identifier text to request-owned instances before
  delegating the existing exact acceptance policy;
- Phase 7dz serializes only relevant authored character context and its exact eligible candidates
  into one stateless strict-schema OpenAI Responses request;
- Phase 7ea completes one bounded OpenAI Responses SSE lifecycle without exposing a partial
  decision or retaining provider text and identifiers; and
- Phase 7eb exposes one caller-composed authenticated OpenAI adapter behind the existing
  provider-neutral concealment-decision interface.

No model binding or scene composition was added. Construction performs no credential or network
input/output. Nothing invokes a provider at startup, starts sabotage automatically, chooses timing,
persists concealment, adds UI/input/events/time/transcripts/movement, or adds another sabotage
effect.

Draft PR #234 is open from implementation commit `1de6f05`. Hosted run `34794615126` completed
with all five jobs at `steps: []`; it is Actions/account infrastructure state, not product-test or
unsigned-build evidence, and should not be retried unchanged.

## Validation and review

- Every slice began with the expected compile failure for its absent public operation or type.
- Focused Infrastructure tests pass strict selection and abstention decoding, distinct parsed-ID
  resolution, complete shape/value/depth/length rejection, bounded trusted-context and exact-schema
  serialization, complete SSE framing and terminal-state rules, authenticated request construction,
  exact custom endpoints, cancellation, redaction, and transport/content-type failures.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 484 Application tests, 602 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters. No
  live-conformance switch was enabled and no provider was invoked.
- Hosted run `34794615126` executed no job steps; host-local quality is the current product
  evidence.

## Exact next bounded slice

Take **Phase 7ec: bind one exact OpenAI concealment model through canonical provider settings**.

Acceptance boundary:

1. Add one Application binding that retains the exact `CharacterModelSettings` and exact
   `ICharacterObjectSabotageConcealmentDecisionModel`, requiring the OpenAI provider and rejecting
   nulls.
2. Add one Infrastructure factory accepting caller-owned `HttpClient`, `IOpenAiCredentialSource`,
   exact OpenAI settings, and an optional validated `OpenAiEndpoint`, returning that exact binding
   around `OpenAiCharacterObjectSabotageConcealmentModel` without credential, network, discovery,
   or scene input/output during construction.
3. Prove exact settings retention, default and custom endpoints on later explicit invocation,
   wrong-provider/null rejection before construction, no credential load or HTTP send during
   construction, and the smallest static public factory surface.
4. Do not add settings selection, scene composition or replacement, startup invocation, timing or
   automatic triggers, persistence, UI/events/time/transcripts, movement, another provider, or
   another sabotage effect.

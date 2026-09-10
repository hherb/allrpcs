# Allrpcs Handover

## Current state

`main` contains merged PR #178 at `0f3c9d3`. The current branch is
`codex/phase-4-ollama-live-conformance` and advances Phase 4 through Phase 4r7.

- Phase 4r5 gives Ollama the existing strict three-request wait and known-target action
  conformance series. Both reuse the provider-neutral evaluator and bounded count-only summary,
  require an explicitly named already-installed model, and remain separate from the permissive
  Ollama safety probe.
- Phase 4r6 adds one explicitly enabled ordinary-scene known-target live harness for either Ollama
  or an OpenAI-compatible endpoint. It requires a completed locally validated response before the
  existing exact examination and deterministic safe-collection chain can occur, while world objects
  and perception remain unchanged.
- Phase 4r7 adds the equivalent ordinary-scene target-free `Wait` harness. It requires bounded
  healer speech and proves there is no physical action or event, time, world-object, or perception
  mutation.

The ordinary harness permits exactly one provider/mode switch, records only the exact
provider-neutral request for containment checks, and emits fixed content-free pass/fail output. It
does not discover or start a server, discover/select/pull/change a model, persist an endpoint, read
an OS credential, or run during standard validation. No live path was run in this branch because no
endpoint/model was explicitly selected for this session.

No response-schema widening, provider-specific parsing, model/server process ownership, API-base
persistence/UI, automatic live recomposition, Anthropic credential store, arbitrary generated
action execution, or Phase 5 memory was added. Native Linux/Windows credential behavior remains
unproved, and full Phase 4 acceptance is incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 405 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 76 standard Godot smoke invocations, including the credential-free generated distinct-target
  examination and safe-collection flow; and
- the bounded graphical Compatibility-renderer launch.

The two new Ollama live facts are included in the 405 Infrastructure tests and return before model
construction when their explicit switches are absent. The new ordinary live scene node likewise
returns before client/model construction when all four live switches are absent. Focused validation
also passed all seven Ollama-live/evaluator tests and the generated examination smoke. The first
sandboxed VSTest and Godot attempts hit the known local-socket/user-log restrictions; the identical
host commands passed. GitHub Actions budget is exhausted for the month: do not dispatch hosted
workflows, and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4r8: explicitly selected live-provider acceptance evidence**.

Acceptance boundary:

1. After the user explicitly identifies one already-running endpoint and model, run that provider's
   fixed three-request wait and action conformance series, then its ordinary-scene wait and action
   harnesses in separate fresh processes. Do not discover, download, start, stop, or change a server
   or model.
2. Require both conformance aggregates to report three completed outcomes and both ordinary flows
   to pass before marking Phase 4 live acceptance complete. A reachable but non-conforming model is
   negative safety evidence only.
3. Retain/report only bounded aggregates and fixed pass/fail text. Do not retain or report model
   speech, prompts, model/server metadata, credentials, endpoint details, or raw failures.
4. Do not widen the schema/parser, add provider-specific accepted variants, persist an endpoint, add
   endpoint UI, alter startup composition, add credential facilities, execute any generated action
   beyond the existing herb-patch examination flow, claim native platform support, or add Phase 5
   memory.

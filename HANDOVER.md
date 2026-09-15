# Allrpcs Handover

## Current state

`main` includes merged PR #265 (`27ce1fc`). Branch
`codex/phase-7-misquotation-evidence-selection` advances Phase 7 through Phase 7ih in commit
`27fa127`. Draft PR #266 is open for this branch:

- one pure bounded selector fully preflights caller-ordered misquotation-evidence candidates before
  eligibility checks, rejects nulls, over-bound collections, and duplicate exact delivered events,
  then returns the exact first candidate observable by the supplied actor or null;
- distinct value-equal delivered events remain separate exact candidates;
- one immutable provider-neutral request retains the exact actor and a copied read-only candidate
  view under the same structural rules; and
- one immutable selection-or-abstention decision is accepted only for the request's exact actor and
  exact still-observable candidate.

No provider was invoked. The work does not expose or apply evidence, construct a consideration
result, create or deliver dialogue, select text, append or rewrite transcripts, judge truth,
automatically correct a claim, mutate characters or world state, or add scene, persistence, model,
retry, fallback, delay, influence, time, navigation, or movement behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing selection, request, decision, and policy compile failures.
- All 17 focused new Domain tests pass. The initial sandbox quality run reached a zero-warning build
  before VSTest hit the known blocked loopback socket; the identical host-local run passed without
  product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,531 Domain tests, 533 Application tests, 826 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch.
- Every new C# file is below 500 lines with no lines longer than 120 characters.
- Session-wide review confirmed full collection preflight, reference-identity event deduplication,
  stable exact retention, copied request ownership, exact decision binding, and fail-closed current
  eligibility without mutation.
- Draft PR #266's credential-free workflow run `34968258214` failed all five jobs before any steps
  began (`steps: []`). This is Actions/account infrastructure, not product-test evidence, and the
  unchanged jobs were not retried.

## Exact next bounded slice

Take **Phase 7ii: bind one accepted exact misquotation-evidence consideration result**.

Acceptance boundary:

1. Add one immutable result retaining the exact
   `CharacterDialogueMisquotationEvidenceConsiderationRequest` and exact accepted
   `CharacterDialogueMisquotationEvidenceDecision`.
2. Resolve `SelectedEvidence` to the same exact request candidate for a selection and null for an
   abstention; reject null, wrong-actor, replacement-candidate, and currently ineligible decisions
   before retaining anything.
3. Prove exact-reference retention, selection and abstention shapes, rejection behavior, and
   unchanged proposals, source events, delivered events, audiences, characters, and caller-owned
   collections.
4. Do not expose or apply evidence, add a transition or task, create or deliver dialogue, invoke a
   model, mutate transcripts or truth, add scene or persistence work, or begin delay or influence
   behavior.

# Allrpcs Handover

## Current state

PR #139 merged Phases 2p13-2p16 into `main` at `2c99655`. The branch
`codex/phase-2p17-plus-save-storage` completes Phases 2p17-2p20:

- Application owns a path-free single-save-document port with explicit saved, loaded, missing,
  and failed outcomes.
- Infrastructure stores strict UTF-8 through a flushed same-directory temporary file and atomic
  replacement at one trusted absolute path.
- The Godot composition root exposes manual save/load operations at `user://save-v1.json`;
  complete capture, strict JSON parsing, scenario preflight, and exact controller restoration
  remain required. No startup or input path invokes them.
- Domain now has immutable discovery-evidence and discovery-scenario vocabulary for the next pure
  validator, including evidence visibility thresholds and explicit criticality.

After code review, the host `./scripts/quality.sh` passed workflow and formatting validation, a
zero-warning build, 1,093 Domain tests, 113 Application tests, 23 Infrastructure tests, seven
architecture tests, all 33 opt-in Godot smokes, and the bounded graphical launch. The focused
storage smoke first reproduced the known restricted Godot user-log denial; the identical host run
and complete host quality run are authoritative. Its file was process-specific under the system
temporary directory and was deleted afterward.

## Exact next bounded slice

Take **Phase 2p21: author the representative discovery scenario**.

Acceptance boundary:

1. Add one pure, engine-independent source that builds a retained `DiscoveryScenario` containing
   the five roadmap examples in stable order: visible herb, concealed herb, partial inscription,
   residue, and tool-gated clue.
2. Give each evidence definition one exact world-object target and visibility threshold. Include
   only the capability, tool, passive-noticing, and knowledge-triggered routes required to make
   those examples representative; mark critical evidence explicitly.
3. Include any referenced tool as a known world object, keep observer ownership exact, and give
   every known object one initial perception entry. Prove exact instances/order, duplicate
   rejection through the existing immutable snapshots, and unchanged source inputs with pure
   tests.
4. Do not add the validator algorithm or scene wiring in this slice. Run `./scripts/quality.sh`,
   review intended paths, and prune this file again.

Explicit exclusions: no scenario-validation decisions yet, reachability simulation, filtered
views, new Godot markers or art, changes to the existing greybox discovery lifecycle, save-schema
changes, migration, autosave, final save UI, provider work, or credentials. Do not merge, release,
distribute, sign, notarize, or change repository visibility without explicit authorization.

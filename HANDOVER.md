# Allrpcs Handover

## Current state

PR #140 merged Phases 2p17-2p20 into `main` at `fdfab79`. The branch
`codex/phase-2p21-plus-discovery-scenario` completes Phases 2p21-2p26 and the remaining Phase 2
roadmap work:

- Domain authors one immutable representative scenario with visible herb, concealed herb, partial
  inscription, residue, and tool-gated clue evidence in stable order. Each target and the required
  tool are known world objects, every object has initial Seeker perception, and only the necessary
  capability, tool, passive-noticing, and knowledge-triggered routes are present.
- Pure structural and reachability policies return stable, actionable issues; critical evidence
  has an explicit unreachable result. The composed validator changes no scenario state.
- `Allrpcs.ScenarioValidator` validates the representative scenario in local and hosted quality.
  A character evidence view exposes only evidence identifiers at or above that character's exact
  visibility threshold, omitting hidden target, threshold, and criticality metadata.

After code review, `./scripts/quality.sh` passed workflow and formatting validation, a zero-warning
build, 1,114 Domain tests, 113 Application tests, 23 Infrastructure tests, eight architecture
tests, the five-evidence scenario validator, all 33 opt-in Godot smokes, and the bounded graphical
launch. The first focused VSTest attempt reproduced the known sandbox loopback denial; focused and
complete host runs are authoritative. No provider, release, signing, or distribution work was
performed.

## Exact next bounded slice

Take **Phase 3a1: define immutable in-world dialogue utterance vocabulary**.

Acceptance boundary:

1. Add pure speaker and utterance vocabulary that retains the speaker's typed character identifier
   and the player's exact bounded, nonblank Unicode text without normalization.
2. Prove exact instance/text retention, bounds, null/blank rejection, and unchanged inputs.
3. Do not add a dialogue panel, hearing-range selection, intent classification, model/provider
   contracts, streaming, persistence, scene wiring, or Godot input. Those remain later slices.

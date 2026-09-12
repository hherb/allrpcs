# Allrpcs Handover

## Current state

`main` contains merged PR #213 at `adaa79c`. The current branch is
`codex/phase-7-ordinary-participation-launch`.

This branch completes three bounded Phase 7 slices:

- a scene coordinator preflights one exact directly addressed LLM listener before
  consuming a participation model, starts one bounded round, hands only a complete result to the
  existing budget-gated follow-up seam, and resets every terminal task outcome;
- ordinary heard player dialogue now installs a fresh two-turn autonomous budget and traverses the
  participation decision, selected healer follow-up, existing generation, validated response,
  single speech occurrence, completion, and one budget advance;
- consecutive eligible player roots install distinct fresh budgets, while explicit player
  cancellation cancels a waiting participation round and interrupts only its exact current chain.

The first Phase 7 parent remains open because the ordinary scene still schedules only its directly
addressed healer and cannot yet select or generate another listener's interjection. The loop parent
also remains open because healer speech does not start a further participation round.

This branch does not add another scene character; infer text addresses; generalize the coordinator
to a complete listener schedule; generate a non-healer follow-up; start a round from character
speech; add local Chat Completions, Anthropic, or Ollama participation dialects; add diagnostics or
retries; persist an active chain; add whispering/private conversations, independent tasks, sabotage,
or Phase 8 work; release; sign; or distribute.

## Validation and review

- the Phase 7an coordinator smoke was introduced before its missing type and failed compilation;
  the Phase 7ao ordinary-flow smoke failed before scene budget and participation routing existed;
  the strengthened repeat smoke failed on the stale shared budget before fresh-chain replacement;
- focused host-local smokes pass exact direct selection, meaningful silence, offline/provider
  failure, timeout, cancellation, interruption, exhaustion, listener/controller eligibility,
  overlap/repeat rejection, fresh-chain replacement, complete ordinary generation/speech/
  completion, and two consecutive player-rooted chains;
- focused existing smokes pass ordinary request, cancellation, repeat, offline-model, configured
  participation-source, and OpenAI/Ollama composition regressions through a credential-free
  test-only direct-response participation fixture;
- host-local restore, formatting, and the zero-warning solution build pass, followed by 2,093
  Domain, 426 Application, 522 Infrastructure, and 8 architecture tests;
- the representative validator passes all 5 evidence definitions through `dotnet exec` on its built
  DLL after the known macOS `dotnet run` apphost stall;
- the complete host-local headless Godot smoke matrix and bounded Compatibility-renderer launch
  pass;
- `git diff --check`, practical 500-line/120-character limits for new files, exact event/character/
  budget identity, no-consumption failed preflight, terminal reset, test-only fixture gating, and
  scope review pass.

Restricted execution reproduced the known VSTest loopback-socket and Godot `user://logs` failures;
the identical checks passed host-local. Hosted Actions remained account-budget blocked with
`steps: []` through PR #213; treat an unchanged recurrence as infrastructure state and do not retry
or dispatch workflows.

## Exact next bounded slice

Take **Phase 7aq: complete ordinary listener participation scheduling**.

Acceptance boundary:

1. Generalize the ordinary coordinator from one scheduled healer to one complete bounded stable
   collection of exact heard LLM listeners plus a complete distinct value-based direct-address set.
2. Preflight the entire schedule, all addresses, budget replacement, idle task, and free turn slot
   before consuming a model or changing budget state; preserve exact caller characters, event, and
   identifiers and keep the coordinator free of copied round state.
3. Keep ordinary player input wired with its existing single healer schedule, and add an isolated
   credential-free two-listener smoke proving direct-response priority, motivated interjection when
   the direct listener is meaningfully silent, all-silent no-turn, stable order, exact motive and
   event identity, overlap rejection, and unchanged surrounding state.
4. Do not yet add another embodied scene character, infer addresses from text, generate or complete
   a non-healer selected turn, start another round from healer speech, add provider dialects or
   retries, drive a loop, or tick either Phase 7 parent item.

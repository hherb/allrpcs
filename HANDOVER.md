# Allrpcs Handover

## Current state

`main` contains merged PR #215 at `ab70722`. The current branch is
`codex/phase-7-ordinary-participant-roster`.

Draft PR #216 is open and mergeable. Its five hosted jobs failed immediately with `steps: []`,
confirming the unchanged Actions/account infrastructure condition rather than a product-test
failure; do not retry or dispatch the unchanged workflows.

This branch advances five bounded Phase 7 slices:

- Phase 7as adds a scene-owned stable read-only ordinary roster containing the exact existing
  healer followed by one fresh representative sage, and resolves exact value-matching awaiting
  turns through `CharacterTurnParticipantLookup` before generation.
- Phase 7at gives the sage a scene-owned hearing position and includes the exact healer and sage in
  ordinary player-event hearing while retaining only the healer as directly addressed.
- Phase 7au generalizes the temporary generation-input source across the exact roster with bounded
  character-specific perception, knowledge, goals, commitment memory, recent speech, and
  affordances. The healer path remains unchanged and the sage receives no unobserved world state or
  physical affordance.
- Phase 7av-7aw resolve response speech to the exact healer body or sage position, schedule the
  complete heard roster, and drive one motivated sage interjection through the existing generation,
  reception, occurrence, completion, and budget-advance seams.
- Phase 7ax starts a further participation round only from exact completed character speech and
  heard LLM roster members. A close-position smoke proves sage then healer turns consume the finite
  two-turn budget in order and no third round starts.

The roadmap parents for motivated interjections/player interruption and bounded LLM-to-LLM loops
are now complete at the ordinary scene seam.

This branch does not add a sage sprite; infer direct address or privacy from text; define whispering
or private-conversation semantics; persist an active chain; add local Chat Completions, Anthropic,
or Ollama participation dialects; add diagnostics or retries; add independent tasks, party
separation, off-screen simulation, sabotage, or Phase 8 work; release; sign; or distribute.

## Validation and review

- The Phase 7as source smoke was authored against the absent roster and failed compilation until
  the source existed; it now proves stable order, exact healer retention, fresh distinct sage
  identity, read-only/closed surface, value-equal lookup, fail-closed absent/non-awaiting behavior,
  and unchanged dialogue, world, perception, model, budget, and presentation state.
- The Phase 7at dialogue-event smoke failed until ordinary hearing included both exact roster
  members; direct healer behavior, participation cancellation, and speech occurrence regressions
  pass.
- The Phase 7au generation-input smoke failed while the source remained healer-only; it now proves
  exact healer and sage character-specific bounded input without generation or mutation.
- The motivated-sage smoke failed before full-roster scheduling; it now proves exact stable
  participation, meaningful healer silence, motivated sage selection, bounded sage generation,
  speech from the sage position, one completion/advance, and unchanged world/perception state.
- The bounded-loop smoke failed before character speech could start a round; it now proves exact
  sage-then-healer generation, occurrence, completion, and budget advancement, with no third round
  after exhaustion.
- Host-local restore, formatting, and the zero-warning solution build pass, followed by 2,098
  Domain, 426 Application, 522 Infrastructure, and 8 architecture tests.
- The representative validator passes all 5 evidence definitions through `dotnet exec` on its
  built DLL after the known macOS `dotnet run` apphost stall.
- The complete host-local headless Godot smoke matrix and bounded Compatibility-renderer launch
  pass on the corrected final worktree.
- Restricted VSTest and Godot reproduced the known loopback-socket and `user://logs` failures; the
  identical checks passed host-local. Do not treat hosted jobs with `steps: []` as product-test
  failures or retry unchanged workflows.

## Exact next bounded slice

Take **Phase 7ay: explicit provider-neutral dialogue delivery mode**.

Acceptance boundary:

1. Add one closed provider-neutral dialogue delivery mode with only ordinary speech and whispering,
   plus one immutable value that retains an exact existing `DialogueUtterance` and one exact mode.
2. Reject null utterances, expose only construction and get-only state, preserve record value
   equality, and add focused Domain tests for exact retention, both closed values, and public
   surface.
3. Document that delivery mode is explicit authored/player intent and never inferred from dialogue
   text.
4. Do not yet change `DialogueEvent`, hearing ranges/candidates, audiences, scene input, response
   generation, transcripts, model schemas/context, persistence, UI, or roadmap parent status; do
   not define private addressee authorization or whisper occlusion.

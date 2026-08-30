# Allrpcs Handover

## Current state

Allrpcs is at the documentation baseline. The workspace began empty. It has now been
initialized as a Git repository on `main`, and the agreed product and implementation plan are
captured in `ROADMAP.md`. No Godot project, C# solution, gameplay code, tests, assets, provider
integration, or CI configuration exists yet.

At the start of the next session, verify the live checkout rather than relying only on this
summary:

```sh
git status --short --branch
git log -1 --oneline
find . -maxdepth 3 -type f -not -path './.git/*' -print | sort
```

## Product summary

The game is a slow-paced, top-down 2D investigation and healing quest. A contagious disease
causes infected people to enter a deep sleep and waste away. The player explores continuous
landscapes, gathers herbs, recovers lost knowledge, tests remedies, and deals with an enemy
agent attempting to spread or prolong the outbreak.

There are no conventional NPCs. Every character is a role controlled by a human, LLM, or
dormant controller. The first version is single-player: the human controls a customizable
Seeker, while LLMs control a healer, two hermits, a sage, an enemy agent, and eventually healed
infected characters.

## Settled architecture

- Godot 4.6 .NET and C# are the chosen implementation stack.
- Godot owns rendering, input, navigation, audio, scenes, and authoring.
- Pure C# domain and application projects own authoritative game rules.
- The intended dependency direction is:

  ```text
  Godot presentation -> Application -> Domain
  Infrastructure ----> Application -> Domain
  ```

- `Domain` must not depend on Godot, persistence, UI, or provider SDKs.
- Provider-specific request and response types remain inside infrastructure adapters.
- OpenAI, Anthropic, and Ollama implement one provider-neutral character-model interface.
- Durable world and character memory is local and does not rely on provider conversation state.
- User dialogue is normal human language. The internal intention protocol is invisible.
- The engine, not an LLM, controls truth, perception, inventory, movement, discovery, disease,
  and action outcomes.
- The quest is an evidence network with redundant routes rather than a linear objective chain.
- The first renderer should be Godot Compatibility unless prototype evidence changes that
  decision.
- Voice is deferred, but ASR and TTS will later enter through provider-neutral interfaces and
  reuse the text dialogue pipeline.

## Gameplay decisions already made

- Continuous top-down movement with smooth scrolling in any direction.
- Real-time movement and animation, with dialogue and close examination pausing the world.
- Disease progression occurs only at meaningful journey or investigation events.
- The human speaks naturally to LLM roles; companions interpret requests, retain agency, and
  may agree, refuse, clarify, negotiate, remain silent, or act independently.
- LLMs receive filtered per-character perceptions and knowledge.
- A character may propose one immediate intention at a time; the engine validates it and
  returns the observation before the character plans from information it did not previously
  possess.
- Character memory distinguishes facts, attributed claims, beliefs, episodic memories,
  relationships, and commitments.
- Hidden objects use authored discovery conditions and progressive visibility states rather
  than pixel hunting.
- Critical quest conclusions require redundant evidence and must have recovery paths.
- Treatments have comfort, stabilization, and cure outcomes.
- The enemy agent uses the same physical, perception, and action rules as every other role.
- Deeply sleeping roles are dormant and make no model calls until healed.

## First vertical slice

The target milestone is a playable opening scenario in which the player creates a Seeker,
meets and converses with the healer, explores the village and nearby woodland, finds evidence
and ingredients through multiple routes, prepares a stabilizing remedy, stabilizes the first
sleeping patient, and saves and resumes with character memory intact.

Planned opening landmarks are the sickhouse, healer's garden, village square, stream, abandoned
laundry, woodland clearing, old shrine, southern road, abandoned wagon, and a closed route into
the deeper forest.

## Exact next bounded slice

Implement **Roadmap Phase 0.1: Local solution skeleton**.

### In scope

1. Inspect installed `godot` and `dotnet` versions; do not guess paths or SDK compatibility.
2. Add an appropriate `.gitignore` and `.editorconfig`.
3. Create the C# solution and these initial projects:
   - `src/Domain`
   - `src/Application`
   - `src/Infrastructure`
   - `tests/Domain.Tests`
4. Establish project references matching the settled dependency direction.
5. Create `game/` as a Godot 4.6 .NET project using the Compatibility renderer.
6. Add a minimal `Main` scene that launches an empty titled window.
7. Add one pure C# smoke test.
8. Add the smallest practical Godot launch smoke check available in the installed environment.
9. Add `README.md` with exact prerequisites and verified build, test, and launch commands.
10. Run the documented commands, update `ROADMAP.md`, and update this handover with evidence and
    the next exact slice.

### Acceptance boundary

A developer can follow the new `README.md`, run the pure C# tests, and launch the empty Godot
application locally without provider credentials. The dependency direction is visible in the
project references, and `Domain` remains engine-independent.

### Explicitly out of scope

- Gameplay movement, camera, landscapes, characters, discovery, or inventory.
- LLM provider adapters or API credentials.
- SQLite or save-game persistence.
- Final art, generated art, audio, ASR, or TTS.
- Cross-platform CI, installers, signing, notarization, or distribution.
- Choosing default provider models.
- Final disease lore or art direction.
- Multiplayer or networking.

If Godot 4.6 .NET or a compatible .NET SDK is unavailable, stop after recording the exact
observed versions and the narrow installation requirement. Do not silently substitute a
different engine, Godot branch, language, or renderer.

## Expected next slice after Phase 0.1

After Phase 0.1 is verified and committed, take Phase 0.2: continuous integration and
architecture guardrails. Do not combine it with greybox gameplay unless explicitly requested.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact next bounded slice rather than reopening overall planning.
- Preserve explicit exclusions.
- Tick roadmap items only after current verification.
- Keep worktree changes narrowly scoped and review `git diff` before staging.
- Stage only intended paths.
- Update both documents at the end of every implemented slice.
- Record commands run, results, manual evidence, limitations, branch, and commit in this file.
- Do not claim unavailable platforms, providers, credentials, visual behaviour, or packaged
  builds were verified.
- Do not publish, push, open a pull request, distribute builds, or use credentials without the
  user's explicit authorization for that action.

## Current open decisions that do not block Phase 0.1

- Final game title and visual identity.
- Exact low-fantasy disease cause and cure recipe.
- Default cloud and local models.
- Art perspective, base resolution, palette, and animation style.
- Final SQLite library and schema design.
- Provider SDK versus direct HTTP choices per adapter.
- Distribution channels and project licence.
- Exact voice providers and voice identities.

## Documentation in this baseline

- `ROADMAP.md`: complete phase checklist and acceptance criteria.
- `HANDOVER.md`: current decisions, status, and the exact next implementation slice.

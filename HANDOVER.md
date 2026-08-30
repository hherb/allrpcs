# Allrpcs Handover

## Current state

Roadmap Phase 0.1, the local solution skeleton, is complete on `main`. The repository now has a
pure C# solution, explicit project-reference direction, a Godot 4.6.3 .NET Compatibility
project, a minimal titled `Main` scene, a domain smoke test, a Godot launch smoke script, and
verified setup commands in `README.md`.

This slice is committed as the current `HEAD` with subject `build: add local Godot solution
skeleton`. The prior prerequisite-handover commit is `052c0cd`. No gameplay, provider,
persistence, CI, art, audio, packaging, or distribution work was added.

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

Implement **Roadmap Phase 0.2a: local compiler and architecture guardrails**. This is the first
bounded part of Phase 0.2; do not add hosted CI or greybox gameplay in the same slice.

### In scope

1. Add `global.json` that pins the verified .NET 8 feature band with an explicit safe roll-forward
   policy.
2. Add shared build properties enabling deterministic builds, current recommended analysis, and
   warning failure for authored C# without breaking Godot-generated files.
3. Add pure C# architecture tests that enforce the current allowed project-reference graph:
   - `Domain` has no project references.
   - `Application` references only `Domain`.
   - `Infrastructure` references only `Application`.
   - `game` references only `Application`.
   - `Domain.Tests` references only `Domain`.
4. Add a reproducible formatting verification command and document the local quality sequence.
5. Prove at least one architecture test fails when a forbidden reference is introduced, then
   revert that deliberate test mutation before committing.
6. Run restore, formatting, build, unit tests, architecture tests, and both Godot smoke checks.
7. Update `ROADMAP.md` only for guardrails actually verified, and write the next handover.

### Acceptance boundary

A developer can run one documented local quality sequence that rejects formatting drift,
compiler warnings, and forbidden project references before launching Godot. All checks remain
credential-free and engine-independent tests do not load Godot.

### Explicitly out of scope

- Hosted CI workflow files and claims about live GitHub runners; take those in Phase 0.2b.
- Unsigned macOS, Linux, or Windows development-build jobs.
- Selecting the source licence; keep this before any publication or distribution.
- Gameplay movement, camera, landscapes, characters, discovery, or inventory.
- LLM provider adapters or API credentials.
- SQLite or save-game persistence.
- Final art, generated art, audio, ASR, or TTS.
- Installers, signing, notarization, publishing, or distribution.
- Multiplayer or networking.

## Phase 0.1 completion evidence (2026-08-30)

Environment:

- Branch: `main`.
- Machine architecture: `arm64`.
- OS: macOS 26.6.2 (build 25G83).
- Godot: `4.6.3.stable.mono.official.7d41c59c4`.
- .NET SDK: `8.0.130`; host runtime: `8.0.30`; RID: `osx-arm64`.

Toolchain installation:

- Homebrew installed versioned formula `dotnet@8` at `/opt/homebrew/opt/dotnet@8`.
- `/Users/hherb/.local/bin/dotnet` links to the formula's command and is already on `PATH`.
- The official [Godot 4.6.3 archive][godot-download] supplied the .NET universal app installed
  as `/Applications/Godot_mono.app`; the [Godot C# documentation][godot-csharp] defines the
  required .NET editor/SDK pairing.
- `/Users/hherb/.local/bin/godot` is an executable wrapper around the app's real binary. A plain
  executable symlink must not replace it: this macOS build then loses its bundle resource path
  and reports `.NET: Assemblies not found`.
- The downloaded editor archive matched the official SHA-512 manifest:

  ```text
  8c4c1c83a7a3e1cfc845e0fd4b4cdefd6f1cc662f63378c22a551b50f31965f2
  a633b7bf7c84ab530847e0fddb5dadc1128e04e87918618dd76c75d66eb67bb9
  ```

Implemented repository structure:

- `Allrpcs.sln` contains `Domain`, `Application`, `Infrastructure`, `Domain.Tests`, and the
  Godot game project.
- `Domain` has no project reference; `Application -> Domain`; `Infrastructure -> Application`;
  `game -> Application`; `Domain.Tests -> Domain`.
- All projects target `net8.0` with nullable references enabled.
- `game/project.godot` selects `GL Compatibility`, names the app `Allrpcs`, and launches
  `Main.tscn` with its minimal C# `Node`.
- `.gitignore` excludes generated Godot/.NET state, local credentials, test/package output,
  editors, and OS metadata without hiding scenes or authored game content.
- `.editorconfig`, `README.md`, and `scripts/smoke-godot.sh` establish the local baseline.

Commands run from `/Users/hherb/src/allrpcs`:

```sh
dotnet restore Allrpcs.sln
dotnet build Allrpcs.sln --no-restore
dotnet test tests/Domain.Tests/Allrpcs.Domain.Tests.csproj --no-build
./scripts/smoke-godot.sh
```

- Restore: all projects current.
- Build: succeeded for all five projects with zero warnings and zero errors.
- Pure C# tests: one passed, zero failed, zero skipped.
- Headless Godot main-scene smoke: exit 0.
- Bounded graphical smoke (`godot --path game --quit-after 60`): exit 0 using OpenGL 4.1 over
  Metal with the Compatibility renderer on Apple M3 Max.
- Review: `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed, and `dotnet list
  reference` confirmed the intended graph above.

Limitations:

- Only the current macOS host was exercised; Linux and Windows are unverified.
- The 60-frame native process established renderer and launch success, but no screenshot or
  user-driven visual inspection was performed.
- The checksum-matching official Godot app contains a stapled notarization ticket, but
  `codesign --verify --deep --strict` reports an invalid arm64 signature on both the untouched
  extracted bundle and installed copy; `spctl` reports an internal Code Signing subsystem error.
  Do not claim Gatekeeper validation from this installation.
- No provider, credential, persistence, gameplay, CI runner, packaging, or distribution path
  was exercised.

[godot-download]: https://godotengine.org/download/archive/4.6.3-stable/
[godot-csharp]: https://docs.godotengine.org/en/4.6/tutorials/scripting/c_sharp/index.html

## Expected next slice after Phase 0.2a

Take Phase 0.2b: credential-free hosted CI for formatting, compiler, architecture, pure C# test,
headless Godot, and unsigned native development-build evidence. Keep licence selection explicit,
and do not combine CI with Phase 1 greybox gameplay.

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

## Current open decisions

- Final game title and visual identity.
- Exact low-fantasy disease cause and cure recipe.
- Default cloud and local models.
- Art perspective, base resolution, palette, and animation style.
- Final SQLite library and schema design.
- Provider SDK versus direct HTTP choices per adapter.
- Distribution channels and project licence.
- Exact voice providers and voice identities.

## Foundation documentation

- `ROADMAP.md`: complete phase checklist and acceptance criteria.
- `HANDOVER.md`: current decisions, status, and the exact next implementation slice.

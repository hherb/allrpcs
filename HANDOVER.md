# Allrpcs Handover

## Current state

Roadmap Phase 0.2b, credential-free hosted CI configuration and explicitly unsigned native
development-build configuration, is complete on `main`. Phase 0.2c hosted activation is
explicitly authorized and prepared locally. The user selected a private GitHub repository and
the `AGPL-3.0-only` licence. The empty private repository is
`https://github.com/hherb/allrpcs`.

The workflow now pins the current `actions/checkout` v7.0.1 release. Static workflow validation
and every existing local quality check pass. Hosted evidence is still pending until `main` is
pushed and the evidence pull request is opened, so runner-dependent roadmap items remain
unticked.

At the start of the next session, verify the live checkout rather than relying only on this
summary:

```sh
git status --short --branch
git log -2 --oneline
git remote -v
find . -maxdepth 3 -type f -not -path './.git/*' -print | sort
```

The expected state after the activation commit is pushed is a clean `main` tracking
`origin/main`, with `origin` set to the private `hherb/allrpcs` repository.

## Exact next bounded slice

Complete **Roadmap Phase 0.2c: explicitly authorized hosted-runner activation and evidence**.
Do not begin Phase 1 gameplay while Phase 0.2 runner evidence remains outstanding.

### Satisfied authorization boundary

1. Current GA runner labels and every pinned action release were revalidated on 2026-08-30.
2. The user selected private visibility and `AGPL-3.0-only`.
3. The user explicitly authorized repository creation, adding `origin`, pushing the required
   refs, opening a pull request, and inspecting both hosted workflow events.
4. Public visibility, merge, release, artifact distribution, signing, and notarization remain
   unauthorized.

### If and only if the preconditions are satisfied

1. Use the exact user-approved remote and visibility. Push only the intended branch or commit.
2. Observe one `push` run and one pull-request run of `.github/workflows/ci.yml`.
3. Require successful results for:
   - pure C# format, zero-warning compilation, domain tests, and architecture tests;
   - the Ubuntu headless Godot main-scene smoke;
   - unsigned Linux x64, macOS Universal 2, and Windows x64 debug exports on their native runner
     operating systems;
   - macOS bundle-signature rejection and Windows Authenticode-signature rejection.
4. Inspect the three `UNSIGNED-DEVELOPMENT-*` artifacts only as build evidence. Confirm each
   contains `UNSIGNED-DEVELOPMENT-EVIDENCE.txt` and the expected target files. Do not publish,
   redistribute, sign, notarize, install, or represent them as releases.
5. Diagnose failures narrowly. Do not add secrets, provider keys, broad permissions, caches,
   signing inputs, unpinned actions, or release automation to make the workflow pass.
6. Only after current successful evidence, tick the pure-test, headless-smoke, unsigned-build,
   and Phase 0.2 acceptance boxes in `ROADMAP.md`. The scenario-validator item remains unticked
   until that validator exists.
7. Update `README.md`, `ROADMAP.md`, and this file with run URLs or IDs, exact runner images,
   artifact names, results, limitations, branch, commit, and pull-request state. Commit only the
   intended paths. Do not merge without separate user authorization.

If authorization is absent, leave the workflow and roadmap claims unchanged and report that
live runner evidence is still unavailable.

## Implemented in Phase 0.2b

### Hosted workflow

`.github/workflows/ci.yml` runs on pull requests and pushes to `main` with only
`contents: read` permission:

- `quality` uses `ubuntu-24.04`, installs only the pinned .NET SDK, and runs formatting, the
  zero-warning solution build, one pure domain test, and five pure architecture tests. It does
  not install or invoke Godot.
- `godot-smoke` uses `ubuntu-24.04`, Godot 4.6.3 .NET, and the existing headless main-scene
  smoke.
- `development-build` uses native `ubuntu-24.04`, Apple Silicon `macos-15`, and
  `windows-2025` jobs for Linux x64, macOS Universal 2, and Windows x64 debug exports.
- Checkout credential persistence is disabled. No provider credential, secret expression,
  dependency cache, signing input, installer, release, deployment, or publication step exists.
- Artifact names begin `UNSIGNED-DEVELOPMENT-`, retention is seven days, and every artifact
  includes a generated evidence-and-limitations manifest.
- The macOS job requires bundle signature verification to fail. The Windows job requires
  Authenticode status `NotSigned` before artifact upload.

### Immutable workflow dependencies

Verified from the maintainers' releases on 2026-08-30 and pinned to full commit SHA:

- `actions/checkout` v7.0.1:
  `3d3c42e5aac5ba805825da76410c181273ba90b1`.
- `actions/setup-dotnet` v6.0.0:
  `a98b56852c35b8e3190ac28c8c2271da59106c68`.
- `chickensoft-games/setup-godot` v2.4.2:
  `c233594225991af5aec714e52457cc76d6df8fa2`.
- `actions/upload-artifact` v7.0.1:
  `043fb46d1a93c77aae656e7c1c64a875d1fc6a0a`.

Primary references:

- GitHub's [hosted-runner reference][github-runners] listed `ubuntu-24.04`, `macos-15`, and
  `windows-2025` as GA labels. The specific labels avoid silent `-latest` migrations.
- Chickensoft's [setup-godot documentation][setup-godot] supports Godot 4.x .NET and export
  templates directly on Ubuntu, macOS, and Windows runners. Its v2.3.4 release added arm64
  support; v2.4.2 was current when pinned.
- Godot's [export documentation][godot-export] permits `export_presets.cfg` in version control
  and defines `--export-debug` plus the required platform output extensions.

[github-runners]: https://docs.github.com/en/actions/reference/runners/github-hosted-runners
[setup-godot]: https://github.com/chickensoft-games/setup-godot
[godot-export]: https://docs.godotengine.org/en/4.6/tutorials/export/exporting_projects.html

### Export contract and local scripts

- `game/export_presets.cfg` defines Linux x64, macOS Universal 2, and Windows x64 development
  presets. macOS code signing and notarization and Windows code signing are explicitly disabled.
- `game/project.godot` imports ETC2/ASTC textures because Godot 4.6.3 rejects Universal 2 or
  arm64 macOS export when that import format is disabled.
- `scripts/export-development.sh` accepts only `linux-x64`, `macos-universal`, or `windows-x64`,
  exports with the debug template, and writes only to ignored
  `artifacts/unsigned-development/`.
- `scripts/validate-workflows.sh` requires `actionlint` and validates all hosted workflows.
- `scripts/quality.sh` now runs workflow validation before restore, formatting, compilation,
  tests, and both Godot smokes.

## Phase 0.2b completion evidence (2026-08-30)

Environment:

- Branch: `main`.
- Machine: Apple M3 Max, `arm64`, macOS 26.6.2 (build 25G83).
- Godot: `4.6.3.stable.mono.official.7d41c59c4`.
- .NET SDK: `8.0.130`; host runtime: `8.0.30`; RID: `osx-arm64`.
- `actionlint`: 1.7.12, Homebrew build for `darwin/arm64`.
- Git remote during Phase 0.2b validation: none.

Static and script validation:

```sh
./scripts/validate-workflows.sh
sh -n scripts/*.sh
./scripts/export-development.sh invalid-target
git diff --check
```

- `actionlint` returned exit 0 with no findings.
- Every shell script parsed successfully.
- The development-export script rejected an unsupported target with the documented exit 64.
- Diff whitespace validation passed.

Export-preset parsing used the installed Godot editor without export templates:

```sh
godot --headless --path game --export-debug "Linux Development" /tmp/allrpcs-ci-preset-check.x86_64
godot --headless --path game --export-debug "macOS Development" /tmp/allrpcs-ci-preset-check.zip
godot --headless --path game --export-debug "Windows Development" /tmp/allrpcs-ci-preset-check.exe
```

- Godot recognized all three preset names and target configurations.
- Linux and Windows stopped only because their 4.6.3 .NET debug/release export templates are
  not installed locally.
- The first macOS parse identified the missing ETC2/ASTC import setting. After adding it, macOS
  stopped only because `macos.zip` is not installed locally.
- No native export or artifact was produced by these parse checks.

Complete local quality command:

```sh
./scripts/quality.sh
```

The final host-shell run passed:

- Workflow static validation: no findings.
- Serial restore: all six projects current.
- Formatting: no changes required.
- Build: six projects, zero warnings, zero errors.
- Pure domain tests: one passed, zero failed, zero skipped.
- Pure architecture tests: five passed, zero failed, zero skipped.
- Headless Godot main-scene smoke: exit 0.
- Bounded 60-frame graphical smoke: exit 0 using OpenGL 4.1 over Metal with the Compatibility
  renderer on Apple M3 Max.

## Limitations and unverified claims

- No GitHub-hosted runner, workflow event, remote repository, branch protection, or pull request
  was exercised.
- Linux, Windows, export templates, uploaded artifacts, and native unsigned-build signature
  assertions were not exercised locally.
- The installed macOS Godot export templates are absent, so no local macOS exported bundle was
  built or inspected.
- `AGPL-3.0-only` was selected before the authorized private-source push. No artifact was
  published or distributed.
- No provider, credential, cache, persistence, gameplay, installer, signing, notarization,
  Gatekeeper, SmartScreen, Store, or release path was exercised.
- The complete test and graphical sequence used the host shell because the workspace sandbox
  denies .NET test-host sockets and other native process facilities.
- Earlier toolchain and architecture negative-proof details remain recorded in the Phase 0.2a
  handover at commit `31daaf8` (`git show 31daaf8:HANDOVER.md`).

## Settled product and architecture

- The game is a slow-paced, top-down 2D investigation and healing quest with human-, LLM-, and
  dormant-controlled roles rather than conventional NPCs.
- Godot 4.6 .NET and C# are the stack. Godot owns rendering, input, navigation, audio, scenes,
  and authoring.
- Pure C# Domain and Application projects own authoritative rules. Infrastructure contains
  provider and persistence adapters.
- Dependency direction is Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain. Domain must not depend on Godot, persistence, UI, or provider SDKs.
- Provider-specific types remain in adapters. Durable memory is local and provider-neutral.
- LLM roles receive filtered perceptions and propose one immediate intention; the engine owns
  truth, perception, inventory, movement, discovery, disease, and action outcomes.
- The first renderer is Compatibility unless prototype evidence changes that decision.

## Explicit exclusions still in force

- Do not begin gameplay, camera, landscape, character, discovery, or inventory work in the next
  hosted-activation slice.
- Do not add LLM adapters, provider credentials, SQLite, save persistence, art, audio, ASR, TTS,
  multiplayer, or networking.
- Do not add installers, signing, notarization, release tags, publication, or distribution.
- Do not choose the project title, disease fiction, models, art direction, persistence library,
  provider SDKs, distribution channels, or voice identities for the user.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact bounded slice and preserve its explicit exclusions.
- Tick roadmap items only after current evidence exists.
- Keep worktree changes narrow, inspect the complete diff including untracked files, and stage
  only intended paths.
- Update `README.md`, `ROADMAP.md`, and `HANDOVER.md` at each slice end.
- Record commands, results, manual evidence, limitations, branch, commit, and remote/PR state.
- Do not claim unavailable platforms, credentials, artifact behavior, signing, distribution, or
  visual behavior was verified.
- Do not publish, push, open or merge a pull request, create a remote, distribute builds, or use
  credentials without the user's explicit authorization for that exact action.

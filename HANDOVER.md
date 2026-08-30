# Allrpcs Handover

## Current state

Roadmap Phase 0.2c, private hosted-runner activation and evidence, is in its final evidence pull
request. Phase 0.2b was completed at `5d0de93`; the authorized activation commit is `cba1ba8`
(`ci: activate private hosted checks`). The current evidence branch is
`ci/hosted-runner-evidence`, based on `main` at `cba1ba8`.

The private remote is `https://github.com/hherb/allrpcs`. The source licence is
`AGPL-3.0-only`; the canonical GNU text is committed as `LICENSE`. Public visibility, merging,
release creation, artifact distribution, signing, and notarization remain unauthorized.

At the start of the next session, verify live state rather than relying only on this summary:

```sh
git status --short --branch
git log -4 --oneline --decorate
git remote -v
gh pr view --repo hherb/allrpcs --json number,state,isDraft,headRefName,headRefOid,url
gh pr checks --repo hherb/allrpcs
```

The expected state is a clean evidence branch tracking its remote, an open unmerged pull
request, and successful current checks. Do not merge without separate user authorization.

## Phase 0.2c evidence

### Local validation

The activation commit updated all three checkout steps to the immutable
`actions/checkout` v7.0.1 SHA `3d3c42e5aac5ba805825da76410c181273ba90b1`. On 2026-08-30,
the maintainers' current releases and the supported runner labels were revalidated:

- `actions/checkout` v7.0.1;
- `actions/setup-dotnet` v6.0.0;
- `chickensoft-games/setup-godot` v2.4.2;
- `actions/upload-artifact` v7.0.1;
- `ubuntu-24.04`, `macos-15`, and `windows-2025` hosted labels.

Static validation passed with `actionlint` 1.7.12 and `sh -n scripts/*.sh`. The complete
host-shell `./scripts/quality.sh` passed with .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official: six projects built with zero warnings or errors, one domain test and
five architecture tests passed, and both headless and bounded graphical Godot smokes exited 0.
The restricted sandbox build also reached zero warnings but its test host could not bind a local
socket; that sandbox failure is not a product or test failure.

The local `LICENSE` SHA-256 is
`0d96a4ff68ad6d4b6f1f30f713b18d5184912ba8dd389f86aa7710db079abcb0`, matching the canonical
GNU AGPL v3 text downloaded from `https://www.gnu.org/licenses/agpl-3.0.txt`.

### Successful push run

Private GitHub Actions [push run 33310947250][push-run] passed for full commit
`cba1ba8b9db199873d34beda3b1019f0a76efbe9`.

- Pure C# quality and tests, job `99255767758`, passed on `ubuntu-24.04` image
  `20260823.283.1`: restore, formatting, zero-warning build, one domain test, and five
  architecture tests.
- Headless Godot smoke, job `99255767626`, passed on the same Ubuntu image with Godot 4.6.3
  .NET.
- Linux x64 unsigned development export, job `99255767765`, passed on the same Ubuntu image.
- macOS Universal 2 unsigned development export, job `99255767744`, passed on
  `macos-15-arm64` image `20260727.0256.1`; the required bundle-signature verification failed,
  proving the bundle was not signed at that boundary.
- Windows x64 unsigned development export, job `99255767820`, passed on
  `windows-2025-vs2026` image `20260824.214.3`; Authenticode status was `NotSigned`.

The three uploaded artifacts expire on 2026-09-06 and were downloaded only to a temporary
inspection directory:

- `UNSIGNED-DEVELOPMENT-linux-x64`, artifact `9731965537`, contained `Allrpcs.x86_64`,
  `Allrpcs.pck`, `Allrpcs.sh`, and `UNSIGNED-DEVELOPMENT-EVIDENCE.txt`.
- `UNSIGNED-DEVELOPMENT-macos-universal`, artifact `9731967952`, contained
  `Allrpcs-macOS.zip` and `UNSIGNED-DEVELOPMENT-EVIDENCE.txt`.
- `UNSIGNED-DEVELOPMENT-windows-x64`, artifact `9731976871`, contained `Allrpcs.exe`,
  `Allrpcs.console.exe`, `Allrpcs.pck`, and `UNSIGNED-DEVELOPMENT-EVIDENCE.txt`.

Each manifest recorded the expected target and preset, exact activation commit, native runner
OS and architecture, Godot 4.6.3, .NET SDK 8.0.130, and the unsigned-development limitations.
No artifact was installed, executed, published as a release, or redistributed.

### Pull-request run

The first pull-request run and pull-request metadata must be added here after the evidence
branch is pushed and the authorized pull request is opened. Require the same five successful
jobs before ticking the Phase 0.2 acceptance item.

[push-run]: https://github.com/hherb/allrpcs/actions/runs/33310947250

## Limitations

- Hosted evidence establishes credential-free compilation, tests, headless launch, debug
  export, bounded unsigned-signature checks, and artifact contents on the recorded runner
  images. It does not establish installation, gameplay, accessibility, performance, Gatekeeper,
  SmartScreen, Store, distribution, signing, notarization, or release readiness.
- The repository is private. Public-source behavior, public-fork pull requests, branch
  protection, required-check rules, and external contributor permissions were not exercised.
- The scenario-validator CI item remains unticked because the validator does not exist.
- No provider, credential, cache, persistence, gameplay, installer, signing, notarization,
  release, or distribution path was added or exercised.

## Exact next bounded slice

First finish review of the Phase 0.2c evidence pull request. Do not merge it without explicit
user authorization. After it is merged, complete **Phase 1a: keyboard greybox movement and
camera foundation**.

Acceptance boundary:

1. Create a small placeholder top-down test region using authored Godot primitives or temporary
   shapes; do not select a final art direction or add final assets.
2. Define named Godot input actions for four-direction movement. Gameplay code must consume
   actions rather than hard-coded physical keys.
3. Add a placeholder Seeker that moves continuously by keyboard at a named, documented speed.
   Keep input-to-motion calculation pure and independently testable where practical.
4. Add a smooth `Camera2D` follow with named tuning values and a stable initial framing.
5. Add focused automated tests for testable movement calculation and retain every architecture,
   domain, workflow, and Godot smoke check.
6. Run `./scripts/quality.sh`, manually launch the native scene, and record what movement and
   camera behavior was actually observed. Do not claim unobserved platforms or input devices.
7. Update `README.md`, `ROADMAP.md`, and this file; commit only reviewed slice paths.

Explicit exclusions for Phase 1a:

- Do not add point-and-click movement, controller mappings, healer behavior, companion AI,
  pathfinding, navigation regions, interaction highlighting, inventory, dialogue, disease,
  providers, persistence, art, audio, networking, or packaging.
- Do not add landscape collision beyond the minimum needed to keep the placeholder Seeker in
  the bounded test region; broader authored terrain and collision remain later Phase 1 work.
- Do not change repository visibility, merge pull requests, create releases, distribute
  artifacts, or add credentials without explicit user authorization.

## Working conventions

- Start every continuation by reading `ROADMAP.md` and this file.
- Take the exact bounded slice and preserve its exclusions.
- Tick roadmap items only after current automated and manual evidence exists.
- Keep dependency direction: Godot presentation to Application to Domain, and Infrastructure to
  Application to Domain.
- Keep changes narrow, inspect tracked and untracked files, and stage only intended paths.
- Record branch, commit, pull request, checks, native evidence, and limitations without
  overstating unavailable platform or release validation.

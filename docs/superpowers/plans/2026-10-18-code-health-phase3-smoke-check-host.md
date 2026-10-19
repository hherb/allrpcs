# Phase 3 SmokeCheckHost

Owner authority: the 18 October Phase 3 decision recorded for
`owner-code-health-phase3-2026-10-18` in `CONTINUATION.json`. This is program Phase 3 item 3 in
[the program plan](2026-10-17-code-health-and-playable-slice.md) (review §1.3).

## Scope and completion

Stop instantiating the test harness in the shipping scene: `Main.tscn` holds no check nodes, and
one host attaches only the checks a launch requests. Every check keeps its source, its gate, its
assertions and its launch; gameplay is unchanged.

Out of scope: the export presets and debug-only test overrides (already done in Phase 2), any
check rewrite, the deferred Phase 2 checks, `Main.cs` line reduction (the other Phase 3 exit
item), the full root gate (due at Phase 3 exit) and any phase-exit claim.

## What is there (measured at `2d6fc19e`)

- `Main.tscn` has 976 nodes. 215 are checks, all direct children of Main: 213 C# script nodes
  and 2 instanced check scenes (`ControlSettingsMainSmokeCheck.tscn`,
  `DialogueAccessibilityMainSmokeCheck.tscn`). None has a serialized property, and every node
  name equals its class and file name.
- Each check gates itself on its own environment variable in `_Ready` and is inert otherwise.
  210 read one `ALLRPCS_*_CHECK` literal; `SeekerCustomizationMenuSmokeCheck` reads two;
  three read a controller's `SmokeCheckEnvironmentVariable` constant; and
  `LiveCharacterModelOrdinaryFlowSmokeCheck` reads four `ALLRPCS_LIVE_*_ORDINARY_*` variables.
  No two checks share a gate, and no check consumes input.
- Launches: `scripts/smoke-godot.sh` runs the main scene with the gate set (or a standalone proof
  scene); `CheckRegistrationTests` pins that every gate is launched there or allowlisted with a
  reason (Movie Maker renders, `quality.py` HEAD stages, historical snapshots). Historical
  `quality.py` stages run in pinned git snapshots and keep their own `Main.tscn`.
- 205 checks reach Main's nodes with `"../Name"` paths, so checks must stay Main's direct
  children.
- Order matters before Main is ready: `LiveCharacterModelOrdinaryFlowSmokeCheck._Ready` sets
  `Main.ConditionalCollectionScaffolding`, which `Main._Ready` reads, and
  `RuntimeWoodlandClearingDressingSmokeCheck` asserts Main's last child is
  `WholeChannelPresentation`.
- Helper fixtures: `HandToolTapRuntimeSmokeCheck.ArrangeSave()` (18 callers),
  `CaskCoverHandlingRuntimeSmokeCheck` and `WaterContactRuntimeSmokeCheck` are reached by other
  checks and Movie Maker proofs through Main's node; `PositionalAudioRuntimeSmokeCheck` requires
  `MaterialAudioRuntimeSmokeCheck` to exist. The proofs reach them only with their
  `*_OUTPUT_CHECK` gate set.
- Shipping: the `shipping_hygiene` export plugin strips harness children from exported scenes.
  `scripts/check-shipping-scene.gd` requires source Main to hold at least 200 harness nodes, and
  `scripts/check-shipping-pack.gd` requires the transformed `Main.tscn.shipping.scn`, which the
  plugin writes only when it removed something.

## Design

- `game/SmokeCheckHost.cs` is a static class with one entry per former node, in `Main.tscn`
  order: the check (a C# type, or a check scene by name), the nearest preceding non-check
  sibling it followed, and the gates that request it. A helper fixture's gates include the gates
  of every check or proof that reaches it.
- `Main._EnterTree` calls `SmokeCheckHost.Attach(this)`. Main's children are instantiated but
  not yet in the tree or ready, and Main is not yet blocked, so the host adds each requested check
  and moves it to its former index. Process and input order follow the index, so they are
  unchanged. Godot 4.6 readies children in insertion order (verified with a scratch project), so a
  hosted check now readies after Main's scene children instead of between them, and still before
  `Main._Ready`. Matching the old ready order exactly would mean removing and re-adding later
  siblings, which clears their owners; the launches decide whether any check depends on it.
  Hosted checks attach in entry order, so several requested together keep their former relative
  ready order. One consequence: Godot's READY re-enables processing on nodes that override
  `_Process`/`_PhysicsProcess`, so before this change seven checks'
  `SetPhysicsProcess(false)` on the later Seeker and Healer was silently undone; now it holds
  (`OpeningRegionSouthernRoadWagon`, `WoodlandClearing`, `VillageTransition`, `StreamLaundry`,
  `OldShrine`, `ClosedForestPath` and `SeekerMovementBoundary` smoke checks). All seven drive
  those bodies directly and pass.
- A check is requested when any of its gates is non-empty; the check still evaluates its own gate
  value, so a mode-valued or mismatched gate behaves as before. Attaching is idempotent per name.
- Exported builds (`template` feature) attach nothing: exports exclude every harness file, which
  matches the plugin removing harness nodes today.
- Pins: architecture tests assert `Main.tscn` holds no check node; every gate in a hosted
  check's partial files (`ALLRPCS_*_CHECK` and live `_ACTION`/`_WAIT` literals, and
  `Type.SmokeCheckEnvironmentVariable` constants) is one of its host gates; every gated game class
  is hosted or in a scene; and anchors are Main children in non-decreasing Main order. A missed attach
  of a launched check never quits, so its `run_check` deadline fails.
- Shipping checks: `check-shipping-scene.gd` now requires source Main to hold no harness node
  (the repack/load and autoload checks stay); `check-shipping-pack.gd` loads the exported Main
  through its normal remap, requires the remap target and every direct dependency to be in the
  pack and the scene to instantiate, and rejects any harness file in the pack.

## Batches

Each batch removes its nodes (and then-unused external resources) from `Main.tscn`, adds their
host entries, and runs its launches. A scratch generator derives entries from the `2d6fc19e`
scene, so entries match the former order exactly.

1. **Host and pilot.** Host, Main hook, architecture tests, shipping checks, and eight checks
   covering each case: `AdaptiveDisplaySmokeCheck`, `OptionalOverlayVisibilitySmokeCheck`,
   `NavigationSmokeCheck`, `RuntimeWoodlandClearingDressingSmokeCheck` (last child),
   `SeekerCustomizationMenuSmokeCheck` (two gates), `SaveDocumentStorageControllerSmokeCheck`
   (controller gate), `ControlSettingsMainSmokeCheck` (scene) and
   `LiveCharacterModelOrdinaryFlowSmokeCheck` (pre-ready property; its live provider launch is not
   run, its build and inert attach are).
2. **Remaining non-audio checks.** World, opening-region, input, dialogue, model, save and
   sabotage checks.
3. **Audio and speech checks and the helper fixtures**, with dependents' gates.

## Verification

- Each batch: `dotnet format --verify-no-changes`, `dotnet build`, the four test projects, and the
  batch's `smoke-godot.sh` launches.
- Final: full `./scripts/smoke-godot.sh` (quarantined #448 retry reported, not failed),
  `MainProcessOrderCheck`, the plain main-scene launch, the HEAD `quality.py` stage commands for
  the allowlisted interior, water, cask, hand-tool and healer-footstep checks, and `godot
  --export-pack` plus `check-shipping-pack.gd` (no export templates needed).

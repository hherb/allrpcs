# First container/cloth action proposal

Prepared 2026-10-10 (Australia/Sydney). Planning is complete; the new implementation
grant is pending. Machine-readable boundaries, source hashes and checks are in
[container-cloth-owner-scope.json](container-cloth-owner-scope.json).

## One proposed action

Add **Adjust cask cover** for the human-controlled Seeker at the existing
`village-water-cask`. One explicit activation seats its wooden lid and smooths a
cotton outer cover. The action finishes with the cask covered, without opening it,
inspecting its contents, taking anything or changing durable world truth.

The wooden cask/lid and cotton cover are **new proposed material declarations**.
The present description says only “A covered cask beside the village well.” Neither
its brown polygon nor accepted A02 establishes those materials. Approval would
author this one environmental handling permission and its materials, reach and
feedback together. Cotton is part of this cover, not a new collectible or an item
silently added to the Seeker's inventory. No cover mesh, sprite, hand animation,
collision or placement change is proposed. The button tooltip identifies the
wooden lid/cotton cover; the existing object name and description remain intact.

This addresses the unfinished roadmap item “Add footsteps by terrain, water, wind,
foliage, interiors, tools, and remedy preparation.” It installs the first distinct
A02 container/cloth owner; it does not finish all container, cloth or tool actions.

## What the live sources establish

| Source | Present contract | Consequence for this proposal |
| --- | --- | --- |
| `TemporaryWorldObjectSource` / `WorldObjectController` | Retain the exact covered village cask among four objects | Bind this object, not a new laundry prop or a value-equal replacement |
| `Main.tscn` / `GreyboxInteractionTarget` | Cask marker at `(2200,900)`, original/replacement bodies, live visibility/enabled flags | Use actual current position and original body; do not change geometry |
| `InteractionFocusController` | Closest eligible focus within 112 pixels | Focus is selection, not the proposed 32-pixel handling reach |
| `TemporarySeekerCharacterSource` | Current human character begins with empty inventory | Do not assume a carried cloth or vessel |
| `TemporaryFocusedSearchContextSource` | Separate field-lens context and inspect-water-sources capability | Neither is a handling commit or permission to alter contents |
| `InventoryUsePolicy` / take/drop/give policies | Ownership/capacity rules for existing inventory objects | Environmental cover handling has its own proposed grant; these policies stay unchanged |
| `CharacterPhysicalAccessPolicy` | Explicit accessible-object snapshot; does not infer access from position | New cover reach belongs solely to the new action; do not synthesize access for sabotage or other actions |
| Move/conceal/substitution/contamination presentation owners | Cask can move, disappear or show a replacement/marker | Observe visible presentation without changing or investigating sabotage truth |
| `OpeningRegionStreamLaundrySceneSource` | Inert laundry shell, routes and bounded interior audio | No container/cloth inventory or action owner exists there |
| Accepted `A02` / `prepare-excerpts.py` | Wooden contact at 1 second, cotton handling at 2–4.5 seconds | Preserve the accepted sequence with one byte-exact slice |

The existing water-contact action, A04 grinding and A05 treatment retain their
owners, commits, resources and review evidence unchanged. This action does not
call their controllers or share their transient state.

## New owner and access boundary

Propose `CaskCoverHandlingController` with exact immutable request/state/commit
contracts. A request retains the current exact character, world object, material
binding and state. Only the controller's issued current request may commit;
copies, stale actors/targets, duplicates and overflow are rejected. Swap accepted
state and install cooldown before exception-isolated observer notification. A
missing stream, bus or observer failure cannot undo or repeat a committed action.

`OpeningRegionCaskCoverHandlingCoordinator` is the sole ordinary producer. On each
offer and again immediately before commit it requires:

- Exact live source nodes, world-object snapshot and cask object; the named marker
  must still resolve to that exact object. The current actor must be the active,
  processing human Seeker. A character change resets transient ownership.
- The target and original `Body` must be visible in the tree, the substitute body
  hidden, and marker visibility/enabled flags true. The current focus must resolve
  to this cask. No hidden contamination/evidence/knowledge test changes availability.
- An unchanged ready/active navigation region, map, polygon and source transforms;
  finite actual Seeker/target positions. Retain target scale/rotation; translation
  follows the existing live move presentation, never the initial position alone.
- Both positions supported by the retained navigation map within 1 pixel, an
  inclusive distance of **32 pixels** from Seeker to marker centre, and a supported
  straight segment between them. Sample that segment at intervals no larger than
  4 pixels, including endpoints; reject any unsupported sample. This is an authored
  interaction abstraction, not a simulated hand trajectory or physical access for
  another subsystem. No navigation cells, walls, collisions or accepted art change.
- Active/focused unpaused play; no text entry, reading/customization overlay or
  nonterminal/reserved shared character turn. Fresh mouse or native Tab/Enter/Space
  activation only; held keys and key echo cannot repeat it. No InputMap change.

Source-only reach example: `(2168,900)` is 32 pixels west of the initial cask centre,
on the Village Edge–Square route. The cell containing it has centre `(2172,900)` and
lies on that route, whose navigation half-width is 24 pixels. Cells traversed by
the segment to `(2200,900)` have centres 4 pixels from the route at most. Ordinary
movement to this example and back still requires live verification after approval.
If the cask is moved to `(1900,1640)`, re-evaluate actual supported access there;
reject its old position. Concealment/substitution cancel or suppress only this new
feedback. Handling never recovers evidence, reveals contamination, cleans water or
modifies sabotage snapshots, presentation markers or discovery results.

Cooldown is **4 seconds of bounded active time** after each commit, longer than the
3.75-second feedback. Admit deltas from 0 through 0.25 seconds; invalid deltas cancel
feedback and preserve remaining cooldown. Pause/focus loss freezes cooldown and
cancels feedback. Successful load invalidates requests/commits, clears cooldown
and cancels without replay; failed load changes nothing. Leaving valid access,
target movement during feedback, teleport over 96 pixels, owner replacement,
overlay, disablement or teardown cancels feedback. A consumed commit stays consumed.
No save field, inventory transfer, disease/time, transcript, perception, LLM
intention, hearing emitter, music interruption or ducking lease is added.

## Input, feedback and extraction

Offer a contextual native **Adjust cask cover** button, disabled during cooldown.
Tooltip: “Seat the wooden lid and smooth its cotton cover.” Show the independent
own-action status “You adjust the cask cover.” for 3.75 seconds. Add one independently
owned seventh caption, **Container and cloth handling**, for the same interval.
It clears only itself and follows the existing captions toggle; status remains
available with captions disabled. Preserve the six other caption owners and their
timers. Review simultaneous caption wrapping and keyboard focus at both viewports.

One local nonlooping Effects player at 0 dB consumes each fresh exact live commit
once. It uses the existing Master/Effects/range/mono controls. Missing stream or
Effects bus produces silence while status/caption and commit still succeed; never
fall back to Master. No additional spatial source or cue-history entry is proposed.

Candidate `A02-C01.wav` is accepted A02 frames **[48000,228000)**, corresponding to
source seconds **[1,4.75)**: the full wooden contact, then full cotton handling and
0.25 seconds of recovery silence. It is 180,000 stereo PCM24 frames at 48 kHz,
3.75 seconds, 1,080,000 PCM bytes. Raw-frame SHA256:
`5dc4e93225eaad07ff94861d531aaed74e697d46bdcb7e85dea1db894c5bc041`.
First and last stereo frames are zero. No resampling, gain, new fade, rearrangement,
mixing or accepted-source edit is allowed. Only a new WAV header/import accompanies
the exact slice. The slice was hashed in memory; no derivative has been created.

Carry both existing local source notices: rubberduck's wooden recording and
khenshom's cotton recording, recorded as CC0-1.0 in the accepted provenance. Package
new `CREDITS.txt`, `provenance.json` and a byte-exact existing `CC0-1.0-deed.html`;
list both authors, sources and the exact slice. Append these notices to the current
in-game credits. This is reuse of accepted local provenance, not a new catalogue,
legal assessment, acquisition or listening decision.

## Minimum implementation and finite checks

The JSON enumerates exact proposed new/modified paths. New Domain/Application action
contracts and access policy have focused tests; new Game coordinator, panel and
audio consumer have a live smoke and stopped output fixture. An extraction script,
output checker and successor boundary validator bind PCM, ownership, protected
inputs, measured output and package contents. Modify only `Main.tscn` for the three
new owners, the caption/credits panels and root `quality.py`; retain the Channel's
final-child order. No other existing product source change is proposed.

After approval, pin the unchanged water validator to this inspected predecessor
commit using the existing isolated-history runner pattern. Retain all existing
product, interior and water checks, then add the new successor validation and modes.
Do not edit any frozen validator to admit the successor.

Required completion evidence is finite:

1. Unit contracts: exact issuance/commit/state, human/actor/target/material guards,
   cooldown bounds/reset/observer isolation; reach boundaries, segment samples,
   invalid positions, navigation support and snapshot replacement.
2. Ordinary movement to/from the source example, mouse and keyboard activation,
   no key-repeat; conceal/substitute/move/restore/cancel checks. Contamination and
   discovery outcomes remain byte-identical after handling. Detached audio fixtures
   cannot manufacture ordinary commits.
3. Six Main schedules: default, quiet, Master-muted, Effects-muted, missing-stream,
   no-action. The first five have identical explicit commits and complete durable
   save/transcript bytes; the no-action control has zero new commits and the same
   durable bytes. Retain all prior interior/water parity schedules too.
4. Twenty actual Master-output cases listed in JSON, with expected silence,
   attenuation, mono, controls, exact cancellations and A04/A05/water caption
   coexistence. Record the active time/cooldown timeline; no acoustic claim from
   a process exit alone.
5. 1280×720 and 1920×1080 views: button/status, captions on/off, all seven lines,
   credits, keyboard focus and native focus-loss cancellation. Reuse accepted art.
6. Native import/archive frame parity and **35 streams / 20 individual notices**
   (17 retained plus this three-file bundle; 16 notice groups). Packaging does not
   establish an executable release or ordinary-device loudness compliance.
7. Successor rejection probes for changed PCM/proposal, forged commit, changed reach
   or geometry, missing measured output and mutated sabotage/save truth. Run root
   `./quality.sh` and require fixture completion strings and final boundary pass.

Planning verifies all 82 accepted material bindings, the exact A02 frame range,
protected tracked-input manifest, retained water verification and continuation
self-tests. Reuse the existing unchanged-product quality evidence for this prose
slice. Full quality and runtime/device checks have not been run for this proposal.

## One pending decision

Approve or decline the exact `phase9-cask-cover-handling-runtime-integration` scope:
one human environmental cover adjustment, the newly declared wooden lid/cotton
cover, 32-pixel supported access, transient owner/cooldown, contextual controls,
exact A02-C01 slice, independent feedback/notices and the enumerated checks.

The active ledger successor waits for this grant before implementation. Approval
would authorize that complete finite implementation and its dependent verification;
it would not authorize drinking, washing, refill, pickup, laundry actions, new
geometry/art, durable state, perception/provider changes or a release. Standing
publication authority covers this completed proposal and its ledger/handover now.

# First hand-tool action proposal

Prepared 2026-10-11 (Australia/Sydney). Planning is complete; the new implementation
grant is pending. Exact boundaries, inspected-source hashes and required checks are
in [hand-tool-owner-scope.json](hand-tool-owner-scope.json).

## One proposed action

Add **Tap practice block** for the human-controlled Seeker at the existing Village
Edge anchor `(1700,900)`. One explicit activation uses a communal wooden mallet to
make one gentle contact with a wooden practice block, then leaves both at the
site. This is a new fixed environmental tool-use permission and material binding.
It does not repair, harvest, prepare a remedy or change the world-event clock.

Neither tool nor block presently exists as a live inventory/world object. They
are **new proposed declarations**, not facts inferred from the accepted artwork,
the field lens or A03. Approval would authorize this one action-only site and its
wooden mallet/block together. No item enters any inventory, and no take/drop/give
or general-purpose tool-use permission is created. The contextual button and
tooltip identify the supplied mallet/block; this bounded implementation does not
draw them or add a prop, hand animation, collision, navigation cell or landmark.
The absence of a depicted tool is an explicit presentation limit of this scope.

This addresses the remaining “Add footsteps by terrain, water, wind, foliage,
interiors, tools, and remedy preparation” roadmap item by supplying the first
distinct A03 owner. Wider tools, their inventories and consequential actions remain
unfinished. Water, cask-cover, grinding and treatment keep their closed owners.

## What the inspected sources establish

| Source | Current fact | Proposed boundary |
| --- | --- | --- |
| `TemporarySeekerCharacterSource` | Live human Seeker begins with empty inventory | No assumed carried mallet |
| `TemporaryFocusedSearchContextSource` | Separate discovery context supplies a field lens | No manual-tool inventory or action permission follows |
| `TemporaryWorldObjectSource` | Four objects; no mallet or block | No snapshot insertion or reuse of a discovery object |
| Inventory use/take/drop/give policies | Recorded ownership and transfer rules | Keep unchanged; environmental permission is local to this action |
| `CharacterPhysicalAccessPolicy` | Access requires an explicit snapshot | Do not grant access for another subsystem |
| `OpeningRegionProjectionSource` / `Main.tscn` | Exact Village Edge anchor at `(1700,900)` | Bind the retained marker and projection, without moving them |
| `GreyboxNavigation` / village geometry | Existing supported interior and route to Square | Check actual supported reach; no route or wall edit |
| Accepted A03 / excerpt preparation | Two distinct demonstrations; wood-hammer member begins at 4 seconds | Use only that one accepted contact, with its existing gain/fade |

The supplied site is a new immutable `HandToolTapSite`, not a `WorldObject`,
`InventoryOwnership` or physical-access snapshot. Its stable key is
`village-edge-practice-block`; its exact mallet/block declaration is retained in
the site. Only the new controller can issue a request against that exact site.

## Owner, access and lifecycle

Propose `HandToolTapController` with immutable request/state/commit contracts.
A request retains the exact current human character, site and state. Only the
issuing controller's current request may commit; copies, other owners, replaced
characters/sites, duplicates, stale states and sequence overflow are rejected.
Install accepted state and cooldown before exception-isolated notification. Audio
availability or an observer failure cannot undo or repeat a commit.

`OpeningRegionHandToolTapCoordinator` is the sole ordinary producer. On offer and
again immediately before commit require:

- Exact live Seeker/character source, projection, Village Edge marker, navigation,
  turn, audio-panel and save-restoration nodes. Retain projection/anchor identity,
  node visibility, processing, parent/global transforms and marker position.
  Replaced, hidden, disabled or moved source bindings suppress the action.
- Active focused unpaused human play; no text entry, reading/customization overlay
  or nonterminal/reserved shared character turn. Native fresh mouse or
  Tab/Enter/Space activation only; held keys and echo cannot repeat it.
- Ready active unchanged navigation region/map/polygon/transform; finite actual
  positions. Both endpoints must be supported within **1 pixel**. Seeker-to-site
  distance is inclusive **32 pixels**. Sample the entire straight segment at
  intervals no larger than **4 pixels**, including endpoints; every sample must
  be supported. This is a new authored interaction abstraction, not a simulated
  arm/mallet trajectory or another subsystem's physical-access grant.

No existing interaction-focus object is required: this site has no discovery
marker. Its contextual control appears only inside the newly granted supported
radius and identifies the action without pixel hunting. The existing focus
selector and four objects remain unchanged. Availability never reads hidden
sabotage, contamination, discovery, knowledge or inventory state.

Source-only reach example: actor `(1732,900)`, site `(1700,900)`, distance 32.
All nine 4-pixel segment samples lie in retained Village Edge interior cells
whose centres are between `(1700,900)` and `(1732,900)`. Those cells lie within
the navigation interior `[1604,724,1796,996]` and on the Edge–Square route.
This calculation is not live movement proof. Ordinary movement there and back
must pass after the implementation grant. In addition to navigation support,
the unchanged physical sources/transforms must remain bound; the example stays
inside the existing room boundaries and never crosses a wall.

Cooldown is **1 second of bounded active time**, longer than the 0.75-second
feedback. Admit deltas from 0 through 0.25 seconds; invalid deltas cancel feedback
and preserve cooldown. Pause/focus loss freezes cooldown and cancels feedback.
Leaving valid access, teleport over 96 pixels, source/actor/site replacement,
overlay, disablement or teardown cancels only this action's feedback. Successful
load invalidates requests/commits, clears cooldown and cancels without replay;
failed load changes nothing. Consumed commits stay consumed. No save field,
transcript, disease/time, hearing emitter, cue history, LLM intention, music hold
or ducking lease is introduced.

## Feedback and exact extraction

Button: **Tap practice block**. Tooltip: “Use the communal wooden mallet to tap
the wooden practice block once; leave both here.” Own-action status:
**You tap the practice block.** Caption: **Wooden mallet contact**. Status and
the new independently owned eighth caption last 0.75 seconds. Caption follows
the existing captions toggle; status remains available with captions disabled.
Cancellation clears only these new owners. Preserve all seven retained caption
owners and their timers; verify coexistence, wrapping and focus at 1280×720 and
1920×1080. Position the new panel away from retained controls/captions.

One nonlooping local Effects player at 0 dB consumes each fresh exact live commit
once. Retain Master/Effects/range/mono behavior. Missing stream or Effects bus is
silent while commit/status/caption still succeed; no Master fallback. Movement,
arrival, eligibility or button focus alone never generates a contact.

Proposed `A03-T01.wav` is accepted A03 frames **[192000,228000)**, seconds
**[4,4.75)**: the complete wood-hammer demonstration and its existing silent tail.
It excludes the separate `tools_02.ogg` demonstration at 1 second. This is 36,000
stereo PCM24 frames at 48 kHz, 0.75 seconds, 216,000 PCM bytes. Raw-frame SHA256:
`215e78f3197e239156367ab55d680940c0bd7a659b983b8d3c96a9661db9b8ed`.
First and last stereo frames are zero. The preparation record declares 15,032
source frames for the wood-hammer member, wholly inside this window. No gain,
resampling, fade, rearrangement, mixing or accepted-source edit is permitted.
Only a new WAV header/import accompanies the exact slice. Planning hashed it in
memory; no derivative was created and no repeat listening gate was opened.

Reuse rubberduck's accepted local CC0-1.0 provenance for “100 CC0 metal and wood
SFX.” Package new `CREDITS.txt`, exact-extraction `provenance.json` and a byte-exact
copy of the retained CC0 deed; append the source notice to in-game credits.
Explicitly append the new notice-folder export patterns in all three existing
presets. This carries existing recorded provenance, not a new legal/catalogue
assessment or acquisition.

## Implementation files and finite verification

The JSON lists exact proposed paths: four Domain site/action contracts, two
Application owner/access classes, Game site binding/coordinator/panel/audio
consumer, focused tests, a live smoke and stopped output fixture, exact extraction
and output/boundary scripts, one WAV/import and three notices. Modify only
`Main.tscn`, caption/credits panels, notice patterns in `export_presets.cfg` and
root `quality.py`; keep Channel the final child. No existing gameplay owner,
inventory, world-object, navigation, collision, save or provider code changes.

After approval, pin the unchanged current cask validator and its fixtures to the
inspected baseline using the isolated-history runner pattern. Retain all current
product/interior/water/cask stages; add six hand-tool modes, measured output and
successor boundary checks. Frozen validators and `scripts/quality.sh` stay intact.

Completion requires:

1. Focused unit checks for exact issuance/ownership/state/overflow/human guards,
   one-second cooldown, delta bounds, reset/observer isolation and finite
   32-pixel reach/1-pixel support/4-pixel segment checks.
2. Six live schedules: default, Quiet, Master-muted, Effects-muted, missing stream
   and no action. First five have identical commits/full durable saves/transcripts;
   no-action has zero new commits and equal durable bytes. Hash and serialize
   protected inventory/discovery/sabotage/time truth before and after.
3. Twenty actual Master-output cases enumerated in JSON, including attenuation,
   mono, missing bus/stream, rejected/stale requests, access failure, lifecycle
   cancellation and retained caption coexistence. Ordinary arrival without
   activation must be silent. Finite capture thresholds are scoped to the fixture.
4. Ordinary movement, native mouse/Tab/Enter/Space, held/echo rejection, status/
   eighth caption/credits and focus-loss cancellation at both viewports. Verify
   hide/move/replace-source rejection without editing accepted geometry.
5. Resource packaging: 36 streams, 23 individual notices in 17 notice groups;
   exact extracted frames survive the documented PCM24-to-PCM16 arithmetic native
   import conversion and archive extraction; prior resources remain unchanged.
   This is not an executable release.
6. Six rejection probes: changed proposal/PCM, forged commit, changed access/site,
   missing output evidence, mutated protected truth and missing export notices.
   Root `./quality.sh` passes retained and new stages with fixture completion
   strings and a final current boundary check.

Planning checks verify the 82 accepted bindings, source slice, static reach,
protected tracked inputs and continuation self-tests. Reuse passing cask/product
evidence for unchanged product/CI inputs; no full-quality rerun for these documents.
Fresh device listening and ordinary-play numerical loudness remain unclaimed.

## One outstanding owner decision

The new grant would authorize exactly the fixed Village Edge mallet/block
declaration, environmental handling permission, supported reach, transient owner,
contextual controls and unchanged A03-T01 feedback described here, together with
the finite implementation/verification files. Planning/publication authority does
not supply that gameplay/material/access grant. Once granted, record it and
complete the implementation and dependent checks in the same session; do not
request another selection of accepted A03 or reopen a closed scope.

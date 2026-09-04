# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. Its bounded-camera movement, navigation,
deterministic healer-command, and interaction-focus greybox keeps testable movement, framing,
navigation-request, follow-target, command-selection, and focus-selection rules plus future
authoritative rules in engine-independent C# projects while Godot owns input mapping, physics
presentation, camera behavior, collision, navigation topology, and scene authoring.

## Prerequisites

- A 64-bit .NET 8 SDK.
- The Godot 4.6.3 .NET editor, not the standard editor without C# support.
- `actionlint` 1.7.12 or newer for local GitHub Actions validation.

Both executables must be available to the shell as `dotnet` and `godot`. Verify them before
building:

```sh
dotnet --info
godot --version
```

The verified local versions for this foundation are .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official. Root `global.json` keeps `dotnet` in the .NET 8.0.1xx feature band
and permits only the latest installed patch in that band. The Godot SDK version is pinned in
`game/Allrpcs.Game.csproj`. To update the toolchain, change these pins and the prerequisite
versions together, then rerun the complete local quality sequence before committing.

## Local quality checks

From the repository root:

```sh
./scripts/quality.sh
```

The sequence restores packages serially, rejects C# formatting drift, builds with current
recommended .NET analysis and compiler warnings treated as errors, runs the domain, application,
and architecture tests, then exercises both headless and 60-frame graphical Godot launches. The
headless launches wait for the navigation map to synchronize, prove a route through the bridge,
reject representative stream, tree, and village targets, and exercise healer follow, wait,
move completion, invalid and excluded command rejection, separation, and cross-bridge return to
follow. A separate interaction-focus launch proves nearest selection, authored-order
tie-breaking, focus changes, out-of-range clearing, and hidden and disabled exclusions. The
movement-boundary launch uses the existing Seeker body and authored collision shapes to prove all
four outer borders plus representative stream, village, and tree collisions, then proves the
bridge in both directions and open lower terrain remain clear. The sequence first runs
`actionlint` against every hosted workflow. A focused-search presentation launch proves the one
temporary Seeker indicator is hidden by default, follows empty, unrelated, one-match, multi-match,
and hide-again decisions, and leaves supplied inputs intact. A separate controller launch proves
condition-aware capability-only, tool-only, and combined starts; fail-closed absent, unsatisfied,
already-active, and wrong-character starts; unrelated-entry handling; successful cancellation and
completion; presentation changes; and exact input and activity-instance retention. The checks do
not require provider credentials or network access after restore. The project selects the
Compatibility renderer for graphical launches.

To verify formatting alone without changing files, run:

```sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
```

The architecture tests inspect authored project files without loading Godot. They enforce the
project-reference directions listed below.

## Credential-free hosted checks

`.github/workflows/ci.yml` configures pull-request and `main` checks on explicit stable runner
images:

- Pure formatting, zero-warning compilation, domain tests, Application movement and camera
  framing tests, and architecture tests run on `ubuntu-24.04` without installing or launching
  Godot.
- The headless main-scene smoke runs separately on `ubuntu-24.04` with Godot 4.6.3 .NET.
- Unsigned debug exports run natively on `ubuntu-24.04`, Apple Silicon `macos-15`, and
  `windows-2025`. Their targets are Linux x64, macOS Universal 2, and Windows x64.

The workflow grants only `contents: read`, disables checkout credential persistence, defines no
provider inputs or secrets, and uses no dependency cache. Uploaded evidence is named
`UNSIGNED-DEVELOPMENT-*`, retained for seven days, and includes a plain-text limitations
manifest. The macOS job rejects a bundle-level signature and the Windows job rejects an
Authenticode signature before upload. These debug outputs are neither signed releases nor
installers and are not approved for distribution.

Every external action is pinned to a full commit SHA with its verified release tag in a comment:
`actions/checkout` v7.0.1, `actions/setup-dotnet` v6.0.0,
`chickensoft-games/setup-godot` v2.4.2, and `actions/upload-artifact` v7.0.1. Before updating
them, recheck the [supported runner labels][github-runners], action release notes, and Godot
compatibility, update the SHA and tag comment together, then run the complete local quality
sequence.

The workflow was first verified on GitHub-hosted runners in private-repository push run
[`33310947250`][push-run] and pull-request run [`33311261187`][pull-request-run]. The exact
resolved images were Ubuntu 24.04 `20260823.283.1`, macOS 15 arm64 `20260727.0256.1`, and
Windows 2025 with Visual Studio 2026 `20260824.214.3`. All quality, headless smoke, native
export, unsigned-signature rejection, and artifact-upload steps passed in both event types. See
`HANDOVER.md` for job, artifact, pull-request, and limitation evidence. Hosted debug exports
remain build evidence only, not releases.

[github-runners]: https://docs.github.com/en/actions/reference/runners/github-hosted-runners
[push-run]: https://github.com/hherb/allrpcs/actions/runs/33310947250
[pull-request-run]: https://github.com/hherb/allrpcs/actions/runs/33311261187

## Unsigned development exports

With Godot 4.6.3 .NET export templates installed, reproduce one CI export locally from the
repository root:

```sh
./scripts/export-development.sh linux-x64
./scripts/export-development.sh macos-universal
./scripts/export-development.sh windows-x64
```

Each command writes only under `artifacts/unsigned-development/`, which Git ignores. The
committed `game/export_presets.cfg` explicitly disables macOS signing and notarization and
Windows code signing. Linux has no signing step. These commands create development-build
evidence only; they do not package, sign, notarize, publish, or distribute the game.

## Launch the keyboard greybox

```sh
godot --path game
```

Use W, A, S, and D to move the gold placeholder Seeker around the 1600-by-900 test region. The
Seeker's named default speed is 240 pixels per second. Diagonal input is normalized so it does
not move faster than a cardinal direction. The camera follows with a named smoothing speed of
6 and world limits matching the region, so ordinary movement cannot reveal space beyond it.
Its named minimum, default, and maximum zoom tuning values are 0.75, 1.0, and 2.0. Zoom and
camera-pan input are intentionally not mapped yet.

Four thin collision walls keep the Seeker inside the temporary outer border. Simple collision
geometry also blocks the blue stream outside the obvious brown temporary bridge, the complete
grey village block, and a representative three-tree cluster. The bridge is traversable in both
directions and the rest of the path remains open.

The credential-free movement-boundary smoke exercises those authored shapes with deterministic,
test-only Godot physics motions from the existing Seeker body. It verifies collision sweeps and
clear paths without synthesizing keyboard input or claiming final input UX coverage:

```sh
ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path game --quit-after 30
```

A deterministically authored `NavigationRegion2D` describes future companion-pathfinding space
inside the same 1600-by-900 region. Its conservative clearance excludes the outer walls, stream
outside the bridge, village block, and tree cluster. The obvious cyan placeholder healer starts
at the named `HealerStart` position on the Seeker's side of the stream. It follows the Seeker over
that navigation surface at a named 180-pixel-per-second speed only while outside a validated
72-pixel stop distance. Its circular collision matches its 36-pixel visible footprint, so the
healer stops without overlapping the Seeker and can route through the bridge after separation.

The small **TEMP GREYBOX** panel shows the current healer mode. Press X to toggle between follow
and wait. Press M to move the healer to the obvious pink ring; the healer uses the existing
navigation surface, stops within a validated 12-pixel distance, then changes to wait. Press X
again to resume follow. Off-world and obstacle-excluded destinations are rejected without
changing the active command. These two keys, the ring, and the status panel are temporary
behavior-test controls, not final command or dialogue UX.

Two nearby temporary markers exercise interaction focus without performing an interaction. A
bright yellow **FOCUS** ring and the top-left status identify only the nearest visible, enabled
marker inside the inclusive 112-pixel range. Exact distance ties resolve by authored scene order,
so focus is stable at the Seeker's initial midpoint. Moving nearer the other marker changes focus;
moving outside both ranges clears it. A hidden marker and an obvious excluded marker exercise the
fail-closed presentation paths and can never receive the indicator. These markers, labels, range,
ordering, and indicator are temporary greybox behavior evidence, not Phase 2 objects, stable
identifiers, perception state, or interaction UX.

Every shape, color, obstacle, collision outline, navigation polygon, target marker, and status
element is an authored primitive for scale and behavior testing only; none selects final art or
terrain. Close the titled **Allrpcs** window to stop the process. To open the project in the editor
instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` contains pure movement, camera-framing, navigation-request validation,
  healer follow-target selection, healer command selection, and bounded interaction-focus
  selection and coordinates future domain use cases; it depends only on `Domain`.
- `src/Infrastructure` is the future adapter boundary and depends on `Application`.
- `game` is the Godot presentation project and depends on `Application`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Application.Tests` tests pure movement, camera-framing, navigation-request, healer
  follow-target, healer command, and interaction-focus behavior without loading Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

The Godot scene also owns credential-free engine checks for navigation topology, healer-command
movement, interaction-focus presentation, and Seeker collision boundaries. These checks load the
authored scene; they do not move Godot physics or presentation policy into `Application`.

## Stable domain identifiers

`Allrpcs.Domain.Identifiers` defines distinct immutable identifiers for characters, world
objects, locations, evidence, knowledge, and authoritative world events. They share one
canonical-text contract: 1 to 64 characters, beginning with a lowercase ASCII letter and then
containing only lowercase ASCII letters, digits, and single internal hyphens. For example,
`healer`, `village-square-2`, and `patient-stabilized` are canonical. Inputs are validated rather
than normalized, so case, whitespace, underscores, repeated hyphens, leading or trailing hyphens,
and non-ASCII text are rejected.

Each identifier's `Value` and `ToString()` return the same canonical text, and its `Parse` method
round-trips that text. Equality requires both the identifier type and its canonical value, so a
character and an object with the same text remain different identifiers. These types are opaque
keys only: they do not contain mutable entity state, scenario content, Godot types, provider or
persistence details, or serialization-framework behavior.

## Controller types

`Allrpcs.Domain.Controllers.ControllerType` defines exactly four controller types: `human`,
`llm`, `deterministic-test`, and `dormant`. `All` exposes the complete set in that stable order,
while `Value` and `ToString()` expose each type's canonical text. `Parse` accepts only those exact
values and returns the corresponding defined instance; case changes, whitespace, provider names,
and other unknown text are rejected rather than normalized or registered.

These values describe controller categories only. They do not create controller instances,
select providers or models, define prompts or behavior, attach a controller to a character, or
introduce runtime orchestration, serialization-framework, persistence, Godot, or UI behavior.

## Character model boundaries

`Allrpcs.Domain.Characters.CharacterIdentity` pairs a stable `CharacterId` with an authored
display name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16
code units as measured by .NET `string.Length`, and are retained exactly rather than trimmed,
case-folded, or otherwise normalized. The immutable value compares by both identifier and display
name.

`PublicCharacterBackground` and `PrivateCharacterBackground` are separate immutable value types.
Their structural separation lets future filtered views carry a public background without also
carrying private text. Both retain authored text exactly, explicitly permit an empty string when
no background is authored, reject null, and are bounded to 4,000 UTF-16 code units. Private
background is ordinary authored character state: it is not hidden chain-of-thought, a credential
container, arbitrary metadata, or a provider instruction channel.

`CharacterCapability` and `CharacterLimitation` are structurally distinct immutable authored
values. Each has a stable key using the existing 1-to-64-character canonical identifier-text
contract and a nonblank description bounded to 500 UTF-16 code units. Valid descriptions retain
authored whitespace, casing, line endings, and Unicode exactly. `CharacterCapabilitySet` and
`CharacterLimitationSet` explicitly permit an empty sequence, preserve authored entry order,
copy caller input, and reject duplicate keys within their own boundary.

A capability describes authored character state only. It is not permission, an executable
action, an input, an affordance, or proof that an attempt will succeed. A limitation is not a
public/private classification, authorization rule, action validator, or provider prompt rule.

`CharacterControllerAssignment` associates one stable `CharacterId` with exactly one existing
closed `ControllerType`. The immutable value retains both objects exactly and compares by their
values. It records only the character's current authored controller-type association: it does not
create a controller instance, configure a provider or model, define prompts or behavior, route
input, or introduce runtime orchestration or switching policy.

`CharacterCondition` is a qualitative authored value with a canonical key and a nonblank
description bounded to 500 UTF-16 code units. Valid descriptions preserve whitespace, casing,
line endings, and Unicode exactly. `CharacterConditionSet` explicitly permits an empty sequence,
preserves authored order, copies caller input, and rejects duplicate keys using ordinal
comparison. These types do not define disease stages, symptoms, severity, progression, treatment,
timers, death, or other runtime health mechanics.

`CharacterInventory` contains only the character inventory's current ordered references to
existing `ObjectId` values. It explicitly permits an empty sequence, copies caller input, exposes
only a read-only collection, and rejects null or duplicate identifiers. It does not define object
or item state, quantities, stacks, slots, weight, capacity, containers, equipment, ownership,
transfer, visibility, use, crafting, consumption, or executable policy.

`CharacterKnowledge` contains only a character's current ordered references to existing
`KnowledgeId` values. It explicitly permits an empty sequence, copies caller input, exposes only
a read-only collection, and rejects null or duplicate identifiers. It does not define knowledge
content, world truth, evidence, claims, beliefs, confidence, provenance, discovery,
reinterpretation, visibility, perception, or executable policy.

`CharacterBelief` is an authored value with a canonical key and a nonblank statement bounded to
500 UTF-16 code units. Valid statements preserve whitespace, casing, line endings, and Unicode
exactly. `CharacterBeliefSet` explicitly permits an empty sequence, preserves authored order,
copies caller input, exposes no mutable collection, and rejects null entries or duplicate keys.
These types record only what is authored as a character belief. They do not classify a belief as
world truth or knowledge, or define evidence, confidence, provenance, attributed claims,
discovery, reinterpretation, visibility, perception, authorization, or executable policy.

`CharacterRelationship` associates an existing related `CharacterId` with a nonblank authored
summary bounded to 500 UTF-16 code units. Valid summaries preserve whitespace, casing, line
endings, and Unicode exactly. `CharacterRelationshipSet` explicitly permits an empty sequence,
preserves authored order, copies caller input, exposes no mutable collection, and rejects null
entries or duplicate related-character identifiers. These types do not define scores,
dimensions, reasons, history, reciprocity, symmetry, self-relationship policy, graph traversal,
visibility, perception, authorization, or executable policy.

`CharacterGoal` is an authored value with a canonical key and a nonblank description bounded to
500 UTF-16 code units. Valid keys and descriptions are preserved exactly. `CharacterGoalSet`
explicitly permits an empty sequence, preserves authored order, copies caller input, exposes no
mutable collection, and rejects null entries or duplicate keys using ordinal equality. These
types do not define priority, status, progress, deadlines, dependencies, plans, completion,
rewards, visibility, mutation, behavior selection, authorization, or executable policy.

`Character` is the immutable composition root for the complete authored character model. It
retains exactly one existing identity, public background, private background, capability set,
limitation set, controller assignment, condition set, inventory, knowledge, belief set,
relationship set, and goal set. Every component is required, and the controller assignment's
character identifier must equal the identity identifier. The aggregate adds no other cross-field
policy and exposes only its supplied components through get-only properties.

The complete character-model boundary still does not define mutable state, lifecycle or event
handling, runtime registration, relationship or goal mechanics, capability authorization,
behavior selection, perception filtering, dialogue context, persistence, serialization, Godot,
or UI behavior.

## Location model boundaries

`Allrpcs.Domain.Locations.LocationIdentity` pairs a stable `LocationId` with an authored display
name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16 code units
as measured by .NET `string.Length`, and are retained exactly rather than trimmed, case-folded,
or otherwise normalized. The immutable value compares by both identifier and display name.

`LocationDescription` is a structurally separate immutable authored value. It retains text
exactly, explicitly permits an empty string when no description is authored, rejects null, and
is bounded to 4,000 UTF-16 code units. It records authoritative authored location state; it is
not proof that any character can perceive, identify, understand, or otherwise access that text.

`Location` is the immutable composition root for the authored location model. It requires and
retains exactly one existing `LocationIdentity` and one existing `LocationDescription`, exposing
only those supplied components through get-only properties. The aggregate adds no cross-field
policy, methods, or events.

The complete location boundary still does not define hierarchy, containment, connections,
coordinates, regions, navigation topology, terrain, access rules, environmental state, mutable
occupancy, lifecycle or event handling, filtered views, visibility, perception, discovery,
persistence, serialization, Godot, or UI behavior.

## World-object model boundaries

`Allrpcs.Domain.WorldObjects.WorldObjectIdentity` pairs a stable `ObjectId` with an authored
display name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16 code
units as measured by .NET `string.Length`, and are retained exactly rather than trimmed,
case-folded, or otherwise normalized. The immutable value compares by both identifier and display
name and remains structurally separate from character and location identities.

`WorldObjectDescription` is a structurally separate immutable authored value. It retains text
exactly, explicitly permits an empty string when no description is authored, rejects null, and is
bounded to 4,000 UTF-16 code units. It records authoritative authored world-object state; it is not
proof that any character can perceive, identify, understand, or otherwise access that text.

`WorldObject` is the immutable composition root for the authored world-object model. It requires
and retains exactly one existing `WorldObjectIdentity` and one existing
`WorldObjectDescription`, exposing only those supplied components through get-only properties.
The aggregate adds no cross-field policy, methods, or events.

The complete world-object boundary still does not define item properties, tags, portability,
quantities, stacks, containers, inventory mechanics, ownership, use, location assignment,
containment, mutable state, lifecycle or event handling, filtered views, visibility, perception,
discovery, persistence, serialization, Godot, or UI behavior.

## Item model boundaries

`Allrpcs.Domain.Items.Item` classifies exactly one existing immutable `WorldObject` as an item.
It retains and exposes that supplied instance through one get-only property without duplicating
the world object's identifier, name, or description.

This classification boundary does not define item properties, tags, portability, quantities,
stacks, containers, inventory mechanics, ownership, use, mutation, lifecycle, events,
interactions, location assignment, persistence, serialization, Godot, or UI behavior.

## Environmental-state model boundaries

`Allrpcs.Domain.EnvironmentalStates.EnvironmentalState` is a qualitative authored value with a
canonical key and a nonblank description bounded to 500 UTF-16 code units. Valid descriptions
preserve authored whitespace, casing, line endings, and Unicode exactly. It remains structurally
separate from `CharacterCondition`.

`EnvironmentalStateSet` contains existing environmental states in stable authored order. It
explicitly permits an empty sequence, copies caller input, exposes a read-only collection, and
rejects null input, null entries, and duplicate canonical keys using ordinal equality.

These boundaries do not assign environmental states to locations or objects or define weather,
lighting, terrain, hazards, transitions, mutation, lifecycle, events, time, simulation,
visibility, perception, discovery, persistence, serialization, Godot, or UI behavior.

## Perception-snapshot boundaries

`Allrpcs.Domain.Perception.VisibilityState` defines exactly `unnoticed`, `noticed`,
`identified`, `understood`, `altered`, and `collected`. `All` exposes those singleton values in
that stable roadmap order. `Value` and `ToString()` expose canonical text, and `Parse` accepts
only that exact text and returns the corresponding defined instance.

`WorldObjectVisibility` associates exactly one existing `ObjectId` with one existing
`VisibilityState`. It requires and retains both supplied instances and compares associations by
their values. It adds no default state or cross-field policy.

`CharacterPerceptionSnapshot` associates exactly one existing `CharacterId` with world-object
visibility entries in stable supplied order. It retains the supplied character identifier and
entry instances, explicitly permits an empty sequence, copies caller input, exposes a read-only
collection, and rejects null input, null entries, and duplicate object identifiers. It adds no
implicit `unnoticed` entries, default state, or cross-entry policy.

These boundaries do not define filtered character views, character-aggregate integration,
progression, transitions, discovery, reinterpretation, permissions, actions, events, inventory
ownership, mutation, persistence, serialization, Godot, or UI behavior.

## Inventory boundaries

`Allrpcs.Domain.Inventory.InventoryOwnership` associates exactly one existing `ObjectId` with
exactly one existing owner `CharacterId`. It requires and retains both supplied instances and
compares associations by their values.

`InventoryOwnershipSnapshot` contains existing ownership associations in stable supplied order.
It explicitly permits an empty sequence, copies caller input, exposes a read-only collection,
retains each supplied entry instance, and rejects null input, null entries, and duplicate object
identifiers. It adds no implicit ownership or default owner.

`InventoryCapacity` associates exactly one existing `CharacterId` with a non-negative maximum
count of distinct owned world objects. It retains the supplied character identifier, explicitly
permits zero capacity, and compares capacities by both values. It does not enforce that capacity
against any ownership snapshot or define a default or unlimited policy.

`InventoryCapacitySnapshot` contains existing capacities in stable supplied order. It explicitly
permits an empty sequence, copies caller input, exposes a read-only collection, retains each
supplied entry instance, and rejects null input, null entries, and duplicate character
identifiers. It adds no implicit capacity, lookup behavior, or cross-snapshot policy.

`InventoryCapacityPolicy.CanOwnAdditionalObject` is a pure, fail-closed decision for one new
distinct ownership entry. It finds capacity by value-equal character identifier, counts only
ownership entries for that character, and permits the addition exactly while that count is below
the recorded maximum. It does not accept an object identifier or mutate either snapshot.

`InventoryTakePolicy.CanTakeObject` is a pure eligibility decision for one existing character and
one existing world object. It rejects an object already owned by any character and otherwise
delegates to `InventoryCapacityPolicy.CanOwnAdditionalObject` for the intended owner. It returns
only eligibility and does not mutate either snapshot or execute a transfer.

`InventoryDropPolicy.CanDropObject` is a pure eligibility decision for one existing character and
one existing world object. It permits the drop exactly when the ownership snapshot records the
value-equal object identifier as owned by the value-equal intended character. It returns only
eligibility and does not mutate the ownership snapshot or execute the drop.

`InventoryGivePolicy.CanGiveObject` is a pure eligibility decision for one existing giver, one
distinct existing recipient, and one existing world object. It requires the giver to own the
object and delegates recipient capacity eligibility to
`InventoryCapacityPolicy.CanOwnAdditionalObject`. It returns only eligibility and does not mutate
either snapshot or execute the transfer.

`InventoryUsePolicy.CanUseObject` is a pure eligibility decision for one existing character and
one existing world object. It permits use exactly when the ownership snapshot records the
value-equal object identifier as owned by the value-equal intended character. It returns only
eligibility and does not mutate the ownership snapshot or execute, consume, equip, or alter the
object.

These boundaries do not define character-inventory integration, capacity reservation, quantities,
stacks, slots, weight, containers, equipment, use execution, crafting, consumption, transfer
execution, location, possession history, visibility, discovery, permissions, actions, events,
mutation, persistence, serialization, Godot, or UI behavior.

## Discovery-requirement boundaries

`Allrpcs.Domain.Discovery.DiscoveryCapabilityRequirement` associates exactly one existing target
`ObjectId` with one required canonical character-capability key. It retains the supplied object
identifier instance, validates the capability key with the shared canonical-identifier contract,
preserves that key exactly, and compares requirements by their values.

`Allrpcs.Domain.Discovery.DiscoveryToolRequirement` associates exactly one existing target
`ObjectId` with one existing required-tool `ObjectId`. It retains both supplied identifier
instances and compares requirements by their values. The value deliberately adds no policy that
the target and required tool must be different objects.

`DiscoveryCapabilityRequirementSet` contains existing capability requirements in stable authored
order. It copies caller input, retains the exact supplied requirement instances, permits an empty
sequence, exposes only a read-only collection, and rejects null input, null entries, and
value-equal duplicate target-object identifiers.

`DiscoveryToolRequirementSet` provides the equivalent boundary for existing tool requirements.
It preserves stable authored order, copies caller input, retains the exact supplied requirement
instances, permits an empty sequence, exposes only a read-only collection, and rejects null input,
null entries, and value-equal duplicate target-object identifiers.

`DiscoveryConditionSet` composes one existing `DiscoveryCapabilityRequirementSet` and one existing
`DiscoveryToolRequirementSet`. It retains the exact supplied set instances through get-only
properties, accepts their existing empty-set behavior, and permits the same target-object
identifier to occur once in each set.

`Allrpcs.Domain.Discovery.DiscoveryCapabilityPolicy.IsSatisfied` is a pure, fail-closed evaluation
of one capability requirement against one existing `CharacterCapabilitySet`. It returns true
exactly when a capability key equals the required key by ordinal value, independent of authored
descriptions, order, and unrelated capabilities, and it does not mutate either input.

`Allrpcs.Domain.Discovery.DiscoveryToolPolicy.IsSatisfied` is the equivalent pure, fail-closed
evaluation of one tool requirement against one existing `CharacterInventory`. It returns true
exactly when the inventory contains the required tool-object identifier by value, independent of
the target object, authored order, and unrelated entries, and it does not mutate either input.

`Allrpcs.Domain.Discovery.DiscoveryConditionPolicy.IsSatisfied` is the pure one-target combined
decision. It finds capability and tool requirements by value-equal target-object identifier,
ignores unrelated targets, fails closed when neither kind is authored, delegates a sole authored
requirement to its existing policy, and requires both existing policies to pass when both kinds
are authored for the target. It rejects null inputs and does not mutate them.

`Allrpcs.Domain.Discovery.FocusedSearchRequest` identifies exactly one existing searcher
`CharacterId` and one existing target `ObjectId`. It retains both supplied identifier instances
through get-only properties and compares requests by their identifier values. The request does
not start or evaluate a search.

`Allrpcs.Domain.Discovery.FocusedSearchActivity` denotes that one existing
`FocusedSearchRequest` is active. It retains the exact supplied request instance through a
get-only property and compares activities by request value. The activity does not start,
progress, complete, cancel, schedule, or evaluate a search.

`Allrpcs.Domain.Discovery.FocusedSearchActivitySnapshot` contains existing focused-search
activities in stable supplied order. It permits an empty sequence, copies caller input, retains
the exact supplied activity instances, exposes a read-only list, and rejects nulls plus duplicate
requests by value. It permits one character to have activities for distinct targets because the
snapshot records state rather than enforcing start eligibility.

`Allrpcs.Domain.Discovery.FocusedSearchStartPolicy.CanStartSearch` is the pure start-eligibility
decision. It permits a request exactly when the activity snapshot contains no existing activity
for a value-equal searcher character identifier. Different characters may search the same target.
It rejects null inputs, ignores unrelated activities, and neither starts a search nor mutates its
inputs.

`Allrpcs.Domain.Discovery.FocusedSearchEligibilityPolicy.CanStartSearch` is the pure combined
eligibility decision. It permits a request only when the existing start policy permits it and the
existing discovery-condition policy is satisfied for the request's target object. It rejects null
inputs, fails closed for missing or unsatisfied target conditions and an already-active searcher,
inherits the delegates' unrelated-entry behavior, and does not mutate any input.

`Allrpcs.Domain.Discovery.FocusedSearchStartTransition.StartSearch` is the pure start transition.
It delegates eligibility to the start policy, rejects an ineligible request, and otherwise returns
a new activity snapshot that retains existing activity instances in stable order before one new
activity retaining the exact supplied request instance. It does not mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchCancellationPolicy.CanCancelSearch` is the pure
cancellation-eligibility decision. It permits cancellation exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It rejects
null inputs, ignores unrelated activities, and neither cancels a search nor mutates its inputs.

`Allrpcs.Domain.Discovery.FocusedSearchCancellationTransition.CancelSearch` is the pure
cancellation transition. It delegates eligibility to the cancellation policy, rejects an
ineligible request, and otherwise returns a new activity snapshot that removes the activity for
the complete value-equal request while retaining every other activity instance in stable order.
It does not mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchCompletionPolicy.CanCompleteSearch` is the pure
completion-eligibility decision. It permits completion exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It rejects
null inputs, ignores unrelated activities, and neither completes a search nor mutates its inputs.

`Allrpcs.Domain.Discovery.FocusedSearchCompletionTransition.CompleteSearch` is the pure
completion transition. It delegates eligibility to the completion policy, rejects an ineligible
request, and otherwise returns a new activity snapshot that removes the activity for the complete
value-equal request while retaining every other activity instance in stable order. It does not
mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchActivityProjection.ForCharacter` is the pure
per-character activity projection. It returns a new snapshot containing every activity whose
request has a value-equal searcher character identifier, retaining the existing activity instances
in stable order. It permits no matches and multiple distinct requests, ignores unrelated
activities, rejects null inputs, and does not mutate either input.

`Allrpcs.Application.Discovery.FocusedSearchActivityIndicatorPolicy.ShouldShow` is the pure
per-character activity-indicator decision. It delegates selection to the existing activity
projection and returns true exactly when the new projected snapshot contains at least one entry,
including multiple distinct matching requests. It returns false for empty or unrelated snapshots,
rejects null inputs, and does not mutate either input.

`Allrpcs.Game.FocusedSearchActivityIndicator.Present` is the temporary Godot presentation adapter
under the existing Seeker. It accepts an existing character identifier and activity snapshot,
delegates the complete decision to the Application policy, and changes only the visibility of one
authored cyan in-world ring. The ring is hidden by default, and ordinary play does not construct or
inject focused-search activity.

`Allrpcs.Game.FocusedSearchActivityController` is the sole live owner of the existing Seeker's
immutable focused-search activity snapshot. It initializes an empty snapshot from the Seeker's one
authored stable character identifier and rejects requests for other characters. Its start path
accepts existing discovery conditions, capabilities, and inventory, delegates the complete start
decision to the combined Domain eligibility policy, and only then delegates state creation to the
existing start transition. Cancellation and completion retain their existing policy and transition
delegation. The controller replaces and presents its snapshot only after a successful operation;
ordinary play does not construct a request or invoke those operations, so the credential-free
controller smoke is the only current caller.

The discovery-condition boundaries do not inspect a complete character, ownership or capacity
snapshots, perception or world state, grant discovery, or change visibility. They do not define
configurable Boolean expressions, alternative requirement groups, further requirement types, tool
classifications or properties, passive noticing, reinterpretation, actions, results, errors,
events, effects, mutable state, persistence, serialization, Godot, or UI behavior. Beyond the pure
combined eligibility decision, the focused-search boundaries add no discovery grant, visibility
change, complete-character or world inspection, replacement, range, duration, location, progress,
outcome, display text, status value, further lifecycle policy, in-place mutation, scheduling,
persistence, production activity source, or final UI behavior. The temporary Godot adapter changes
only its one authored indicator's visibility, and the temporary controller owns only its immutable
snapshot, condition-aware start delegation, and existing cancellation and completion delegation.

Point-and-click movement, zoom or camera-pan input, controller mappings beyond the temporary
healer-command keys, interaction actions or outcomes, Phase 2 objects and perception models,
natural-language commands, final command or interaction UI, LLM-controlled companions, providers,
persistence, credentials, signing, installers, and distribution configuration are not part of
this greybox slice.

## Licence

Allrpcs is licensed under the GNU Affero General Public License version 3 only
(`AGPL-3.0-only`). See `LICENSE` for the complete terms.

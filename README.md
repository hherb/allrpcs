# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. Its bounded-camera movement, navigation,
deterministic healer-command, and interaction-focus greybox keeps testable movement, framing,
navigation-request, follow-target, command-selection, and focus-selection rules plus future
authoritative rules in engine-independent C# projects while Godot owns input mapping, physics
presentation, camera behavior, collision, navigation topology, and scene authoring.

The provider-neutral dialogue foundation retains exact in-world speech, audiences, declared act
types, requests, responses, intentions, commitments, and bounded state proposals. Its first pure
deterministic character behavior selects fixed accept, refuse, clarify, or conditionally-accept
responses solely from the declared act type. Immutable commitment progress advances only one
value-equal pending intention per observation. Character-turn activity can represent awaiting,
speaking, completed, and cancelled states, with pure cancellation, response-reception, and
completion eligibility and transition rules. A focused Godot controller owns one exact activity,
delegates those transitions, and exposes an explicit player cancellation action for awaiting and
speaking turns. Character-turn transcript entries structurally distinguish exact spoken utterances
from exact non-wait physical intentions, while immutable snapshots and a pure append transition
preserve occurrence order
without treating response proposals as executed actions. A character-turn world-event occurrence
now retains an exact authoritative event together with one exact tagged transcript occurrence.
Pure eligibility and transition rules require that exact authored event, then atomically record its
identifier, apply its declared qualitative-time effect, and append the tagged occurrence while
retaining the exact occurrence in the result. Response-proposal execution remains later work.
The live greybox now derives the first in-range healer request from the exact occurred player event
using an explicitly supplied act type and starts one awaiting-response activity. A separate
credential-free temporary source can select the existing deterministic response for that exact
request without installing or presenting it.

The first live-model boundary remains provider-neutral through Application: one exact affordance
snapshot produces the bounded context entries shown to a model and validates the completed response
returned for that turn. The generation controller applies one safe text-free retry and synthesizes
caller cancellation or deadline timeout without allowing any terminal failure to carry a response.
Opt-in redacted diagnostics observe provider attempts plus controller-synthesized cancellation and
timeout through caller-owned sinks, time sources, and independent diagnostic cancellation. Fixed
content-free presentation states cover generating, completed, cancelled, timed-out, and failed
streams. The scene now owns one exact immutable stream lifecycle, projects bounded text deltas
separately from those content-free states, and applies asynchronously observed updates only on the
Godot main thread. Completed validated responses enter only their exact awaiting turn; explicit
player cancellation cancels both generation and turn, while timeout or failure cancels the pending
turn without fabricating dialogue. Ordinary heard healer requests now use the authored LLM
assignment, exact bounded input, and a scene model source that consumes one exact caller-supplied
model for the next generation or falls back to one stable offline model. A completed terminal flow
can reset to a fresh empty stream for a later exact request without retaining prior presentation or
content. Credential-free ordinary-path checks prove one validated target-free `Wait` response
speaks exactly once, a hallucinated target is rejected without speech or mutation, and caller
cancellation preserves only already-accepted stream text while cancelling the exact turn.

Infrastructure contains caller-composed OpenAI Responses, Anthropic Messages, and loopback-only
Ollama HTTP adapters with stateless strict structured output, bounded streaming completion, and
optional terminal token diagnostics. Immutable provider settings retain one explicit selection and
support immutable replacement, append, selection, and unselected-provider removal. They resolve only
a binding that shares the exact configured settings object. Redacted connection tests use that same
selection, and a strict versioned JSON boundary contains only canonical provider IDs and bounded
model names. Infrastructure factories compose each provider's exact model and tester over the same
caller-owned dependencies without credential, network, discovery, or scene input/output. The
ordinary scene loads one persisted selection into shared settings/composition state, while macOS
Keychain, Linux Secret Service, and Windows Credential Manager OpenAI credential stores are present.
Settings and credential UI remain explicit and do not save on edit. Anthropic credential storage
and generated actions beyond the single known herb-patch examination flow remain later work.

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
infrastructure, and architecture tests, then exercises both headless and 60-frame graphical Godot
launches. The headless launches wait for the navigation map to synchronize; the interaction-focus
check also
requires the authored healer move destination to resolve onto that map before exercising input.
They prove a route through the bridge, reject representative stream, tree, and village targets, and
exercise healer follow, wait,
move completion, invalid and excluded command rejection, separation, and cross-bridge return to
follow. A separate interaction-focus launch proves nearest selection, authored-order
tie-breaking, focus changes, out-of-range clearing, hidden and disabled exclusions, distinct
authored object identifiers, exact focused identifier-instance retention, null and mismatched
world-object request rejection, and eligible focused-search starts through exact or distinct
value-equal world objects for both visible targets. The mapped F path resolves the focused identifier
through the world-object controller and rejects a missing object without invoking the start path or
changing surrounding state. The same launch proves focus-independent cancellation and
world-object-gated outcome-aware completion through the exact retained active requests, exact
outcome recording, explicit world-object-bound first-recorded Seeker outcome resolution,
mismatched-object rejection, target-only perception replacement, outcome consumption, unchanged
inputs and world-object state, missing-object completion and resolution failure-state identity,
activity-indicator presentation, and deterministic changed/unchanged result text for every
lifecycle action. Resolved-perception checks prove that each exact looked-up object's authored
description appears only for the exact identified visibility singleton, including through a
distinct value-equal object with replacement text. Null, absent, unrelated, and non-identified
inputs clear and hide the reused label without changing source objects, controller snapshots,
marker presentation, or lifecycle result text. The same lifecycle evidence reaches both authored
temporary descriptions and hides a missing object again. It also proves that
released, unrelated, movement, and healer input preserve all controller snapshots and the exact
prior text, and that the temporary focused-search source stably authors one
capability-only first-world-object context and one tool-only second-world-object context from the
existing world-object snapshot, retains their exact nested instances, fails explicitly for an
undersized snapshot, and forwards its exact aggregate instances through the input path. The
temporary Seeker perception-source launch proves exact candidate, Seeker, and world-object
identifier alignment, value-equal target lookup with exact source-identifier retention, explicit
missing-target failure, stable order, target-only `Unnoticed` seeding, and unchanged inputs and
surrounding state. The separate perception-controller launch proves the ordinary resolved scene
state, then isolates exact one-time source initialization, wrong-character, repeated, and null
rejection while preserving activity, outcome, and presentation state. The passive candidate-source
launch proves exact ordinary production and atomic resolution consume the one retained candidate,
change only its live perception target to `Noticed`, and leave source perception, focused-search,
and presentation state unchanged. The observation-reinterpretation controller launches prove empty
ownership, authoritative exact/value-equal production, stable first-eligible trigger-aware
resolution, target-only identification, complete-candidate consumption, fail-closed mismatches and
nulls, and unchanged passive-noticing, focused-search, and presentation state. Its source launch
proves exact character, second-world-object, and trigger-knowledge identifier retention plus the
ordinary one-time production/resolution path and source-state preservation. The
outcome-resolution-controller launch
proves value-equal recorded-outcome and matching value-equal world-object resolution, target-only
perception replacement, consumption, stable unrelated-entry order and instances, unchanged inputs,
and fail-closed mismatched and null rejection without changing activity or presentation. The
world-object-controller launch proves exact lookup of both scene-authored objects, including through
a value-equal distinct identifier, absent and uninitialized results, null rejection, stable exact
snapshot retention, and unchanged source, targets, focused-search state, and presentation. The
world-event source, controller, and startup-recording launches prove two independently authored,
stable events in source order, exact `greybox-ready` startup selection and recording, and unchanged
initial qualitative time and surrounding state. The focused-search world-event launch drives the
ordinary F/E/R input path against the authored herb patch. It proves start and completion leave the
event log and qualitative time unchanged, then successful resolution consumes the exact outcome,
identifies and presents the herb, records the exact `herb-patch-identified` event, and advances
qualitative time. Repeated or newly completed but ineligible resolution preserves the exact
focused-search, world-event, world-time, and presentation state. The world-time controller launch
retains its focused boundary checks.
The movement-boundary launch uses the existing Seeker body and authored collision shapes to prove
all four outer borders plus representative stream, village, and tree collisions, then proves the
bridge in both directions and open lower terrain remain clear. The sequence first runs
`actionlint` against every hosted workflow. A focused-search presentation launch proves the one
temporary Seeker indicator is hidden by default, follows empty, unrelated, one-match, multi-match,
and hide-again decisions, and leaves supplied inputs intact. A separate controller launch proves
condition-aware capability-only, tool-only, and combined starts; fail-closed absent, unsatisfied,
already-active, and wrong-character starts; unrelated-entry handling; successful cancellation and
completion; presentation changes; and exact input and activity-instance retention. The checks do
not require provider credentials or network access after restore. The save-document storage launch
uses only a process-specific system-temporary file and proves exact persisted restoration, missing
and failed reads, malformed JSON rejection, replacement recovery, cleanup, and preservation of
unrelated scene state. The project selects the
Compatibility renderer for graphical launches.

To verify formatting alone without changing files, run:

```sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
```

The architecture tests inspect authored project files without loading Godot. They enforce the
project-reference directions listed below.

The quality sequence also runs the pure representative discovery-scenario validator. It exits
nonzero if its five ordered evidence definitions contain broken ownership or object references,
unavailable requirements, or evidence that cannot reach its required visibility through the
authored initial, capability, tool, passive-noticing, or knowledge-triggered routes:

```sh
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
```

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

The small **TEMP** panel shows the current controls, healer mode, interaction focus, and last
explicit focused-search attempt. Press F to start a focused search only when the current valid
interaction focus resolves to an existing world object, C to cancel the Seeker's active search, E
to complete it, or R to resolve the first retained Seeker outcome; these temporary search controls
are detailed below. Press X to
toggle between follow and wait. Press M to move the
healer to the obvious pink ring; the healer uses the existing navigation surface, stops within a
validated 12-pixel distance, then changes to wait. Press X again to resume follow. Off-world and
obstacle-excluded destinations are rejected without changing the active command. These keys, the
ring, and the status panel are temporary behavior-test controls, not final command or dialogue UX.

Two nearby temporary markers exercise interaction focus without performing an interaction. A
bright yellow **FOCUS** ring and the top-left status identify only the nearest visible, enabled
marker inside the inclusive 112-pixel range. Exact distance ties resolve by authored scene order,
so focus is stable at the Seeker's initial midpoint. Moving nearer the other marker changes focus;
moving outside both ranges clears it. A hidden marker and an obvious excluded marker exercise the
fail-closed presentation paths and can never receive the indicator. These markers, labels, range,
ordering, and indicator are temporary greybox behavior evidence. Each marker also authors one
canonical object-identifier text value, which its Godot adapter parses once while ready. The focus
controller exposes the exact identifier instance for only the currently selected visible, enabled,
in-range marker and clears it with focus. Its request factory also receives an existing nullable
world object, rejects null or a target mismatch, accepts a distinct value-equal identifier, and
retains the matching object's exact identifier instance. Its start-attempt boundary supplies the
activity controller's exact Seeker identifier to that factory, then passes an existing
discovery-condition set, capability set, and inventory unchanged to the condition-aware start path
only for a created request. It returns false without invoking that path when focus is clear or the
object is absent or mismatched. This bridge does not make the
markers authoritative Phase 2 objects or add perception state, search input, search execution, or
interaction UX.

After a successful temporary R resolution, the existing small cyan label beneath only the matching
herb patch or stone marker presents that exact looked-up world object's authored description. The
label is cleared and hidden for every state other than exactly identified. It reflects the
controller's retained Seeker perception, not current interaction focus, and is temporary behavior
evidence rather than final discovery UI.

Every shape, color, obstacle, collision outline, navigation polygon, target marker, and status
element is an authored primitive for scale and behavior testing only; none selects final art or
terrain. Close the titled **Allrpcs** window to stop the process. To open the project in the editor
instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` contains pure movement, camera-framing, navigation-request validation,
  healer follow-target selection, healer command selection, bounded interaction-focus and dialogue
  hearing selection, and the storage-independent save-document port; it depends only on `Domain`.
- `src/Infrastructure` owns JSON and local-file save adapters and depends on `Application`.
- `game` is the Godot presentation and composition project and depends on `Application` and
  `Infrastructure`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Application.Tests` tests pure movement, camera-framing, navigation-request, healer
  follow-target, healer command, interaction-focus, dialogue-hearing, and save-document port
  behavior without loading Godot.
- `tests/Infrastructure.Tests` tests serialization and isolated local-file behavior without loading
  Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

The Godot scene also owns credential-free engine checks for navigation topology, healer-command
movement, interaction-focus presentation, and Seeker collision boundaries. These checks load the
authored scene; they do not move Godot physics or presentation policy into `Application`.

## Save-schema version boundary

`Allrpcs.Domain.Persistence.SaveSchemaVersion` is the engine-independent immutable root version
discriminator for later versioned saves. Version `1` is the explicit first supported value.
Construction accepts positive integers, while `Parse` accepts only their invariant canonical
decimal text. Whitespace, signs, leading zeroes, zero, negative values, overflow, and non-numeric
text are rejected rather than normalized. `Value`, `ToString()`, parsing, and record equality are
stable round-trip boundaries.

`SaveSchemaVersionSupportPolicy.CanLoad` separately accepts only version `1`, including a distinct
value-equal instance. Version `2` now has a complete JSON contract but remains outside this live-load
gate until scene-owned character memory can be restored atomically with discovery and world-event
state. Later positive versions remain valid version values but fail closed. The policy leaves its
input unchanged and does not parse, migrate, serialize, or load anything.

These version boundaries do not themselves define a save aggregate, serializer, file or path
policy, storage provider, migration, load orchestration, autosave, replay, Godot integration,
credentials, or UI.

`Allrpcs.Domain.Persistence.DiscoverySaveState` is the first immutable save-state composition. It
retains the exact existing character-perception, focused-search activity, focused-search outcome,
passive-noticing candidate, and observation-reinterpretation candidate snapshots through get-only
properties. It rejects null snapshots and adds no cross-snapshot validation, copying, projection,
transition, serialization, restoration, or controller behavior.

`WorldEventSaveState` separately retains the exact authoritative event-log and qualitative
world-time snapshots. Authored world-event definitions remain outside this dynamic save state so a
later restoration boundary can validate saved identifiers against the running scenario. The value
rejects null snapshots and adds no consistency decision, event replay, time advancement,
serialization, restoration, or controller behavior.

`SaveSnapshot` is the immutable in-memory root. It retains one exact schema version, discovery
save state, world-event save state, canonical all-character memory, and the version-3 first-patient
composition through get-only properties and rejects invalid version/component combinations. Its
legacy version-`1` constructor supplies one shared immutable empty-memory snapshot, and version `1`
rejects populated memory. Record equality continues to use the version value and retained state
instances.

`SaveSnapshotLoadPolicy.CanLoad` is the pure root-level version gate. It delegates to
`SaveSchemaVersionSupportPolicy`, accepts versions 1 through 3, preserves the root and every nested
instance, and does not inspect state contents, migrate, deserialize, or restore.

`WorldEventSaveStateRestorationPolicy.CanRestore` is the pure event-state restoration gate. Every
saved log identifier must resolve against an exact authored world-event definition, and projecting
those resolved events must reproduce the saved qualitative time by identifier value. Empty initial
state is valid; undefined identifiers, missing advances, stale advances, no-advance identifiers,
and unrelated time identifiers fail closed. The policy leaves all inputs unchanged and does not
replay events, advance time, replace state, deserialize, or access storage.

`DiscoverySaveStateRestorationPolicy.CanRestore` is the pure discovery-state restoration gate for
one supplied searcher. The perception owner plus every focused-search activity, outcome,
passive-noticing candidate, and reinterpretation candidate owner must be value-equal to that
searcher. Every perception or lifecycle target must resolve against the authoritative world-object
snapshot. Empty owned collections and distinct value-equal identifiers are accepted; mismatched
owners and missing objects fail closed. The policy does not evaluate discovery conditions,
knowledge triggers, visibility changes, lifecycle eligibility, serialization, or controller state.

`SaveSnapshotRestorationPolicy.CanRestore` is the complete pure root gate. It validates every input
before delegating unchanged to the schema-version, discovery-state, world-event-state, and
first-patient restoration boundaries. Version-3 eligibility additionally requires every saved
disease, infection, preparation recipe, input and result ingredient, ordered step, quantity unit,
administration, and experiment event to resolve compatibly against the exact authored treatment
context and world events. It returns only a Boolean and does not deserialize, migrate, replay,
replace controller state, choose a file, or access storage.

`FocusedSearchActivityController.TryRestoreDiscoveryState` is the first live save-state restoration
boundary. It delegates complete eligibility to `DiscoverySaveStateRestorationPolicy` using the
controller's Seeker and a supplied authoritative world-object snapshot. Success installs the save
state's exact perception, focused-search activity and outcome, passive-noticing candidate, and
observation-reinterpretation candidate snapshots together, then presents only the restored activity
through the existing indicator. Rejected and repeated attempts preserve the installed snapshots.
This operation does not restore world events, coordinate a complete save root, deserialize, migrate,
access storage, or orchestrate startup loading.

`WorldEventController.TryRestoreWorldEventState` is the matching live event-state restoration
boundary. It validates an existing save state against the controller's exact initialized authored
event definitions, then installs the save state's exact event-log and qualitative-time snapshots
together while retaining those definitions. Unsupported logs, inconsistent time, uninitialized
controllers, and null input fail without changing controller state. The operation does not replay
events, coordinate discovery restoration, deserialize, migrate, access storage, or orchestrate
startup loading.

`SaveSnapshotRestorationController.TryRestore` coordinates all live boundaries for one
already-materialized `SaveSnapshot`. It completes root preflight and constructs the rebound
first-patient graph before changing any controller. Success atomically installs discovery,
world-event, memory, and first-patient state while retaining authored definitions and presenting
only the restored focused-search activity. Rejected roots do not partially alter any controller.
This remains an in-memory seam: it does not parse JSON, choose a file or path, autosave, replay
events, or run automatically during startup.

`SaveSnapshotCaptureController.Capture` is the matching scene-owned in-memory capture seam. It
returns no snapshot until discovery perception, authored world events, the event log, and qualitative
time plus all first-patient state are initialized. Each successful call creates fresh
`DiscoverySaveState`, `WorldEventSaveState`, `FirstPatientSaveState`, and version-3 `SaveSnapshot`
compositions while retaining the controllers' exact current nested snapshots. Capture does not
mutate or present controller state and does not serialize, choose a file or path, access storage,
migrate, replay, autosave, or orchestrate startup loading.

`Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonSerializer` writes compact canonical JSON for
versions 1 through 3 while preserving the exact legacy documents. Version 2 adds canonical
all-character memory; version 3 adds complete disease truth, completed preparations, remaining
batches, administration observations, and universal experiment records. Stable document-local
references reconnect shared memories, preparations, and events without serializing runtime object
references. Provider identifiers, credentials, provider state, hidden reasoning, and an additional
authoritative transcript are absent. Serialization preserves every input and does not write a file,
choose a path, validate a scenario, migrate, encrypt, or restore runtime state.

`SaveSnapshotJsonDeserializer` is the strict inverse for all three known JSON versions. It accepts only
each version's complete closed object shape and exact JSON value kinds, rejecting malformed JSON,
comments, trailing commas, duplicate, missing, or unknown properties, non-canonical versions and
identifiers, unknown closed values, invalid collections, and unresolved memory references.
Successful reads materialize fresh immutable Domain values in serialized order while reconnecting
document-local shared identities. `DeserializeCurrent` migrates version 1 through version 2 to
version 3 using the caller-owned exact initial first-patient composition, and leaves an existing
version-3 root unchanged. Scenario eligibility and controller mutation remain separate; neither
entry point accesses storage.

`SaveSnapshotJsonController` is the scene composition seam for serialized in-memory round trips.
`CaptureSerialized` delegates exact live capture to `SaveSnapshotCaptureController` before canonical
serialization. `TryRestoreSerialized` supplies exact authored initial first-patient state for legacy
migration, rejects invalid or incompatible documents, and delegates the current materialized root to
`SaveSnapshotRestorationController`, whose complete scenario preflight prevents partial restoration.

`Allrpcs.Application.Persistence.ISaveDocumentStore` is the storage-independent single-document
port. It accepts only a non-empty already-serialized document and returns explicit `Saved` or
`Failed` write results. Reads distinguish `Loaded`, `Missing`, and `Failed`; only `Loaded` carries
the exact non-empty document. The port exposes no path, slot, Godot, JSON, provider, or credential
type.

`FileSaveDocumentStore` is the local Infrastructure adapter. It requires one trusted absolute path,
writes strict UTF-8 without a byte-order mark to a unique sibling temporary file, flushes it before
same-directory replacement, and reports expected I/O, access, encoding, and platform failures
without treating empty or invalid UTF-8 data as a loaded save. `SaveDocumentStorageController`
composes that adapter at the fixed Godot `user://save-v1.json` path with exact capture and complete
preflighted restoration. It exposes callable save/load operations but does not invoke them from
startup or input, add slots or user-selected paths, migrate, encrypt, compress, sync, replay, handle
credentials, or provide final save UI. Its smoke uses a process-specific system-temporary file and
deletes it after the check, so validation cannot overwrite an ordinary user save.

## Discovery-scenario validation vocabulary

`DiscoveryEvidence` maps one exact evidence identifier to its authoritative world object, minimum
visibility threshold, and explicit criticality. `DiscoveryEvidenceSnapshot` retains stable authored
order while rejecting null entries and duplicate evidence identifiers. `DiscoveryScenario` composes
the exact observer, authoritative objects, evidence definitions, initial perception, focused-search
conditions, bounded available capabilities, tool inventory, knowledge, passive-noticing candidates,
and reinterpretation candidates needed by the next pure validator. These immutable values do not
yet author the five representative discoveries, validate cross-references or reachability, filter a
character view, or change scene behavior.

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
a read-only collection, and rejects null or duplicate identifiers. Separate `KnowledgeEntry`
values associate an exact existing identifier with nonblank authored text bounded to 4,000 UTF-16
code units. `KnowledgeSnapshot` retains those entries in stable duplicate-free identifier order,
and `KnowledgeLookup` returns the exact value-equal match. These contracts do not classify content
as world truth, evidence, a claim, or a belief, and do not define confidence, provenance,
discovery, reinterpretation, visibility, perception, relevance ranking, or executable policy.

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

`RepresentativeHermitCharacterSource` creates exactly two fresh LLM-controlled hermits in stable
woodland-then-riverside order. Each composition aligns one exact identity with its controller
assignment and carries a distinct non-empty private background, knowledge reference, belief, and
goal. The returned collection is read-only, and the source adds no scheduling, generation,
provider, autonomous-action, sabotage, scene, persistence, or presentation behavior.

`RepresentativeSageCharacterSource` creates one fresh LLM-controlled village sage whose authored
capabilities are limited to comparing presented evidence and challenging unsupported assumptions.
Explicit limitations prevent the role from using unavailable evidence or converting advice into
authoritative truth, while one private background, knowledge reference, belief, and goal supply
bounded character state. The source does not select hints, evaluate evidence, mutate an
investigation, schedule dialogue, call a provider, or add runtime behavior.

`RepresentativeEnemyAgentCharacterSource` creates one fresh LLM-controlled covert role with a
private national order, ordinary cover, bounded influence capabilities, and explicit physical,
informational, authoritative-action, and investigation-recovery limitations. Competing delay and
civilian-safety goals plus one authored belief make changed allegiance representable without
implementing an allegiance transition. These values grant no action authority and add no
sabotage, scheduling, provider, scene, or persistence mechanics.

`RepresentativeInfectedCharacterSource` creates exactly two fresh dormant roles in stable
forester-then-scribe order. Each exact identity is shared with its dormant controller assignment
and carries distinct latent background, capability, knowledge, and goal state plus an authored
deep-sleep condition and dormant-response limitation. The source does not define authoritative
disease state, healing, wake-up or controller transitions, dialogue, scenes, or persistence.

The complete character-model boundary still does not define mutable state, lifecycle or event
handling, runtime registration, relationship or goal mechanics, capability authorization,
behavior selection, perception filtering, dialogue context, persistence, serialization, Godot,
or UI behavior.

## Dialogue model boundaries

`DialogueUtterance` retains one exact typed `CharacterId` speaker and the player's exact nonblank
Unicode text without trimming, normalization, case folding, or line-ending changes. Text is bounded
to 2,000 UTF-16 code units. `DialogueAudienceSnapshot` retains that exact speaker plus at most 32
exact, duplicate-free listener identifiers in stable supplied order; it copies caller input and
rejects the speaker as a listener.

`CharacterTurnSpeakerSchedulingPolicy` orders at most 32 distinct eligible speaker identifiers by
placing value-equal directly addressed candidates first while retaining stable eligible order and
the exact eligible instances inside both priority partitions. Absent addresses are ignored, input
collections are unchanged, and the returned schedule is an independent read-only collection. The
policy does not infer addresses, decide whether anyone should speak, start turns, generate text,
schedule interjections, or control runtime work.

`CharacterTurnParticipationKind` defines exactly respond, interject, and remain-silent choices in
stable order. One immutable `CharacterTurnParticipationDecision` retains an exact scheduled
character identifier and exact kind. Interjection and meaningful silence require one exact existing
authored goal as their motive, while a direct response cannot retain a motive. The pure acceptance
policy additionally requires the character's exact identity and exact retained goal, permits a
response only when directly addressed, and permits an interjection only when not directly
addressed. From at most 32 distinct, already-accepted decisions in scheduled order, the pure
selection policy returns the exact first response or interjection and returns no selection when all
characters remain silent. A complete-round policy accepts at most 32 distinct exact scheduled
characters, one positionally aligned decision per character, and at most 32 distinct direct
addresses. It uses value-based address membership while preserving exact character, decision, and
authored-goal identity, validates every decision before selection, and returns that existing first
speaking decision or no selection. One immutable provider-neutral participation-consideration
request retains an exact scheduled `Character`, exact heard `DialogueEvent`, and explicit direct-
address flag only when that character's identifier is present by value among the event listeners.
Distinct value-equal audience identity is accepted without replacing any supplied or nested
instance. These contracts do not generate or infer motives or addresses, assemble model context,
invoke models, start or cancel turns, impose interjection frequency or conversation-loop bounds,
integrate scenes, or alter player interruption behavior.

`AutonomousConversationBudget` retains a nonnegative completed-character-turn count, a positive
configured maximum no greater than the existing 16-turn recent-conversation window, and whether the
player has interrupted the chain. Pure continuation and transition rules permit another turn only
below that maximum and before interruption, record exactly one completion at a time, and make
interruption monotonic. A focused scene controller owns one exact immutable budget and delegates
those transitions. A pure composition policy validates a complete participation round first and
returns its exact selected speaker only while that budget permits another autonomous turn, so an
exhausted or interrupted chain cannot expose a follow-up selection. It does not transition the
budget, create a conversation loop, schedule or start speakers, infer participation, or wire
ordinary player input.

The pure Application `DialogueHearingSelector` accepts one exact speaker, a finite world position,
a finite positive inclusive range, and at most 32 uniquely identified finite-position candidates.
It excludes the speaker and returns every in-range listener in stable candidate order, retaining
exact identifiers. It does not inspect visibility, perception, obstacles, line of sight, acoustic
properties, scene nodes, or Godot state.

`DialogueEvent` immutably associates an exact utterance with a value-matching audience.
`DialogueActType` defines only the eight canonical roadmap categories, and `DialogueAct` pairs one
exact utterance with one explicitly declared type. These values perform no text classification;
prompt-shaped text remains ordinary in-world speech and gains no instruction or action authority.

The temporary bottom dialogue panel submits either Return or its button through the focused
`DialogueInputController`. The controller initializes once with the exact scene Seeker identifier,
constructs `DialogueUtterance` directly so the Domain remains the sole text validator, and exposes
only the exact last accepted utterance. Rejected null, blank, or oversized text preserves the last
accepted value; successful repeated submissions create distinct occurrences and clear the field.

`Main` observes each fresh accepted utterance exactly once. `DialogueEventController` delegates the
scene's Seeker and healer positions and the named 192-pixel inclusive range to
`DialogueHearingSelector`, then retains the exact resulting in-world event. The scene's empty
`CharacterTurnTranscriptController` appends that same utterance as structurally tagged spoken
speech through `CharacterTurnTranscriptAppendTransition`. When no turn is active, the temporary
request source gates the exact healer against that event's audience, pairs the event's exact
utterance with the explicitly supplied `request` act type, and starts one exact awaiting-response
activity. Awaiting and speaking activity cannot be replaced; after completion or cancellation, a
later heard utterance can start a fresh exact activity. No text classification occurs.

`CharacterTurnRequest` is provider-neutral and retains one exact responding character, dialogue
event, and act. It rejects a responder outside the event audience and rejects an act for a
different utterance. The initial `CharacterIntentionType` vocabulary is limited to `wait`,
`examine-object`, and `collect-object`. `CharacterIntention` retains that type and an optional exact
target `ObjectId`, including invalid shapes so `CharacterIntentionValidationPolicy` can return the
closed missing-target or unexpected-target error without mutating input. The policy does not look
up world objects or authorize or execute an action.

`CharacterTurnDisposition` is the closed exact-singleton vocabulary `accept`, `refuse`, `clarify`,
and `conditionally-accept`. `CharacterCommitment` defensively retains one non-empty ordered sequence
of exact proposed intentions without selecting, advancing, observing, or executing a step.
`CharacterStateProposal` retains only a canonical key and exact nonblank proposed value bounded to
500 UTF-16 code units. It is proposed character-owned text, not world truth or mutation authority.

`CharacterTurnResponse` retains its exact request and disposition, optional exact bounded speech
from the value-matching responder, exactly one immediate intention, a copied nullable-free list of
commitments, and a copied stable list of at most eight state proposals. It does not validate or
execute intentions, advance commitments, apply proposals, or inspect world state.

The pure deterministic response selector maps only the declared dialogue-act type to fixed
accept, refuse, clarify, or conditionally-accept behavior. Commitment progress advances one
value-equal pending intention per observed action. Character-turn activity contracts represent
awaiting, speaking, completed, and cancelled states. Pure reception accepts only a complete
response retaining the exact awaited request and produces speaking activity; pure completion
accepts only speaking activity. Both transitions retain exact request and response instances.
Pure start eligibility permits a fresh awaiting activity only when current activity is absent,
completed, or cancelled, and its transition retains the exact new request without changing the
prior immutable activity.

The separate deterministic examination selector accepts an exact request and explicitly supplied
target identifier. It produces a fresh accepted response with fixed healer speech, an exact-target
`examine-object` immediate intention, and no commitments or state proposals. The temporary live
response source uses that selector with the exact authored herb-patch identifier; it does not infer
the target from dialogue text.

Pure immediate-intention occurrence policy requires the exact speaking response, responder,
non-wait action, latest observed response speech, and authoritative eligible world event before
delegating atomic event/time/transcript recording. `CharacterExaminationResult` can retain only a
targeted observed examination occurrence and one closed `safe-to-collect` or
`unsafe-to-collect` result. The deterministic post-examination selector proposes collection of the
exact examined target only for the safe result and otherwise waits. A separate pure collection
occurrence gate additionally requires the exact examination occurrence to remain latest in the
transcript before atomic authoritative collection recording. These contracts do not infer an
examination result from speech, descriptions, or hidden world state.

The pure deterministic examination-result selector returns a fresh `safe-to-collect` result only
when one observed occurrence is a targeted `examine-object` action retaining the exact explicitly
supplied herb-patch identifier. Distinct value-equal identifiers and malformed occurrences fail
closed. The temporary result source delegates only that exact boundary. Separate authored advancing
events identify the healer's herb-patch examination and collection without changing the object.

The focused Godot `CharacterTurnActivityController` initializes once from one exact activity and
delegates fresh starts, cancellation, response reception, and speaking completion to those Domain
contracts. The live scene begins without an activity. On a later process pass, a separate temporary
response source delegates the exact awaiting request to the pure deterministic selector and the
controller receives that fresh complete response as speaking activity. The dialogue panel reveals
only that already complete exact speech, one Unicode text element per process pass. A displayed
prefix has no response or action authority.

While the response is speaking, a temporary recorder emits its exact healer utterance once through
the existing hearing boundary and appends that same utterance once to the transcript. It does
nothing for awaiting, completed, or cancelled activity. A separate completion controller first
observes that exact event and transcript entry, preserves a one-frame speaking cancellation window,
and only then delegates completion. The temporary examination flow separately occurs the exact
response intention through the authoritative event/time/transcript coordinator. On the following
pass it selects the exact safe result, then selects and occurs collection only while the exact
examination remains the latest transcript occurrence. Intervening speech therefore prevents
collection. Consecutive heard player utterances still produce distinct completed activities after
the prior one is terminal.

Transcript entries structurally distinguish exact spoken utterances from exact non-wait physical
intentions. Immutable transcript snapshots and append transitions retain exact entries in stable
occurrence order. `CharacterTurnWorldEventOccurrence` links one exact authoritative `WorldEvent`
definition to one exact tagged transcript entry without inferring execution from a response
proposal. Its pure policy accepts only the exact event instance retained in the authored snapshot
and delegates existing world-event eligibility. The pure transition records that exact event
identifier, applies its authored qualitative-time effect, appends the exact transcript occurrence,
and returns all resulting immutable state together with the retained occurrence. Failed
eligibility changes nothing.

The completed deterministic Phase 3 loop does not classify dialogue text, stream provider content,
apply state proposals, infer an examination target or general safety, advance live commitments, add
provider contracts, or mutate inventory, world objects, discovery, or movement from a response.
Examination and collection are authoritative recorded occurrences only; the herb patch remains in
the live world and no item is added to an inventory.

## Live character-model boundaries

The Application layer now owns the provider-neutral `ICharacterModel` seam. It accepts one exact
`CharacterModelGenerationRequest`, observes caller cancellation, and exposes an asynchronous stream
of closed Application update types. Exact non-empty text deltas permit whitespace-only fragments,
retain text without normalization, and are individually and cumulatively bounded to
`DialogueUtterance.MaximumTextLength`. An immutable stream prefix preserves exact updates in stable
order and rejects unknown updates, excessive combined text, multiple terminal outcomes, or any
update after termination.

One terminal update distinguishes completed, caller-cancelled, timed-out, and failed generation.
Failures carry only a bounded provider-neutral code for unavailable, authentication, rate-limit,
transport, invalid-response, or provider-rejection outcomes; provider error bodies and DTOs do not
enter Domain state. The generation controller enforces one caller-supplied deadline across the
whole operation, translates caller cancellation and deadline expiry into distinct terminal states,
and rejects a stream that ends without a terminal update as an invalid response. It permits one
retry only for unavailable, rate-limited, or transport failures that emitted no text; it never
retries exposed partial text, authentication, invalid responses, provider rejection, cancellation,
or timeout. An opt-in progress observer receives accepted text deltas and only the final validated
terminal update; retryable text-free failures stay hidden and observer failure or independent
cancellation cannot alter generation. The scene queues those observations across the async boundary,
applies them on the Godot main thread, and presents text without parsing or executing it. The
authored healer's canonical LLM assignment routes ordinary heard player requests through the exact
bounded generation input and a scene-owned model source. The source consumes one exact
caller-composed model for the next generation only, then resumes its stable credential-free
unavailable default. One scripted ordinary response proves exact target-free `Wait` speech can be
validated, recorded once, and completed without physical authority; the next request resets and
fails offline without retaining the prior result. Prompt-shaped dialogue remains untrusted when a
scripted response targets a hallucinated object: validation replaces that completion with a bounded
invalid-response failure and no speech or mutation. Explicit ordinary cancellation preserves an
already-accepted text delta only as presentation and cancels the exact pending turn. Additional
credential-free ordinary checks prove a hidden text-free unavailable attempt safely retries the
same exact request, a non-retryable provider rejection runs once, and the scene's 30-second deadline
produces a distinct timeout. An unterminated stream with exposed text cannot retry and becomes a
bounded invalid response. Two consecutive caller-composed models receive distinct exact turns,
with stream presentation reset and both speeches recorded once in order. Every unsuccessful path
cancels only its pending turn, exposes only canonical status plus accepted presentation text, and
does not fabricate dialogue, fallback, action, or authoritative mutation. The deterministic
response source remains explicit focused-test scaffolding only. No real provider call occurs in
standard validation. Ordinary gameplay exposes only one targeted model affordance and executes it
only after the completed response passes the existing provider-neutral validation boundary.

Provider-neutral model context is an immutable stable sequence of typed text entries, bounded to
128 entries, 4,000 UTF-16 code units per entry, and 32,000 code units in total. Its closed kinds
separate filtered perception, relevant knowledge, goals, memories, recent speech, valid affordances,
and untrusted player dialogue. The assembler derives identified perception, the exact responding
character's current authored knowledge, and goals, then retains only separately supplied memory
entries, derives recent local speech, retains only separately supplied valid-affordance entries,
and appends exactly one untrusted player-dialogue entry. Perception projection exposes only
non-empty authored descriptions for exact
`VisibilityState.Identified` entries backed by an authoritative world object; all other visibility
states, identifiers, display names, and absent objects remain hidden. Knowledge projection follows
the character's ordered references, excludes unreferenced authored content, and fails closed when
a referenced entry is missing. Both preserve exact text instances. Goal projection preserves exact
authored description text and order without exposing goal keys or other character state.
Recent-speech projection preserves only exact utterance text from supplied dialogue events the
character spoke or was recorded as hearing; unrelated dialogue is omitted. The assembler rejects
mismatched character/perception inputs, null supplied entries, and every kind other than exact
`Memory` or exact `ValidAffordance` in its corresponding supplied input. Generation requests reject
missing, duplicate, changed, or merely value-equal replacement player text. The assembler does not
derive memories or valid affordances, claim current affordance eligibility, define a retained
conversation store or window, or call a provider.

Complete provider-neutral responses pass one pure Application validation gate before they can be
accepted. The gate requires the exact caller-expected turn request, delegates existing structural
validation for the immediate intention and every future commitment intention, and rejects targeted
intentions whose value-equal identifier is absent from the authoritative world-object snapshot.
The immediate intention also must be `Wait` or match an exact type and value-equal target pair in an
immutable current-affordance snapshot. Current eligibility is deliberately not projected onto
future commitments. Errors retain the exact offending intention in deterministic response order;
the response, requests, intentions, world objects, and affordances remain unchanged. The
affordance snapshot is still supplied by a trusted caller rather than derived from gameplay state or
serialized into model context.

Phase 5 begins with provider-neutral local memory contracts rather than provider-owned state. A
`CharacterRecentConversationWindow` retains up to 16 exact completed character turns and 16,000
UTF-16 code units of their heard and response speech in stable oldest-to-newest order. Each entry
has a canonical `CharacterTurnId`; identifiers are unique per window, and pure append evicts only
the oldest turns needed to restore both bounds. Construction and transition copy their collections,
retain exact supplied character, activity, and turn instances, and reject incomplete activities,
mismatched characters, and duplicate turn identities.

Significant episodic memories remain separate from both the recent window and authoritative truth.
Each `CharacterEpisodicMemory` has a canonical memory identity, one exact remembering character, a
bounded unique participant list that includes that character, an exact location, an exact
qualitative-world-time snapshot, completed-turn provenance, and a non-blank summary of at most
1,000 UTF-16 code units. A copied read-only per-character snapshot preserves up to 128 exact memory
instances in supplied order with unique identities. These initial contracts do not yet derive,
retrieve, persist, summarize, migrate, or project memory into model context, and do not define UI
or scene ownership.

Epistemic memory is also local and provider-neutral. Each `CharacterEpistemicMemory` has a canonical
identity, one exact remembering character, one exact completed source turn, bounded statement text,
and exactly one closed category: confirmed fact, attributed claim, or uncertain belief. Only an
attributed claim accepts—and requires—an exact speaker. A copied per-character snapshot retains at
most 256 exact entries in stable unique-identity order. This structural separation records the
character's classification; it does not establish authoritative world truth or derive memory from
dialogue.

An uncertain belief alone can have a `CharacterBeliefAssessment`. It retains the exact belief,
stored confidence from 0 to 10,000 basis points, and independently ordered supporting and
contradicting `EvidenceId` references. Each disposition is copied, duplicate-free, capped at 32,
and cannot contain an identity from the other disposition. The assessment does not calculate
confidence, reinterpret evidence, or promote or demote epistemic categories.

`CharacterRelationshipMemory` stores one directional character-to-character view over the closed
trust, affection, respect, and fear dimensions, each bounded from -100 to 100. Recorded reasons
have canonical identities, exact completed-turn provenance, one retained dimension, and bounded
text. Dimensions and reasons are copied in stable order; dimensions and reason identities are
unique, reasons can reference only retained dimensions, each relationship retains at most 64
reasons, and each per-character snapshot retains at most 64 distinct related characters. These
contracts do not update values, imply reciprocity, or yet bound change across an exchange.

One `CharacterRelationshipChange` can alter one retained dimension by only 1 to 10 points in either
direction and carries one exact locally authored reason whose source turn identifies the exchange.
The pure eligibility policy requires value-equal directional ownership, an existing dimension,
available reason capacity, a unique reason and source turn, and an in-range result; it rejects
overflow instead of clamping. The immutable transition replaces only that dimension, appends the
exact reason, and retains unrelated dimension and reason instances. A source turn can therefore
affect a relationship only once, and no ordinary exchange can reverse a strongly held value.

Unfinished character commitments are stored separately as promises, assigned tasks, or an explicit
other kind. Each `CharacterCommitmentMemory` has a canonical identity, exact responsible character,
exact counterparty, exact completed source turn, and bounded description. A copied per-character
snapshot preserves at most 128 exact commitments in stable unique-identity order. There is no
completion transition, scheduling, execution policy, or provider-side state in these contracts.

Deterministic memory indexing uses four closed local categories—episodic, epistemic, relationship,
and commitment—and four closed canonical tag kinds: topic, entity, location, and relationship. A
category-safe reference retains exactly one existing memory and its exact owner; each index entry
copies 1 to 16 unique tags in supplied order, and a per-character index copies at most 576 exact
entries with unique category-specific memory identities. The index does not copy memory content or
derive, summarize, persist, or project any memory. A per-character query copies 1 to 8 unique exact
tags and requests 1 to 32 results. Pure retrieval requires all query tags, distinguishes the same
value under different tag kinds, returns exact entries in index order, and stops at the requested
bound without scoring, ranking, fuzzy matching, or input mutation.

Older-conversation summaries remain separate from both the bounded recent window and the
authoritative transcript. Each local `CharacterConversationSummary` has a canonical identity, one
exact remembering character, 1 to 32 copied unique exact source-turn identities in supplied order,
and non-blank text of at most 2,000 UTF-16 code units. A copied per-character snapshot retains at
most 128 exact summaries with unique identities. These contracts do not derive or generate text,
persist, migrate, index, retrieve, or project summaries. A pure recording transition accepts only
source-turn identities present in the recent window in the same relative order, appends the exact
caller-supplied summary, and returns the exact recent window and authoritative transcript unchanged;
it neither edits nor evicts either conversation source.

`CharacterMemoryAggregate` is the canonical local provider-neutral memory state for one character.
It retains the exact recent window, older summaries, episodic and epistemic memories, belief
assessments, relationships, unfinished commitments, and deterministic index while requiring
value-equal ownership throughout. Every indexed reference must retain the exact memory instance in
its corresponding category snapshot; a missing, merely value-equal replacement, or same-valued
memory from another category fails closed. The aggregate has no provider conversation identifier,
provider response state, reasoning, or chain-of-thought field. Provider requests remain stateless
and reasoning-free, so no provider-owned state can replace this local canonical aggregate.

`CharacterMemorySnapshot` copies zero to 32 exact per-character aggregates into stable supplied
order, rejects null entries and value-equal duplicate character identities, and exposes only a
get-only read-only collection. The in-memory save root can retain this exact snapshot under the
version-`2` character-memory vocabulary. A pure one-way migration accepts only a version-`1` root
and produces a fresh version-`2` root retaining the exact discovery, world-event, and shared empty
memory instances. Strict current-schema loading executes that migration before complete restoration;
already-version-`2` roots retain their fully materialized memory. The scene owns one exact immutable
memory snapshot, starts explicitly empty, captures canonical version-`2` JSON, and installs memory
with discovery and world-event state only after one complete preflight. Rejected roots preserve all
three state areas, and legacy documents cannot silently discard populated memory. This boundary does
not synthesize memory, complete commitments, change relationships, add save UI, or alter the
authoritative transcript.

One pure Application projection now selects only the value-equal responding character's exact
unfinished commitments from canonical memory. It emits stable `Memory` entries containing only the
closed commitment kind, counterparty identity, completed source-turn identity, and exact description;
missing character memory yields no entries, while null or over-budget input fails closed through the
canonical memory and existing model-context bounds. The temporary scene supplies those entries from
the scene-owned memory controller to the existing context assembler. A credential-free ordinary-flow
smoke serializes a healer promise, resets and restores canonical memory, and proves the same bounded
promise and source reach distinct consecutive OpenAI- and Anthropic-bound requests after an exact
future-turn binding replacement, without supplying the complete campaign transcript. Providers still
cannot own, rewrite, derive, summarize, complete, or otherwise mutate canonical memory.

Phase 6 begins with hidden engine-owned disease truth rather than character belief, memory, dialogue,
or provider state. Canonical `DiseaseId` values identify up to 32 exact immutable authored disease
definitions. `DiseaseTruthSnapshot` copies up to 256 exact character-disease infections in stable
order, permits multiple distinct diseases per character, rejects duplicate character/disease pairs,
and requires every infection to retain the exact definition instance in the authoritative snapshot.
The closed progression vocabulary is exposed, early illness, drowsing, deep sleep, wasting, and
final decline.

Pure disease progression replaces only one exact retained infection and advances exactly one stage.
It requires the exact authored `Advance` world event to be retained, recorded, and current, and each
infection retains the exact last progression-event identity so the same meaningful-time event cannot
be consumed twice. Final decline cannot progress. A separate Application selector yields a dormant
effective controller from deep sleep onward without replacing the configured assignment; before deep
sleep or without matching infection it returns that assignment by exact instance. Qualitative
condition projection keeps exposure invisible and emits only fixed nonnumeric appearance text for
later stages, without disease identity or model-context wiring. Disease persistence, scene ownership,
automatic progression, and authored-scene ownership remain unimplemented.

Successful cure is an explicit immutable outcome retaining the exact authoritative infection and
exact authored world event; it is never inferred from provider text or a qualitative condition. A
pure eligibility policy requires that infection, event definition, and most recently recorded event
identity. The transition removes only the cured infection while retaining exact definitions and every
unrelated infection in stable order. The existing effective-controller selector consequently returns
the exact configured assignment after the last deep infection is removed, while any other deep
infection keeps the character dormant.

Authored disease exposures likewise retain the exact character, disease definition, and world event.
Eligible exposures require that event to be the most recently recorded occurrence and append one exact
`Exposed` infection only when the disease and event are canonical, the character is not already
infected, capacity remains, and no matching active protection exists. Protective-measure definitions
link an exact disease to a discovery-evidence identity; applying one requires that exact definition,
the evidence in the exact character view, and the most recently recorded authored application event.
Protection and infection state stay bounded and immutable. There is still no automatic spread,
duration or removal of protection, scene ownership, persistence, recipe, ingredient, experiment,
journal, UI, or provider wiring.

Patient assessment is a pure, fail-closed Application projection over engine-owned truth. Immutable
requests and character-to-character perception inputs retain exact assessor and patient identities.
Eligibility requires a distinct patient, matching perception, the exact assessor character, an
authored `assess-patients` capability, and no more than 32 inspected capabilities. Eligible results
contain only the existing fixed qualitative condition vocabulary for the perceived patient; exposure
and disease identities remain hidden, and equal symptoms from multiple diseases are deduplicated.
The result is an observation, not a diagnosis, fact, belief, or treatment authority.

Each infection now retains one closed supportive-care state: unassisted, comforted, or stabilized.
Explicit comfort and stabilization outcomes retain the exact infection, stronger care state, and
authored world event. Their pure policy requires that infection and event by exact instance and that
event as the most recently recorded occurrence; the transition replaces only the cared-for
infection. Comfort leaves meaningful-event progression active, while stabilization blocks further
progression. Successful cure remains the separate explicit outcome that removes the infection.
No provider text or qualitative observation can apply care or cure.

Canonical `IngredientId` values identify authored ingredients independently of physical world
objects. One immutable ingredient definition retains that exact identity. Ingredient quantities use
only positive bounded integers in the closed count, milligram, or millilitre vocabulary; units never
convert implicitly. One immutable ingredient state retains the exact definition and quantity plus
closed qualitative freshness, pre-preparation physical condition, and contamination values. There
is no decay clock, state transition, inventory, scene, persistence, provider, or discovery wiring.

Ingredient preparation has a closed grinding, heating, steeping, cooling, and fermentation
vocabulary. A plan copies one to 16 exact steps in authored order and permits repeated process kinds.
Immutable progress exposes only the exact next step; a pure policy and transition advance it exactly
once, and distinct value-equal or out-of-order steps fail closed. Canonical preparation-recipe
identities distinguish authored recipes. A recipe retains one exact plan, one to 16 ordered exact
input requirements with unique ingredient identities, and one exact result definition and quantity.
Requirements and ingredient-state snapshots are copied, read-only, bounded, and duplicate-free;
amounts must use the same exact unit and never convert implicitly.

A pure availability policy requires every recipe input by exact definition instance, exact unit, and
sufficient integer amount. It deliberately does not invent freshness, physical-condition, or
contamination requirements. Pure consumption removes depleted states, replaces only partially
consumed states in their original positions, and preserves unrelated exact instances. Starting a
recipe atomically consumes eligible inputs and creates progress at step zero. An activity can advance
only through its exact next authored step, and only complete progress produces the exact
recipe-authored result outcome. The result is not automatically added to ingredient storage.

A dosage retains one exact completed preparation outcome and a positive quantity no larger than that
outcome in the same exact unit. An authored administration outcome retains exact administrator and
patient identities, that exact dosage, and one exact meaningful world event; it does not itself
validate event occurrence or change disease truth. Self-administration is not prohibited at this
structural boundary. A reusable prepared batch begins with the exact completed preparation and its
full exact quantity. Pure consumption accepts only a dosage retaining that exact preparation and
unit with no more than the remaining amount; partial use returns a new batch with the exact
preparation and reduced positive quantity, while complete use returns no remaining batch. Rejected
dosage leaves the input batch unchanged. A bounded immutable prepared-batch snapshot retains up to
256 distinct exact preparation instances. Its pure consumption transition finds only the dosage's
exact preparation, replaces a partially consumed batch in place, removes a fully consumed batch, and
preserves every unrelated exact batch in stable order. One combined administration transition
preflights that batch, the exact latest authored event, and both journal boundaries before returning
new batch state, the rich administration-specific record, and the universal experiment record as one
externally atomic result. Rejection exposes no partial state. The transition still does not occur the
event, apply disease truth or consequences, own scene state, or persist the snapshots.

Ingredient-administration experiment records retain a canonical experiment identity, one exact
administration outcome, and one exact qualitative condition observation. Empty observations remain
valid and do not mean "no effect." The bounded immutable journal preserves exact records in authored
order, rejects duplicate experiment identities, and provides a pure append transition. Recording is
now atomically gated on the administration outcome's exact authored world event being the most recent
authoritative occurrence. The transition constructs the exact record only after that check and then
delegates duplicate and capacity enforcement to the existing journal append transition. It does not
infer efficacy, consume dosage, mutate disease truth, or provide scene ownership or persistence, so
the richer administration-specific record remains available for treatment logic.

A separate universal experiment journal can record every kind of authored experiment rather than
only ingredient administration. Each immutable record retains an exact experiment identity, the
exact authoritative world event, and bounded nonblank procedure and observed-outcome text without
interpreting efficacy. The journal copies up to 256 records in stable order and rejects duplicate
experiment or event identities. Its pure atomic recorder accepts any exact authored world event only
when that event identity is the newest authoritative log entry; stale or value-equal replacement
events, duplicates, and capacity exhaustion leave the input journal unchanged. Scene ownership and
persistence remain separate later boundaries.

Investigation contracts keep open questions, hypotheses, evidence, conclusions, and consequences
separate. Questions, hypotheses, conclusions, and non-executable consequences retain canonical
identities plus bounded exact authored text; hypotheses remain possible answers rather than facts.
Evidence links use only the closed `supports` or `contradicts` relationship and do not bypass the
existing character-filtered evidence view. An authored conclusion plan contains one to eight
distinct alternate routes, each requiring one to 16 unique evidence identities. A pure confirmation
policy succeeds only when one whole route is visible and never pools partial alternate routes.
Confirmation atomically removes the exact open question and appends the exact authored conclusion;
it does not parse natural language, execute consequences, reveal hidden evidence, or change gameplay
state.

A hypothesis-recognition foundation canonicalizes a proposed question and hypothesis identity
against exact open-question and authored-hypothesis snapshots while retaining the exact source
natural-language utterance. Its output remains only a possible answer: it does not create a
conclusion, evidence, belief, fact, or gameplay mutation. A provider-neutral Application request
retains that exact utterance plus the exact bounded open-question and authored-hypothesis snapshots.
One cancellable interpreter may propose only a question/hypothesis identity pair or no match; the
controller delegates every proposal to the Domain canonicalization policy.

The first caller-composed Infrastructure interpreter serializes only that bounded state through a
narrow supplied transport seam. Its deterministic JSON marks the exact source text as untrusted
in-world text and carries only the exact open-question and hypothesis identities and authored
statements. Its strict response boundary accepts either an explicit no-match or one canonical
question/hypothesis identity pair; malformed, extra, duplicate, oversized, unknown-shape, and partial
documents fail without retaining provider text. Unknown but well-formed identities reach the existing
canonicalization policy and fail closed. Construction and the standard suite perform no network or
credential access. No provider binding, retry, scene ownership, or persistence exists yet.

Critical-conclusion route validation associates each evidence identity with a distinct authored
source and whether that source requires character cooperation. Every critical plan must have at
least two complete routes using at least three distinct sources, include one route independent of
cooperation, and retain a complete route after any single source is removed. The representative
first-patient investigation has four sources and two disjoint routes: abandoned-laundry residue plus
an old-shrine inscription, or an assistant preparation note plus a cooled-dose observation. This
recoverability is reused by the representative first-patient scenario.

The representative first-patient scenario retains an exact sleeping-patient identity, hidden
grey-sleep truth at deep sleep and without care, available exact moonwort leaf and clean-water state, one
grind/heat/steep/cool recipe, its stabilizing-remedy link, and the recoverable critical investigation.
A pure structural policy requires the exact patient infection and disease, exact recipe ingredient
definitions and units, and an acceptable critical-route scenario. Value-equal replacement patient,
disease, remedy, ingredient, and conclusion objects fail closed.

Stabilization additionally requires a distinct exact remedy-to-conclusion evidence requirement and
the exact authored conclusion in current confirmed state. Only an administration using the exact
patient, exact completed recipe, latest exact authored event, and an exact retained prepared batch
with enough quantity can delegate to the existing care and experiment-execution transitions. One
atomic first-patient result returns newly stabilized disease truth, the dose-consumed prepared-batch
snapshot, and newly appended administration-specific and universal experiment journals. Authored
procedure and qualitative outcome text is bounded by the universal journal and never interpreted as
efficacy. Depleted or replacement batches, duplicate or full journals, stale events, wrong
conclusions, and treatment-ineligible state expose no partial result and leave every supplied
snapshot unchanged. Focused representative tests prove either independent evidence route can
authorize early stabilization, any one source can be lost, an unauthored reasonable theory cannot
authorize care, and a universally recorded informative failed experiment can precede deterioration
and later successful recovery. These pure contracts do not occur events, execute consequences, or
own scene or persistence state.

The first Phase 6 scene controller owns one exact disease-truth snapshot, prepared-batch snapshot,
administration-specific journal, and universal experiment journal. It initializes all four once and
accepts stabilization only when the treatment context retains its exact current disease truth. A
successful call delegates to the pure first-patient transition before installing all four exact
result snapshots; rejected, stale, duplicate, full, or uninitialized calls preserve every installed
snapshot. The controller still does not occur the administration event, interpret observed text,
execute a consequence, save or load state, expose UI, or call a provider.

The ordinary scene now retains one exact authored representative first-patient treatment context and
initializes the scene controller once from that context's exact disease truth plus explicit empty
prepared-batch, administration-experiment, and universal-experiment snapshots. Startup creates no
completed preparation or administered dose. A focused credential-free smoke proves stable context
access, exact disease identity, empty journals and batches, and fail-closed repeated initialization
without changing world events, memory, movement, dialogue, model, or presentation state.

One separate scene controller now initializes from the context's exact authored remedy recipe and
ingredient snapshot and owns a single bounded preparation lifecycle. Starting the exact recipe
atomically installs the existing Domain transition's consumed ingredient snapshot and new activity;
only each exact next authored step can advance it, and only complete progress produces the exact
recipe-authored outcome.
Repeated, unavailable, value-equal-step, out-of-order, incomplete, or post-completion operations
preserve every installed value. Completion does not yet create a prepared batch, occur an event,
administer a dose, alter disease truth or journals, persist state, expose UI, or call a provider.

One pure prepared-batch append transition now turns an exact completed preparation into a full batch
at the end of stable existing order. It rejects duplicate exact preparations and full snapshots
without changing the input. The first-patient scene controller accepts that outcome only for the
exact current treatment context and its exact authored remedy recipe, then atomically installs the
new prepared-batch snapshot while retaining disease truth and both journals. It still does not occur
an administration event, select a dosage, stabilize the patient automatically, or add input/UI.

The temporary authoritative world-event source now also retains one exact advancing event for the
healer administering the first-patient remedy, without occurring it at startup. One pure composite
transition requires that exact authored event and one exact retained prepared batch, prospectively
occurs the event, constructs the dosage and administration from the retained preparation, and
delegates stabilization against the prospective latest event log. Its single immutable result
returns the exact new event log, world time, disease truth, prepared batches, administration-specific
journal, and universal journal together. Replacement events or batches, unavailable quantities,
stale or repeated occurrences, wrong contexts or preparations, and duplicate or full journals expose
no partial state.

One scene coordinator gathers both initialized controllers, delegates that whole preflight once,
then installs the already-accepted result into authoritative world-event and first-patient state
without a second fallible boundary. The credential-free ordinary-flow smoke prepares the exact
authored recipe in order, installs its full batch, occurs exactly one authored administration event,
consumes the exact bounded dose, stabilizes the exact patient infection, and appends both exact
experiment records atomically. Rejected replacement and repeated calls preserve both controllers and
unrelated scene state. There is still no player input or final UI, observed-text efficacy inference,
investigation-consequence execution, Phase 6 persistence, provider call, release, or distribution.

`Allrpcs.Domain.Persistence.FirstPatientSaveState` now composes the exact disease-truth,
prepared-batch, administration-specific experiment, and universal experiment snapshots as one
immutable value. `FirstPatientStateController.Capture` returns a fresh composition only when all
four live snapshots are initialized. It does not mutate or reinterpret first-patient state, and its
credential-free ordinary-scene smoke proves world-event, preparation, discovery, memory, dialogue,
movement, model, and presentation state remain unchanged.

`SaveSchemaVersion.FirstPatient` defines version `3`, whose in-memory `SaveSnapshot` retains one
exact first-patient composition in addition to version-2 character memory and the earlier dynamic
state. Earlier roots cannot retain this composition, and version `3` cannot omit it.
`SaveSnapshotFirstPatientMigration` is the pure version-2 to version-3 prerequisite: callers must
supply the exact authored initial first-patient state rather than having the migration fabricate
disease truth. Canonical version-3 JSON now writes and strictly reads complete disease truth,
completed preparations, remaining prepared batches, administration observations, and universal
experiment records. Stable document-local preparation indices preserve exact shared identity across
batches and administrations without serializing runtime object references. The reader rejects
unknown structure, invalid references, incomplete preparations, conflicting shared events, and
non-canonical values. `FirstPatientSaveStateRestorationTransition` then fails closed unless that
materialized graph is compatible with the exact authored disease, ingredients, recipe, steps, units,
patient infection, and authoritative events. It returns a fresh immutable graph rebound to those
exact authored instances while preserving saved order and dynamic values. The scene controller and
complete root coordinator install the rebound state only after full preflight; live capture,
legacy migration, canonical JSON, and local storage now carry complete version-3 state end to end.

The temporary scene now authors one complete stable healer whose exact identifier is shared with the
live turn source and whose controller assignment is the canonical LLM type. Its generation-input
source accepts only an exact awaiting healer activity and delegates context construction to the
Application assembler. One identified herb-patch description, one referenced knowledge entry, one
goal, only canonical unfinished commitment memory for the healer, the exact heard in-world request,
and the same text explicitly marked as untrusted player dialogue enter the bounded request. The
unnoticed stone marker, unreferenced knowledge, other characters' memory, display names, and other
scene state stay absent. The returned bundle retains the exact authoritative world-object and
targeted-affordance snapshots used for response validation.

`Allrpcs.Infrastructure.Dialogue.CharacterTurnResponseJsonDeserializer` is the first strict
provider-response boundary. It accepts one exact provider-neutral turn request and a complete JSON
document, then materializes only existing Domain response values. Exact property sets reject
unknown, missing, or duplicate members at every level; canonical closed disposition and intention
vocabularies reject unknown actions; existing structural intention validation rejects missing or
unexpected targets. The initial closed intention vocabulary has no destination-bearing action, so
a proposed destination has no accepted document shape. The transport document, commitment count,
and intentions per commitment are bounded independently of the Domain response. This decoder does
not retain provider DTOs or error
bodies, execute intentions, or call a provider. Object existence and current immediate-action
eligibility are enforced separately by the Application validation gate.

Shared Infrastructure-only prompt and schema writers keep trusted bounded context separate from the
exact untrusted player utterance while preserving one closed local response decoder. The OpenAI
boundary serializes a stateless Responses request with `store:false`, streaming enabled, and the
fully bounded strict schema. A strict UTF-8 server-sent-event reader accepts LF or CRLF terminated
frames, complete comment heartbeats, optional event names, and repeated data lines while bounding
line length, line count, frame count, document count, and joined document length. Malformed,
truncated, excessive, or NUL-containing streams return no partial provider content.

`OpenAiCharacterModel` implements the provider-neutral model seam over a caller-owned `HttpClient`,
immutable provider/model settings, `IOpenAiCredentialSource`, and validated `OpenAiEndpoint`. The
official version-one API base remains the default. Caller-composed alternatives must use HTTPS, or
HTTP on a numeric loopback address with an explicit port; paths other than `/v1`, user information,
queries, and fragments are rejected. HTTPS bases use the derived Responses resource and bounded
semantic SSE completion. Numeric-loopback bases instead use deterministic non-streaming Chat
Completions so local servers can fully disable reasoning without their Responses compatibility
layer reopening a hidden reasoning budget. That path accepts only one bounded assistant choice with
an exact `stop` finish. Both yield one provider-neutral terminal update. Credentials exist only on
the transient request authorization header; unsuccessful response bodies are never read. HTTP
authentication, rate-limit, transient service, request-rejection, and transport outcomes map to
bounded codes without provider details.

The Anthropic boundary serializes one stateless Messages request with two trusted system text
blocks, one exact untrusted user message, streaming enabled, and `output_config.format` structured
JSON. Its provider schema omits unsupported string and array maximum constraints while the strict
local decoder still enforces every bound. The Messages accumulator accepts only one indexed text
block, buffers its JSON deltas, permits content-free pings, requires an `end_turn` message delta and
final stop, and rejects thinking, tools, refusal, unknown events, invalid ordering, malformed usage,
or incomplete output without retaining provider content. `AnthropicCharacterModel` uses a
caller-owned `HttpClient`, Anthropic settings, and read-only credential source; it posts only to the
official Messages endpoint with `x-api-key` and `anthropic-version: 2023-06-01`, never reads failure
bodies, and yields one provider-neutral terminal update with optional redacted usage diagnostics.

The Ollama request boundary disables thinking, requests streaming structured output with the same
closed schema, uses trusted system messages plus one exact untrusted user message, and requests
deterministic temperature zero. Provider-unsupported length and maximum-count schema keywords are
omitted while the strict local decoder retains every bound. A strict UTF-8 NDJSON reader bounds
every line, document, and stream count, rejects blank or truncated input, and leaves caller-owned
streams open. The native accumulator accepts only ordered assistant content and one successful
terminal stop, discards provider metadata, derives wholly present bounded usage, and maps malformed,
thinking, tool, error, or post-terminal shapes to redacted provider-neutral failures without
exposing partial structured JSON.

The Application diagnostic boundary retains only a canonical provider, terminal status or bounded
failure code, elapsed duration, and wholly present or absent input/output/total token counts. Counts
must be non-negative, additive, and no greater than one million; elapsed time is bounded to one
hour. A caller-owned cancellation-aware sink is isolated by a best-effort recorder so its failure
cannot replace the generation result. Responses usage is read only from the completed event, while
Chat Completions usage is mapped only from its bounded terminal envelope; all response metadata is
discarded. Optional adapter instrumentation uses caller-owned monotonic time and the sink for
completed or failed provider attempts. Absent usage remains unknown and malformed usage fails closed
without exposing partial provider content.

The provider/model settings boundary retains one exact canonical OpenAI, Anthropic, or Ollama
provider instance and one bounded model name. An immutable selection snapshot copies one to three
exact settings in caller order, requires exactly one per provider, and retains one explicitly
selected configured provider. Selection transitions preserve the exact settings instances and
order. Settings updates replace one provider at its stable position or append one newly configured
canonical provider, retaining the exact supplied setting and every unrelated instance. Removal is
limited to a configured unselected provider and likewise preserves stable order and exact unrelated
instances. An immutable copied binding snapshot retains at most one exact caller-composed model,
settings object, and connection tester per canonical provider. Resolution and redacted connection
testing require the selected binding to share the exact configured settings object. There is no
default, substitution, credential fallback, or model-name rewriting; missing or mismatched bindings
return only provider-neutral `Unavailable`.

The strict version-1 provider-settings JSON boundary writes only `schemaVersion`, canonical provider
identifiers, and bounded model names in stable settings order. Deserialization rejects malformed,
future, duplicate, missing, unknown, non-canonical, excessive, or selection-incomplete documents;
credentials and API bases have no field in the closed schema. The ordinary scene source retains the
exact settings selection and binding snapshots and replaces them only after validating the exact
selected binding. Replacement affects future `TakeModel` calls, preserves an already-taken model and
one-shot supplied-model precedence, and leaves rejected state unchanged. Its stable credential-free
offline default remains intact. It does not construct a provider adapter or read a settings document.

`ICharacterModelSettingsDocumentStore` is the provider-settings-specific Application port for one
already-serialized non-empty document. Writes return only `Saved` or `Failed`; reads return
`Loaded`, `Missing`, or `Failed`, and only a loaded result retains the exact document. The port does
not reuse the gameplay-save contract or expose paths, JSON, provider details, credentials, or
storage exceptions. `FileCharacterModelSettingsDocumentStore` implements strict UTF-8 reads and
same-directory atomic replacement at one trusted absolute path. Empty or invalid UTF-8 data fails
closed, and the document remains secret-free.

`CharacterModelSettingsDocumentController` composes the strict JSON boundary with that port. Loads
distinguish a missing document, storage failure, and invalid supported-schema input without leaking
parser details. The Godot `CharacterModelSettingsStorageController` owns the fixed
`user://character-model-settings-v1.json` path but neither saves nor loads in response to input on
its own. Its smoke uses a process-specific temporary file, so validation cannot replace ordinary
user settings.

The scene-owned `CharacterModelSettingsController` retains one exact loaded selection and delegates
replacement, append, selection, and removal to the immutable Application transitions. Rejected
changes preserve its current snapshot. Its atomic apply operation computes both the setting update
and selection before replacing current state. Saving is a separate explicit operation over only the
exact current snapshot; uninitialized and storage-failed saves return a bounded failure without
changing memory, and initialization, apply, update, selection, and removal never save implicitly.

The temporary settings panel exposes the three canonical providers and one bounded exact model name
without a default selection. Apply changes only scene-owned memory, remove accepts only a configured
unselected provider, save explicitly replaces the secret-free document, and activate explicitly
initializes or replaces the exact current supported binding for future turns. Activation performs no
save, credential read, connection test, or generation. Rejected input or activation retains the
exact prior live model, and activation during an ordinary turn cannot replace the model already taken
for that turn.

The same panel exposes a masked OpenAI credential field backed by a scene-owned controller. The
controller composes only the current OS credential store and performs no startup operation. Save
validates the bounded key, clears the field immediately, and delegates it without retaining or
presenting the key; delete never loads it first. Both controls expose only fixed saved, deleted,
missing, busy, or failed text. Scene exit cancels an active operation, and deterministic smokes use
only an injected store rather than a user's credential facility.

At scene startup, `TemporaryCharacterModelCompositionController` consumes that same retained valid
selection and composes only its exact selected setting. Ollama uses the fixed validated loopback
endpoint; OpenAI uses the existing OS-specific credential source on macOS, Linux, or Windows.
Construction reads no credential and makes no HTTP request. Missing, failed, invalid,
unsupported-platform, and Anthropic selections preserve the stable offline behavior. The scene does
not create or discover a local server, own a model process, or claim native Linux/Windows credential
proof. Explicit activation composes the current supported selection without input/output and
atomically replaces only future model takes. An explicit connection button composes only the exact
current selected supported binding and delegates its existing redacted tester. It displays only
connected, busy, or failed, performs no generation or scene mutation, and does nothing at startup.
The deterministic smoke harness disables automatic persisted composition globally and uses isolated
settings, credential stores, and bindings, so local validation cannot inherit ordinary user settings
or credentials or call a provider.

The OpenAI and Anthropic binding factories retain one exact caller-supplied settings object,
`HttpClient`, and credential source across the corresponding model and connection tester. OpenAI's
endpoint-specific overloads also share the exact validated API base. Numeric-loopback OpenAI-
compatible endpoints use deterministic non-streaming Chat Completions with `reasoning_effort` set
to `none`, a zero thinking budget, disabled thinking template flags, temperature zero, and a fixed
seed. HTTPS endpoints retain the Responses API path. Both paths use strict structured output and
the same bounded local decoder. The Ollama factory likewise shares one exact caller-supplied
`HttpClient` and validated loopback endpoint. Explicit diagnostics
overloads retain the exact caller-owned `TimeProvider` and redacted diagnostic sink on the model.
Construction rejects null or provider-mismatched inputs before creating a binding and performs no
credential load, HTTP request, provider discovery, or scene mutation.

OpenAI-compatible live conformance is distinct from the permissive fail-closed transport safety
probe. `ALLRPCS_LIVE_OPENAI_CONFORMANCE=1` runs three fixed target-free wait requests, while
`ALLRPCS_LIVE_OPENAI_ACTION_CONFORMANCE=1` runs three fixed known-target examination requests through
the existing response validation policy. Both require one completed response per request, exact
request identity, bounded healer speech, no commitments or state proposals, and the expected safe
intention. Their failure output contains only bounded aggregate counts; it retains no model text,
prompt, credential, or raw provider detail. Both remain network-free unless explicitly enabled and
also require the existing explicit API-base and model environment values.

Ollama has separately enabled parity checks over its existing fixed loopback endpoint.
`ALLRPCS_LIVE_OLLAMA_CONFORMANCE=1` runs the same three target-free wait requests, while
`ALLRPCS_LIVE_OLLAMA_ACTION_CONFORMANCE=1` runs the same three known-target examination requests.
Both require `ALLRPCS_LIVE_OLLAMA_MODEL` to name an already-installed model and reuse the same
provider-neutral evaluator and count-only failure summary. The original permissive Ollama safety
probe remains independent, and the standard suite performs no Ollama request when the live switches
and model are absent.

`ALLRPCS_LIVE_OLLAMA_ORDINARY_WAIT=1` and `ALLRPCS_LIVE_OLLAMA_ORDINARY_ACTION=1` separately supply
that explicitly named Ollama model to one ordinary healer turn in the Godot scene. The equivalent
OpenAI-compatible paths use `ALLRPCS_LIVE_OPENAI_ORDINARY_WAIT=1` or
`ALLRPCS_LIVE_OPENAI_ORDINARY_ACTION=1` plus the existing explicit API-base and model values. Exactly
one provider and mode may be enabled. The wait check requires completed bounded speech without
physical, event, time, world-object, or perception mutation. The action check submits the known
herb-patch request, requires a completed locally validated response, and proves examination and
deterministic safe collection retain the exact action chain while world objects and perception
remain unchanged. Console output is fixed and content-free. The checks do not discover or start a
server, select, pull, or modify a model, or run during standard validation.

The temporary ordinary healer input now exposes only one targeted action in addition to always-safe
wait: examining the identified herb patch. A completed model response still has to pass the existing
known-object and current-affordance validation before scene reception. The temporary examination
flow compares the validated canonical object-id value so an Infrastructure-deserialized target can
be distinct from the authoritative identifier instance, while the resulting occurrence, safe result,
collection, transcript, and world event retain the exact proposed action chain. Unknown or
unafforded targets remain failures and do not mutate authoritative state.

All three HTTP adapters remain caller-composed. `OllamaEndpoint` permits only an explicit HTTP
IP-loopback origin and derives the native `/api/chat` and `/api/show` resources; hostnames,
LAN/public addresses, credentials, paths, queries, fragments, HTTPS, and implicit ports are
rejected. `OpenAiEndpoint` derives only `/v1/responses`, `/v1/chat/completions`, and `/v1/models/`
resources from its bounded API base. The OpenAI and Anthropic connection testers retrieve only the
exact configured model
through their official caller-owned authenticated HTTP boundary; the Ollama tester posts only the
exact configured model to `/api/show` at its validated loopback endpoint. None generates content or
reads success or failure bodies, and all expose only provider-neutral outcomes.

`MacOsOpenAiCredentialStore` implements save/replace, load, and delete for one exact generic
password identity in macOS Keychain. The `/usr/bin/security` process is isolated behind an injected
boundary, never uses a shell, receives save bytes only through redirected standard input, bounds
and clears native output buffers, discards standard error, and maps OS details to loaded, missing,
saved, deleted, or failed outcomes.

`LinuxSecretServiceOpenAiCredentialStore` uses the fixed `Allrpcs.OpenAI` application and `api-key`
account attributes through `/usr/bin/secret-tool`. Availability is explicit and fails closed. The
native runner permits only exact store, lookup, and clear commands, passes save bytes through
redirected standard input without a shell or added newline, bounds lookup output, clears retained
secret buffers, and discards error details. Because `secret-tool clear` does not distinguish a
missing item, delete first performs a bounded lookup probe and clears its output before clearing the
exact item. Standard tests use deterministic fake boundaries and never touch a user's real
keychain or Secret Service. Linux support is not claimed until the same bytes are proved on Linux.

`WindowsOpenAiCredentialStore` uses one fixed generic `Allrpcs.OpenAI` target and `api-key` user
identity behind an injected Credential Manager seam. The native implementation uses the current
user's local-machine-persistent credential set, replaces only that exact target, maps native errors
to redacted outcomes, and clears managed input, unmanaged write memory, copied output, and the
Credential Manager read buffer before releasing it. Standard tests are fake-boundary-only and do
not touch a user's credential set. Windows support is not claimed until the same bytes are proved
on Windows. No Anthropic credential store, automatic credential read, diagnostic export, automatic
live recomposition, or generated action beyond the bounded known herb-patch examination exists. The opt-in
`OllamaCharacterModelLiveTests` and `OpenAiCharacterModelLiveTests` check explicitly named
already-installed or local models without pulling or modifying them; the standard suite does not
access a local server. Earlier oMLX Responses probes safely rejected non-conforming or incomplete
output and exposed an integration defect: reasoning-capable local models could spend the entire
output budget in reasoning despite a simple bounded game turn, while oMLX's streamed delta text
could differ from its final structured text. The loopback Chat Completions path disables reasoning
through every locally supported control and treats only one bounded `stop` completion as a response.
Local oMLX `LFM2.5-8B-A1B-MLX-8bit` and `Qwen3.6-35B-A3B-8bit`, plus native Ollama
`qwen3.5:4b-q8_0`, each passed the fixed three-request wait and known-target action series and both
ordinary-scene flows. No provider output, reasoning, target metadata, or raw failure was retained.

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

`WorldObjectSnapshot` contains existing world objects in stable supplied order. It explicitly
permits an empty sequence, copies caller input exactly once, exposes a read-only list, retains
each supplied world-object instance, and rejects null input, null entries, and value-equal
duplicate `WorldObject.Identity.Id` values. It does not compare display names or descriptions for
identity.

`WorldObjectLookup.FindById` selects only by value-equal `WorldObject.Identity.Id`. It returns the
exact retained world-object instance or `null` without changing either input; display names and
descriptions do not participate in selection.

The complete world-object boundary still does not define item properties, tags, portability,
quantities, stacks, containers, inventory mechanics, ownership, use, location assignment,
containment, projection, filtering, replacement, mutable state, lifecycle or event handling,
visibility, perception, discovery, persistence, serialization, Godot, or UI behavior.

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

`Allrpcs.Domain.Discovery.FocusedSearchOutcome` records one existing focused-search request and
one existing resulting `VisibilityState`. It retains both supplied instances through get-only
properties and compares outcomes by their values. It does not produce an outcome, apply it to
perception, validate or advance visibility, or connect outcome data to completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeSnapshot` copies one supplied sequence of outcomes
exactly once and exposes the exact outcome instances in stable order through a read-only list.
Empty snapshots are valid; null values and outcomes with value-equal duplicate requests are
rejected. The snapshot does not compare or advance resulting visibility states, produce outcomes,
apply them to perception, or connect them to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeProjection.ForCharacter` is the pure
per-character outcome projection. It returns a new snapshot containing every outcome whose
request has a value-equal searcher character identifier, retaining the existing outcome instances
and their request and resulting-visibility-state instances in stable order. It permits no matches
and multiple distinct requests, ignores unrelated outcomes, rejects null inputs, and does not
mutate either input or apply any outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeLookup.FindByRequest` is the pure exact-request
lookup. It matches both the searcher character and target object by request value, returning the
exact retained outcome or `null` when no outcome matches. It rejects null inputs, relies on the
snapshot's duplicate-request invariant, leaves every input unchanged, and does not produce or
apply an outcome.

`Allrpcs.Domain.Perception.CharacterPerceptionLookup.FindByObject` is the pure exact-object
lookup for one character's perception snapshot. It matches the object identifier by value,
returning the exact retained visibility entry or `null` when no entry matches. It rejects null
inputs, relies on the snapshot's duplicate-object invariant, leaves every input unchanged, and
does not inspect the complete character, compare or advance visibility, or apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicabilityPolicy.CanApply` is the pure
perception-applicability decision for one focused-search outcome. It returns true only when the
perception snapshot represents the outcome request's searcher and the existing perception lookup
finds that request's target object. It rejects null inputs, ignores current and resulting
visibility states, leaves every input unchanged, and does not decide a visibility transition or
apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeChangePolicy.WouldChange` is the pure
visibility-state change decision for one focused-search outcome and perception snapshot. It first
delegates applicability to the existing policy, then delegates exact target selection to the
existing perception lookup and returns true only when the retained target entry's current state
differs from the outcome's resulting state. It rejects null inputs, treats every distinct state
pair alike without ranking it, leaves every input unchanged, and does not apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicationTransition.Apply` is the pure
perception transition for one existing focused-search outcome. It delegates the complete decision
to the change policy, rejects inapplicable and unchanged outcomes, and otherwise returns a new
snapshot that retains the exact character identifier, stable entry order, and every unrelated
entry instance. Only the target entry is replaced, retaining its exact object identifier and the
outcome's exact resulting visibility-state instance. It leaves both inputs unchanged and does not
rank states, produce or consume outcomes, or connect application to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionPolicy.CanConsume` is the pure
consumption-eligibility decision for one existing focused-search outcome. It delegates exact
request selection to the existing outcome lookup and returns true only when the retained complete
outcome is value-equal to the supplied outcome, including its resulting visibility state. It
rejects null inputs, leaves every input and retained instance unchanged, and does not remove or
otherwise consume an outcome, change perception, or connect consumption to completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionTransition.Consume` is the pure
consumption transition for one eligible focused-search outcome. It delegates the complete
decision to the consumption policy, rejects outcomes that are not exactly retained, and otherwise
returns a new snapshot without the outcome for that exact request. It preserves stable order and
every unrelated outcome instance, leaves both inputs unchanged, and does not change perception or
connect consumption to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionPolicy.CanResolve` is the pure combined
resolution-eligibility decision for one existing focused-search outcome. It delegates perception
applicability and visibility-state change detection completely to the existing change policy, then
delegates exact recorded-outcome eligibility completely to the existing consumption policy. It
returns true only when both decisions succeed, rejects null inputs through those delegated
policies, leaves every input and retained instance unchanged, and does not apply a visibility
change, remove an outcome, or connect resolution to focused-search completion.

`Allrpcs.Domain.Discovery.WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve` is the
pure world-object-bound resolution-eligibility decision for one existing focused-search outcome.
It rejects null inputs, requires the supplied world object's stable identifier to be value-equal to
the outcome request's target, then delegates the unchanged complete perception-and-outcome decision
to the existing resolution policy. It leaves the outcome, world object, perception, outcome
snapshot, and every retained nested instance unchanged; it does not inspect authored object text,
apply or consume an outcome, mutate a world object, or make any source or controller authoritative.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionResult` is one immutable resolution-result
value retaining exact existing character-perception and focused-search-outcome snapshot instances
through get-only properties. Its record equality therefore uses those retained snapshot instances.
It rejects null inputs and does not perform resolution, apply a visibility change, remove an
outcome, validate either snapshot, or connect resolution to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionTransition.Resolve` is the pure combined
resolution transition for one eligible focused-search outcome. It delegates complete eligibility
to the existing resolution policy, rejects ineligible resolution, then delegates perception
replacement and outcome removal to the existing application and consumption transitions. It
returns one resolution result retaining those exact returned snapshots, leaves every input and
retained unrelated instance unchanged, and does not produce outcomes, grant discovery, or connect
resolution to focused-search completion.

`Allrpcs.Domain.Discovery.WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve` is the
pure world-object-bound resolution transition for one eligible focused-search outcome. It delegates
complete eligibility to the world-object-bound resolution policy, rejects an ineligible resolution,
then delegates unchanged state creation to the existing resolution transition and returns its exact
result. It accepts value-equal target identifiers, leaves the supplied world object and every other
input unchanged, and does not inspect authored object text, mutate a world object, make any source
or controller authoritative, or grant discovery.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingPolicy.CanRecord` is the pure
recording-eligibility decision for one existing focused-search outcome. It delegates exact-request
selection to the existing outcome lookup and returns true only when no outcome for that request is
already retained. It allows both partial request matches, rejects exact-instance and value-equal
duplicate requests, rejects null inputs through the delegated lookup, leaves every input and
retained instance unchanged, and does not record or produce an outcome or connect recording to
focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingTransition.Record` is the pure recording
transition for one eligible focused-search outcome. It delegates complete eligibility to the
existing recording policy, rejects outcomes whose request is already retained, and otherwise
returns a new snapshot preserving every retained outcome instance in stable order before the exact
supplied outcome instance. It leaves both inputs unchanged and does not produce an outcome, connect
recording to focused-search completion, or apply a visibility change.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome` is the pure
combined completion-eligibility decision for one existing focused-search outcome. It delegates
active-request eligibility completely to the existing completion policy and recording eligibility
completely to the existing recording policy, returning true only when the request is active and no
outcome for that request is already retained. It rejects null inputs before delegation, leaves every
input and retained instance unchanged, and does not complete an activity, record or produce an
outcome, change perception, or connect the decision to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionResult` is one immutable
completion-with-outcome result retaining exact existing focused-search activity and outcome
snapshot instances through get-only properties. Its record equality therefore uses those retained
snapshot instances. It rejects null inputs and does not complete an activity, record or produce an
outcome, inspect or validate either snapshot, change perception, or connect completion to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome` is the
pure combined completion transition for one eligible existing focused-search outcome. It delegates
complete eligibility to the existing combined policy, rejects inactive or already-recorded
requests, then delegates exact-request activity removal and exact-outcome append to the existing
completion and recording transitions. It returns one completion result retaining those exact
returned snapshots, preserves stable order and every unrelated retained instance, leaves every
input unchanged, and does not produce or resolve outcomes, change perception, or connect
completion to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchActivityProjection.ForCharacter` is the pure
per-character activity projection. It returns a new snapshot containing every activity whose
request has a value-equal searcher character identifier, retaining the existing activity instances
in stable order. It permits no matches and multiple distinct requests, ignores unrelated
activities, rejects null inputs, and does not mutate either input.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidate` identifies exactly one earlier
observation by an existing observer `CharacterId` and target `ObjectId`, together with the existing
`KnowledgeId` that could trigger its reinterpretation. It rejects null inputs, retains all three
supplied identifier instances through get-only properties, and uses only their existing value
semantics. It does not inspect character knowledge or perception, decide whether the trigger is
satisfied, define a resulting visibility state, reinterpret an observation, or change state.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateSnapshot` contains existing
reinterpretation candidates in stable supplied order. It explicitly permits an empty sequence,
copies caller input, exposes a read-only list, retains each exact candidate instance, and rejects
null input, null entries, and value-equal complete duplicates. Candidates may share any proper
subset of observer, target, and trigger identifiers. The snapshot does not project, look up,
select, rank, order, evaluate, apply, or consume candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateProjection.ForCharacter` is the pure
per-character candidate projection. It returns a new snapshot containing every candidate whose
observer identifier is value-equal to the supplied character identifier, retaining exact candidate
and nested identifier instances in stable source order. It permits no matches and multiple matches
with any target or trigger knowledge, omits unrelated observers, rejects null inputs, and does not
mutate, evaluate, rank, reorder, or select among candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateLookup.Find` is the pure exact
candidate lookup. It matches the complete observer, target, and trigger-knowledge value and returns
the exact retained candidate or `null`. It rejects partial matches and null inputs, leaves every
input unchanged, and does not project, produce, rank, reorder, evaluate, apply, or consume
candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateTriggerPolicy.CanTrigger` is the
pure knowledge-trigger decision. It returns true only when the complete character has a
value-equal observer identity and references the candidate's value-equal trigger knowledge. It
rejects null inputs and leaves the complete character, its ordered knowledge, and the candidate
unchanged without inspecting perception or the target object.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateApplicabilityPolicy.CanApply` adds
only the existing-perception boundary. It delegates trigger satisfaction, requires a perception
snapshot for the value-equal observer, and uses exact object lookup to require the target without
inspecting its current visibility state. It rejects null inputs and changes nothing.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateChangePolicy.WouldChange` delegates
complete applicability and returns true only when the retained target is exactly `Noticed`.
Every missing, mismatched, untriggered, `Unnoticed`, `Identified`, `Understood`, or
`AlteredOrCollected` combination is false without mutation.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateApplicationTransition.Apply`
rejects every non-changing combination, then returns a new perception snapshot with only the
value-equal target changed from `Noticed` to `Identified`. It preserves the perception owner's
exact identifier, stable entry order, every unrelated entry instance, the target's exact retained
object identifier, the complete character, and all inputs.

`ObservationReinterpretationCandidateConsumptionPolicy.CanConsume` and
`ObservationReinterpretationCandidateConsumptionTransition.Consume` provide complete-candidate
retention checking and pure removal. They accept a distinct value-equal candidate; eligibility is
false and removal is rejected for empty, absent, or partial-match snapshots, while null inputs are
rejected. Both preserve stable order, every unrelated exact candidate, every nested identifier,
and all inputs.

`ObservationReinterpretationCandidateResolutionPolicy.CanResolve`,
`ObservationReinterpretationCandidateResolutionResult`, and
`ObservationReinterpretationCandidateResolutionTransition.Resolve` combine trigger-aware
`Noticed`-to-`Identified` application with complete candidate consumption. Resolution succeeds only
when both delegated operations are eligible, returns only the two new immutable snapshots, and
leaves the character and every input snapshot unchanged.

`ObservationReinterpretationCandidateRecordingPolicy.CanRecord` and
`ObservationReinterpretationCandidateRecordingTransition.Record` provide duplicate-free pure
recording. `ObservationReinterpretationCandidateProductionPolicy.CanProduce` and
`ObservationReinterpretationCandidateProductionTransition.Produce` additionally require the
candidate's observer and target to match an existing authoritative character and world object.
They deliberately do not require that the trigger knowledge has already been gained; later
resolution performs that check. Eligible production appends the exact candidate in stable order.

`ObservationReinterpretationCandidateResolutionSelection.SelectNext` and
`ObservationReinterpretationCandidateResolutionSequence.ResolveNext` provide deterministic
per-character resolution. Selection returns the exact first retained candidate that the supplied
complete character can resolve against its perception, skipping other observers, unmet triggers,
missing targets, and non-`Noticed` targets. Sequencing delegates that selection and the atomic
transition, changing and consuming only the selected candidate.

`Allrpcs.Domain.WorldEvents.WorldEventLogSnapshot` is the immutable ordered authoritative
world-event identifier log. It explicitly permits an empty sequence, copies caller input, exposes
only a read-only list, retains every exact supplied `WorldEventId`, and rejects null input, null
entries, and value-equal duplicate identifiers.

`WorldEventLogLookup.Find`, `WorldEventLogAppendPolicy.CanAppend`, and
`WorldEventLogAppendTransition.Append` provide exact retained-identifier lookup and duplicate-free
pure append. Lookup returns the exact retained value-equal identifier or `null`; eligible append
returns a new snapshot preserving stable order and every retained instance before the exact supplied
identifier. Duplicate and null rejection leave all inputs unchanged. These contracts do not define
event payloads, time, replay, persistence, or production.

`WorldEventRecordingPolicy.CanRecord` and `WorldEventRecordingTransition.Record` add the
authoritative-definition boundary to identifier-log append. A requested identifier is recordable
only when it resolves to a defined event and is absent from the log; recording appends the exact
identifier retained by that authoritative event, even for a distinct value-equal request.

`WorldEventTimeEffect` is the closed world-event classification vocabulary: exact singleton values
`no-advance` and `advance` in stable order with strict canonical parsing. Immutable `WorldEvent`
values retain one exact existing identifier and time effect. `WorldEventSnapshot` copies ordered
events into a read-only collection and rejects nulls and duplicate event identifiers even when
their time effects differ; `WorldEventLookup.FindById` returns the exact retained value-equal event.

`WorldEventLogProjection.Resolve` deterministically maps the identifier log to exact authoritative
events in log order, ignores unrecorded definitions, and fails closed when an identifier has no
definition. `WorldTimeSnapshot` represents qualitative time only as the exact latest event that
advanced it, with null as the explicit initial state. `WorldTimeAdvancementPolicy.CanAdvance` and
`WorldTimeAdvancementTransition.Advance` reject `no-advance` events and the current value-equal
event. `WorldTimeProjection.FromEvents` reconstructs the same state from ordered events by selecting
the exact latest `advance` identifier. These contracts define no elapsed time, phase names,
durations, scheduling, disease progression, persistence, or serialization.

`WorldEventOccurrencePolicy.CanOccur`, `WorldEventOccurrenceResult`, and
`WorldEventOccurrenceTransition.Occur` combine authoritative recording with qualitative time as one
immutable Domain operation. Every eligible event is recorded; `no-advance` preserves the exact
supplied time snapshot, while `advance` installs a new snapshot retaining the authoritative event's
exact identifier. Undefined, already-recorded, or repeated-current advancing events fail closed.

`Allrpcs.Game.WorldEventLogController` and `WorldTimeController` remain focused fixtures for their
individual Domain boundaries. The scene's `WorldEventController` is the ordinary runtime authority:
it initializes once from exact authoritative-event, identifier-log, and qualitative-time snapshots,
then delegates occurrence eligibility and transition to the Domain contracts. A successful
occurrence installs the returned log and time snapshots together. Uninitialized, undefined,
duplicate, repeated-current, and null cases preserve every controller snapshot or reject null.

`Allrpcs.Game.TemporaryWorldEventSource` independently authors one retained ordered snapshot. Its
exact `greybox-ready`/`no-advance` event is the explicit startup selection; its exact
`herb-patch-identified`/`advance` event is the first meaningful-time definition. `Main._Ready`
initializes the world-event controller and records only the startup event, preserving the exact
initial time snapshot.

Ordinary herb-patch focused-search resolution preflights both the existing world-object-bound
resolution policy and world-event occurrence policy. Success consumes the exact outcome, changes
only the target perception, records the exact authored event identifier, and advances qualitative
time. If either side is ineligible, both controller states remain unchanged. This temporary
cross-controller boundary does not add generic event payloads, clocks, disease progression,
persistence, serialization, or new presentation.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidate` identifies exactly one existing observer
`CharacterId` and one existing target `ObjectId` without selecting or evaluating the candidate. It
rejects null inputs, retains both supplied identifier instances through get-only properties, and
uses only their existing value semantics. It does not inspect world or character state, grant
discovery, or change perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateSnapshot` contains existing passive-noticing
candidates in stable supplied order. It explicitly permits an empty sequence, copies caller input,
exposes a read-only list, retains each supplied candidate instance, and rejects null input, null
entries, and value-equal duplicate candidates. The same observer may have distinct targets and the
same target may have distinct observers. The snapshot does not produce, select, filter, rank,
reorder, or evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateProjection.ForCharacter` is the pure
per-character candidate projection. It returns a new snapshot containing every candidate whose
observer identifier is value-equal to the supplied character identifier, retaining the exact
candidate and nested identifier instances in stable source order. It permits no matches and
multiple distinct matches, omits unrelated candidates, rejects null inputs, leaves both inputs
unchanged, and does not produce, select, rank, reorder, or evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateLookup.Find` is the pure exact-candidate lookup.
It matches the complete candidate value, including both observer and target identifiers, and
returns the exact retained candidate or `null` when no candidate matches. It rejects either null
input, leaves every input unchanged, and does not produce, project, select among, rank, reorder, or
evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateApplicabilityPolicy.CanApply` is the pure
candidate-to-perception applicability decision. It returns true only when the perception snapshot
belongs to the candidate's value-equal observer and the existing exact-object lookup finds the
candidate target. It rejects null inputs, retains every input instance unchanged, and deliberately
does not inspect the current visibility state, decide a change, or mutate perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateChangePolicy.WouldChange` is the pure
passive-noticing change decision. It delegates complete applicability, selects the exact retained
target through the existing perception lookup, and returns true only when that target is exactly
`Unnoticed`. It rejects null inputs and leaves every input unchanged; later visibility states are
never regressed or redundantly rewritten, and the policy does not itself change perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateApplicationTransition.Apply` is the pure
passive-noticing perception transition. It delegates complete change eligibility and returns a new
snapshot whose candidate target alone is replaced at its stable position with `Noticed`, retaining
the perception-owned character and target-object identifier instances and every unrelated entry.
It rejects inapplicable or non-changing candidates, leaves all inputs unchanged, and does not
produce, retain, consume, or select a candidate.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateConsumptionPolicy.CanConsume` is the pure
candidate-retention decision. It delegates complete value matching to the existing exact-candidate
lookup and returns true only when a value-equal candidate is retained in the supplied snapshot. It
rejects null inputs, preserves every exact instance and stable order, and does not remove, apply,
produce, or select among candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateConsumptionTransition.Consume` is the pure
candidate-removal transition. It delegates complete consumption eligibility and returns a new
snapshot without the value-equal complete retained candidate, preserving every unrelated exact
candidate and nested identifier instance in stable order. It accepts a distinct value-equal
candidate, rejects empty, absent, partial-match, and null inputs, and leaves every input unchanged.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionPolicy.CanResolve` is the pure combined
resolution decision. It returns true only when the existing change policy permits an
`Unnoticed`-to-`Noticed` perception change and the complete candidate is retained for consumption.
It leaves both snapshots and every nested instance unchanged and does not create resolution state.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionResult` is the immutable result shape
for one passive-noticing resolution. It retains the exact supplied perception and candidate
snapshot instances behind get-only properties, rejects null snapshots, and performs no resolution.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionTransition.Resolve` is the pure atomic
resolution transition. It delegates complete combined eligibility, then delegates the existing
application and consumption transitions, returning only the resulting perception and candidate
snapshots. The target alone changes from `Unnoticed` to `Noticed`, the complete candidate alone is
removed, and every unrelated exact instance and stable position is retained without mutating input.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateRecordingPolicy.CanRecord` and
`PassiveNoticingCandidateRecordingTransition.Record` provide duplicate-free pure candidate
recording. Eligibility rejects only an existing value-equal complete candidate while allowing both
partial matches; recording appends the exact supplied eligible candidate after all existing entries
in stable order. Both preserve all inputs and do not prove that either identifier names an existing
authoritative character or world object.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateProductionPolicy.CanProduce` and
`PassiveNoticingCandidateProductionTransition.Produce` bind recording to an existing authoritative
character and world object. Eligibility requires both authoritative identities to be value-equal to
the supplied candidate and delegates duplicate rejection to the recording policy. Production then
delegates exact append behavior to the recording transition, returning a new snapshot that retains
the exact supplied candidate. Both accept distinct value-equal identifier instances, reject every
null, mismatch, and complete duplicate, and leave all character, object, candidate, and snapshot
inputs unchanged.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionSelection.SelectNext` and
`PassiveNoticingCandidateResolutionSequence.ResolveNext` provide deterministic per-observer
resolution. Selection walks the retained snapshot in stable order and returns the exact first
candidate for a value-equal observer that delegates successfully to complete resolution
eligibility, skipping other observers, absent targets, and targets no longer at `Unnoticed`.
Resolution delegates that exact selection to the existing atomic transition, changing only its
target to `Noticed` and removing only the selected complete candidate in the returned snapshots.
When no candidate is eligible, selection returns `null` and resolution rejects without changing
inputs; neither operation ranks candidates or inspects coordinates, distance, line of sight, or
world state.

`Allrpcs.Application.Discovery.FocusedSearchActivityIndicatorPolicy.ShouldShow` is the pure
per-character activity-indicator decision. It delegates selection to the existing activity
projection and returns true exactly when the new projected snapshot contains at least one entry,
including multiple distinct matching requests. It returns false for empty or unrelated snapshots,
rejects null inputs, and does not mutate either input.

`Allrpcs.Game.FocusedSearchActivityIndicator.Present` is the temporary Godot presentation adapter
under the existing Seeker. It accepts an existing character identifier and activity snapshot,
delegates the complete decision to the Application policy, and changes only the visibility of one
authored cyan in-world ring. The ring is hidden by default, and ordinary play does not construct or
inject activity outside the explicit temporary lifecycle inputs described below.

`Allrpcs.Game.FocusedSearchActivityController` is the sole live owner of the existing Seeker's
immutable focused-search activity, outcome, passive-noticing candidate,
observation-reinterpretation candidate, and optional perception snapshots. It initializes all
candidate and lifecycle snapshots empty from the Seeker's one authored stable character identifier.
Its separate one-time perception initialization accepts only an existing snapshot for that
value-equal Seeker identifier and retains the exact supplied snapshot instance. Null,
wrong-character, and repeated initialization are rejected without changing any owned snapshot or
presentation. Its start path accepts existing discovery conditions, capabilities, and inventory,
delegates the complete start decision to the combined Domain eligibility policy, and only then
delegates state creation to the existing start transition. Cancellation and request-only
completion retain their existing policy and transition delegation. Its separate outcome-aware
completion path accepts an existing outcome, delegates the combined completion decision and
transition, installs both exact result snapshots, and only then presents the resulting activity
snapshot. Rejected lifecycle operations preserve every owned snapshot and presentation. Ordinary play
reaches the outcome-aware path only through the explicit temporary completion input; the
credential-free focused-search, outcome-controller, and interaction-focus smokes cover these
boundaries. Its separate outcome-resolution path accepts an existing outcome and world object,
rejects null inputs and target-mismatched objects before applying or consuming anything, delegates
complete eligibility to the world-object-bound policy and state creation to the world-object-bound
transition, then installs the exact returned perception and outcome snapshots without changing
activity, the supplied world object, or presentation. Ordinary play reaches that boundary only
through the explicit temporary resolution input described below.

Its passive-noticing production boundary accepts an existing candidate, complete character, and
world object, requires the candidate's observer to identify this controller's Seeker, and delegates
the remaining authoritative identity and duplicate checks plus exact append behavior to the
existing Domain production policy and transition. Its passive-noticing resolve-next boundary fails
closed while perception is uninitialized or no retained candidate is eligible, otherwise delegates
stable selection and atomic state construction to the existing Domain selection and sequence and
installs both returned snapshots. These paths retain value-equal candidate identifiers, keep the
controller as the only live perception owner, and do not change focused-search or presentation
state. During `Main._Ready`, ordinary play resolves the source candidate's exact target through
the initialized world-object controller, produces it from the exact retained candidate, character,
and world object, then invokes resolve-next once. Either impossible step fails explicitly; success
consumes the candidate and replaces only its `Unnoticed` live perception entry with `Noticed`.
This one-time sequence does not poll, inspect coordinates, distance, line of sight, focus, or input,
and it does not add passive-noticing presentation.

Its observation-reinterpretation production boundary likewise requires the candidate observer to
identify this controller's Seeker, then delegates authoritative character/world-object matching and
duplicate-free exact append behavior to the Domain production boundary. Resolve-next requires a
matching complete Seeker character and initialized perception, then delegates stable selection and
atomic `Noticed`-to-`Identified` application plus complete-candidate consumption. Rejections preserve
every owned snapshot and presentation. During `Main._Ready`, the scene produces the exact temporary
candidate through live character and world-object authority, resolves it once against the
character's exact trigger knowledge, and refreshes the existing resolved-perception indicators.
There is no polling, proximity, timing, focus, input, knowledge grant, or new UI authority.

Each existing `Allrpcs.Game.GreyboxInteractionTarget` parses its one exported canonical object-ID
text through the Domain `ObjectId` boundary when the node becomes ready and retains that exact
identifier instance. On each refresh, the existing `InteractionFocusController` resolves every
marker identifier through `WorldObjectController` and makes only matching existing objects eligible
for the otherwise unchanged pure visible, enabled, in-range selection. A missing nearest object
therefore falls back to the next eligible marker under the existing distance and authored-order
rules; no eligible object clears the focus indicator, status, index, and identifier. A selection
exposes the exact looked-up object's identifier instance, including a distinct value-equal instance,
and presents the focus status from that lookup's authored `WorldObject.Identity.DisplayName` rather
than the greybox marker label. The status refreshes when a value-equal replacement changes that name
without changing the selected authored index, returns to `Interaction focus: NONE` when focus
clears, and never inspects or exposes the object description. Each existing labelled marker is
likewise refreshed from its exact matching object's display name, including for a matching disabled
marker, while a missing object clears that marker label and an unlabelled hidden marker remains
unlabelled. Markers no longer author a separate display-name input: their `Label` nodes are
presentation-only and receive text through this lookup. The refresh does not alter focus eligibility
or retain a world object beyond the refresh. Its request-construction boundary
accepts an existing `CharacterId` and nullable `WorldObject`, returns no request when focus is clear
or the object is absent or target-mismatched, and otherwise returns a new `FocusedSearchRequest`
retaining the exact character identifier and the matching object's exact identifier. Exact and
distinct value-equal object identifiers are accepted. Its separate start-attempt boundary supplies
the activity controller's exact Seeker identifier to that factory and, for a valid request only,
forwards the request plus existing
`DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` instances to the
controller's condition-aware start path. It returns the existing start result and adds no Domain
eligibility, transition, input, discovery-context authoring, world-object filtering, or ranking
behavior.

`Allrpcs.Game.TemporaryFocusedSearchContextSource` is the narrow scene source for those three
aggregate start inputs. It derives its target identifiers only from the existing temporary
world-object snapshot: the first object's exact identifier authors the capability-only requirement,
and the second object's exact identifier authors the tool-only requirement. It fails explicitly
when fewer than two objects are supplied and never falls back to greybox interaction targets. The
source retains the exact created capability, tool identifier, requirements, sets, and aggregates and
exposes only the existing discovery-condition set, capability set, and inventory needed by the start
bridge. Independent-snapshot smoke evidence proves exact nested retention without selecting by
authored identity text or changing the supplied snapshot and objects. The source does not start a
search, poll focus, own lifecycle state, grant discovery, or make temporary scene state
authoritative.

`Allrpcs.Game.TemporaryWorldObjectSource` independently authors only the visible herb-patch and
stone-marker identities into one retained `WorldObjectSnapshot` when the scene becomes ready. It
creates one distinct world object per identity in that authored order, retaining the existing exact
identifier values and display names. Each source identifier is a distinct instance that is
value-equal to its marker's independently parsed `ObjectIdText` reference. The source also authors
and retains one distinct non-empty `WorldObjectDescription` for each object: the herb patch has
`A cluster of narrow leaves gives off a sharp scent.`, while the stone marker has
`Weathered carvings mark one face of the standing stone.`. These descriptions are not focused-search
inputs and are not copied into marker labels, focus status, or focused-search result presentation.
The existing resolved-perception indicator receives a description only when the controller's
matching perception entry is `Identified`. The source does not read, mutate, or replace greybox targets;
become authoritative; inspect visibility or focus; filter or rank objects; call controllers; grant
discovery; or change presentation or ordinary-play behavior.

`Allrpcs.Game.WorldObjectController` is the separate scene owner of one optional immutable
world-object snapshot. Its one-time initialization boundary rejects null, retains the exact first
supplied snapshot instance, and fails closed on repeated initialization without replacing that
snapshot. Its lookup boundary rejects a null identifier, returns no object while uninitialized or
when the identifier is absent, and otherwise delegates to the pure Domain lookup to return the
exact retained object for a value-equal identifier. During `Main._Ready`, the scene passes the
temporary source's exact retained snapshot once and fails explicitly if initialization cannot be
established. The temporary F, E, and R paths call lookup only for the focused identifier, active
request target, and selected recorded-outcome target respectively. The controller does not filter,
rank, or mutate world objects; inspect focused-search or perception state; grant discovery; or
change presentation itself.

`Allrpcs.Game.TemporaryFocusedSearchOutcomeSource` is a separate narrow scene source for one
explicitly temporary deterministic focused-search result. Each call accepts an existing request
plus an existing nullable world object. It rejects a null request and returns no outcome for a null
or target-mismatched object. A matching object, including one with a distinct value-equal stable
identifier, produces a distinct `FocusedSearchOutcome` retaining the exact request and the exact
existing `VisibilityState.Identified` singleton. The source leaves the world object, its identity,
and its description unchanged and does not inspect authored object text, focus, activity, outcomes,
perception, or broader world state; decide lifecycle eligibility; call a controller; own state;
apply or consume an outcome; or change visibility. Ordinary play invokes it only from the pressed
temporary E completion path when an active Seeker request exists and its target resolves through
the world-object controller.

`Allrpcs.Game.TemporarySeekerPerceptionSource` authors one temporary immutable perception snapshot
when the scene becomes ready. It receives the exact retained temporary passive-noticing candidate and
derives its ordered visibility entries only from the existing temporary world-object snapshot. It
resolves the candidate target through value-equal world-object lookup and fails explicitly when that
target is absent. Every entry retains its world object's exact stable identifier in source order; the
resolved candidate target alone begins at the exact `VisibilityState.Unnoticed` singleton and every
other object begins at `Noticed`. The snapshot retains the candidate's exact observer identifier, and
repeated scene access returns the same snapshot instance. Derivation leaves the candidate, snapshot,
objects, identities, and descriptions unchanged. The source does not read greybox targets, authored
identity text, focus, or lifecycle state; call a controller; or make either source authoritative.
During `Main._Ready`, the scene passes this exact snapshot to the controller's one-time initialization
boundary before the one-time passive-noticing sequence replaces only the controller-owned snapshot.

`Allrpcs.Game.TemporarySeekerCharacterSource` authors and retains one complete immutable temporary
Seeker `Character` after the existing Seeker controller is ready. Its `CharacterIdentity` and human
`CharacterControllerAssignment` both retain the controller's exact parsed `CharacterId` instance.
The remaining required aggregate components are explicitly temporary empty authored backgrounds,
capabilities, limitations, conditions, inventory, beliefs, relationships, and goals. Its knowledge
contains exactly one authored `stone-marker-meaning` reference used only by the bounded
reinterpretation path. The source exposes only its one retained character and does not grant
knowledge or change focused-search, perception, movement, input, or presentation state.

`Allrpcs.Game.TemporaryPassiveNoticingCandidateSource` separately authors and retains one
identifier-only candidate after the character and world-object sources are ready. It uses the
temporary character's exact identity identifier and the first authoritative world object's exact
identifier without reading authored text, greybox coordinates, distance, line of sight, focus,
input, or perception. It does not call the controller, produce or resolve a candidate, change
visibility, or alter any presentation. During `Main._Ready`, the separate scene coordinator supplies
that exact candidate to the controller's one-time ordinary production and resolution sequence; the
source retains its unchanged candidate after the controller consumes the live snapshot entry.

`Allrpcs.Game.TemporaryObservationReinterpretationCandidateSource` separately retains one candidate
using the temporary character's exact identifier, the second authoritative world object's exact
identifier, and the character's exact first knowledge reference. It does not parse copied identity
text, inspect perception, or call the controller. During `Main._Ready`, the coordinator resolves the
target through the live world-object controller, produces the exact candidate from the exact source
character and object, then resolves it once. The atomic result changes only the stone marker from
`Noticed` to `Identified`, consumes the live candidate, leaves every source unchanged, and exposes
the already-authored stone-marker description through the existing indicator.

The temporary `focused_search_start` action maps physical F to one `Main._UnhandledInput` branch.
On that pressed action only, `Main` resolves the current focused identifier through
`WorldObjectController` and passes only the exact returned object plus the context source's exact
retained condition, capability, and inventory aggregates to the interaction-focus start boundary.
A matching herb-patch or stone-marker object may therefore start the existing immutable activity
and show its indicator. Clear, hidden, disabled, out-of-range, missing-object, or already-active
attempts preserve the existing focused-search snapshots and surrounding world-object, target,
perception, context, and presentation state; the missing-object path reports `UNCHANGED` without
invoking the start boundary.

The separate temporary `focused_search_cancel` action maps physical C to one narrow
`Main._UnhandledInput` branch. On that pressed action only, `Main` finds the Seeker's active
activity in the controller's current immutable snapshot and passes that activity's exact retained
request to `FocusedSearchActivityController.TryCancelSearch`. Cancellation does not read or depend
on the current interaction focus. A successful cancellation reaches the existing policy and
transition, replaces the snapshot with its empty successor, and hides the activity indicator; no
active search and repeated cancellation preserve the existing snapshot instance and presentation.

The separate temporary `focused_search_complete` action maps physical E to the equivalent narrow
`Main._UnhandledInput` path. It selects the same exact retained active request, resolves only that
request's target through `WorldObjectController`, and passes the exact returned object to the
temporary outcome source. Only a created `Identified` outcome is forwarded to
`FocusedSearchActivityController.TryCompleteSearchWithOutcome`, without reading or depending on
interaction focus. A successful completion replaces the activity snapshot with its empty
successor, appends the exact outcome to a new outcome snapshot, and hides the indicator. No active
search, an absent target object, and repeated completion preserve the focused-search snapshots,
world-object source and controller state, targets, perception, and presentation; the attempted E
path reports `UNCHANGED`.

The separate temporary `focused_search_resolve` action maps physical R to one narrow
`Main._UnhandledInput` path. It selects the first retained outcome in stable recorded order whose
request belongs to the controller's value-equal Seeker identifier, asks `WorldObjectController` for
that outcome's target identifier, and passes both the exact outcome and exact looked-up world-object
instance to `FocusedSearchActivityController.TryResolveOutcome` only when the object exists. The
controller delegates complete bound eligibility to the pure Domain policy before applying or
consuming anything. That decision accepts distinct value-equal target identifiers and rejects a
mismatch. The path does not inspect interaction focus, mutate the world-object snapshot or object,
or make either world-object component authoritative. A successful resolution for either
scene-authored target replaces only the matching perception entry with its `Identified` successor,
consumes the exact outcome, and preserves completed activity absence plus every unrelated entry
instance and order. With no matching outcome, a target absent from the current world-object
snapshot, a target-mismatched controller input, or a repeated R after consumption, all three
focused-search snapshots plus world-object, source, target, and presentation state retain their
exact instances; R reports `UNCHANGED` where input was attempted.

Each visible greybox target owns one instance of
`Allrpcs.Game.TemporaryResolvedPerceptionIndicator`. Its presentation boundary receives only that
target's existing nullable `WorldObject` and the controller's current Seeker perception, uses only
the object's exact stable identifier with `CharacterPerceptionLookup`, and shows its authored
`IDENTIFIED` label only for the exact `VisibilityState.Identified` singleton. Null objects or
perception, absent or unrelated entries, `Noticed`, and every other state remain hidden without
ranking or exposing object identity or description. `Main` initializes both indicators after the
existing one-time perception initialization and refreshes them only after a successful R
resolution. Each refresh resolves the visible target through `WorldObjectController` and passes
only the exact returned object; a missing object hides that target's marker without changing state.
Failed and repeated resolution paths preserve the existing presentation.

These inputs add no automatic polling or outcome processing, discovery grant, failure-reason
presentation, or final interaction UI.

One separate temporary label initializes to `Focused search: NO ATTEMPT`. A pressed F, C, E, or R
updates it with the attempted action and `CHANGED` or `UNCHANGED`, using only the existing Boolean
return from that lifecycle call path. It does not infer or present a failure reason. Released
focused-search actions, unrelated input, and healer actions preserve the exact prior text, while
the existing handled-input behavior remains unchanged.

The discovery-condition boundaries do not inspect a complete character, ownership or capacity
snapshots, perception or world state, grant discovery, or change visibility. They do not define
configurable Boolean expressions, alternative requirement groups, further requirement types, tool
classifications or properties, passive-noticing collection, selection, ordering, eligibility,
outcomes, or transitions, reinterpretation, actions, results, errors, events, effects, mutable
state, persistence, serialization, Godot, or UI behavior. Beyond the pure combined eligibility
decision, the focused-search boundaries add no discovery grant, visibility change,
complete-character or world inspection, replacement, range, duration, location, progress, outcome
production or application, reason value, further lifecycle policy, in-place mutation, scheduling,
persistence, production activity source, or final UI behavior. The temporary Godot adapters change
only the authored indicator visibility and the Boolean-derived result text, and the temporary
controller owns only the immutable snapshots and bounded lifecycle delegations described above.
The interaction-focus controller adds only
world-object-gated focus availability plus valid-focus and matching-world-object request/start
composition, while the temporary context source only supplies retained greybox inputs. Ordinary
greybox play invokes the start bridge only through physical F,
cancellation only through physical C, and completion only through physical E.

Point-and-click movement, zoom or camera-pan input, controller mappings beyond the documented
temporary keys, final interaction actions or authoritative outcomes, Phase 2 objects and
controller-owned perception models,
natural-language commands, final command or interaction UI, LLM-controlled companions, providers,
persistence, credentials, signing, installers, and distribution configuration are not part of
this greybox slice.

## Licence

Allrpcs is licensed under the GNU Affero General Public License version 3 only
(`AGPL-3.0-only`). See `LICENSE` for the complete terms.

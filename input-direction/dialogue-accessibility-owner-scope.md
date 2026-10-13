# Dialogue keyboard and accessibility presentation proposal

Prepared after publication of the finite controls implementation on 2026-10-13.
The owner instructed continuation to the next slice. This completes read-only
planning for the remaining Phase 11 keyboard-only/screen-reader UI item. It does
not authorize the new presentation changes below. The companion JSON binds the
actual current sources, exact proposed files and finite verification boundary.

## Missing deliverable

The temporary DialogueOverlay has an unrestricted LineEdit, Submit button,
Whisper check button and delivery/response labels. TemporaryDialogueInputPanel
routes Enter and pointer Submit to the same DialogueInputController.TrySubmit;
accepted input clears, rejected input remains, and the checkbox selects the
existing delivery mode. This contract and its downstream turn owners stay exact.

The shared optional-overlay toggle also uses default Tab. With the optional
panels visible, that handler consumes Tab before GUI focus traversal. The controls
implementation solves navigation inside its own panel and focuses the Controls
entry on opening settings, but the dialogue form has no deliberate keyboard entry,
contained navigation or visible close control. Its input is described by a
placeholder; no explicit assistive name/description or live-status policy is set.
Existing label fields are temporary presentation, not screen-reader evidence.

The installed Godot 4.6.3 C# API contains Control.AccessibilityName,
AccessibilityDescription, AccessibilityLive and label/description relationships.
That establishes available metadata hooks only. It does not prove VoiceOver or
another assistive application's announcement, order, text visibility or usability.

## Exact proposed implementation grant

Approve **phase11-dialogue-accessibility-runtime-integration** for one existing
DialogueOverlay panel and its optional-overlay navigation integration:

1. Add a visible Dialogue entry beside the existing settings entries and an
   explicit Close control in the dialogue panel. Keep the Controls entry's initial
   focus and Tab→Enter entry behavior; keep the whole Controls editor, documents,
   event catalog, capture/defaults and prompts unchanged. Use a finite input-row
   adjustment within the current panel to fit Input, Submit and Close at 1280×720;
   preserve every existing named node path and response/text owner. Proposed row
   bounds: Input right 718; Submit left/right 726/816; Close left/right 824/924
   (existing row top/bottom 5/36). New Dialogue/Settings Close entries use the
   existing left entry column at 12–180, with rows 298–338 and 352–392.
2. When settings entries own focus, default Tab/Shift+Tab traverse the finite
   Controls→Audio→Dialogue→Settings Close entry cycle instead of closing the overlay.
   Provider form controls remain untouched and outside this bounded entry cycle.
   The retained overlay-toggle action
   still opens settings from world context and closes from a non-focus-navigation
   binding. Add a visible settings Close and UI cancel route so default Tab never
   strands the player. This is a new presentation navigation rule, explicitly
   requiring this grant; no remapping action or persisted schema changes.
3. Dialogue entry shows DialogueOverlay, hides SettingsOverlay and gives Input
   first focus. Inside dialogue, honor existing UI focus actions for a fixed
   cycle Input→Whisper→Submit→Close and reverse. UI cancel and visible Close restore
   the Dialogue entry. Hiding a view releases only its retained focus. Return from
   Audio/Controls continues to restore those exact entries; it must not redirect
   their focus into dialogue. World input stays blocked by the existing GUI gate;
   world time, AI and task progress continue.
4. Keep Enter and Submit on the same existing submission path. Empty/invalid
   input stays rejected and retained; accepted input clears once. Whisper mode,
   speaker identity, delivery text and downstream character response qualification
   remain unchanged. Do not add response generation, provider calls, transcript
   storage, commands or new domain/application transitions.
5. Set explicit assistive names/descriptions for Input, Whisper, Submit and Close.
   Associate the input hint and submission status with Input through supported
   Control metadata. Use a polite live submission-status region, never an assertive
   or per-frame repeated announcement. Name the existing final response region;
   keep its authoritative text and response owner unchanged. Hidden sibling views
   must not be exposed as active form controls. No new hidden or partial provider
   response is copied into accessibility metadata or diagnostics.
6. Prepare a named native screen-reader evaluation protocol and report each actual
   result as observed, failed or not performed. Actual VoiceOver evaluation on
   macOS may run only when an assistive session and owner-authorized access are
   available. No OS screen-reader setting is changed by this scope. Synthetic
   focus or property inspection cannot substitute for actual reader output.

## Files and preservation

Modify only Main.tscn, TemporaryDialogueInputPanel.cs,
OptionalOverlayVisibilityController.cs, README.md, ROADMAP.md, HANDOVER.md,
CONTINUATION.json and quality.py. Add one opt-in DialogueAccessibilityMainSmokeCheck
script/scene and one validator, with only their engine-generated UID sidecar.
Verification-only current DialogueInputSmokeCheck and
OptionalOverlayVisibilitySmokeCheck assertions may be updated for the approved
entry/navigation/metadata behavior; exact baseline hashes and reasons must be
recorded before edits. Never change frozen validators or acceptance evidence.

Keep DialogueInputController, Domain utterance/delivery rules, response/model
labels' source code, controls storage/catalog/editor/context, provider credentials,
world/save schema, audio/art resources, geometry and all admission owners unchanged.
Keep Main’s final WholeChannelPresentation root-child position and its exact five-sprite owner.
The companion JSON enumerates exact paths. The accepted controls validator runs
in its published baseline when this explicit additive presentation successor
changes shared source hashes; it must remain unmodified, with the historical
stage inventory and source/byte evidence preserved.

## Finite completion and evaluation

Complete when the new entry/close/focus flow and metadata are implemented, existing
submission semantics remain exact, both native window sizes pass, actual reader
results or their unavailability are recorded honestly, the finite validator rejects
missing/stale evidence, full root quality passes, and the slice is closed/committed/
pushed under standing publication authority.

The genuine Main fixture must use ordinary keyboard event routing to enter the
form, traverse all four controls forwards/backwards, submit one fixed synthetic
ordinary utterance and one whisper, reject empty input, confirm retained/cleared
text and exact speaker/delivery, close/reopen, and verify world commands do not
escape focus. Test default and one remapped UI focus/cancel configuration without
altering the accepted controls document/schema. Return from Audio/Controls must
still restore the corresponding entry. Retain native focus/metadata reports and
inspect actual PNGs at 1280×720 and 1920×1080. Fixtures disable providers and log
only canned test identifiers/outcomes, never real dialogue or credentials.

The named-reader protocol records OS/engine/reader version, keyboard path, spoken
name/role/mode, accepted/rejected submission status, final response availability,
focus return and hidden sibling exposure. Store concise outcome notes rather than
recordings of user content. If no assistive session exists, mark this evaluation
not performed and keep the Phase 11 screen-reader checkbox open. Even a successful
single-panel VoiceOver result cannot certify journal, map, provider settings or all
text-heavy UI. Windows/Linux readers, controller-only gameplay, executable exports
and broad accessibility certification remain outside this finite grant.

Planning completes here. The sole next decision is this exact presentation grant;
no runtime or UI behavior changes were made by preparing it.

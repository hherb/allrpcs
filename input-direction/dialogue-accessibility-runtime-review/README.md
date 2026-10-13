# Finite dialogue keyboard and accessibility presentation

The owner approved the exact `phase11-dialogue-accessibility-runtime-integration`
proposal on 2026-10-13. Its grant and immutable proposal bindings are retained in
[authority](../dialogue-accessibility-runtime-authority.json).

Settings opens with Controls focused. Its contained entry cycle is Controls,
Audio, Dialogue, Close settings. Dialogue opens with Input focused, hides the
settings siblings and cycles Input, Whisper, Submit, Close in both directions.
Visible Close and the current UI cancel return to the exact originating entry.
A toggle that is not a focus-navigation binding still closes settings. Enter and
Submit use the unchanged `DialogueInputController.TrySubmit` path: exact Seeker
identity, ordinary/whisper delivery, accepted text cleared, rejected text retained.
The editor stays active after Enter. Existing response/model/domain/provider,
controls document/editor, world-time/save and audio/art owners are unchanged.

Input, Whisper, Submit and Close have explicit assistive names/descriptions;
Input references its hint and submission status. Submission status is a polite
live region updated only on a changed result. The final authoritative response
label is named without duplicating hidden or partial model content. The two
approved overlays use layer 6 above the independent caption layer, and the four
settings entries use opaque surfaces; native review found their prior draw order
obscured the text. The existing Controls editor remains on its own layer 30 and
Audio on 20. WholeChannelPresentation stays the final Main root child unchanged.

[Verification](../dialogue-accessibility-runtime-verification.json) binds tested
sources, a protected predecessor inventory, successful native/headless reports,
raw/decompressed log hashes, six inspected PNGs and the named reader protocol.
Each genuine Main mode passes 14 checks with a 59-step keyboard focus trace,
covering default and remapped focus/cancel, both cycles, Controls/Audio entry
return, ordinary Enter and whisper Submit, empty/whitespace rejection,
close/reopen, world-input suppression, metadata and native pointer Submit/Close
with Left mapped to UI Cancel. Providers are disabled;
only canned fixture identifiers/outcomes are logged. These are synthetic keyboard
events and actual native rendering, not physical-device or reader observations.

The inspected settings, Input-focus and Close-focus views at 1280×720 and
1920×1080 have readable controls/status, visible focus outlines and contained
form geometry. The retained provider panel still clips at these scaled sizes;
provider-form layout is outside this grant and was not changed or certified.

[VoiceOver protocol](reader-protocol.md) and [outcome](reader-evaluation.json):
**not performed**. Host process inspection found no running VoiceOver session;
no OS setting was changed. No heard reader name/role/mode, status announcement,
response availability or hidden-sibling exposure is claimed. The wider Phase 11
keyboard-only/screen-reader roadmap checkbox remains open. Journal, map, provider
settings, Windows/Linux readers and broad UI certification remain unevaluated.
The existing C# executable-export limitation remains.

The frozen controls validator and all eight retained stages run unchanged in
published revision `1b0cab4060dd1eaca9c780cf85c38be1662d8513`, with explicit restore,
build and import. No frozen validator, acceptance hash or evidence is rewritten
for this successor. Current checks add the finite dialogue validator/rejection
probes and Main fixture. Full root quality passes 195 stages and 5,576 unit tests. Its summary, every
referenced attempt log and root completion output are retained under `quality/`,
with exact raw and decompressed bindings. The earlier formatting failure is kept
as diagnostic evidence only; the successful complete run is the completion record.

Verification commands:

- `python3 scripts/validate-dialogue-accessibility-runtime.py --self-test`
- `python3 scripts/validate-dialogue-accessibility-runtime.py --complete`
- `python3 scripts/check-continuation.py --self-test`
- `python3 quality.py --self-test`
- `./quality.sh`

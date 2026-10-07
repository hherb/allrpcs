# Allrpcs audio direction and dynamic range policy

Version 0.1 · 2026-10-07 · Phase 9.3 proposal for owner review.

The direction is **quiet, tactile naturalism**: rain, water, leaves, weathered timber,
cloth and hand tools support slow exploration, conversation and healing. Sounds should
make the world feel inhabited and understandable while leaving room to think. Music is
sparse and optional. Silence is a useful part of the composition.

This document and [dynamic-range-policy.json](dynamic-range-policy.json) describe the
same proposal. The JSON is a design record, not runtime configuration. Numerical targets
are original provisional audition targets, not a claim of compliance with a delivery
standard, measured assets, hearing safety, or implemented accessibility. No audio has
been generated, purchased, installed or auditioned for this slice. Owner acceptance is
not yet recorded; preparation authority comes from the current continuation instruction.

## Settled constraints and current implementation

- [Roadmap vision and decisions](../ROADMAP.md#product-vision): investigation and healing,
  unrestricted dialogue, engine-owned truth/perception and future replaceable ASR/TTS.
- [Direction A art bible](../art-direction/representative-scene/ART_BIBLE.md): restrained
  storybook presentation, quiet traversal space and redundant gameplay cues.
- [Authored environmental baseline](../src/Domain/EnvironmentalStates/RepresentativeOpeningRegionEnvironmentalSource.cs):
  four regions with explicit rain/water/wind sound states. These are qualitative records,
  not audio playback. Keep the authored rain despite the art bible's visual light target;
  this policy does not change weather, lighting or accepted imagery.
- [Inscription variation](../game/OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator.cs):
  woodland rain thins only after the existing identified-inscription transition. It does
  not currently play sound. A future audio mapping may reflect that exact accepted state.
- [Dialogue delivery modes](../src/Domain/Dialogue/DialogueDeliveryMode.cs): ordinary speech
  and whispering have authoritative hearing boundaries. Mixing cannot grant perception.

The inspected scene/source has no production audio playback or bus/control implementation.
All behavior below is a requirement for later work, not a description of current features.

## Palette and regional roles

Prefer close, soft-edged recordings with recognizable material detail and limited layers.
Avoid relentless high-frequency hiss, heavy sub-bass, startling attack peaks and exaggerated
cinematic impacts. Do not add voices, crowds or animal populations that imply unauthored
characters or world facts. A creak is material movement, not evidence of an unseen person.

| Authored region / sound key | Proposed audible treatment |
| --- | --- |
| Village shelter / `muted-village-rain` | Soft rain on shelter surfaces, restrained room tone; distinct local cloth, containers and tools only when an action occurs. Sickhouse remains calm, without a disease alarm or countdown. |
| Waterway / `running-water-and-rain` | Broad rain bed with localized running water. Laundry material/water interaction follows actual use; water should remain identifiable without overwhelming conversation. |
| Woodland edge / `rain-under-trees` | Light leaf-rain texture, occasional branch movement and sparse foliage detail. No concealed-plant beacon, shrine whisper or unopened-gate lure. |
| Southern road / `road-wind-and-wagon` | Exposed wind/rain and occasional inert wagon creak. Spatial creak can orient the listener without suggesting an active agent or actionable clue. |
| Woodland / `inscription-revealed-sound` | Gently thin the existing woodland-rain layer after the exact state is installed. No discovery fanfare, hidden hint before identification, or extra weather change. |

Continuous beds describe place; localized emitters describe a particular source. Crossfade
boundaries instead of stacking four full-volume region beds. Do not turn source attenuation
into a substitute for world hearing, discovery or collision rules.

## Cue hierarchy and information authority

| Priority | Responsibility | Collision policy |
| --- | --- | --- |
| 1 | Intelligible authorized dialogue/speech and an authorized immediate consequential event | Keep speech intelligible; provide readable text for both. A consequential event may briefly interrupt decorative sound, never silently discard required information. |
| 2 | Player action confirmation, tool/remedy interaction and authorized meaningful off-screen events | Brief and recognizable. Suppress duplicate presentation, queue/coalesce repeated notices and preserve the text equivalent. |
| 3 | Footsteps and ordinary local material movement | Sparse variations with controlled peaks; reduce companion clutter and defer optional transients during speech. |
| 4 | Regional rain, water, wind, foliage and room tone | Maintain orientation at a lower level, gently duck where foreground competes. |
| 5 | Music and purely decorative details | First to yield; allow long gaps and complete user mute. |

Priority is a mixing rule, not new domain authority. Before presenting an information-bearing
sound **or its caption**, resolve whether the listener is entitled to perceive the event.
Off-screen eligibility must come from engine-owned reception/perception, not camera bounds,
an LLM claim, a renderer distance guess or the audio mix. Unknown sources stay unknown:
"wood creaks to the east" must not become "enemy at wagon". Captions must not reveal hidden
identity, exact position, disease state or a private/whispered utterance. Decorative rain
does not enter character memory as evidence. Player volume, mono mode, mute and mix preset
must never change which events characters hear or which actions succeed.

One optional short consequence cue may accompany an already delivered treatment or discovery
result. No proximity pings for hidden clues, continuous marker tones, ambient puzzle passwords,
or urgency that suggests wall-clock disease progression. Required knowledge remains recoverable
through the existing redundant evidence network and text interaction with all audio muted.

## Music

Audition a small acoustic palette: soft plucked strings, breath-like sustained tones without
vocal content, and occasional warm wooden resonance. Sparse motifs may support companionship,
rest and understanding. Avoid a constant melodic loop, rhythmic pressure, rising countdowns,
combat stingers or harmonic cues that secretly identify truth, deception or danger.

Default traversal may be music-free. Offer one sparse exploration texture and one short
rest/healing motif for the later audition; neither is assigned to runtime here. Trigger only
from explicit authored presentation events, with cooldowns and gentle fades. Dialogue, reading
and inventory should allow concentration; their presentation must not imply that disease time
already pauses, since that separate roadmap item remains unfinished.

## Provisional mix and dynamic range

Measure later at reference user gains, before user attenuation, on representative rendered
scenes. File normalization alone does not establish a coherent scene mix. Integrated LUFS
describes a complete sustained passage; LRA describes a suitably long passage, not a short
footstep or utterance. Audition transient effects relative to speech and the scene using
peak measurements and listening. Never boost a whisper to ordinary speech loudness by default.

| Proposed starting target | Scope / interpretation |
| --- | --- |
| Complete default mix: -23 LUFS-I, acceptable initial audition band -25 to -21 | Measure a five-minute traversal/action/dialogue sequence at reference gains; revise after real assets and device listening. |
| Ordinary future speech: -20 LUFS-I, initial band -22 to -18 | Measure a representative multi-utterance passage, excluding long deliberate gaps; assess quiet utterances individually for clarity. Not a target for whispered speech. |
| Working master peaks at or below -6 dBFS before final peak protection | Reserve summing space in the busiest representative scene; fix source gains and concurrency before relying on a limiter. |
| Delivered master true peak at or below -1 dBTP | Verify after rendering/encoding. This ceiling does not justify loud transients or guarantee device output level. |
| Default LRA: 8–12 LU; reduced-range LRA: 4–6 LU | Measure extended representative passages; listening decides suitability. Reduced range narrows peaks/floor without inventing new perceptible events. |
| Ordinary speech foreground margin: about 6 dB over competing ambience/music | Starting relative bus balance during the same passage; tune frequency masking and clarity by listening, not a promised intelligibility score. |

Default mode retains soft ambience and some action contrast. Reduced-range mode lowers action
peaks and steadies the authorized foreground for quiet environments. It must not lift noise
into a constant bed, expose previously unauthorized whispers or fight a user's sliders.
Compression cannot satisfy accessibility alone; readable text and captions carry information.

Starting ducking envelope (relative to each user's chosen bus gain):

| Foreground | Music reduction | Ambience reduction | Other effects reduction | Attack / hold / release |
| --- | --- | --- | --- | --- |
| Authorized speech playback | 6 dB | 3 dB | 3 dB for optional footsteps/details only | 100 / 150 / 600 ms |
| Authorized consequential action or cue | 3 dB | 2 dB | Defer optional details; retain necessary events | 50 / 100 / 400 ms |

Hold begins after the foreground ends. Overlap uses the strongest applicable reduction,
not summed reductions; release starts after all eligible foreground has ended. Smooth ramps
must avoid pumping under many short utterances. Reading typed dialogue may request the gentle
3 dB music / 2 dB ambience focus reduction while the panel is open; it does not pretend that
speech audio is playing. Do not duck speech to emphasize music or footsteps. Foreground cue
levels are bounded even when a user mutes competing buses. A muted bus stays muted.
Typed-reading focus uses the 100 / 150 / 600 ms speech envelope, with hold/release beginning
when the panel closes; overlapping speech or cues still use the strongest reduction.

## Bus ownership and controls

Proposed routing is five independently controlled categories into **Master**. Route each
sound once; do not make Speech a child of Dialogue or duplicate rain across Effects/Ambience.

| Bus / user control | Owns |
| --- | --- |
| Master | Final common gain, mute and output peak protection. |
| Music | Score, motifs and musical transitions. |
| Ambience | Continuous/positional water, rain, wind, foliage beds, room tone and decorative wagon creaks. |
| Effects | Footsteps, physical interaction, tools, remedy preparation and world action/result cues. |
| Dialogue | Optional nonverbal conversation start/end cues; no per-letter typing sound by default. |
| Speech | All future spoken character dialogue/TTS, including authorized whisper delivery; reserve the control before speech exists. |

Future controls: Master and each category get a slider and explicit mute, with a true zero
endpoint, readable current value, keyboard/controller focus, persistence and restore defaults.
Begin audition at unity category gain with assets balanced into the proposed mix; no hardcoded
minimum gain or automatic override. Explain that Speech is reserved until playback exists.
General UI confirmation sounds route to Effects and must never be the only confirmation.
Supply Default and Reduced range, mono output, sound-caption settings, and music mute.
These are requirements for a later scope; no existing settings UI is changed here.

## Accessibility and future voice

- Preserve full typed dialogue, subtitles and consequence text at all times. Meaningful
  non-speech sounds need optional readable captions with source/direction only as authorized;
  do not caption every rain transient or require hearing/stereo to solve anything.
- Meaningful cues remain distinct through rhythm/timbre and text. Check stereo, mono, speakers,
  headphones, low playback volume, all categories muted and reduced range. Pan alone cannot
  carry location; captions may use coarse direction and proximity without granting extra facts.
- Let captions persist long enough to read; cap decorative caption density and retain important
  events in an accessible history. Avoid startling cues, mandatory high frequencies, rapid
  repetitive alerts and sound-only focus feedback.
- Future TTS consumes the same authorized utterance/delivery event as text, routes to Speech,
  supports cancellation/interruption, and releases ducking when cancelled or failed. If speech
  is unavailable, text completes normally without holding the mix down or blocking interaction.
- Future ASR uses the existing dialogue pipeline with push-to-talk first. User volume and
  captions do not alter recognition authority or automatically enable a microphone. Capture,
  feedback prevention, device failure and permissions remain Phase 10 implementation work.
- Speech playback duration, ambience loops and music triggers must not advance disease time
  or world decisions. Any pause semantics require the separate gameplay slice.

## Asset classes, provenance and finite next boundary

Keep editable recordings/session projects separate from delivery exports. Later acquired or
generated assets require a source/license record, usage rights, edit lineage, loop boundaries,
channel/sample metadata and measured level/peak notes. No provider or acquisition method is
selected here. Propose lossless working/delivery masters; choose runtime encoding only in the
later measured platform scope. Do not turn candidate IDs into canonical domain facts.

The finite later audition is **16 candidates**, grouped for one review:

1. Four regional bed excerpts: sheltered rain, running water/rain, leaf rain, exposed wind/rain
   with a sparse wagon creak.
2. Four walking material excerpts: packed earth, gravel, leaf litter and timber. Each excerpt
   includes several contacts; select actual material ownership in the later runtime scope.
3. Six action/feedback excerpts: water contact, container/cloth, hand tool, mortar/preparation,
   restrained treatment result and UI/conversation feedback (demonstrating separate routing).
4. Two music excerpts: sparse exploration texture and rest/healing motif.

Compare the complete series in one standalone five-minute mix and a Reduced range variant,
plus one music-free render. Include representative spoken reference material only with explicit
rights and permission in that audition scope; it is a mixing reference, not production TTS.
Owner listening should assess the series together before runtime installation.

Recommended next implementation boundary after policy review: the roadmap item **Add volume
buses and independent controls for music, ambience, effects, dialogue, and future speech**.
First prepare a concrete scope for five sibling buses plus Master, persistent category gains,
mute, mix preset, mono and captions settings with a disposable routing demonstration. Runtime
changes, test clips, asset acquisition and audition each need their actual named scope; this
proposal grants none of those. Do not start the whole sound/music/voice roadmap at once.

Later audition passes when source rights are recorded, the complete candidate series and
representative mixes are listened to, loudness/peak/range and loop measurements are retained,
speech and consequence information stays readable, muted/mono/reduced-range checks pass, and
one owner selection binds the agreed series. Later bus/control work passes when independent
routing, exact mute/restore/persistence, interrupted ducking, mono and accessible focus are
demonstrated. Those completion conditions are not claimed by this documentation slice.

## Review and authority

Repository consistency review checked the rain baseline, inscription transition, dialogue
delivery boundaries, independent category ownership and correspondence with the JSON targets.
No listening, acoustic/device testing, runtime functionality or owner aesthetic acceptance is
claimed. The one owner decision is whether to accept this policy direction, or revise it.
The active policy slice remains open for that review; previous visual scopes remain closed.

# Herb-collection source and accepted HC01

The owner granted the exact bounded source/use/runtime scope with **approved**
on 2026-10-13. See [authority](../herb-collection-runtime-authority.json).
The owner accepted the exact sound, window and level with **suitable** on 2026-10-13; unchanged HC01 is installed. No further listening decision remains.

One source download, **20 Rustles of dry leaves** by **qubodup**, is retained as
`originals/qubodup-rustle.7z`. Its [creator upload](https://opengameart.org/content/20-rustles-dry-leaves)
declares CC0, links the [CC0 1.0 deed](https://creativecommons.org/publicdomain/zero/1.0/)
and describes twenty kitchen-recorded, stereo 24-bit/96-kHz FLAC sounds. Catalogue
and deed HTML (catalogue stored as lossless gzip to preserve exact whitespace),
retrieval metadata, download/archive/member hashes and exact edit
lineage are retained in [provenance.json](provenance.json). No purchase or account
action was needed. Preserve attribution and do not imply creator endorsement.

The only selected lossless member is **rustle/rustle14.flac**. The exact archive
is the source original; preparation extracts this unchanged member in memory.
The other members are not prepared, installed or included in runtime packaging.
The creator supplies no detailed processing history; none is invented here.

HC01 selects the contiguous **0.10–1.10-second** window. The required conversion
retains stereo and resamples 96 to 48 kHz, then applies 10-ms onset and 50-ms
ending linear fades and one uniform **−20.26732775-dB** attenuation. The candidate
is one second, 48,000 frames, stereo PCM24, nonlooping; its measured sample peak
is **−24.000006 dBFS** and integrated loudness **−37.5 LUFS**. No filtering,
denoising, layering, synthesis, pitch/time change or widening is used.
Measurements describe this short isolated file, not device or ordinary-play output.

The numerical envelope was used to nominate a window, not to claim that an agent
heard a clean leaf sound. The owner's exact listening acceptance qualifies this unchanged sound and level.
The intended accessible phrase is **Soft rustle**; it conveys no herb, safety,
actor, collection or outcome facts. The installed emitter is conditional on
the exact existing commit and actual 32-pixel actor / exclusive 192-pixel listener
bounds, as specified in the frozen [proposal](../herb-collection-owner-scope.md).

The [exact listening candidate](../herb-collection-runtime-review/source/HC01-candidate.wav)
and [measurements](../herb-collection-runtime-review/source/passage-measurements.json)
retain the exact reviewed bytes. The runtime asset at `game/assets/audio/herb-collection/HC01.wav` is byte-identical, imported as uncompressed native PCM16. HC01 SHA256:
`cab3292c8907a7ae69b5a09a15ce8d0384298e4cfd2346126312eb322d6c4528`.

Reproduce with `python3 scripts/prepare-herb-collection-passage.py`. Eleven
meaningful grant/source/encoding/listening-gate rejection probes pass with
`python3 scripts/prepare-herb-collection-passage.py --self-test`. `--install`
requires an exact owner statement binding the candidate bytes and window.
The recorded exact listening acceptance satisfies that gate. No relaxed source exception or
substitution of accepted PCM is permitted.

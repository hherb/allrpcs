# Exact covered-pot source and H01

The owner accepted TinyWorlds' nominated `cooking_with_cover_01.ogg` with
“I just listened to it - sounds perfect!” and subsequently granted “runtime approved”.
The [runtime authority](../remedy-heating-runtime-authority.json) covers the supplied
Vorbis encoding. No lossless original is claimed. Embedded artist metadata names
`kddekadenz`; the catalogue identifies TinyWorlds. The retained catalogue and CC0
notice are in `rights/`. The live Creative Commons endpoint returned HTTP 403;
the deed is the exact previously retained CC0 document, with that provenance disclosed.

[Provenance](provenance.json) binds the original, decode, trim, resample and installed
WAV. Reproduce with `python3 scripts/prepare-remedy-heating-passage.py` from the
repository root using the recorded FFmpeg version. Decode stereo 44.1 kHz Vorbis;
select source frames 22050 through 154349 (0.5–3.5 seconds); convert once to 48 kHz
with the recorded swr filter; apply 100 ms linear fades and one attenuation-only gain;
store stereo PCM24 without looping. The source already meets the ceiling, so the
uniform gain is 0 dB. H01 has 144000 frames, a -26.73 dBFS sample peak and -43.4 LUFS
integrated loudness. Its SHA256 is
`55928018939d77698b96b710339313daffbae03ec592ccba9a269eb4f2f9a327`.
Godot imports uncompressed PCM16; mounted byte comparisons verify that conversion.

The selected interior window excludes the original endpoints. Decode/frame/finite/
peak checks passed and the owner’s source listening acceptance is carried forward.
The final three-second cue was presented inline during implementation. No independent
agent acoustic review, fresh-device listening or recovery of lost Vorbis information
is claimed. No other source or synthesized layer was introduced.

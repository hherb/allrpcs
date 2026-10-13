# Offline bilingual TTS spike

Qwen2.5-Omni is discarded by the owner. Its original audition is preserved unchanged; see
[the model decisions](../model-decisions.json). The [new owner request](owner-request.json) authorizes
this isolated Phase 10 TTS experiment. No game backend is selected or integrated.

Pocket TTS 3.3.0 was the original CPU recommendation. Because expressive acting is important,
the spike also auditions Qwen3-TTS 1.7B CustomVoice with a dedicated instruction field.
The actual machine is an Apple M3 Max with 128 GB unified RAM. M1 16 GB and other platforms are untested.

## Measured result

| Backend | Warm first PCM chunk | Complete generation | Peak memory | Word checks |
| --- | ---: | ---: | --- | --- |
| Pocket, CPU float32 | 31–35 ms | 0.35–0.55 s for 3–5 s audio | 1.05 GiB lifetime max RSS | 4/6 exact word matches |
| Qwen3, MLX 4-bit | 96–123 ms | 1.10–1.66 s for 4–6 s audio | 2.71 GiB peak MLX allocation; 2.69 GiB max RSS | 6/8 exact word matches |

The first Pocket inference took 0.38 s with a 91 ms first chunk. Language/model loading took
0.30 s (English) and 0.29 s (German); cached voice state loading was under 2 ms.
The Qwen3 load took **34.98 s** inside the measured load call. Its first inference took
**4.70 s**, including 3.63 s to the first chunk. Keeping its worker/model alive matters.

GPU allocation and RSS overlap on Apple unified memory: do not add them. Pocket used one
PyTorch intra-op thread and one inter-op thread; the library also overlaps generation and
decoding on worker threads. Pocket languages were loaded sequentially, not simultaneously.
Model loading followed checkpoint hash checks, so the operating-system file cache was warm.

These are single trials under uncontrolled background load, with no game or dialogue model.
The Pocket run overlapped the final Qwen checkpoint download; the Qwen timing run followed
download completion and the Pocket transcript check. Inference used local files with
`HF_HUB_OFFLINE=1` and `TRANSFORMERS_OFFLINE=1`; network access was not blocked at OS level.
First PCM availability is not speaker playback latency. The [WAV verification](wav-verification.json)
also identifies the first delivered chunk containing a sample above −40 dBFS: 33–91 ms for
Pocket and 96–123 ms for warm Qwen3. This threshold is a diagnostic, not human audibility.

Both candidates have substantially more memory headroom than the rejected Omni configuration.
The results justify a real M1 test, but do not prove the combined 16 GB game/LLM/speech budget.

## Listening comparison

The short English line is “Stay close. There is still hope for the village.”
The German line is “Bleib bei mir. Es gibt noch Hoffnung für das Dorf.”

Pocket uses Alba for English and Juergen for German. Its punctuation trials replace periods
with exclamation marks; they do not represent an instruction-controlled urgent performance.
The short Pocket clips all passed the auxiliary word check. Reference-based acting was not
tested: the cloning-enabled checkpoint returned a gated-access rejection with the cached
Hugging Face account. The public checkpoint and official stock voice states were used.

Qwen3 uses Ryan in both languages. Calm, urgent and whisper use identical words, voice and
seed, changing only the separate delivery instruction. Ryan is an English-native preset;
these clips do not establish a native German character voice. Timing/waveform differences
establish a response to the request, not convincing acting or stable character identity.

| Backend / clip | Audio | Generation | First PCM | Word check |
| --- | ---: | ---: | ---: | --- |
| [pocket / en-normal](output/pocket/en-normal.wav) | 2.88 s | 0.38 s | 91 ms | Match |
| [pocket / en-punctuation](output/pocket/en-punctuation.wav) | 3.12 s | 0.35 s | 33 ms | Match |
| [pocket / en-game](output/pocket/en-game.wav) | 4.08 s | 0.46 s | 34 ms | Mismatch |
| [pocket / de-normal](output/pocket/de-normal.wav) | 4.56 s | 0.50 s | 35 ms | Match |
| [pocket / de-punctuation](output/pocket/de-punctuation.wav) | 4.56 s | 0.49 s | 31 ms | Match |
| [pocket / de-game](output/pocket/de-game.wav) | 5.12 s | 0.55 s | 33 ms | Mismatch |
| [qwen3 / en-calm](output/qwen3/en-calm.wav) | 4.16 s | 4.71 s | 3631 ms | Mismatch |
| [qwen3 / en-urgent](output/qwen3/en-urgent.wav) | 4.96 s | 1.34 s | 102 ms | Match |
| [qwen3 / en-whisper](output/qwen3/en-whisper.wav) | 5.84 s | 1.66 s | 123 ms | Match |
| [qwen3 / en-game](output/qwen3/en-game.wav) | 4.96 s | 1.27 s | 109 ms | Match |
| [qwen3 / de-calm](output/qwen3/de-calm.wav) | 4.40 s | 1.10 s | 96 ms | Match |
| [qwen3 / de-urgent](output/qwen3/de-urgent.wav) | 5.68 s | 1.51 s | 104 ms | Match |
| [qwen3 / de-whisper](output/qwen3/de-whisper.wav) | 6.16 s | 1.57 s | 116 ms | Match |
| [qwen3 / de-game](output/qwen3/de-game.wav) | 5.20 s | 1.28 s | 97 ms | Mismatch |

The longer lines test negation, a herb name and bandages/healer vocabulary. Both Pocket
longer lines, Qwen3 English calm and Qwen3 German longer dialogue had ASR mismatches:

| Clip | Independent Whisper transcript |
| --- | --- |
| pocket / en-game | Do not take the moonwirt yet. Ring two clean bandages to the healer. |
| pocket / de-game | Nimm den Montrautenfahren noch nicht. Bringt zwei saubere Verbände zur Heilerin. |
| qwen3 / en-calm | Stay close. Mary is still hope for the village. |
| qwen3 / de-game | Nimm den Mondrautenfahren noch nicht. Brings soweit saubere Verbände zu heilern. |

Whisper base was run separately with the language specified, temperature zero and no
expected-text prompt. It can make errors, especially on names, compounds and whispers.
The mismatches require listening review; neither these transcripts nor matching text
prove the exact audible words. There is no owner listening acceptance recorded.

## Reproduce

Create separate Python 3.12 environments from [Pocket requirements](requirements-pocket.txt)
and [Qwen3 requirements](requirements-qwen3.txt). The current temporary environments are
`/private/tmp/allrpcs-pocket-venv` and `/private/tmp/allrpcs-qwen3-tts-venv`.
The operating system can clear temporary environments; requirements and source are retained.

Run [prepare_models.py](prepare_models.py) with the Pocket environment to download the pinned
public checkpoints and verify published SHA-256 values. Weights live under this checkout’s
ignored `artifacts/speech-models/`; no weights are committed. This preparation step uses
networking and is separate from inference. English and German Pocket configs are copied
from the pinned package and rewritten to absolute local weight/tokenizer paths.

From the repository root, with native CPU/Metal access:

```sh
HF_HUB_OFFLINE=1 TRANSFORMERS_OFFLINE=1 /path/to/pocket-env/bin/python \
  speech-direction/offline-tts-spike/benchmark.py pocket --output-dir /path/to/new-pocket-output
HF_HUB_OFFLINE=1 TRANSFORMERS_OFFLINE=1 USE_TORCH=0 /path/to/qwen3-env/bin/python \
  speech-direction/offline-tts-spike/benchmark.py qwen3 --output-dir /path/to/new-qwen3-output
```

Qwen3 uses the community 4-bit conversion, including its bundled speech tokenizer; this
does not qualify the unquantized original or another runtime. Streaming is enabled with
a 0.32 s chunk interval, 256 speech tokens maximum and temperature 0.9. No case exhausted
the token budget. The seed is reset to 20261013 for every case. Both backends write raw,
unnormalised 24 kHz mono PCM16. All fourteen clips are finite, hash-checked and unclipped.

The optional [transcript checker](check_transcripts.py) used a separate cached multilingual
`mlx-community/whisper-base-mlx` snapshot `a8f14b616d37f94a159016b04f80a7f7003a1b8f`,
`mlx-audio==0.2.8` and `mlx==0.30.1` in the earlier Omni environment. Pass a local Whisper
model directory with `--model-dir` and a benchmark output directory with `--output-dir`.
The checker resamples 24 kHz WAVs to 16 kHz, then compares case/punctuation-insensitive words.

## Evidence and limits

- [Pinned checkpoint verification](model-verification.json), [case protocol](cases.json)
  and [finite closure](closure.json).
- [Pocket measurements](output/pocket/results.json), [benchmark log](pocket-benchmark.log)
  and [transcript check](output/pocket/transcript-check.json).
- [Qwen3 measurements](output/qwen3/results.json), [benchmark log](qwen3-benchmark.log)
  and [transcript check](output/qwen3/transcript-check.json).

The Qwen loader emitted a Transformers model-type warning but initialized the text and
speech tokenizers and completed all clips. Its raw warning is retained. A redundant setup
download encountered a Brotli/httpx decompression incompatibility; adding `brotli==1.2.0`
to the isolated preparation environment fixes that dependency mismatch. The successful
main download and published checkpoint hashes establish the assets used for inference.

No microphone, ASR backend, game interface, playback/cancellation integration, cross-platform
package or M1 test is implemented by this spike. No ledger or concurrent controls work changed.
The next hardware evaluation should measure cold startup, repeated warm latency, character
voice consistency and pronunciation with Godot and the local dialogue model participating.

## Primary sources and attribution

- [Kyutai Pocket TTS](https://github.com/kyutai-labs/pocket-tts): CPU engine, language configs
  and voice-conditioning API. Engine: MIT; public checkpoint: CC BY 4.0.
- [Public Pocket checkpoint](https://huggingface.co/kyutai/pocket-tts-without-voice-cloning).
- [Voice catalogue](https://huggingface.co/kyutai/tts-voices): Alba MacKenna’s casual voice
  recording is CC BY 4.0. Juergen is the official German stock voice; its source is pinned
  by Pocket’s catalogue in `kyutai/pocket-tts/de-DE-juergen.mp3`. Individual voice-source
  provenance must be resolved before choosing/distributing a game voice.
- [Official Qwen3-TTS repository](https://github.com/QwenLM/Qwen3-TTS): language support,
  CustomVoice instruction control and Ryan’s English-native voice description.
- [Auditioned MLX conversion](https://huggingface.co/mlx-community/Qwen3-TTS-12Hz-1.7B-CustomVoice-4bit/tree/f35faf19b0cc2160865af64ecf0f22f83d335135): Apache 2.0 model card.
- [MLX Audio implementation](https://github.com/Blaizzy/mlx-audio): Apple Silicon inference
  and streaming implementation. Its use here does not establish Windows/Linux support.

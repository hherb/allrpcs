# Qwen2.5-Omni-7B local speech audition

The owner requested this finite Phase 10 experiment on 2026-10-13; see
[the exact request](owner-request.json). Expressive acting and both English and German
are required. This package evaluates speech output before backend selection or game
integration. The separate herb-collection implementation remains in its existing ledger.

## Result

The four local speech clips were generated successfully. English passed an independent
Whisper word check; German did not. Qwen's generated **text** matched every requested
line, but the German **audio** produced heavily garbled ASR transcripts. This is a
significant pronunciation concern, with listening review still needed. No model has
been selected for the game.

| Clip | Audio duration | Generation time | Real-time factor | Whisper word check |
| --- | ---: | ---: | ---: | --- |
| [English calm, Chelsie](output/en-chelsie-calm.wav) | 3.92 s | 47.55 s | 12.13 | Match |
| [English urgent, Chelsie](output/en-chelsie-urgent.wav) | 3.22 s | 15.04 s | 4.67 | Match |
| [German calm, Ethan](output/de-ethan-calm.wav) | 8.28 s | 57.75 s | 6.97 | Mismatch |
| [German urgent, Ethan](output/de-ethan-urgent.wav) | 4.72 s | 21.13 s | 4.48 | Mismatch |

Real-time factor is generation time divided by audio duration; lower is better.
Model loading took 4.90 s, separately from generation and from the weight download.
Loading followed hash verification, so the operating-system file cache was warm.
The first English clip was the first inference in this process. The first German
clip was the first use of Ethan. These single trials do not establish steady-state
latency or isolate compilation, voice, length and acting effects. Background load
was uncontrolled and is retained in the report.

Maximum sampled Metal driver memory was **22.82 GiB**, with 21.51 GiB maximum
allocated Metal memory. Process maximum RSS was 5.70 GiB. These overlapping
measurements must not be summed. Model parameter storage alone was 20.83 GiB;
this BF16/FP32 configuration exceeds the 16 GB target before the game participates.
No CPU-fallback warning appeared in the retained log, although fallback was enabled.

All four WAVs are finite, mono 24 kHz PCM16, hash-checked and unclipped. Delivery
requests changed the timing and waveform, but that does not establish whether the
requested emotions were convincingly acted.

The auxiliary cached `mlx-community/whisper-base-mlx` check used specified languages,
temperature zero and no requested-text prompt. It transcribed German urgent as
"Pleibe mich, ist Gibno Hofung Feldestorf." and German calm as
"Es kabe mich. Es kabe nahe hofffond, fond, jays, doth." Whisper can make errors;
these are diagnostic transcriptions, not an authoritative audible transcript.

Detailed evidence:

- [Generation report](output/results.json) and [raw benchmark log](benchmark.log).
- [Auxiliary transcript report](output/transcript-check.json) and
  [transcript-check log](transcript-check.log).
- [Checkpoint hash inventory and retained cache location](model-verification.json).
- [Finite experiment closure](closure.json).

## Experiment

- Apple M3 Max, 16 CPU cores, 128 GB unified memory.
- Official `Qwen/Qwen2.5-Omni-7B`, pinned to
  `ae9e1690543ffd5c0221dc27f79834d0294cba00`.
- PyTorch Metal (`mps`), BF16 Thinker/Talker, FP32 Token2Wav, SDPA attention.
- Four short text-input/audio-output cases: calm and urgent English with Chelsie,
  then calm and urgent German with Ethan. Each language pair uses identical words
  and the same seed, changing only the requested delivery.
- The official required system prompt is preserved. Acting directions are in the
  user message. They are requests to evaluate, not verified model capabilities.
- Bounded generation: 64 text tokens and 512 speech tokens. No microphone, video,
  visual input, game process or separate local dialogue LLM participates.
- Timings include completed Metal execution for the Thinker, Talker and Token2Wav.
  The first clip is cold; the following clips are warm. This buffered API does not
  measure streaming time to first audible sample.
- Process RSS and Metal driver/allocated memory are reported separately. They overlap
  on Apple unified memory and must not be added. Sampled peaks are approximate;
  process lifetime maximum RSS is also retained.
- WAV output is unnormalised PCM16 at 24 kHz. Raw peak/clipping counts and output
  hashes are retained. Generated response text is compared with the requested text;
  that comparison does not establish the audible transcript.

## Reproduce

Use Python 3.12 and an isolated environment with [requirements.txt](requirements.txt).
Download the pinned official snapshot once (about 22 GB) to a separate model directory:

```python
from huggingface_hub import snapshot_download
snapshot_download(
    "Qwen/Qwen2.5-Omni-7B",
    revision="ae9e1690543ffd5c0221dc27f79834d0294cba00",
    local_dir="/absolute/path/to/Qwen2.5-Omni-7B",
)
```

Run from this package's directory with host Metal access:

```sh
HF_HUB_OFFLINE=1 TRANSFORMERS_OFFLINE=1 PYTORCH_ENABLE_MPS_FALLBACK=1 \
  /path/to/venv/bin/python -u benchmark.py \
  --model-dir /absolute/path/to/Qwen2.5-Omni-7B \
  --output-dir /absolute/path/to/new-audition-output
```

All generation uses local files; downloading the weights is separate from inference.
CPU fallback is permitted for unsupported Metal operators, and any warnings must
be retained with the results. A later M1 test needs the same checkpoint/cases and an
explicit precision/deployment decision; the full checkpoint alone exceeds 16 GB.

The verified M3 checkpoint is retained in this checkout's ignored directory
`artifacts/speech-models/Qwen2.5-Omni-7B-ae9e1690543f`. The temporary audition
environment is `/private/tmp/allrpcs-qwen25-omni-venv`; rebuild it from the pinned
requirements if the operating system clears temporary files. Model weights are
excluded from Git.

The optional [transcript checker](check_transcripts.py) additionally used
`mlx==0.30.1` and `mlx-audio==0.2.8`. Run it after generation in a separate process
with a cached multilingual Whisper model:

```sh
HF_HUB_OFFLINE=1 TRANSFORMERS_OFFLINE=1 \
  /path/to/venv/bin/python -u check_transcripts.py \
  --model-dir /absolute/path/to/cached-whisper-base-mlx \
  --output-dir /absolute/path/to/audition-output
```

## Sources and limits

- [Official repository and preset voices](https://github.com/QwenLM/Qwen2.5-Omni).
- [Pinned checkpoint](https://huggingface.co/Qwen/Qwen2.5-Omni-7B/tree/ae9e1690543ffd5c0221dc27f79834d0294cba00).
- [Official technical report](https://arxiv.org/abs/2503.20215).

Omni generates response text and conditions its Talker on the Thinker's representations.
This experiment is not standalone synthesis of an independently committed game reply.
Speech recognition, cancellation, feedback-loop handling, game coexistence, M1 latency,
16 GB suitability and Windows/Linux deployment are outside this initial audition.
Listening quality and audible adherence remain owner review decisions.

For the later 16 GB M1 test, investigate the
[officially linked four-bit MNN conversion](https://huggingface.co/taobao-mnn/Qwen2.5-Omni-7B-MNN).
Qwen's [MNN deployment table](https://github.com/QwenLM/Qwen2.5-Omni#deployment-with-mnn)
reports a 5.8 GB memory peak for 7B on two Snapdragon platforms, including Talker
and Code2Wav speed measurements. That makes it a candidate for the memory constraint;
it does not establish M1 compatibility, latency or equivalent audio quality. This
M3 audition uses the original checkpoint as a quality reference.

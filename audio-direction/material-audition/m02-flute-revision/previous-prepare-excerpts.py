#!/usr/bin/env python3
"""Make reproducible external audition excerpts from approved originals; partial work is explicitly labelled."""
from pathlib import Path
import argparse
import hashlib
import json
import subprocess
import numpy as np
import soundfile as sf
from scipy.signal import butter, sosfilt, find_peaks

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
RATE = 48000


def digest(path):
    """Bind editable inputs and actual export bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def decode(path):
    """Decode source material to editing PCM; this is not a recovery of lost lossy-source information."""
    data = subprocess.check_output(['ffmpeg', '-v', 'error', '-nostdin', '-i', str(path),
                                    '-f', 'f32le', '-ac', '2', '-ar', str(RATE), 'pipe:1'])
    return np.frombuffer(data, dtype='<f4').reshape(-1, 2).copy()


def gain(data, peak_db=None, rms_db=None):
    """Apply explicit source gain while retaining dynamics rather than normalizing each sample."""
    level = np.max(np.abs(data)) if peak_db is not None else np.sqrt(np.mean(data.astype(float) ** 2))
    target = 10 ** ((peak_db if peak_db is not None else rms_db) / 20)
    factor = target / max(float(level), 1e-8)
    return data * factor, float(20 * np.log10(factor))


def fade(data, seconds=.025):
    """Fade trim edges without adding generated sound or changing the source identity."""
    result = data.copy()
    frames = min(round(seconds * RATE), len(result) // 2)
    if frames:
        ramp = np.linspace(0, 1, frames)[:, None]
        result[:frames] *= ramp
        result[-frames:] *= ramp[::-1]
    return result


def place(length, parts):
    """Sequence source-derived parts in a finite labelled audition interval."""
    result = np.zeros((round(length * RATE), 2), dtype=np.float32)
    for start, data in parts:
        offset = round(start * RATE)
        count = min(len(data), len(result) - offset)
        assert offset >= 0 and count > 0
        result[offset:offset + count] += data[:count]
    return result


def main():
    """Prepare exactly the planned classes available from retained originals and write honest edit records."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--allow-partial', action='store_true', help='Prepare internal work without claiming a complete series')
    arguments = parser.parse_args()
    plan = json.loads((REPO / 'audio-direction/material-audition-source-plan.json').read_text())
    provenance = json.loads((ROOT / 'provenance.json').read_text())
    originals = provenance['sources']
    missing = sorted({s for c in plan['candidates'] for s in c['source_ids']} - set(originals))
    assert not missing or arguments.allow_partial, 'Originals still missing: ' + ', '.join(missing)
    (ROOT / 'clips').mkdir(exist_ok=True)
    candidates = {'schema_version': 1, 'status': 'partial_internal_preparation' if missing else 'complete_technical_candidates',
                  'missing_source_ids': missing, 'owner_selected': False, 'acoustic_listening': 'not_assessed',
                  'script_sha256': digest(Path(__file__)), 'editing_rate_hz': RATE,
                  'editing_precision': 'float32 decoded PCM; PCM24 review exports', 'candidates': []}
    cache = {}
    edits = []

    def load(identity, member=None):
        """Resolve only exact recorded originals or safe extracted archive members."""
        source = originals[identity]
        if member:
            matched = [m for m in source['members'] if m['extracted'] and m['member_name'] == member]
            assert len(matched) == 1, 'Missing exact archive member: ' + member
            source = matched[0]
        path = REPO / source['path']
        assert digest(path) == source['sha256'], 'Input changed: ' + identity
        key = str(path)
        if key not in cache:
            cache[key] = decode(path)
        edits.append({'source_id': identity, 'member_name': member, 'path': source['path'],
                      'sha256': source['sha256'], 'decoded_rate_hz': RATE,
                      'decoded_frames': len(cache[key]), 'lossy_original': path.suffix.lower() in {'.mp3', '.ogg'}})
        return cache[key]

    def take(identity, seconds, start=0, member=None, peak=-20):
        """Select an exact window with edge fades and an explicit peak gain."""
        data = load(identity, member)
        begin = round(start * RATE)
        end = min(len(data), begin + round(seconds * RATE))
        assert end > begin
        clip, db = gain(fade(data[begin:end]), peak_db=peak)
        edits[-1].update(trim_begin_sample=begin, trim_end_sample=end, edge_fade_seconds=.025, applied_gain_db=db)
        return clip

    def bed(identity, start, rms, lowpass=None, member=None):
        """Build a source-derived thirty-second repeat with a documented circular crossfade."""
        data = load(identity, member)
        count = RATE * 30
        overlap = round(.75 * RATE)
        begin = round(start * RATE)
        assert len(data) >= begin + count + overlap, 'Bed source window too short'
        window = data[begin:begin + count + overlap].copy()
        if lowpass:
            window = sosfilt(butter(2, lowpass, fs=RATE, output='sos'), window, axis=0).astype(np.float32)
        output = window[:count].copy()
        ramp = np.linspace(0, 1, overlap)[:, None]
        output[:overlap] = window[count:count + overlap] * (1 - ramp) + window[:overlap] * ramp
        output, db = gain(output, rms_db=rms)
        edits[-1].update(trim_begin_sample=begin, trim_end_sample=begin + count + overlap,
                        loop_begin_sample=0, loop_end_sample=count, circular_crossfade_samples=overlap,
                        lowpass_hz=lowpass, applied_gain_db=db)
        return output

    def walking(identity):
        """Choose separated energy onsets for technical contact drafts; foot/material identity needs listening."""
        data = load(identity)
        hop = 480
        block = data[:len(data) // hop * hop].reshape(-1, hop, 2)
        envelope = np.sqrt(np.mean(block.astype(float) ** 2, axis=(1, 2)))
        peaks, _ = find_peaks(envelope, distance=55, prominence=max(float(envelope.max()) * .04, .00001))
        chosen = sorted(peaks, key=lambda p: envelope[p], reverse=True)[:6]
        chosen.sort()
        assert len(chosen) >= 3, 'Fewer than three distinct detected contact candidates'
        windows = []
        parts = []
        for index, onset in enumerate(chosen):
            onset = int(onset)
            begin = max(0, onset * hop - round(.10 * RATE))
            end = min(len(data), onset * hop + round(.40 * RATE))
            parts.append((1 + index * 1.3, fade(data[begin:end])))
            windows.append({'begin_sample': begin, 'end_sample': end, 'excerpt_start_seconds': 1 + index * 1.3})
        output, db = gain(place(10, parts), peak_db=-16)
        edits[-1].update(contact_windows=windows, edge_fade_seconds=.025, applied_gain_db=db,
                        contact_detection='10 ms RMS peaks; minimum 550 ms separation; no anatomical identity inferred')
        return output

    for candidate in plan['candidates']:
        identity = candidate['id']
        if not set(candidate['source_ids']) <= set(originals):
            continue
        edits = []
        components = []
        if identity == 'B01':
            audio = bed('rain', 60, -30, 5500)
        elif identity == 'B02':
            audio = bed('stream', 0, -27, member='stream-waterfall/stream1.ogg') + bed('rain', 100, -33, 7500)
        elif identity == 'B03':
            audio = bed('leaf-rain', 5, -30, 8000)
        elif identity == 'B04':
            audio = bed('wind', 0, -32, 5000) + bed('rain', 160, -32, 7500)
            audio += place(30, [(17, take('creak', 1.595, peak=-25))])
        elif identity.startswith('W'):
            audio = walking(candidate['source_ids'][0])
        elif identity == 'A01':
            audio = place(8, [(1, take('water-contact', 1.5, member='splash_01.ogg', peak=-19)),
                              (4, take('water-contact', 1.5, member='splash_03.ogg', peak=-20))])
        elif identity == 'A02':
            audio = place(8, [(1, take('wood-tool', 1, member='wood_hit_03.ogg', peak=-22)),
                              (2, take('cloth', 2.5, start=5, peak=-22))])
        elif identity == 'A03':
            audio = place(8, [(1, take('wood-tool', 2, member='tools_02.ogg', peak=-20)),
                              (4, take('wood-tool', 1, member='wood_hammer_01.ogg', peak=-22))])
        elif identity == 'A04':
            audio = place(8, [(1, take('mortar', 5, start=3, peak=-19))])
        elif identity == 'A05':
            audio = place(8, [(1, take('cloth', 1.2, start=15, peak=-25)),
                              (2, take('wood-tool', .6, member='wood_hit_05.ogg', peak=-24))])
        elif identity == 'A06':
            effects = place(8, [(1, take('interface', .5, member='Audio/click_001.ogg', peak=-24))])
            dialogue = place(8, [(4, take('interface', .5, member='Audio/click_003.ogg', peak=-24)),
                                 (6, take('interface', .5, member='Audio/click_005.ogg', peak=-25))])
            audio = effects + dialogue
            for bus, part in [('Effects', effects), ('Dialogue', dialogue)]:
                path = ROOT / 'clips' / (identity + '-' + bus + '.wav')
                sf.write(path, part, RATE, subtype='PCM_24')
                components.append({'bus': bus, 'path': str(path.relative_to(REPO)), 'sha256': digest(path)})
        elif identity == 'M01':
            audio, db = gain(fade(load('exploration')[RATE * 30:RATE * 55], 2), rms_db=-26)
            edits[-1].update(trim_begin_sample=RATE * 30, trim_end_sample=RATE * 55,
                            edge_fade_seconds=2, applied_gain_db=db)
        elif identity == 'M02':
            audio, db = gain(fade(load('rest')[RATE * 20:RATE * 45], 2), rms_db=-26)
            edits[-1].update(trim_begin_sample=RATE * 20, trim_end_sample=RATE * 45,
                            edge_fade_seconds=2, applied_gain_db=db)
        else:
            raise ValueError('Unplanned candidate: ' + identity)
        assert len(audio) == candidate['excerpt_seconds_proposed'] * RATE
        # A sparse source can have a high crest factor even at a quiet RMS target.
        # Attenuate the whole excerpt, preserving its dynamics and documenting the target shortfall.
        headroom_gain_db = 0.0
        if np.max(np.abs(audio)) > 10 ** (-7 / 20):
            audio, headroom_gain_db = gain(audio, peak_db=-7)
        assert np.isfinite(audio).all() and np.max(np.abs(audio)) < .5, 'Unexpected clipping/headroom'
        path = ROOT / 'clips' / (identity + '.wav')
        sf.write(path, audio, RATE, subtype='PCM_24')
        peak = float(20 * np.log10(max(float(np.max(np.abs(audio))), 1e-12)))
        rms = float(20 * np.log10(max(float(np.sqrt(np.mean(audio.astype(float) ** 2))), 1e-12)))
        candidates['candidates'].append({**candidate, 'path': str(path.relative_to(REPO)), 'sha256': digest(path),
                                        'actual_seconds': len(audio) / RATE, 'sample_peak_dbfs': peak, 'rms_dbfs': rms,
                                        'final_uniform_headroom_gain_db': headroom_gain_db,
                                        'source_edits': edits, 'routed_components': components,
                                        'qualification_status': 'technical_draft_pending_human_listening',
                                        'acoustic_listening': 'not_assessed'})
        print('Prepared', identity, candidate['title'], 'peak', round(peak, 2), flush=True)
        # Bound editing memory and retain original bytes rather than full decoded copies on disk.
        cache.clear()
    temporary = ROOT / 'candidates.json.tmp'
    temporary.write_text(json.dumps(candidates, indent=2) + '\n')
    temporary.replace(ROOT / 'candidates.json')
    print('Prepared', len(candidates['candidates']), 'of 16 excerpts; missing source IDs:', missing)


if __name__ == '__main__':
    main()

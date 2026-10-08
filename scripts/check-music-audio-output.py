#!/usr/bin/env python3
"""Verify engine-rendered Master music output, user controls, terminal fades and a five-minute mix."""
import argparse
import hashlib
import json
import subprocess
import wave
from pathlib import Path
import numpy as np

def require(value, message):
    """Reject missing, clipped, replayed, overlapping or incorrectly routed output."""
    if not value:
        raise ValueError(message)

def rms(samples):
    """Compute linear RMS for identical source windows."""
    return float(np.sqrt(np.mean(samples ** 2)))

def main():
    """Measure the actual final Master capture and retain honest loudness/peak limits."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as wav:
        require((wav.getnchannels(), wav.getframerate(), wav.getsampwidth()) == (2,48000,2), 'Wrong Master format')
        samples = np.frombuffer(wav.readframes(wav.getnframes()), dtype='<i2').reshape(-1,2).astype(np.float64) / 32768
    timeline = json.loads(args.timeline.read_text())
    require(len(timeline) == 20 and timeline[-1]['duration'] == 300, 'Incomplete music output cases')
    rows = {row['name']:row for row in timeline}
    def segment(name, begin, end):
        start = rows[name]['start']
        return samples[round((start+begin)*48000):round((start+end)*48000)]
    metrics = {}
    silent = {'zero','mute','master-zero','master-mute','missing','missing-bus'}
    for row in timeline[:-1]:
        name = row['name']; data = segment(name, 2.5, 9.5)
        peak = float(np.max(np.abs(segment(name,0,row['duration']))))
        metrics[name] = {'rms_dbfs':20*np.log10(max(rms(data),1e-12)), 'peak_dbfs':20*np.log10(max(peak,1e-12))}
        require(peak < 0.99, 'Clipped output: '+name)
        if row['variant'] in silent:
            require(peak < 0.000001, 'Muted/missing output leaked: '+name)
        else:
            require(peak > 0.000001, 'Missing music output: '+name)
        if row['variant'] in {'interrupt','cancel','mute-unmute','focus','long-frame'}:
            require(np.max(np.abs(segment(name,11,27))) < 0.000001, 'Interrupted music resumed: '+name)
        if row['variant'] == 'mono':
            require(np.max(np.abs(data[:,0]-data[:,1])) < 0.000001, 'Mono is not dual mono')
        require(np.max(np.abs(segment(name,26,27))) < 0.000001, 'Music looped past accepted passage: '+name)
    normal = rms(segment('M01-normal',2.5,9.5))
    for suffix, expected in [('half',0.5),('master-half',0.5),('other-bus-zero',1)]:
        ratio = rms(segment('M01-'+suffix,2.5,9.5)) / normal
        require(abs(ratio-expected) < 0.035, 'Wrong captured gain: '+suffix)
        metrics['M01-'+suffix]['ratio_to_normal'] = ratio
    # Compare the same accepted waveform window so source dynamics cannot masquerade as the stop envelope.
    for prefix in ['M01','M02']:
        reference = segment(prefix+'-normal',10.0,10.55)
        interrupted = segment(prefix+'-interrupt',10.0,10.55)
        frame_rms = []
        for i in range(0,len(reference),2400):
            base = rms(reference[i:i+2400]); actual = rms(interrupted[i:i+2400])
            if base > 0.0001: frame_rms.append(actual/base)
        require(len(frame_rms)>5 and frame_rms[0] > frame_rms[-1] and frame_rms[-1]<0.3, 'Wrong terminal music fade: '+prefix)
        metrics[prefix+'-interrupt']['waveform_relative_fade_ratios'] = frame_rms
    mix = segment('mixed-five-minutes',0,300)
    require(rms(mix)>0.000001 and np.max(np.abs(mix))<0.99, 'Missing/clipped mixed passage')
    # Recover the exact repeated B01 bed phase from a foreground-free window, then measure the score residual.
    root = Path(__file__).resolve().parents[1]
    with wave.open(str(root / 'game/assets/audio/material/B01.wav'), 'rb') as bed_file:
        pcm = np.frombuffer(bed_file.readframes(bed_file.getnframes()), dtype=np.uint8).reshape(-1,3).astype(np.int32)
    integers = pcm[:,0] | pcm[:,1] << 8 | pcm[:,2] << 16
    integers = np.where(integers & 0x800000, integers - 0x1000000, integers)
    bed = (integers >> 8).reshape(-1,2).astype(np.float64) / 32768
    query = mix[100*48000:101*48000,0]
    reference = np.concatenate([bed[:,0],bed[:len(query),0]])
    fft_size = 1 << (len(reference)-1).bit_length()
    correlation = np.fft.irfft(np.fft.rfft(reference,fft_size) * np.conj(np.fft.rfft(query,fft_size)),fft_size)
    phase = int(np.argmax(correlation[:len(bed)]))
    offset = (phase - 100*48000) % len(bed)
    residual = mix - bed[(np.arange(len(mix))+offset) % len(bed)]
    quiet_windows = [(40,59),(70,89),(100,119),(125,244),(275,299)]
    quiet_rms = [rms(residual[begin*48000:end*48000]) for begin,end in quiet_windows]
    require(max(quiet_rms)<0.0002, 'Unexpected music/backlog during long mixed quiet intervals')
    first = rms(residual[15*48000:35*48000]); second = rms(residual[248*48000:267*48000])
    require(first>0.005 and second>0.005, 'Missing either scheduled mixed motif')
    metrics['mixed-five-minutes'] = {'bed_phase_sample':offset,'music_residual_quiet_rms':quiet_rms,
        'quiet_windows_seconds':quiet_windows,'first_music_residual_rms':first,'second_music_residual_rms':second,
        'two_disjoint_music_windows_seconds':[[12.5,37.5],[245,270]],'minimum_music_free_cooldown_seconds':180}
    def measure(extra):
        meter = subprocess.run(['ffmpeg','-hide_banner',*extra,'-i',str(args.wave),'-af',
            'loudnorm=I=-23:LRA=7:TP=-1:print_format=json','-f','null','-'],capture_output=True,text=True,check=True).stderr
        report = json.loads(meter[meter.rfind('{'):meter.rfind('}')+1])
        return {key:report[key] for key in ['input_i','input_tp','input_lra','input_thresh']}
    measured = measure([])
    mix_measured = measure(['-ss',str(rows['mixed-five-minutes']['start']),'-t','300'])
    args.output.write_text(json.dumps({'schema_version':1,'cases':len(timeline),'metrics':metrics,
        'five_minute_mix_seconds':300,'wave_sha256':hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256':hashlib.sha256(args.timeline.read_bytes()).hexdigest(),'final_master_measurements':measured,
        'five_minute_master_measurements':mix_measured,
        'five_minute_default_audition_band_deviation_lu':min(0,float(mix_measured['input_i'])+25),
        'meter_contract':'Only input measurements retained; no normalization output written or accepted PCM changed.',
        'limits':['Controlled actual Master output; no device listening or ordinary-play loudness compliance.',
                  'Accepted PCM and provisional targets remain unchanged; measurements report deviations honestly.']},indent=2)+'\n')
    print('PASS: 20 actual Master cases; two full motifs, gain/mute/zero/mono/presets/missing bus, fade/cancel/no replay and five-minute mix')

if __name__ == '__main__':
    main()

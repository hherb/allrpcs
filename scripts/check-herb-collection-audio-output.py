#!/usr/bin/env python3
"""Measure every native Master herb case; never infer audio success from player state alone."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import wave
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
REVIEW=ROOT/'audio-direction/herb-collection-runtime-review/output'
CASES=['default',
        'quiet',
        'mono',
        'effects-half',
        'master-half',
        'effects-zero',
        'master-zero',
        'effects-muted',
        'master-muted',
        'ambience-zero',
        'captions-off',
        'missing-stream',
        'missing-bus',
        'pause',
        'focus-loss',
        'reading-overlay',
        'text-entry',
        'journal-overlay',
        'map-overlay',
        'dialogue-overlay',
        'settings-overlay',
        'customization-overlay',
        'teleport',
        'invalid-delta',
        'source-hidden',
        'source-moved',
        'source-disabled',
        'source-replaced',
        'patch-drift',
        'actor-leaves',
        'listener-range',
        'listener-drift',
        'navigation-disabled',
        'spatial-left',
        'spatial-right',
        'spatial-edge',
        'actor-interior',
        'listener-interior',
        'mute-unmute',
        'missing-stream-repair',
        'load-success',
        'load-failure',
        'foreground',
        'retained-overlap',
        'recipe-overlap']
REJECTED={'source-hidden','source-moved','source-disabled','source-replaced','patch-drift','navigation-disabled','actor-interior','listener-interior'}
SILENT=REJECTED|{'effects-zero','master-zero','effects-muted','master-muted','missing-stream','missing-bus','missing-stream-repair'}
CANCELLED={'pause','focus-loss','reading-overlay','text-entry','journal-overlay','map-overlay','dialogue-overlay','settings-overlay',
           'customization-overlay','teleport','invalid-delta','mute-unmute','load-success','actor-leaves','listener-range','listener-drift'}


def require(value,message):
    """Reject missing or failed native evidence under optimized Python too."""
    if not value:
        raise ValueError(message)


def measure(path):
    """Check gain ratios, silence, mono and cancelled tails directly from rendered samples."""
    with wave.open(str(path)) as reader:
        require((reader.getnchannels(),reader.getframerate(),reader.getsampwidth())==(2,48000,2),'Unexpected Master format')
        samples=np.frombuffer(reader.readframes(reader.getnframes()),dtype='<i2').reshape(-1,2).astype(np.float64)/32768
    rows=json.loads((REVIEW/'timeline.json').read_text());require([r['name'] for r in rows]==CASES,'Missing/reordered cases')
    metrics={}
    for row in rows:
        name=row['name'];start=row['startFrame']*800;data=samples[start:start+240000]
        require(len(data)==240000,'Truncated native case '+name)
        left_rms=float(np.sqrt(np.mean(data[:,0]**2)));right_rms=float(np.sqrt(np.mean(data[:,1]**2)))
        peak=float(np.max(np.abs(data)));rms=float(np.sqrt(np.mean(data**2)))
        require(peak<.99,'Clipping '+name)
        require(peak<1e-6 if name in SILENT else peak>1e-6,'Wrong native output '+name)
        require(row['presentations']==(0 if name in REJECTED else 1),'Wrong caption/history admission '+name)
        require(row['history']==(0 if name in REJECTED or name=='load-success' else 1),'Wrong isolated history '+name)
        active=np.flatnonzero(np.max(np.abs(data),axis=1)>1e-6)
        onset=float(active[0]/48000) if len(active) else None
        last=float(active[-1]/48000) if len(active) else None
        if name not in SILENT:
            require(20/60-.05<=onset<=20/60+.1,'Early or late onset '+name)
            if name not in CANCELLED and name not in {'retained-overlap','recipe-overlap'}:
                require(1.29<=last<=1.42,'Output exceeds one-second passage '+name)
        if name in CANCELLED:
            require(float(np.max(np.abs(data[round((50/60+.15)*48000):])))<1e-6,'Cancellation/replay tail '+name)
        if name=='mono':
            require(float(np.max(np.abs(data[:,0]-data[:,1])))<1e-6,'Mono mismatch')
        require(float(np.max(np.abs(data[-4800:])))<1e-6,'Unstopped tail '+name)
        meter=subprocess.run(['ffmpeg','-hide_banner','-f','s16le','-ar','48000','-ac','2','-i','pipe:0','-af','loudnorm=I=-23:LRA=7:TP=-1:print_format=json','-f','null','-'],input=(data*32768).astype('<i2').tobytes(),capture_output=True,check=True).stderr.decode()
        loudness=json.loads(meter[meter.rfind('{'):meter.rfind('}')+1])
        metrics[name]={'peak':peak,'peak_dbfs':float(20*np.log10(peak)) if peak else None,'rms':rms,'loudness':loudness,'presentations':row['presentations'],'left_rms':left_rms,'right_rms':right_rms,'onset_seconds':onset,'last_active_seconds':last}
    baseline=metrics['default']['rms']
    for name,expected in [('effects-half',.5),('master-half',.5),('ambience-zero',1),('captions-off',1),('load-failure',1),('foreground',1)]:
        ratio=metrics[name]['rms']/baseline;require(abs(ratio-expected)<=.05*expected,'Gain ratio '+name+': '+str(ratio));metrics[name]['ratio']=ratio
    # Preserve the accepted source's unequal stereo balance; compare spatial shifts to that baseline.
    balance = metrics['default']['left_rms'] / metrics['default']['right_rms']
    left_balance = metrics['spatial-left']['left_rms'] / metrics['spatial-left']['right_rms'] / balance
    right_balance = metrics['spatial-right']['left_rms'] / metrics['spatial-right']['right_rms'] / balance
    require(left_balance > 1.01, 'Left pan failed to shift the accepted stereo balance')
    require(right_balance < .99, 'Right pan failed to shift the accepted stereo balance')
    metrics['spatial-left']['baseline_normalized_balance'] = left_balance
    metrics['spatial-right']['baseline_normalized_balance'] = right_balance
    require(metrics['spatial-edge']['rms'] < baseline*.05, 'Exclusive edge attenuation missing')
    # This accepted low-level cue stays below the existing -30 dB compressor threshold.
    quiet_ratio = metrics['quiet']['rms'] / baseline
    require(abs(quiet_ratio - 1) <= .02, 'Quiet changed the below-threshold rustle unexpectedly')
    metrics['quiet']['ratio'] = quiet_ratio
    record={'schema_version':1,'status':'passed','cases':metrics,'capture_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
            'timeline_sha256':hashlib.sha256((REVIEW/'timeline.json').read_bytes()).hexdigest(),
            'limits':'Actual controlled native Master output; no fresh-device/ordinary-play loudness or executable release claim.'}
    (REVIEW/'measurements.json').write_text(json.dumps(record,indent=2)+'\n')
    print('PASS: herb native Master measurements:',len(metrics),'independent cases')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('wave_path',nargs='?');args=parser.parse_args()
    if args.wave_path:
        measure(Path(args.wave_path))
    else:
        with tempfile.TemporaryDirectory() as directory:
            p=Path(directory)/'master.wav';p.write_bytes(gzip.decompress((REVIEW/'master.wav.gz').read_bytes()));measure(p)

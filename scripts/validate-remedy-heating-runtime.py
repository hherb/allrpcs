#!/usr/bin/env python3
"""Verify the granted heating successor, frozen owners, real parity and native/package evidence."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
from zipfile import ZipFile

ROOT=Path(__file__).resolve().parents[1]
REVIEW=ROOT/'audio-direction/remedy-heating-runtime-review'
MODES=['default','quiet','master-muted','effects-muted','missing-stream','captions-off','heating-disabled']


def require(value,message):
    """Fail incomplete or unauthorized evidence even with optimized Python."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Hash exact bytes, not mutable names or timestamps."""
    return hashlib.sha256(data).hexdigest()


def load(path):
    """Read one explicit repository contract or evidence record."""
    return json.loads((ROOT/path).read_text())


def bound(row):
    """Require current exact bound bytes."""
    require(sha((ROOT/row['path']).read_bytes())==row['sha256'],'Binding drift: '+row['path'])


def contracts():
    """Check the exact new event/one-shot/history contract independently of reviewed hashes."""
    co=(ROOT/'game/OpeningRegionRemedyPreparationInputCoordinator.cs').read_text()
    projection=(ROOT/'src/Application/Audio/RemedyHeatingPresentationProjection.cs').read_text()
    controller=(ROOT/'game/OpeningRegionRemedyHeatingAudioController.cs').read_text()
    history=(ROOT/'src/Application/Audio/RemedyHeatingCueHistory.cs').read_text()
    require(all(x in co for x in ['bool advanced = preparation.TryAdvance(step);','HeatingCompleted?.GetInvocationList()',
            'ReferenceEquals(heatingCommit, commit)','ReferenceEquals(preparation.Activity, commit.After)','GrindingCompleted?.GetInvocationList()',
            'RemedyHeatingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), seekerPosition','catch (Exception exception)']), 'Commit admission/isolation drift')
    require(all(x in projection for x in ['before.Progress.NextStepIndex == 1','after.Progress.NextStepIndex == 2',
            'ReferenceEquals(recipe.Plan.Steps[1], step)','InteractionPixels = 48','PassageSeconds = 3','TeleportPixels = 96','Phrase = "Heating preparation"']), 'Exact step/duration/range drift')
    require(all(x in controller for x in ['Bus = "Effects"','ReferenceEquals(consumed, commit)','consumed = commit;',
            'coordinator.OwnsHeating(commit)','PresentationReset += ResetAfterLoad','history.Clear()',
            'AudioServer.GetBusIndex("Effects") >= 0','VolumeLinear = 1','MaxPolyphony = 1','PitchScale = 1',
            'candidate.Format == AudioStreamWav.FormatEnum.Format16Bits','player.Stop()','if (!Automatic','scene.GlobalPosition == new Vector2(2300, 1300)']), 'Playback/reset/ownership drift')
    require('Ducking.Begin' not in controller and 'AudioStreamPlayer2D' not in controller and 'Capacity = 16' in history,'Unapproved lease/spatial/history change')
    captions=(ROOT/'game/MaterialSoundCaptionPanel.cs').read_text()
    require('Name = "RemedyHeating"' in captions and 'SetRemedyHeating' in captions and 'ClearRemedyHeating' in captions,'Independent caption missing')
    require('Remedy heating cues' in (ROOT/'game/PositionalSoundCueHistoryPanel.cs').read_text(),'Independent history section missing')


def narrow_edits(revision):
    """Reverse only named additions; retained coordinator, controls and composition must match exactly."""
    paths=['game/Main.tscn','game/MaterialAudioCreditsPanel.cs','game/MaterialSoundCaptionPanel.cs',
           'game/OpeningRegionRemedyPreparationInputCoordinator.cs','game/PositionalSoundCueHistoryPanel.cs','game/export_presets.cfg']
    for path in paths:
        text=(ROOT/path).read_text()
        if path.endswith('Main.tscn'):
            text=text.replace('load_steps=433','load_steps=431')
            text=re.sub(r'\[ext_resource[^\n]+id="heating_audio"\]\n\[ext_resource[^\n]+id="heating_check"\]\n\n','',text)
            text=re.sub(r'\[node name="OpeningRegionRemedyHeatingAudioController"[^\n]+\nscript = ExtResource\("heating_audio"\)\n\n\[node name="RemedyHeatingAudioRuntimeSmokeCheck"[^\n]+\nscript = ExtResource\("heating_check"\)\n\n','',text)
        elif path.endswith('MaterialAudioCreditsPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if 'assets/audio/remedy-heating/CREDITS.txt' not in line)
        elif path.endswith('export_presets.cfg'):
            text=text.replace(',assets/audio/remedy-heating/*.txt,assets/audio/remedy-heating/*.json,assets/audio/remedy-heating/*.html','')
        elif path.endswith('MaterialSoundCaptionPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if not any(x in line for x in ['private Label heatingLabel','private string heating','heatingLabel = new()', 'lines.AddChild(heatingLabel);','heatingLabel.Text =']))
            text=re.sub(r'    /// <summary>Shows only the independent anonymous heating operation.</summary>\n.*?internal void ClearRemedyHeating[^\n]+\n\n','',text,flags=re.S)
            text=text.replace('panel.OffsetTop = -464','panel.OffsetTop = -430').replace(', heatingLabel }', ' }').replace(' || heating.Length > 0','')
        elif path.endswith('PositionalSoundCueHistoryPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if not any(x in line for x in ['private OpeningRegionRemedyHeatingAudioController','heating = GetNodeOrNull','IReadOnlyList<string> heat =','string warming =']))
            text=text.replace('foliage, wind, rain and heating cues','foliage, wind and rain cues').replace(' + "\\n\\nRemedy heating cues\\n" + warming','')
        else:
            text=re.sub(r'    /// <summary>Independent presentation notification.*?    internal const string ReadyStatusText', '    internal const string ReadyStatusText',text,flags=re.S)
            text=re.sub(r'        var restoration = GetNode<SaveSnapshotRestorationController>.*?        TreeExiting[^\n]+\n','',text,flags=re.S)
            text=re.sub(r'            if \(advanced && RemedyHeatingPresentationProjection.IsCommit.*?            return advanced;', '            return advanced;',text,flags=re.S)
        before=subprocess.check_output(['git','show',revision+':'+path],cwd=ROOT,text=True)
        require(text==before,'Unapproved retained behavior/composition edit: '+path)


def parity(rows,saves):
    """Fail complete mode/save/motion/truth/event differences independently of audio preferences."""
    require(len(rows)==7 and len(saves)==7,'Missing mode')
    require(all(x==saves[0] for x in saves),'Complete save parity failed')
    for row in rows:
        require(row['motion']==rows[0]['motion'] and row['transcript']==rows[0]['transcript'] and row['truthBefore']==row['truthAfter']==rows[0]['truthBefore'],'Motion/transcript/protected truth drift')
        require(row['commits']==1 and row['grinding']==1 and row['treatment']==1,'Recipe/accepted action count drift')
        require(row['presentations']==(0 if row['mode']=='heating-disabled' else 1),'Presentation count drift')


def rejection_probes():
    """Exercise actual validator rejection paths using in-memory mutations, preserving every reviewed file."""
    from unittest.mock import patch
    import copy
    passed=[]
    read=Path.read_text;read_bytes=Path.read_bytes
    def rejected(name,action):
        try:
            action()
        except (ValueError,FileNotFoundError):
            passed.append(name);return
        raise ValueError('Mutation escaped: '+name)
    for name,path,old,new in [
        ('commit identity','game/OpeningRegionRemedyPreparationInputCoordinator.cs','ReferenceEquals(heatingCommit, commit)','true'),
        ('bus','game/OpeningRegionRemedyHeatingAudioController.cs','Bus = "Effects"','Bus = "Master"'),
        ('duration','src/Application/Audio/RemedyHeatingPresentationProjection.cs','PassageSeconds = 3','PassageSeconds = 4'),
        ('phrase','src/Application/Audio/RemedyHeatingPresentationProjection.cs','Heating preparation','Boiling ready'),
        ('lease','game/OpeningRegionRemedyHeatingAudioController.cs','private const string Resource','// Ducking.Begin\n    private const string Resource'),
        ('retained controls','game/MaterialSoundCaptionPanel.cs','panel.OffsetLeft = 16','panel.OffsetLeft = 17')]:
        with patch.object(Path,'read_text',lambda obj,*a,**kw: read(obj,*a,**kw).replace(old,new) if obj==ROOT/path else read(obj,*a,**kw)):
            rejected(name,lambda: narrow_edits(load('audio-direction/remedy-heating-runtime-authority.json')['baseline_commit']) if name=='retained controls' else contracts())
    grant=load('audio-direction/remedy-heating-runtime-authority.json')
    for name,path in [('proposal','audio-direction/remedy-heating-owner-scope.json'),('source','audio-direction/remedy-heating-source/originals/cooking_with_cover_01.ogg'),('derivative','game/assets/audio/remedy-heating/H01.wav')]:
        row={'path':path,'sha256':sha((ROOT/path).read_bytes())}
        with patch.object(Path,'read_bytes',lambda obj: read_bytes(obj)+b'drift' if obj==ROOT/path else read_bytes(obj)):
            rejected(name,lambda: bound(row))
    rows=[load('audio-direction/remedy-heating-runtime-review/live/'+m+'.json') for m in MODES]
    saves=[(REVIEW/'live'/f'{m}.json.save.json').read_bytes() for m in MODES]
    for name,key in [('truth','truthAfter'),('transcript','transcript'),('movement','motion'),('treatment','treatment')]:
        changed=copy.deepcopy(rows);changed[2][key]='drift';rejected(name,lambda: parity(changed,saves))
    rejected('complete save',lambda: parity(rows,[*saves[:-1],b'drift']))
    rejected('missing measurements',lambda: bound({'path':'audio-direction/remedy-heating-runtime-review/output/absent-native-measurements.json','sha256':'none'}))
    return passed


def source_boundary():
    """Protect every baseline input outside exact permitted edits and the recorded local exception."""
    grant=load('audio-direction/remedy-heating-runtime-authority.json')
    require(grant['owner_statement']=='runtime approved','Runtime grant missing')
    bound(grant['proposal']);proposal=load(grant['proposal']['path']);bound(proposal['proposal'])
    for row in grant['accepted_audio_bindings']:
        bound(row)
    allowed=set(grant['implementation_files']['narrow_existing_edits'])|{'game/project.godot'}
    revision=grant['baseline_commit'];narrow_edits(revision);count=0
    for item in subprocess.check_output(['git','ls-tree','-rz',revision],cwd=ROOT).split(b'\0'):
        if not item:
            continue
        metadata,name=item.split(b'\t',1);path=name.decode()
        if path in allowed:
            continue
        data=(ROOT/path).read_bytes();require(hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==metadata.decode().split()[2],'Protected tracked input drift: '+path);count+=1
    extra=subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=ROOT,text=True).splitlines()
    permitted=set(grant['implementation_files']['new_exact_paths'])
    for path in extra:
        if path in permitted or path.startswith('audio-direction/remedy-heating-runtime-review/'):
            continue
        if path.startswith('game/') and path.endswith('.gd.uid') and re.fullmatch(rb'uid://[a-z0-9]+\s*',(ROOT/path).read_bytes()):
            before=subprocess.check_output(['git','show',revision+':'+path[:-4]],cwd=ROOT)
            require((ROOT/path[:-4]).read_bytes()==before,'Generated sidecar source changed');continue
        require(False,'Unapproved new path: '+path)
    local=grant['protected_repository_snapshot']['local_exception'];bound(local)
    source=load('audio-direction/remedy-heating-source/provenance.json')
    require(source['original_sha256']==sha((ROOT/'audio-direction/remedy-heating-source/originals/cooking_with_cover_01.ogg').read_bytes()),'Original drift')
    require(source['wav_sha256']==sha((ROOT/'game/assets/audio/remedy-heating/H01.wav').read_bytes()),'PCM drift')
    require(source['frames']==144000 and source['rate_hz']==48000 and source['channels']==2 and not source['loop'] and source['uniform_gain_db']<=0 and source['sample_peak_dbfs']<=-24,'Derivative contract drift')
    require(source['source_acceptance']['statement']=='I just listened to it - sounds perfect!','Owner source acceptance missing')
    require('compress/mode=0' in (ROOT/'game/assets/audio/remedy-heating/H01.wav.import').read_text(),'Compressed runtime import')
    contracts()
    return count


def output_only():
    """Reanalyse exact retained native samples; validate complete case inventory and recording inputs."""
    subprocess.run([sys.executable,str(ROOT/'scripts/check-remedy-heating-audio-output.py')],cwd=ROOT,check=True)
    evidence=load('audio-direction/remedy-heating-runtime-review/output/capture-inputs.json')
    for row in evidence['inputs']:
        bound(row)


def reviewed():
    """Require actual ordinary-mode parity, native UI, mounted audio bytes and sealed review bindings."""
    rows=[load('audio-direction/remedy-heating-runtime-review/live/'+m+'.json') for m in MODES]
    saves=[(REVIEW/'live'/f'{m}.json.save.json').read_bytes() for m in MODES]
    parity(rows,saves)
    require(len(rows[0]['checks'])>=2,'Missing live rejection/lifecycle cases')
    views=load('audio-direction/remedy-heating-runtime-review/views/native.json')
    require(views['status']=='passed' and views['captionOwners']==13 and views['capacity']==16 and views['returnFocus'],'Native UI missing')
    package=load('audio-direction/remedy-heating-runtime-review/packaging/inventory.json')
    require(package['streams']==40 and package['individual_notices']==35 and package['notice_groups']==21 and package['retained_entries_byte_identical']==110,'Packaging totals drift')
    require(len(package['presets'])==3 and all(x['exact'] for x in package['presets']),'Mounted preset PCM16 missing')
    with ZipFile(REVIEW/'packaging/audio-subset.zip') as current,ZipFile(ROOT/'audio-direction/rain-runtime-review/packaging/audio-subset.zip') as previous:
        require(len(current.namelist())==115,'Native audio entries drift')
        require(all(current.read(n)==previous.read(n) for n in previous.namelist()),'Prior mounted audio changed')
        for row in package['entries']:
            require(sha(current.read(row['path']))==row['sha256'],'Mounted entry drift')
    verification=load('audio-direction/remedy-heating-runtime-verification.json')
    for row in verification['evidence_bindings']:
        bound(row)
    output_only()


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--source-only',action='store_true');parser.add_argument('--output-only',action='store_true');parser.add_argument('--reviewed',action='store_true');parser.add_argument('--self-test',action='store_true');args=parser.parse_args()
    if args.self_test:
        probes=rejection_probes();(REVIEW/'rejection-probes.json').write_text(json.dumps({'status':'passed','probes':probes},indent=2)+'\n');print('PASS: heating validator rejects',len(probes),'mutations')
    elif args.output_only:
        output_only()
    else:
        count=source_boundary()
        if args.reviewed:
            reviewed()
        print('PASS: heating boundary:',count,'protected tracked inputs; exact granted commit/source and retained owners'+('; live/native/UI/mounted evidence' if args.reviewed else ''))

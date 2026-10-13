#!/usr/bin/env python3
"""Verify the granted herb successor, frozen owners, real parity and native/package evidence."""
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
REVIEW=ROOT/'audio-direction/herb-collection-runtime-review'
MODES=['default','quiet','master-muted','effects-muted','missing-stream','captions-off','audio-disabled']


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
    """Check producer admission, spatial boundaries, fixed output, lifecycle and anonymous equivalence."""
    checks={
        'game/TemporaryCharacterExaminationFlowController.cs': ['ReferenceEquals(token, LastCollectionCommit)',
            'ReferenceEquals(token.Occurrence, occurrenceController.LastOccurrence)', 'ReferenceEquals(token.EventLog,',
            'ReferenceEquals(token.WorldTime,', 'ReferenceEquals(token.Transcript,', 'ReferenceEquals(token.Transcript.Entries[^1]',
            'CollectionCommitted?.GetInvocationList()', 'catch (Exception error)', 'NotifyCollection(result);'],
        'src/Application/Audio/HerbCollectionPresentationProjection.cs': ['ActorPixels = 32', 'ListenerPixels = 192',
            'CaptionSeconds = 2', 'PassageSeconds = 1', 'TeleportPixels = 96', 'Phrase = "Soft rustle"',
            'ReferenceEquals(result.Safety, CharacterExaminationSafety.SafeToCollect)', 'ReferenceEquals(action.TargetObjectId, target)',
            '< ListenerPixels * ListenerPixels', '<= ActorPixels * ActorPixels'],
        'game/OpeningRegionHerbCollectionAudioController.cs': ['Bus = "Effects"', 'coordinator.OwnsCollection(commit)',
            'ReferenceEquals(consumed, commit)', 'consumed = commit;', 'consumed = coordinator.LastCollectionCommit',
            'MaxDistance = 192', 'Attenuation = 1', 'PanningStrength = 0.6f', 'AreaMask = 0', 'VolumeLinear = 1',
            'MaxPolyphony = 1', 'PitchScale = 1', 'PresentationReset += ResetAfterLoad', 'history.Clear()', 'player.Stop()',
            'scene.Exterior(healer.GlobalPosition)', 'scene.Exterior(seeker.GlobalPosition)', 'listener.IsCurrent()',
            'candidate.Data.Length == 192000', 'AudioServer.GetBusIndex("Effects") >= 0'],
        'game/OpeningRegionHerbCollectionAudioSource.cs': ['MapGetIterationId(map) == 0', 'RegionGetIterationId',
            'polygon.GetVertices().SequenceEqual(vertices)', 'local.DistanceSquaredTo(localClosest) <= 1',
            'supported && OpeningRegionInteriorAudioProjection.Select', 'ReferenceEquals(roster.Participants[0], character)',
            'new Vector2(920, 350)', 'patch.GetChildren().SequenceEqual(patchChildren)'],
        'src/Application/Audio/HerbCollectionCueHistory.cs': ['Capacity = 16', 'Array.AsReadOnly(entries.ToArray())']}
    for path, fragments in checks.items():
        text=(ROOT/path).read_text();require(all(x in text for x in fragments), 'Runtime contract drift: '+path)
    controller=(ROOT/'game/OpeningRegionHerbCollectionAudioController.cs').read_text()
    require('Ducking.Begin' not in controller and 'TryOccur' not in controller and 'CommandMove' not in controller, 'Unapproved gameplay/lease')
    require('Nearby rustle cues' in (ROOT/'game/PositionalSoundCueHistoryPanel.cs').read_text(), 'Anonymous history missing')
    require('Name = "HerbCollection"' in (ROOT/'game/MaterialSoundCaptionPanel.cs').read_text(), 'Sixteenth independent caption missing')


def narrow_edits(revision):
    """Reverse only granted rustle additions; every retained product byte must match."""
    paths=['game/Main.tscn','game/MaterialAudioCreditsPanel.cs','game/MaterialSoundCaptionPanel.cs',
           'game/TemporaryCharacterExaminationFlowController.cs','game/PositionalSoundCueHistoryPanel.cs','game/export_presets.cfg']
    for path in paths:
        text=(ROOT/path).read_text()
        if path.endswith('Main.tscn'):
            text=text.replace('load_steps=440','load_steps=437')
            text=re.sub(r'\[ext_resource[^\n]+id="herb_source"\]\n\[ext_resource[^\n]+id="herb_audio"\]\n\[ext_resource[^\n]+id="herb_check"\]\n\n','',text)
            text=re.sub(r'\[node name="OpeningRegionHerbCollectionAudioSource"[^\n]+\nscript = ExtResource\("herb_source"\)\n\n\[node name="OpeningRegionHerbCollectionAudioController"[^\n]+\nscript = ExtResource\("herb_audio"\)\n\n\[node name="HerbCollectionAudioRuntimeSmokeCheck"[^\n]+\nscript = ExtResource\("herb_check"\)\n\n','',text)
        elif path.endswith('MaterialAudioCreditsPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if 'assets/audio/herb-collection/CREDITS.txt' not in line)
        elif path.endswith('export_presets.cfg'):
            text=text.replace(',assets/audio/herb-collection/*.txt,assets/audio/herb-collection/*.json,assets/audio/herb-collection/*.html','')
        elif path.endswith('MaterialSoundCaptionPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if not any(x in line for x in ['private Label herbLabel','private string herb','herbLabel = new()', 'lines.AddChild(herbLabel);','herbLabel.Text =']))
            text=re.sub(r'    /// <summary>Sets only the independent anonymous rustle line.</summary>\n.*?internal void ClearHerbCollection[^\n]+\n\n','',text,flags=re.S)
            text=text.replace('panel.OffsetTop = -564','panel.OffsetTop = -532').replace(', herbLabel }',' }').replace(' || herb.Length > 0','')
        elif path.endswith('PositionalSoundCueHistoryPanel.cs'):
            text=''.join(line for line in text.splitlines(keepends=True) if not any(x in line for x in ['private OpeningRegionHerbCollectionAudioController','herb = GetNodeOrNull','IReadOnlyList<string> soft =','string rustles =']))
            text=text.replace('Reply, footstep, foliage, wind, rain, preparation and rustle cues · cleared when loading a save','Reply, footstep, foliage, wind, rain, heating, steeping and cooling cues · cleared when loading a save').replace(' + "\\n\\nNearby rustle cues\\n" + rustles','')
        else:
            text=re.sub(r'    // BEGIN herb collection presentation ownership\n.*?    // END herb collection presentation ownership\n\n','',text,flags=re.S)
            text=text.replace('            NotifyCollection(result);\n','')
        before=subprocess.check_output(['git','show',revision+':'+path],cwd=ROOT,text=True)
        require(text==before,'Unapproved retained behavior/composition edit: '+path)


def parity(rows,saves,gameplay,placement):
    """Compare complete durable and non-durable gameplay state across seven genuine flow modes."""
    require([r['mode'] for r in rows]==MODES and len(saves)==7 and len(gameplay)==7, 'Missing or reordered mode')
    require(all(data==saves[0] for data in saves) and all(data==gameplay[0] for data in gameplay), 'Complete save/gameplay parity failed')
    for row in rows:
        require(row['unchangedBefore']==row['unchangedAfter']==rows[0]['unchangedBefore'], 'Protected state drift')
        require(row['placement']==placement and row['commits']==1, 'Genuine flow occurrence changed')
        expected=1 if placement=='near' and row['mode']!='audio-disabled' else 0
        require(row['presentations']==expected and row['history']==expected, 'Actual spatial equivalence drift')


def source_boundary():
    """Protect all persistent owners and exact additive integration, rights and installed accepted source."""
    grant=load('audio-direction/herb-collection-runtime-authority.json')
    require(grant['owner_statement']=='approved' and grant['source_use_runtime_authorized'], 'Bounded scope grant missing')
    for row in grant['proposal_bindings']: bound(row)
    qualification=grant['source_qualification']
    require(qualification['status']=='qualified' and qualification['owner_statement']=='suitable', 'Exact listening acceptance missing')
    proposal=load('audio-direction/herb-collection-owner-scope.json');files=proposal['implementation_files']
    allowed=set(files['narrow_existing_edits'])|{'game/project.godot'};revision=grant['baseline_commit'];narrow_edits(revision);count=0
    for item in subprocess.check_output(['git','ls-tree','-rz',revision],cwd=ROOT).split(b'\0'):
        if not item: continue
        metadata,name=item.split(b'\t',1);path=name.decode()
        if path in allowed: continue
        data=(ROOT/path).read_bytes()
        require(hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==metadata.decode().split()[2], 'Protected tracked input drift: '+path);count+=1
    retained = grant.get('retained_predecessor_sidecars', {}).get('bindings', [])
    old_uids = [r for r in load('audio-direction/remedy-cooling-runtime-verification.json')['evidence_bindings'] if r['path'].endswith('.cs.uid')]
    require(retained == old_uids, 'Accepted predecessor UID inventory drift')
    for row in retained: bound(row)
    permitted=set(files['new_exact_paths'])|{r['path'] for r in retained}|{'audio-direction/herb-collection-source/rights/catalogue.html.gz'}
    for path in subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=ROOT,text=True).splitlines():
        if any(path.startswith(row['path']) for row in grant.get('unrelated_untracked_directories', [])): continue
        if path in permitted or path.startswith('audio-direction/herb-collection-runtime-review/') or path==grant['selected_source']['original_path']: continue
        if path.startswith('game/') and path.endswith('.gd.uid') and re.fullmatch(rb'uid://[a-z0-9]+\s*',(ROOT/path).read_bytes()):
            require((ROOT/path[:-4]).read_bytes()==subprocess.check_output(['git','show',revision+':'+path[:-4]],cwd=ROOT), 'Generated sidecar source changed');continue
        require(False, 'Unapproved new path: '+path)
    bound(grant['local_exception']);bound({'path':grant['selected_source']['original_path'],'sha256':grant['selected_source']['original_sha256']})
    source=load('audio-direction/herb-collection-source/provenance.json')
    require(source['status']=='qualified' and source['licence']=='CC0-1.0' and source['frames']==48000 and source['rate_hz']==48000
        and source['channels']==2 and source['window_seconds']==[.1,1.1], 'Source lineage/encoding/window drift')
    require(sha((ROOT/'game/assets/audio/herb-collection/HC01.wav').read_bytes())==qualification['candidate_sha256']==source['wav_sha256'], 'Owner accepted PCM bytes drift')
    require((ROOT/'game/assets/audio/herb-collection/provenance.json').read_bytes()==(ROOT/'audio-direction/herb-collection-source/provenance.json').read_bytes(), 'Installed lineage drift')
    for row in grant['selected_source']['rights_retrieval']:
        path=ROOT/(row.get('stored_path') or 'audio-direction/herb-collection-source/rights/deed.html')
        data=gzip.decompress(path.read_bytes()) if path.suffix=='.gz' else path.read_bytes()
        require(sha(data)==row['sha256'], 'Exact rights capture drift')
    require('compress/mode=0' in (ROOT/'game/assets/audio/herb-collection/HC01.wav.import').read_text(), 'Compressed import')
    contracts();return count


def output_only():
    """Reanalyse retained native samples and bind actual executed capture inputs."""
    subprocess.run([sys.executable,str(ROOT/'scripts/check-herb-collection-audio-output.py')],cwd=ROOT,check=True)
    capture = load('audio-direction/herb-collection-runtime-review/output/capture-inputs.json')
    for row in capture['inputs'] + capture.get('measurement_inputs', []): bound(row)


def reviewed():
    """Require genuine spatial parity, native accessibility, every mounted preset and byte-bound evidence."""
    for placement in ['near','remote']:
        rows=[load('audio-direction/herb-collection-runtime-review/parity/'+placement+'-'+m+'.json') for m in MODES]
        saves=[(REVIEW/'parity'/f'{placement}-{m}.json.save.json').read_bytes() for m in MODES]
        gameplay=[(REVIEW/'parity'/f'{placement}-{m}.json.gameplay.json').read_bytes() for m in MODES]
        parity(rows,saves,gameplay,placement)
    probes=load('audio-direction/herb-collection-runtime-review/admission/summary.json')
    require(probes['status']=='passed' and len(probes['cases'])>=6,'Live boundary/discard probes missing')
    views=load('audio-direction/herb-collection-runtime-review/views/native.json')
    require(views['status']=='passed' and views['captionOwners']==16 and views['capacity']==16 and views['returnFocus'], 'Native accessibility missing')
    package=load('audio-direction/herb-collection-runtime-review/packaging/inventory.json')
    require(package['streams']==43 and package['individual_notices']==42 and package['notice_groups']==24
        and package['retained_entries_byte_identical']==125 and len(package['presets'])==3 and all(p['exact'] for p in package['presets']), 'Mounted resources incomplete')
    with ZipFile(REVIEW/'packaging/audio-subset.zip') as current, ZipFile(ROOT/'audio-direction/remedy-cooling-runtime-review/packaging/audio-subset.zip') as previous:
        require(len(current.namelist())==130 and all(current.read(n)==previous.read(n) for n in previous.namelist()), 'Prior mounted resources changed')
        for row in package['entries']: require(sha(current.read(row['path']))==row['sha256'], 'Mounted entry drift')
    for row in load('audio-direction/herb-collection-runtime-verification.json')['evidence_bindings']: bound(row)
    output_only()


def rejection_probes():
    """Exercise actual guards with in-memory changes, without rewriting protected inputs."""
    from unittest.mock import patch
    import copy
    passed=[];read=Path.read_text
    def reject(name,action):
        try: action()
        except (ValueError, FileNotFoundError): passed.append(name);return
        raise ValueError('Mutation escaped: '+name)
    for path,old,new in [
        ('game/TemporaryCharacterExaminationFlowController.cs','ReferenceEquals(token, LastCollectionCommit)','true'),
        ('src/Application/Audio/HerbCollectionPresentationProjection.cs','ActorPixels = 32','ActorPixels = 33'),
        ('src/Application/Audio/HerbCollectionPresentationProjection.cs','< ListenerPixels * ListenerPixels','<= ListenerPixels * ListenerPixels'),
        ('src/Application/Audio/HerbCollectionPresentationProjection.cs','Soft rustle','Herb collected'),
        ('game/OpeningRegionHerbCollectionAudioController.cs','Bus = "Effects"','Bus = "Master"'),
        ('game/OpeningRegionHerbCollectionAudioController.cs','consumed = coordinator.LastCollectionCommit','consumed = null'),
        ('game/OpeningRegionHerbCollectionAudioSource.cs','supported && OpeningRegionInteriorAudioProjection.Select','OpeningRegionInteriorAudioProjection.Select'),
        ('game/MaterialSoundCaptionPanel.cs','panel.OffsetLeft = 16','panel.OffsetLeft = 17')]:
        with patch.object(Path,'read_text',lambda obj,*a,**kw: read(obj,*a,**kw).replace(old,new) if obj==ROOT/path else read(obj,*a,**kw)):
            reject(old, lambda: narrow_edits(load('audio-direction/herb-collection-runtime-authority.json')['baseline_commit']) if path.endswith('MaterialSoundCaptionPanel.cs') else contracts())
    for placement in ['near','remote']:
        rows=[load('audio-direction/herb-collection-runtime-review/parity/'+placement+'-'+m+'.json') for m in MODES]
        saves=[(REVIEW/'parity'/f'{placement}-{m}.json.save.json').read_bytes() for m in MODES]
        games=[(REVIEW/'parity'/f'{placement}-{m}.json.gameplay.json').read_bytes() for m in MODES]
        for key in ['unchangedAfter','presentations','commits']:
            changed=copy.deepcopy(rows);changed[1][key]='drift';reject(placement+' '+key,lambda: parity(changed,saves,games,placement))
        reject(placement+' complete save',lambda: parity(rows,[*saves[:-1],b'drift'],games,placement))
        reject(placement+' complete gameplay',lambda: parity(rows,saves,[*games[:-1],b'drift'],placement))
    return passed


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    for flag in ['source-only','output-only','reviewed','self-test']: parser.add_argument('--'+flag,action='store_true')
    args=parser.parse_args()
    if args.self_test:
        probes=rejection_probes();(REVIEW/'rejection-probes.json').write_text(json.dumps({'status':'passed','probes':probes},indent=2)+'\n');print('PASS: herb validator rejects',len(probes),'mutations')
    elif args.output_only: output_only()
    else:
        count=source_boundary()
        if args.reviewed: reviewed()
        print('PASS: herb boundary:',count,'protected tracked inputs; exact granted token/source'+('; live/native/UI/mounted evidence' if args.reviewed else ''))

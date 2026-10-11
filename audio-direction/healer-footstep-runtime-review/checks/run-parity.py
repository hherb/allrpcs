import os,subprocess,json
from pathlib import Path
root=Path('/Users/hherb/src/allrpcs');review=root/'audio-direction/healer-footstep-runtime-review';modes=('default','quiet','master-muted','effects-muted','missing-resources','captions-off','observer-disabled')
for mode in modes:
 output=review/'live'/f'{mode}.json';log=review/'checks'/f'{mode}.log'
 env=dict(os.environ,ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED='1',ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='1',ALLRPCS_HEALER_FOOTSTEP_CHECK='1',ALLRPCS_HEALER_FOOTSTEP_MODE=mode,ALLRPCS_HEALER_FOOTSTEP_REPORT=str(output))
 for attempt in range(3):
  with log.open('wb') as f:r=subprocess.run(['godot','--headless','--fixed-fps','60','--path','game','--quit-after','1200'],cwd=root,env=env,stdout=f,stderr=subprocess.STDOUT)
  if r.returncode==0:break
  text=log.read_text();assert r.returncode in (-6,134) and 'mutex lock failed' in text and 'ERROR:' not in text,(mode,r.returncode,text[-3500:])
  log.rename(log.with_suffix(f'.attempt{attempt+1}.log'))
 assert r.returncode==0 and 'PASS: Healer footstep Main:' in log.read_text(),mode
 print('PASS',mode,flush=True)
reports=[json.loads((review/'live'/f'{m}.json').read_text()) for m in modes]
saves=[(review/'live'/f'{m}.json.save.json').read_bytes() for m in modes]
assert all(s==saves[0] for s in saves)
assert all(r['transcript']==reports[0]['transcript'] and r['motion']==reports[0]['motion'] and r['truthBefore']==r['truthAfter']==reports[0]['truthBefore'] for r in reports)
assert len({r['contacts'] for r in reports[:-1]})==1 and reports[-1]['contacts']==0
assert len({r['presentations'] for r in reports[:-1]})==1 and reports[-1]['presentations']==0
(review/'live'/'parity.json').write_text(json.dumps({'status':'passed','modes':list(modes),'contacts':reports[0]['contacts'],'presentations':reports[0]['presentations'],'complete_save_transcript_motion_truth_parity':True},indent=2)+'\n')
print('PASS all seven modes complete save/transcript/motion/truth parity',flush=True)

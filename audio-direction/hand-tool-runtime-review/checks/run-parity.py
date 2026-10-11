from pathlib import Path
import os,subprocess,json,hashlib
root=Path(__file__).resolve().parents[3];review=root/'audio-direction/hand-tool-runtime-review';modes=['default','quiet','master-muted','effects-muted','missing-stream','no-action']
env=dict(os.environ,ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED='1',ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='1',ALLRPCS_TOOL_MAIN_CHECK='1')
results=[]
for mode in modes:
 output=review/'parity'/f'{mode}.json';args=['godot','--headless','--fixed-fps','60','--path','game','--quit-after','1200'];attempts=[]
 for attempt in range(1,4):
  r=subprocess.run(args,env=dict(env,ALLRPCS_TOOL_MODE=mode,ALLRPCS_TOOL_CHECK_OUTPUT=str(output)),cwd=root,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
  log=review/'checks'/f'main-{mode}-{attempt}.log';log.write_bytes(r.stdout);text=r.stdout.decode(errors='replace')
  attempts.append({'log':str(log.relative_to(root)),'exit_code':r.returncode,'sha256':hashlib.sha256(r.stdout).hexdigest()})
  if r.returncode==0 and 'PASS: hand-tool Main:' in text:break
  if not ('MutexLock: pthread_mutex_lock failed' in text and not any(s in text for s in ['ERROR:','Unhandled exception'])):raise RuntimeError(text[-5000:])
 else:raise RuntimeError('Native shutdown did not succeed')
 results.append({'mode':mode,'command':args,'attempts':attempts});print('PASS hand-tool parity',mode,flush=True)
saves=[(review/'parity'/f'{mode}.json.save.json').read_bytes() for mode in modes];reports=[json.loads((review/'parity'/f'{mode}.json').read_text()) for mode in modes]
assert all(s==saves[0] for s in saves);assert all(r['transcript']==reports[0]['transcript'] for r in reports)
assert all(r['commits']==6 and r['presentations']==6 for r in reports[:-1])
assert reports[-1]['commits']==0 and reports[-1]['presentations']==0
record={'schema_version':1,'status':'passed','modes':modes,'save_sha256':hashlib.sha256(saves[0]).hexdigest(),'transcript':reports[0]['transcript'],'commits':reports[0]['commits'],'presentations':reports[0]['presentations'],'no_action_commits':0,'results':results}
(review/'parity'/'verification.json').write_text(json.dumps(record,indent=2)+'\n');print('PASS: five identical contact modes plus no-action control have exact same complete durable state/transcript',flush=True)

import importlib.util,json,os,subprocess
from pathlib import Path
root=Path.cwd(); out=root/'artifacts/dialogue-repeat-diagnosis'; out.mkdir(parents=True,exist_ok=True)
spec=importlib.util.spec_from_file_location('quality',root/'quality.py'); q=importlib.util.module_from_spec(spec); spec.loader.exec_module(q)
revision='1f90251ac841216a1ba8d0c3b26575ce19b1d53a'
with q.snapshot(revision,editor=True) as (tree,env):
 p=tree/'game/OllamaParticipationMainCheck.cs'; old=p.read_text()
 start=old.index('            LineEdit input =')
 end=old.index('            main.QueueFree();',start)
 body='''            LineEdit input = main.GetNode<LineEdit>("DialogueOverlay/Panel/Input");
            CharacterModelGenerationFlowController generation = main.GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
            CharacterTurnTranscriptController transcript = main.GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
            List<object> turns = [];
            string[] inputs = ["Healer, please say hello and wait here.", "Healer, please say hello again and wait here.", "Healer, please tell me about herbs, but remain here and wait.", "Healer, please tell me what you know, but remain here and wait.", "Healer, please say hello and wait here.", "Healer, thank you. Please wait here.", "Healer, please say hello again and wait here.", "Healer, please say hello and wait here."];
            for (int turn = 0; turn < inputs.Length; turn++)
            {
                var before = generation.Result;
                int beforeReplies = handler.ReplyCalls;
                seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
                input.Text = inputs[turn];
                main.GetNode<Button>("DialogueOverlay/Panel/Submit").EmitSignal(BaseButton.SignalName.Pressed);
                ulong started = Time.GetTicksMsec();
                while (Time.GetTicksMsec() - started < 15_000UL)
                {
                    await Task.Delay(10); await Frames(1);
                    if (generation.Result is not null && !ReferenceEquals(before, generation.Result)) { break; }
                }
                var termination = generation.Result?.Updates.OfType<CharacterModelStreamTermination>().LastOrDefault();
                var row = new {
                    turn = turn + 1, elapsedMilliseconds = Time.GetTicksMsec() - started,
                    newResult = !ReferenceEquals(before, generation.Result),
                    replyTermination = termination?.Status.ToString(), replyFailure = termination?.Failure?.Code.ToString(),
                    replyCalls = handler.ReplyCalls - beforeReplies, participationCalls = handler.ParticipationCalls,
                    nativeMessageFields = handler.NativeMessageFields, nativeReplies = handler.NativeReplies.ToArray(),
                    transcriptEntries = transcript.Transcript?.Entries.Count
                };
                turns.Add(row);
                File.WriteAllText(report, JsonSerializer.Serialize(new { revision = "REVISION", fictionalInputsOnly = true, personalAudioCaptured = false, turns }, JsonOptions));
                GD.Print("REPEAT TURN " + (turn + 1) + ": " + termination?.Status + "/" + termination?.Failure?.Code);
                handler.NativeReplies.Clear();
                await Task.Delay(1500); await Frames(4);
            }
'''.replace('REVISION',revision)
 old=old[:start]+body+old[end:]
 old=old.replace('GetTree().Quit(liveFailed ? 1 : 0);','GetTree().Quit(0);')
 old=old.replace('private readonly HttpMessageInvoker liveTransport', 'public List<string> NativeReplies { get; } = [];\n        private readonly HttpMessageInvoker liveTransport')
 old=old.replace('NativeMessageFields = fields.Order', '''if (!root.GetProperty("format").GetProperty("properties").TryGetProperty("kind", out _)) {
                        NativeReplies.Add(string.Concat(lines.Select(line => { using var d = JsonDocument.Parse(line); return d.RootElement.TryGetProperty("message", out var m) ? m.GetProperty("content").GetString() : ""; })));
                    }
                    NativeMessageFields = fields.Order''')
 old=old.replace('root.GetProperty("format").GetProperty("properties").TryGetProperty("kind", out _)', 'IsParticipation(root)')
 old=old.replace('        public int ParticipationCalls', '        private static bool IsParticipation(JsonElement root) => root.GetProperty("format").TryGetProperty("properties", out JsonElement properties) && properties.TryGetProperty("kind", out _);\n        public int ParticipationCalls')
 p.write_text(old)
 subprocess.run(['git','apply',str(root/'speech-direction/player-microphone-qualification/response-failure-diagnosis/proposal.patch')],cwd=tree,check=True)
 (out/'prototype-method.json').write_text(json.dumps({'revision':revision,'method':'Disposable committed snapshot; diagnostic-only fixture extension, eight fictional turns. No root runtime edits; no microphone or speech playback. Wait instructions and listener repositioning keep hearing in range.'},indent=2)+'\n')
 e=dict(os.environ,**env,ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED='1',ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1',ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json',ALLRPCS_OLLAMA_PARTICIPATION_MODE='live',ALLRPCS_OLLAMA_PARTICIPATION_REPORT=str(out/'prototype-main.json'),ALLRPCS_CHARACTER_MODEL_SETTINGS_PATH=str(out/'isolated-model-settings.json'),ALLRPCS_PLAYER_ASR_CONFIGURATION_PATH='/private/tmp/allrpcs-asr-missing.json',ALLRPCS_POCKET_SPEECH_CONFIGURATION_PATH='/private/tmp/allrpcs-speech-missing.json')
 for name,args in [('build',['dotnet','build','game/Allrpcs.Game.csproj','-m:1','/nr:false']),('import',['godot','--headless','--editor','--path','game','--import'])]:
  r=subprocess.run(args,cwd=tree,env=e,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,timeout=240)
  (out/(name+'.log')).write_text(r.stdout)
  if r.returncode: print(r.stdout[-2500:]); raise SystemExit(r.returncode)
 print('Disposable repeated-turn diagnostic built',flush=True)
 r=subprocess.run(['godot','--headless','--path',str(tree/'game'),'res://OllamaParticipationMainCheck.tscn'],cwd=tree,env=e,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,timeout=650)
 (out/'run.log').write_text(r.stdout)
 print(r.stdout[-3500:],flush=True)
 raise SystemExit(r.returncode)

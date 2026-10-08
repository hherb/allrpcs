extends SceneTree
func _initialize():
    var stream = load('res://assets/audio/positional/A06-D01.wav') as AudioStreamWAV
    if stream == null or stream.format != AudioStreamWAV.FORMAT_16_BITS or not stream.stereo or stream.mix_rate != 48000 or stream.data.size() != 96000 or stream.loop_mode != AudioStreamWAV.LOOP_DISABLED:
        push_error('Unexpected native decoded resource')
        quit(1)
        return
    var file = FileAccess.open('/Users/hherb/src/allrpcs/audio-direction/positional-cue-runtime-review/decoded-A06-D01.pcm', FileAccess.WRITE)
    file.store_buffer(stream.data)
    print('PASS: native noncompressed PCM16 stereo 48kHz; 24000 frames; no loop; bytes retained')
    quit()

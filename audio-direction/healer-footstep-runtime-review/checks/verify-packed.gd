extends SceneTree

func _init():
    if not ProjectSettings.load_resource_pack('/Users/hherb/src/allrpcs/audio-direction/healer-footstep-runtime-review/packaging/audio-subset.zip', true):
        push_error('Missing native exported pack')
        quit(1)
        return
    for material in range(1, 5):
        for variant in range(1, 7):
            var clip = 'W%02d-%02d' % [material, variant]
            var stream = ResourceLoader.load('res://assets/audio/footsteps/' + clip + '.wav', '', ResourceLoader.CACHE_MODE_IGNORE)
            if not stream is AudioStreamWAV or stream.mix_rate != 48000 or not stream.stereo or stream.loop_mode != 0 or abs(stream.get_length() - 0.5) > 0.001:
                push_error('Changed packed native stream: ' + clip)
                quit(1)
                return
            FileAccess.open('/private/tmp/healer-' + clip + '.pcm16', FileAccess.WRITE).store_buffer(stream.data)
    print('PASS: all 24 native exported footstep streams decoded from mounted pack; stereo 48000 nonlooping')
    quit()

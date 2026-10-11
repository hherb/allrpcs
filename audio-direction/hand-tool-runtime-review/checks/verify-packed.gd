extends SceneTree

func _init():
    if not ProjectSettings.load_resource_pack('/private/tmp/allrpcs-tool-resources.zip', true):
        push_error('Missing native exported pack')
        quit(1)
        return
    var stream = ResourceLoader.load('res://assets/audio/hand-tool/A03-T01.wav', '', ResourceLoader.CACHE_MODE_IGNORE)
    if not stream is AudioStreamWAV or stream.mix_rate != 48000 or not stream.stereo or stream.loop_mode != 0:
        push_error('Changed packed hand-tool native stream')
        quit(1)
        return
    FileAccess.open('/private/tmp/allrpcs-tool-packed.pcm16', FileAccess.WRITE).store_buffer(stream.data)
    print('PASS: native exported hand-tool stream decoded from mounted pack; stereo 48000 nonlooping')
    quit()

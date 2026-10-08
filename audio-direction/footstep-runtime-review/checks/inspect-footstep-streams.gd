extends SceneTree

func _initialize():
    DirAccess.make_dir_recursive_absolute("/tmp/allrpcs-footstep-decoded")
    var rows = []
    for material in range(1, 5):
        for variant in range(1, 7):
            var name = "W%02d-%02d" % [material, variant]
            var stream = load("res://assets/audio/footsteps/" + name + ".wav") as AudioStreamWAV
            if stream == null or stream.format != AudioStreamWAV.FORMAT_16_BITS or not stream.stereo or stream.mix_rate != 48000 or stream.data.size() != 96000:
                push_error("Unexpected decoded contact: " + name)
                quit(1)
                return
            var output = FileAccess.open("/tmp/allrpcs-footstep-decoded/" + name + ".pcm", FileAccess.WRITE)
            output.store_buffer(stream.data)
            rows.append({"id": name, "format": "PCM16", "stereo": stream.stereo, "rate_hz": stream.mix_rate, "bytes": stream.data.size(), "loop_disabled": stream.loop_mode == AudioStreamWAV.LOOP_DISABLED})
    var output = FileAccess.open("/tmp/allrpcs-footstep-decoded/inventory.json", FileAccess.WRITE)
    output.store_string(JSON.stringify(rows, "  "))
    print("PASS: 24 loaded streams are noncompressed stereo PCM16 at 48 kHz; actual decoder bytes retained for parity")
    quit()

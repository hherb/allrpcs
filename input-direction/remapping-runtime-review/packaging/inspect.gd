extends SceneTree
func _initialize():
    if not ProjectSettings.load_resource_pack(OS.get_environment("CONTROLS_PACK")):
        push_error("Cannot mount pack")
        quit(1)
        return
    var manifest = JSON.parse_string(FileAccess.get_file_as_string("/tmp/controls-mount-manifest.json"))
    var rows = []
    for row in manifest["new_files"] + manifest["retained_audio"]:
        var path = "res://" + row["path"]
        var source_path = path
        var remapped = false
        if path.ends_with(".tscn") and FileAccess.file_exists(path + ".remap"):
            var mapping = ConfigFile.new()
            if mapping.load(path + ".remap") != OK:
                push_error("Cannot read scene remap")
                quit(1)
                return
            path = mapping.get_value("remap", "path")
            remapped = true
        if not FileAccess.file_exists(path):
            push_error("Missing packed member " + path)
            quit(1)
            return
        var data = FileAccess.get_file_as_bytes(path)
        var hash = HashingContext.new()
        hash.start(HashingContext.HASH_SHA256)
        hash.update(data)
        var actual = hash.finish().hex_encode()
        if not remapped and actual != row["sha256"]:
            push_error("Packed member drift " + path)
            quit(1)
            return
        rows.append({"path":row["path"],"sha256":row["sha256"],"packed_sha256":actual,"bytes":data.size(),"converted_scene":remapped,"packed_path":path})
        var target = "/tmp/controls-mounted-subset/" + row["path"]
        DirAccess.make_dir_recursive_absolute(target.get_base_dir())
        var file = FileAccess.open(target, FileAccess.WRITE)
        file.store_buffer(data)
        file.close()
    var report = {"status":"passed","new_members":manifest["new_files"].size(),"retained_audio":manifest["retained_audio"].size(),"entries":rows,"limits":"mounted resource bytes; no executable or platform-installation claim"}
    var output = FileAccess.open(OS.get_environment("CONTROLS_PACK_REPORT"),FileAccess.WRITE)
    output.store_string(JSON.stringify(report))
    print("PASS: controls mounted resources: " + str(rows.size()))
    quit()

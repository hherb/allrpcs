extends SceneTree


func _initialize() -> void:
	var packet: String = get_script().resource_path.get_base_dir()
	if FileAccess.file_exists(packet.path_join("owner-runtime-acceptance.json")):
		push_error("Accepted runtime script UID is immutable")
		quit(1)
		return
	for relative in ["../scripts/capture-laundry-ground-runtime.gd.uid", "RuntimeLaundryGroundSmokeCheck.cs.uid"]:
		var path := ProjectSettings.globalize_path("res://").path_join(relative).simplify_path()
		if FileAccess.file_exists(path):
			push_error("Refusing to replace an existing script UID")
			quit(1)
			return
		var file := FileAccess.open(path, FileAccess.WRITE)
		if file == null:
			quit(1)
			return
		file.store_string(ResourceUID.id_to_text(ResourceUID.create_id()) + "\n")
		file.close()
	var texture_ids := {"proof": ResourceUID.id_to_text(ResourceUID.create_id()),
		"runtime": ResourceUID.id_to_text(ResourceUID.create_id())}
	var ids_file := FileAccess.open(packet.path_join("texture-uids.json"), FileAccess.WRITE)
	ids_file.store_string(JSON.stringify(texture_ids, "\t") + "\n")
	ids_file.close()
	print("PASS: generated independent native UIDs for both new scripts")
	quit()

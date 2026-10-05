extends SceneTree


func _initialize() -> void:
	var packet: String = get_script().resource_path.get_base_dir()
	if FileAccess.file_exists(packet.path_join("owner-runtime-acceptance.json")):
		push_error("Accepted runtime script UID is immutable")
		quit(1)
		return
	var path := ProjectSettings.globalize_path("res://").path_join(
		"../scripts/capture-sickhouse-ground-runtime.gd.uid").simplify_path()
	var uid := ResourceUID.id_to_text(ResourceUID.create_id())
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("Could not save new capture UID")
		quit(1)
		return
	file.store_string(uid + "\n")
	file.close()
	print("Generated canonical native UID for the new capture script: ", uid)
	quit()

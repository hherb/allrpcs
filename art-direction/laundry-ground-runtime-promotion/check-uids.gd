extends SceneTree


func _initialize() -> void:
	var path: String = get_script().resource_path.get_base_dir().path_join("uid-inputs.json")
	var file := FileAccess.open(path, FileAccess.READ)
	var inputs: Dictionary = JSON.parse_string(file.get_as_text())
	file.close()
	var old_ids := {}
	for uid in inputs["old"]:
		old_ids[ResourceUID.text_to_id(uid)] = true
	for name in inputs["new"]:
		var uid: String = inputs["new"][name]
		var native_id := ResourceUID.text_to_id(uid)
		if native_id == ResourceUID.INVALID_ID or ResourceUID.id_to_text(native_id) != uid \
			or old_ids.has(native_id):
			push_error("New UID is invalid, noncanonical or reused: " + name)
			quit(1)
			return
		old_ids[native_id] = true
	print("PASS: four canonical native new UIDs are mutually distinct and absent from predecessor")
	quit()

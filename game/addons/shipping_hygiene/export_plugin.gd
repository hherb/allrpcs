@tool
extends EditorExportPlugin
## Transforms export scene copies and lifts harness autoloads from in-memory settings until
## _export_end; never saves source settings/scenes. A failed transform ships the original scene.
## Main itself holds no harness (SmokeCheckHost attaches checks at launch);
## scripts/check-shipping-pack.gd (run by export-development.sh) rejects a transformed Main, any
## harness file, and a Main remap target or dependency missing from the pack.

const PATTERNS := ["*Check*", "*Proof*", "*Probe*"]
var saved_autoloads: Dictionary = {}

func _get_name() -> String:
	return "AllrpcsShippingHygiene"

static func is_harness_path(path: String) -> bool:
	for pattern in PATTERNS:
		if path.matchn(pattern):
			return true
	return false

func _export_begin(_features: PackedStringArray, _debug: bool, _path: String, _flags: int) -> void:
	for setting in ProjectSettings.get_property_list():
		var key: String = setting.name
		if key.begins_with("autoload/"):
			var value: String = ProjectSettings.get_setting(key)
			if is_harness_path(value):
				saved_autoloads[key] = value
				ProjectSettings.set_setting(key, null)

func _export_end() -> void:
	restore_autoloads()

func restore_autoloads() -> void:
	for key in saved_autoloads:
		ProjectSettings.set_setting(key, saved_autoloads[key])
	saved_autoloads.clear()

func _export_file(path: String, type: String, _features: PackedStringArray) -> void:
	if type != "PackedScene":
		return
	var packed: PackedScene = load(path)
	if packed == null:
		push_error("Shipping hygiene could not load " + path)
		return
	var scene := packed.instantiate(PackedScene.GEN_EDIT_STATE_MAIN)
	var removed := remove_harness_children(scene)
	if removed == 0:
		scene.free()
		return
	var result := PackedScene.new()
	var error := result.pack(scene)
	scene.free()
	if error != OK:
		push_error("Shipping hygiene could not repack " + path)
		return
	# A per-process temporary file is only a serialization buffer, never a source asset.
	var temporary := OS.get_temp_dir().path_join("allrpcs-shipping-%s.scn" % OS.get_process_id())
	error = ResourceSaver.save(result, temporary)
	if error != OK:
		push_error("Shipping hygiene could not serialize " + path)
		return
	var bytes := FileAccess.get_file_as_bytes(temporary)
	DirAccess.remove_absolute(temporary)
	if bytes.is_empty():
		push_error("Shipping hygiene could not read serialized " + path)
		return
	# add_file(remap=true) suppresses the original and makes Godot write its remap.
	add_file(path + ".shipping.scn", bytes, true)

static func remove_harness_children(parent: Node) -> int:
	var removed := 0
	for child in parent.get_children():
		var script: Script = child.get_script()
		if is_harness_path(child.scene_file_path) or (script != null and is_harness_path(script.resource_path)):
			parent.remove_child(child)
			child.free()
			removed += 1
		else:
			removed += remove_harness_children(child)
	return removed

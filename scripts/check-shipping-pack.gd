extends SceneTree
## Resource-pack inspection only: local editor assemblies may satisfy managed script loading.
## Usage: godot --headless --path game --script ../scripts/check-shipping-pack.gd -- pack.zip

func _initialize() -> void:
	var arguments := OS.get_cmdline_user_args()
	if not require(arguments.size() == 1, "Expected one exported ZIP path"):
		return
	var archive := ZIPReader.new()
	if not require(archive.open(arguments[0]) == OK, "Cannot read exported ZIP"):
		return
	var files := archive.get_files()
	if not require(not files.has("Main.tscn"), "Original Main shadows export replacement"):
		return
	if not require(files.has("Main.tscn.shipping.scn"), "Missing transformed Main"):
		return
	var remap := archive.read_file("Main.tscn.remap").get_string_from_utf8()
	if not require(remap.contains('path="res://Main.tscn.shipping.scn"'), "Wrong Main remap"):
		return
	# project.binary uses length-prefixed UTF-8 strings, so this token must be absent.
	var settings := archive.read_file("project.binary")
	if not require(settings.size() > 0, "Missing project settings"):
		return
	if not require(not settings.get_string_from_utf8().contains("CheckDeadline"), "Harness autoload retained"):
		return
	for file in files:
		var harness := file.matchn("*Check*") or file.matchn("*Proof*") or file.matchn("*Probe*")
		if not require(not harness, "Harness file retained: " + file):
			return
	if not require(ProjectSettings.load_resource_pack(arguments[0]), "Cannot mount exported ZIP"):
		return
	var dependencies := ResourceLoader.get_dependencies("res://Main.tscn.shipping.scn")
	for entry in dependencies:
		var path := entry.get_slice("::", entry.get_slice_count("::") - 1).trim_prefix("res://")
		var present := files.has(path) or files.has(path + ".remap") or files.has(path + ".import")
		if not require(present, "Pack missing runtime dependency: " + path):
			return
	var packed: PackedScene = load("res://Main.tscn.shipping.scn")
	if not require(packed != null, "Cannot load exported Main"):
		return
	var scene := packed.instantiate()
	if not require(scene != null, "Cannot instantiate exported Main"):
		return
	scene.free()
	archive.close()
	print("PASS: shipping pack: ", files.size(), " entries; ", dependencies.size(),
		" direct runtime dependencies; Main remap/load/instantiate; no harness or deadline autoload")
	quit(0)

func require(condition: bool, message: String) -> bool:
	if not condition:
		push_error(message)
		quit(1)
	return condition

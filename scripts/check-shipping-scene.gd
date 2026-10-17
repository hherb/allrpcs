extends SceneTree
## Loads and repacks Main without entering the tree or touching runtime services.

func _initialize() -> void:
	call_deferred("verify")

func verify() -> void:
	await create_timer(0.2).timeout
	while Engine.is_editor_hint() and EditorInterface.get_resource_filesystem().is_scanning():
		await process_frame
	var exporter = load("res://addons/shipping_hygiene/export_plugin.gd").new()
	var original_autoload = ProjectSettings.get_setting("autoload/CheckDeadline")
	exporter._export_begin([], true, "", 0)
	if ProjectSettings.has_setting("autoload/CheckDeadline"):
		fail("export retains harness autoload")
		return
	exporter._export_end()
	if ProjectSettings.get_setting("autoload/CheckDeadline") != original_autoload:
		fail("export end did not restore autoload")
		return
	# Repeated cleanup covers plugin exit after an interrupted export and is idempotent.
	exporter._export_begin([], false, "", 0)
	exporter.restore_autoloads()
	exporter.restore_autoloads()
	if ProjectSettings.get_setting("autoload/CheckDeadline") != original_autoload:
		fail("interrupted export cleanup did not restore autoload")
		return
	var packed: PackedScene = load("res://Main.tscn")
	var scene := packed.instantiate(PackedScene.GEN_EDIT_STATE_MAIN)
	var before := count_harness(scene, exporter)
	if before < 200:
		fail("source Main unexpectedly lost harness composition")
		return
	exporter.remove_harness_children(scene)
	if count_harness(scene, exporter) != 0:
		fail("export scene retains harness nodes")
		return
	var result := PackedScene.new()
	if result.pack(scene) != OK:
		fail("customized Main could not be repacked")
		return
	scene.free()
	var output := OS.get_environment("ALLRPCS_SHIPPING_SCENE_OUTPUT")
	if output.is_empty():
		output = OS.get_temp_dir().path_join("allrpcs-shipping-scene-%s.tscn" % OS.get_process_id())
	if ResourceSaver.save(result, output) != OK:
		fail("customized Main could not be saved")
		return
	for dependency in ResourceLoader.get_dependencies(output):
		if exporter.is_harness_path(dependency):
			fail("customized Main retains harness dependency: " + dependency)
			return
	var reloaded: PackedScene = ResourceLoader.load(output, "PackedScene", ResourceLoader.CACHE_MODE_IGNORE)
	if reloaded == null:
		fail("customized Main could not be loaded")
		return
	var instance := reloaded.instantiate(PackedScene.GEN_EDIT_STATE_MAIN)
	if count_harness(instance, exporter) != 0:
		fail("reloaded Main retains harness nodes")
		return
	instance.free()
	DirAccess.remove_absolute(output)
	print("PASS: shipping scene removed ", before, " harness nodes; repack/load and autoload restoration verified")
	quit(0)

func count_harness(node: Node, exporter: EditorExportPlugin) -> int:
	var count := 0
	for child in node.get_children():
		var script: Script = child.get_script()
		if exporter.is_harness_path(child.scene_file_path) or (script != null and exporter.is_harness_path(script.resource_path)):
			count += 1
		else:
			count += count_harness(child, exporter)
	return count

func fail(message: String) -> void:
	push_error(message)
	quit(1)

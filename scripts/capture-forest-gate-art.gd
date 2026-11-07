extends SceneTree

# Captures the ordinary Main closed forest gate with the Seeker standing at it and away from it.
# No replacement textures or scene packing; only UI, autonomous motion and the follow camera are
# suppressed, and the Seeker is placed for each shot.
const OUTPUT := "../art-direction/forest-gate-art"
# The 640x360 world view centred here shows the gate (y 200), the forest path north of it and the clearing south.
const CENTER := Vector2(744, 230)
# One shot with the Seeker just south of the gate (as close as its collision allows), one with the
# Seeker off to the south-west so the whole gate is unobstructed.
const SEEKER_AT_GATE := Vector2(760, 236)
const SEEKER_AWAY := Vector2(600, 330)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Forest gate capture did not finish within 120 seconds"))
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(OUTPUT).simplify_path()
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		_fail("Forest gate capture directory could not be created")
		return
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	var healer: CharacterBody2D = scene.get_node("Healer")
	seeker.set_physics_process(false)
	healer.set_physics_process(false)
	seeker.get_node("FollowCamera").enabled = false
	var camera := Camera2D.new()
	# Zoom 2 on the forest gate.
	camera.zoom = Vector2(2, 2)
	camera.position = CENTER
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	for _frame in range(12):
		await process_frame
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}}
	for path in ["res://Main.tscn", "res://art/runtime/structures/woodland-04.png",
		"res://../scripts/capture-forest-gate-art.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Forest gate capture input could not be hashed: " + path)
			return
	for shot in [[SEEKER_AT_GATE, "seeker-at-gate"], [SEEKER_AWAY, "seeker-away"]]:
		seeker.global_position = shot[0]
		if not await _save(viewport, output, "forest-gate-" + shot[1], record):
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Forest gate capture evidence could not be written")
		return
	if not file.store_string(JSON.stringify(record, "\t") + "\n"):
		_fail("Forest gate capture evidence write failed: " + error_string(file.get_error()))
		return
	file.close()
	print("PASS: Main forest gate captures with the Seeker at and away from it")
	quit()


func _save(viewport: SubViewport, output: String, name: String, record: Dictionary) -> bool:
	for _frame in range(8):
		await process_frame
	# UI controllers can show themselves after readiness; suppress only in this capture.
	for control in viewport.find_children("*", "Control", true, false):
		control.visible = false
	RenderingServer.force_draw()
	var picture := viewport.get_texture().get_image()
	var relative := "captures/" + name + ".png"
	if picture == null:
		_fail("Forest gate capture failed: " + relative)
		return false
	var saved := picture.save_png(output.path_join(relative))
	if saved != OK:
		_fail("Forest gate capture failed: " + relative + ": " + error_string(saved))
		return false
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	if record.captures[relative].is_empty():
		_fail("Forest gate capture could not be hashed: " + relative)
		return false
	print("Captured ", relative)
	return true


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

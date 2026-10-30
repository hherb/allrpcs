extends SceneTree

# Captures the ordinary Main stone marker with the Seeker standing south (in front) and north
# (behind) of it. No replacement textures or scene packing; only UI, autonomous motion and the
# follow camera are suppressed, and the Seeker is placed for each shot.
const OUTPUT := "../art-direction/stone-marker-art"
const CENTER := Vector2(1080, 350)
const SEEKER_SOUTH := Vector2(1080, 470)
const SEEKER_NORTH := Vector2(1080, 345)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Stone marker capture did not finish within 120 seconds"))
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(OUTPUT).simplify_path()
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		_fail("Stone marker capture directory could not be created")
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
	# Zoom 2 on the marker; the 640x360 world view stays inside the 3200x1800 map.
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
	for path in ["res://Main.tscn", "res://art/runtime/objects/inscriptions-01.png",
		"res://../scripts/capture-stone-marker-art.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Stone marker capture input could not be hashed: " + path)
			return
	for shot in [[SEEKER_SOUTH, "seeker-south"], [SEEKER_NORTH, "seeker-north"]]:
		seeker.global_position = shot[0]
		if not await _save(viewport, output, "stone-marker-" + shot[1], record):
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Stone marker capture evidence could not be written")
		return
	if not file.store_string(JSON.stringify(record, "\t") + "\n"):
		_fail("Stone marker capture evidence write failed: " + error_string(file.get_error()))
		return
	file.close()
	print("PASS: Main stone marker captures with the Seeker south and north of it")
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
		_fail("Stone marker capture failed: " + relative)
		return false
	var saved := picture.save_png(output.path_join(relative))
	if saved != OK:
		_fail("Stone marker capture failed: " + relative + ": " + error_string(saved))
		return false
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	if record.captures[relative].is_empty():
		_fail("Stone marker capture could not be hashed: " + relative)
		return false
	print("Captured ", relative)
	return true


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

extends SceneTree

# Captures the ordinary Main wagon clue with the Seeker standing south (in front) and north
# (behind) of it. No replacement textures or scene packing; only UI, autonomous motion and the
# follow camera are suppressed, and the Seeker is placed for each shot.
const OUTPUT := "../art-direction/wagon-clue-art"
# The 640x360 world view centred here ends at the map's southern edge (y 1800).
const CENTER := Vector2(1900, 1620)
# The strap's foot is at y 1652: one shot south of it with the body over the strap, one 5 px north so the
# strap covers a boot. Both stand a little east so part of the strap stays visible.
const SEEKER_SOUTH := Vector2(1930, 1700)
const SEEKER_NORTH := Vector2(1930, 1647)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Wagon clue capture did not finish within 120 seconds"))
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(OUTPUT).simplify_path()
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		_fail("Wagon clue capture directory could not be created")
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
	# Zoom 2 on the wagon clue.
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
	for path in ["res://Main.tscn", "res://art/runtime/objects/clue-objects-05.png",
		"res://../scripts/capture-wagon-clue-art.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Wagon clue capture input could not be hashed: " + path)
			return
	for shot in [[SEEKER_SOUTH, "seeker-south"], [SEEKER_NORTH, "seeker-north"]]:
		seeker.global_position = shot[0]
		if not await _save(viewport, output, "wagon-clue-" + shot[1], record):
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Wagon clue capture evidence could not be written")
		return
	if not file.store_string(JSON.stringify(record, "\t") + "\n"):
		_fail("Wagon clue capture evidence write failed: " + error_string(file.get_error()))
		return
	file.close()
	print("PASS: Main wagon clue captures with the Seeker south and north of it")
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
		_fail("Wagon clue capture failed: " + relative)
		return false
	var saved := picture.save_png(output.path_join(relative))
	if saved != OK:
		_fail("Wagon clue capture failed: " + relative + ": " + error_string(saved))
		return false
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	if record.captures[relative].is_empty():
		_fail("Wagon clue capture could not be hashed: " + relative)
		return false
	print("Captured ", relative)
	return true


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

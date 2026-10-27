extends SceneTree

# Captures the ordinary Main sickhouse: closed exterior, then the cutaway with each breathing pose.
# No replacement textures or scene packing; only UI, autonomous motion and the follow camera are
# suppressed, and the Seeker and breathing pose are placed for each shot.
const OUTPUT := "../art-direction/sickhouse-patient-bed"
const CENTER := Vector2(2640, 1350)
const SEEKER_INSIDE := Vector2(2528, 1410)
const SEEKER_AWAY := Vector2(3000, 1700)


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(OUTPUT).simplify_path()
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
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
	# Keep the 3200x1800 map filling the frame, as the building captures do.
	camera.position = CENTER.clamp(Vector2(640, 360), Vector2(3200 - 640, 1800 - 360))
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var breathing: AnimationPlayer = scene.get_node("OpeningRegionFirstPatientSceneSource/Patient/Breathing")
	for _frame in range(12):
		await process_frame
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}}
	for path in ["res://Main.tscn", "res://MapStructuresPresentation.cs",
		"res://art/runtime/objects/sickbed-01.png", "res://art/runtime/characters/mira/dormant-01.png",
		"res://art/runtime/characters/mira/dormant-02.png", "res://../scripts/capture-sickhouse-patient-bed.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
	seeker.global_position = SEEKER_AWAY
	seeker.visible = false
	if not await _save(viewport, output, "sickhouse-exterior", record):
		return
	seeker.global_position = SEEKER_INSIDE
	seeker.visible = true
	# Hold each pose still so the capture is deterministic.
	for pose in [[0.1, "pose-1"], [1.7, "pose-2"]]:
		breathing.seek(0.0, true)
		breathing.advance(pose[0])
		breathing.pause()
		if not await _save(viewport, output, "sickhouse-cutaway-" + pose[1], record):
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Sickhouse capture evidence could not be written")
		return
	file.store_string(JSON.stringify(record, "\t") + "\n")
	print("PASS: Main sickhouse captures: closed exterior and cutaway with both breathing poses")
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
	if picture == null or picture.save_png(output.path_join(relative)) != OK:
		_fail("Sickhouse capture failed: " + relative)
		return false
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	print("Captured ", relative)
	return true


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

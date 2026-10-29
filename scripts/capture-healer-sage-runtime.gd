extends SceneTree

# Captures ordinary Main at the village square: the Healer and Sage at rest, then the Healer
# mid-stride walking east. No replacement textures or scene packing; UI, autonomous motion and the
# follow camera are suppressed, the Seeker is hidden off-shot, the away Healer is shown, and each
# animation is paused on a fixed frame.
const OUTPUT := "../art-direction/healer-sage-runtime"
const CENTER := Vector2(2120, 900)
const SEEKER_AWAY := Vector2(2120, 1150)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Healer/Sage capture did not finish within 120 seconds"))
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
	# The Healer begins away on its authored independent task; show it, as the footstep proof does.
	healer.visible = true
	seeker.get_node("FollowCamera").enabled = false
	seeker.global_position = SEEKER_AWAY
	seeker.visible = false
	var camera := Camera2D.new()
	camera.position = CENTER
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var healer_art: AnimatedSprite2D = healer.get_node("WalkAnimation")
	var sage_art: AnimatedSprite2D = scene.get_node("SageHearingPosition/Figure")
	for _frame in range(12):
		await process_frame
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}}
	for path in ["res://Main.tscn", "res://Healer.cs", "res://SeekerWalkAnimationPresenter.cs",
		"res://art/runtime/characters/healer/animations.tres",
		"res://art/runtime/characters/orin/animations.tres",
		"res://../scripts/capture-healer-sage-runtime.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Healer/Sage capture input could not be hashed: " + path)
			return
	for shot in [["idle_south", 0, "village-square-at-rest"], ["walk_east", 2, "healer-walking-east"]]:
		healer_art.play(shot[0])
		healer_art.pause()
		healer_art.frame = shot[1]
		sage_art.pause()
		sage_art.frame = 0
		if not await _save(viewport, output, shot[2], record):
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Healer/Sage capture evidence could not be written")
		return
	if not file.store_string(JSON.stringify(record, "\t") + "\n"):
		_fail("Healer/Sage capture evidence write failed: " + error_string(file.get_error()))
		return
	file.close()
	print("PASS: Main village-square captures: Healer and Sage at rest, Healer walking east")
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
		_fail("Healer/Sage capture failed: " + relative)
		return false
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	print("Captured ", relative)
	return true


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

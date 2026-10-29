extends SceneTree

# Captures ordinary Main at the village square with the depth-order presenter running: the Seeker
# at the water cask just north of the Sage (the Sage draws in front of both), then the Seeker
# 72 px north of the Healer, where a follow stops (the Healer draws in front). No replacement
# textures or scene packing; UI and autonomous motion are suppressed, the follow camera is
# replaced by a fixed one, the away Healer is shown, and each animation is paused on frame 0.
# Run without --headless.
const OUTPUT := "../art-direction/character-depth-order"
const CENTER := Vector2(2160, 880)
const SHOTS := [
	# [Seeker position, Healer position, capture name]
	[Vector2(2240, 915), Vector2(2000, 950), "seeker-behind-sage-at-cask"],
	[Vector2(2060, 878), Vector2(2060, 950), "seeker-behind-healer"],
]


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Depth-order capture did not finish within 120 seconds"))
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
	var camera := Camera2D.new()
	camera.position = CENTER
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var sprites: Array[AnimatedSprite2D] = [seeker.get_node("WalkAnimation"),
		healer.get_node("WalkAnimation"), scene.get_node("SageHearingPosition/Figure")]
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}}
	for path in ["res://Main.tscn", "res://CharacterDepthOrderPresentation.cs",
		"res://../src/Application/Presentation/DepthDrawOrder.cs",
		"res://../scripts/capture-character-depth-order.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Depth-order capture input could not be hashed: " + path)
			return
	# Render every shot before writing any, so a failed run leaves the retained package untouched.
	var pictures := {}
	for shot in SHOTS:
		seeker.global_position = shot[0]
		healer.global_position = shot[1]
		for sprite in sprites:
			sprite.play("idle_south")
			sprite.pause()
			sprite.frame = 0
		var picture := await _render(viewport)
		if picture == null:
			_fail("Depth-order capture rendered no full-size image: " + shot[2])
			return
		pictures["captures/" + shot[2] + ".png"] = picture
	for relative in pictures:
		var path := output.path_join(relative)
		if pictures[relative].save_png(path) != OK:
			_fail("Depth-order capture failed: " + relative)
			return
		record.captures[relative] = FileAccess.get_sha256(path)
		if record.captures[relative].is_empty():
			_fail("Depth-order capture could not be hashed: " + relative)
			return
		print("Captured ", relative)
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Depth-order capture evidence could not be written")
		return
	if not file.store_string(JSON.stringify(record, "\t") + "\n"):
		_fail("Depth-order capture evidence write failed: " + error_string(file.get_error()))
		return
	file.close()
	print("PASS: Main village-square captures: Seeker behind the Sage at the cask, Seeker behind the Healer")
	quit()


## Returns the viewport image, or null when nothing full-size was rendered (e.g. under --headless).
## The frames it waits also let the presenter in Main re-rank the moved characters.
func _render(viewport: SubViewport) -> Image:
	for _frame in range(8):
		await process_frame
	# UI controllers can show themselves after readiness; suppress only in this capture.
	for control in viewport.find_children("*", "Control", true, false):
		control.visible = false
	RenderingServer.force_draw()
	var picture := viewport.get_texture().get_image()
	if picture == null or picture.is_empty() or picture.get_size() != viewport.size:
		return null
	return picture


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)

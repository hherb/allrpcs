extends SceneTree

# Captures ordinary Main's Seeker on the village square ground: for each direction, its idle pose
# and four walk poses (quarter-cycle steps through the reference gait), cropped around the Seeker
# and laid out as one review sheet. UI, autonomous motion and the follow camera are suppressed;
# the Healer is hidden off-shot. Each crop is the same window, so the feet sharing one ground line
# across idle and walk poses is visible directly.
const OUTPUT := "../art-direction/seeker-reference-gait-runtime"
const CENTER := Vector2(2120, 900)
const CROP := Vector2i(200, 220)
# The Seeker's origin (its feet) sits this far down the crop.
const FEET_IN_CROP := 190
const DIRECTIONS := ["south", "west", "east", "north"]


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging.
	create_timer(120).timeout.connect(_fail.bind("Seeker gait capture did not finish within 120 seconds"))
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
	healer.visible = false
	seeker.get_node("FollowCamera").enabled = false
	seeker.global_position = CENTER
	var camera := Camera2D.new()
	camera.position = CENTER
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var art: AnimatedSprite2D = seeker.get_node("WalkAnimation")
	for _frame in range(12):
		await process_frame
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}}
	for path in ["res://Main.tscn", "res://SeekerWalkAnimationPresenter.cs",
		"res://art/runtime/characters/seeker/walk-animations.tres",
		"res://../scripts/capture-seeker-reference-gait-runtime.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
		if record.inputs[path].is_empty():
			_fail("Seeker gait capture input could not be hashed: " + path)
			return
	# The camera centres the Seeker's origin in the 1280x720 view.
	var crop_origin := Vector2i(640 - CROP.x / 2, 360 - FEET_IN_CROP)
	var sheet := Image.create(CROP.x * 5, CROP.y * DIRECTIONS.size(), false, Image.FORMAT_RGBA8)
	for row in DIRECTIONS.size():
		var direction: String = DIRECTIONS[row]
		var walk := "walk_" + direction
		var count := art.sprite_frames.get_frame_count(walk)
		var shots := [["idle_" + direction, 0]]
		for quarter in 4:
			shots.append([walk, quarter * count / 4])
		for column in shots.size():
			# play() goes through the presenter's AnimationChanged anchoring, as gameplay does.
			art.play(shots[column][0])
			art.pause()
			art.frame = shots[column][1]
			var picture := await _render(viewport)
			if picture == null:
				_fail("Seeker gait capture rendered no full-size image: %s %d" % shots[column])
				return
			sheet.blit_rect(picture, Rect2i(crop_origin, CROP), Vector2i(column * CROP.x, row * CROP.y))
	var relative := "captures/seeker-idle-and-walk-poses.png"
	var path := output.path_join(relative)
	if sheet.save_png(path) != OK:
		_fail("Seeker gait capture failed: " + relative)
		return
	record.captures[relative] = FileAccess.get_sha256(path)
	print("Captured ", relative)
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if file == null:
		_fail("Seeker gait capture evidence could not be written")
		return
	file.store_string(JSON.stringify(record, "  ") + "\n")
	file.close()
	quit(0)


func _render(viewport: SubViewport) -> Image:
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	var image := viewport.get_texture().get_image()
	if image == null or image.get_size() != viewport.size:
		return null
	image.convert(Image.FORMAT_RGBA8)
	return image


func _fail(message: String) -> void:
	push_error(message)
	quit(1)

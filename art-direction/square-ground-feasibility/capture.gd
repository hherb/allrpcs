extends SceneTree

# External disposable proof only. Never save/pack Main or import the proof layers.
const CENTER := Vector2(2200, 900)
const PAINT_ORIGIN := Vector2(1920, 620)


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/square-ground-feasibility").simplify_path()
	if FileAccess.file_exists(output.path_join("owner-acceptance.json")):
		push_error("Accepted Square proof captures are immutable; prepare a separate revision")
		quit(1)
		return
	var sources := ["res://Main.tscn", get_script().resource_path,
		output.path_join("inventory.json"), output.path_join("baseline.json"),
		output.path_join("proof-inputs/hard-support-layer.png"),
		output.path_join("proof-inputs/feathered-layer.png")]
	var record := {"engine": Engine.get_version_info().get("string"),
		"scope": "External Square proof; actual retained routes/objects; no saved runtime change",
		"inputs": {}, "captures": {}, "frames": {}, "target_center": [2200, 900]}
	for path in sources:
		record["inputs"][path] = FileAccess.get_sha256(path)
	var viewport := SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.size = Vector2i(1280, 720)
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = CENTER
	seeker.visible = false
	for child in scene.get_children():
		if child is CanvasLayer or child is CharacterBody2D:
			child.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	var hard := _sprite(output.path_join("proof-inputs/hard-support-layer.png"))
	var feather := _sprite(output.path_join("proof-inputs/feathered-layer.png"))
	if hard == null or feather == null:
		quit(1)
		return
	scene.add_child(hard)
	scene.add_child(feather)
	for zoom in [0.8, 1.0, 2.0]:
		for frame in [["wide", 720], ["tall", 827]]:
			viewport.size = Vector2i(1280, frame[1])
			camera.zoom = Vector2(zoom, zoom)
			camera.reset_smoothing()
			for variant in ["baseline", "hard-support", "feathered"]:
				hard.visible = variant == "hard-support"
				feather.visible = variant == "feathered"
				for _frame in range(8):
					await process_frame
				RenderingServer.force_draw()
				var image := viewport.get_texture().get_image()
				var filename := "captures/%s-%.1fx-%s.png" % [variant, zoom, frame[0]]
				if image == null or image.get_size() != viewport.size \
					or image.save_png(output.path_join(filename)) != OK:
					push_error("Square external capture failed: " + filename)
					quit(1)
					return
				record["captures"][filename] = FileAccess.get_sha256(output.path_join(filename))
				var transform := viewport.canvas_transform
				record["frames"][filename] = {"zoom": zoom, "viewport": [1280, frame[1]],
					"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
					"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
						transform.origin.x, transform.origin.y]}
				print("Square external preview: ", filename)
	for path in sources:
		if record["inputs"][path] != FileAccess.get_sha256(path):
			push_error("Square preview input drift: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Square capture evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	quit()


func _sprite(path: String) -> Sprite2D:
	var image := Image.new()
	if image.load(path) != OK or image.get_size() != Vector2i(560, 560):
		push_error("Square external paint input failed: " + path)
		return null
	var sprite := Sprite2D.new()
	sprite.texture = ImageTexture.create_from_image(image)
	sprite.position = PAINT_ORIGIN
	sprite.centered = false
	sprite.z_index = -90
	sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	sprite.texture_repeat = CanvasItem.TEXTURE_REPEAT_DISABLED
	return sprite

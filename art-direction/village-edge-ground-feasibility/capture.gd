extends SceneTree

# External disposable proof only. Never save/pack Main or import proof textures.
const CENTER := Vector2(1700, 840)
const PAINT_ORIGIN := Vector2(1480, 620)
const VARIANTS := ["baseline", "hard-support", "feather-16", "feather-24"]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/village-edge-ground-feasibility").simplify_path()
	if FileAccess.file_exists(output.path_join("owner-acceptance.json")) \
		or FileAccess.file_exists(output.path_join("capture-evidence.json")):
		push_error("Existing Village Edge captures are immutable; prepare a separate revision")
		quit(1)
		return
	var directory_error := DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	if directory_error != OK:
		push_error("Cannot create external capture directory")
		quit(1)
		return
	var sources := ["res://Main.tscn", get_script().resource_path,
		output.path_join("inventory.json"), output.path_join("../village-edge-ground-coverage/baseline.json")]
	var inventory_file := FileAccess.open(output.path_join("inventory.json"), FileAccess.READ)
	if inventory_file == null:
		push_error("Missing external inventory")
		quit(1)
		return
	var inventory = JSON.parse_string(inventory_file.get_as_text())
	inventory_file.close()
	if not inventory is Dictionary or not inventory.get("sources") is Dictionary:
		push_error("Invalid external inventory")
		quit(1)
		return
	for path in inventory["sources"]:
		var absolute: String = path if path.begins_with("/") else output.path_join("../..").path_join(path).simplify_path()
		if FileAccess.get_sha256(absolute) != inventory["sources"][path]:
			push_error("Bound external source drift: " + path)
			quit(1)
			return
		sources.append(absolute)
	for variant in VARIANTS.slice(1):
		var path := output.path_join("proof-inputs/%s-layer.png" % variant)
		if FileAccess.get_sha256(path) != inventory["outputs"]["proof-inputs/%s-layer.png" % variant]:
			push_error("External layer drift: " + variant)
			quit(1)
			return
		sources.append(path)
	var record := {"engine": Engine.get_version_info().get("string"),
		"scope": "External Village Edge apron; actual retained scene; no saved runtime changes",
		"inputs": {}, "captures": {}, "frames": {}, "target_center": [1700, 840],
		"hidden": "Root CanvasLayers/CharacterBody2Ds only; Village Edge shell/openings, woodland transitions, all accepted ground/channel and labels retained"}
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
	for child in scene.get_children():
		if child is CanvasLayer or child is CharacterBody2D:
			child.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	var sprites := {}
	for variant in VARIANTS.slice(1):
		var sprite := _sprite(output.path_join("proof-inputs/%s-layer.png" % variant))
		if sprite == null:
			quit(1)
			return
		sprite.visible = false
		scene.add_child(sprite)
		sprites[variant] = sprite
	for zoom in [0.8, 1.0, 2.0]:
		for frame in [["wide", 720], ["tall", 827]]:
			viewport.size = Vector2i(1280, frame[1])
			camera.zoom = Vector2(zoom, zoom)
			camera.reset_smoothing()
			for variant in VARIANTS:
				for name in sprites:
					sprites[name].visible = name == variant
				for _frame in range(8):
					await process_frame
				RenderingServer.force_draw()
				var image := viewport.get_texture().get_image()
				var filename := "captures/%s-%.1fx-%s.png" % [variant, zoom, frame[0]]
				if FileAccess.file_exists(output.path_join(filename)):
					push_error("Refusing to overwrite external evidence: " + filename)
					quit(1)
					return
				if image == null or image.get_size() != viewport.size \
					or image.save_png(output.path_join(filename)) != OK:
					push_error("Village Edge external capture failed: " + filename)
					quit(1)
					return
				record["captures"][filename] = FileAccess.get_sha256(output.path_join(filename))
				var transform := viewport.canvas_transform
				record["frames"][filename] = {"zoom": zoom, "viewport": [1280, frame[1]],
					"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
					"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
						transform.origin.x, transform.origin.y]}
				print("Village Edge external preview: ", filename)
	for path in sources:
		if record["inputs"][path] != FileAccess.get_sha256(path):
			push_error("Village Edge preview input drift: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Village Edge capture evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	quit()


func _sprite(path: String) -> Sprite2D:
	var image := Image.new()
	if image.load(path) != OK or image.get_size() != Vector2i(500, 390):
		push_error("Village Edge external paint input failed: " + path)
		return null
	var sprite := Sprite2D.new()
	sprite.texture = ImageTexture.create_from_image(image)
	sprite.position = PAINT_ORIGIN
	sprite.centered = false
	sprite.z_index = -78
	sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	sprite.texture_repeat = CanvasItem.TEXTURE_REPEAT_DISABLED
	return sprite

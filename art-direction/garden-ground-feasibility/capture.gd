extends SceneTree

# Disposable external comparison; never save or pack the ordinary scene.
const CENTER := Vector2(2300, 1300)
const PAINT_ORIGIN := Vector2(2048, 1120)
const VIEW_SIZE := Vector2i(1280, 720)


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/garden-ground-feasibility"
	).simplify_path()
	var viewport := SubViewport.new()
	var record := {
		"schema_version": 1,
		"engine": Engine.get_version_info().get("string"),
		"scope": "Disposable external Garden preview; no saved scene or runtime promotion",
		"inventory_sha256": FileAccess.get_sha256(output.path_join("inventory.json")),
		"scene_sha256": FileAccess.get_sha256("res://Main.tscn"),
		"script_sha256": FileAccess.get_sha256(get_script().resource_path),
		"viewport": [VIEW_SIZE.x, VIEW_SIZE.y],
		"camera_center": [CENTER.x, CENTER.y],
		"paint_origin": [PAINT_ORIGIN.x, PAINT_ORIGIN.y],
		"soil_layer_sha256": FileAccess.get_sha256(output.path_join("proof-inputs/soil-layer.png")),
		"meadow_layer_sha256": FileAccess.get_sha256(output.path_join("proof-inputs/meadow-layer.png")),
		"captures": {},
	}
	viewport.size = VIEW_SIZE
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = CENTER
	seeker.visible = false
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
		elif child is CharacterBody2D:
			child.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	var meadow := _proof_sprite(output.path_join("proof-inputs/meadow-layer.png"), -90)
	var soil := _proof_sprite(output.path_join("proof-inputs/soil-layer.png"), -89)
	if meadow == null or soil == null:
		quit(1)
		return
	scene.add_child(meadow)
	scene.add_child(soil)
	for zoom in [1.0, 2.0]:
		camera.zoom = Vector2(zoom, zoom)
		camera.reset_smoothing()
		for variant in ["baseline", "soil-only", "meadow-apron"]:
			meadow.visible = variant == "meadow-apron"
			soil.visible = variant != "baseline"
			for _frame in range(8):
				await process_frame
			# Obscured macOS windows can stop emitting frame_post_draw.
			RenderingServer.force_draw()
			var image := viewport.get_texture().get_image()
			var path := output.path_join("review/godot-%s-%.1fx.png" % [variant, zoom])
			if image == null or image.get_size() != VIEW_SIZE or image.save_png(path) != OK:
				push_error("Garden external capture failed: " + path)
				quit(1)
				return
			print("External Garden preview: ", path)
			record["captures"]["review/godot-%s-%.1fx.png" % [variant, zoom]] = FileAccess.get_sha256(path)
	if record["inventory_sha256"] != FileAccess.get_sha256(output.path_join("inventory.json")) \
		or record["scene_sha256"] != FileAccess.get_sha256("res://Main.tscn") \
		or record["soil_layer_sha256"] != FileAccess.get_sha256(output.path_join("proof-inputs/soil-layer.png")) \
		or record["meadow_layer_sha256"] != FileAccess.get_sha256(output.path_join("proof-inputs/meadow-layer.png")):
		push_error("Garden preview inputs changed during capture")
		quit(1)
		return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Garden capture evidence could not be saved")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	quit()


func _proof_sprite(path: String, depth: int) -> Sprite2D:
	var image := Image.new()
	if image.load(path) != OK or image.get_size() != Vector2i(512, 384):
		push_error("Garden proof image input failed: " + path)
		return null
	var sprite := Sprite2D.new()
	sprite.texture = ImageTexture.create_from_image(image)
	sprite.centered = false
	sprite.position = PAINT_ORIGIN
	sprite.z_index = depth
	return sprite
